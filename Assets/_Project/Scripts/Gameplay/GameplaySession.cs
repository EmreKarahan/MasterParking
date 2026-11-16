using System;
using MasterParking.App;
using MasterParking.Audio;
using MasterParking.CameraSystem;
using MasterParking.Core;
using MasterParking.Input;
using MasterParking.Utilities;
using MasterParking.Vehicle;
using UnityEngine;

namespace MasterParking.Gameplay
{
    /// <summary>
    /// One level attempt. Owns the timer, parking evaluator, traffic and crash
    /// checks; the screen owns lifecycle and buttons. The session is deliberately
    /// not a MonoBehaviour so it is constructed and disposed explicitly.
    /// </summary>
    public sealed class GameplaySession
    {
        private const float FixedStep = 1f / 60f;
        private const int MaxStepsPerFrame = 4;

        private readonly AppRoot _app;
        private readonly LevelWorld _world;
        private readonly VehicleController _vehicle;
        private readonly GameCameraRig _camera;
        private readonly IGameInput _input;
        private readonly IGameplayHud _hud;
        private readonly LevelTimer _timer = new LevelTimer();
        private readonly ParkingEvaluator _parking;
        private readonly VehicleCollisionProbe _probe;
        private readonly AudioService _audio;

        private ParkingStatus _parkingStatus;
        private GameState _stateBeforePause = GameState.Playing;
        private float _accumulator;
        private bool _disposed;

        public GameplaySession(
            AppRoot app,
            LevelWorld world,
            VehicleController vehicle,
            GameCameraRig camera,
            IGameInput input,
            IGameplayHud hud)
        {
            _app = app;
            _world = world;
            _vehicle = vehicle;
            _camera = camera;
            _input = input;
            _hud = hud;
            _audio = app.Audio;
            _parking = new ParkingEvaluator(new ParkingSettings());
            _probe = new VehicleCollisionProbe(world.Obstacles);

            app.StateMachine.ResetTo(GameState.Playing);
            _timer.Start();
            _audio.BeginGameplay();

            _hud.SetLevelInfo(world.Level.Name, world.Level.Id, app.Levels.Count);
            _hud.SetGear(_input.Gear);
            _hud.SetCameraMode(_camera.Mode);
            _hud.SetControlsEnabled(true);
            _camera.Track(_vehicle.State);
        }

        public event Action RestartRequested;

        public LevelWorld World => _world;
        public bool IsPaused => _app.StateMachine.State == GameState.Paused;
        public bool IsFinished => _app.StateMachine.State == GameState.Crashed ||
                                  _app.StateMachine.State == GameState.LevelCompleted;

        public void Tick(float deltaTime)
        {
            if (_disposed)
            {
                return;
            }

            GameState state = _app.StateMachine.State;
            if (state != GameState.Playing && state != GameState.Parking)
            {
                return;
            }

            float dt = Mathf.Clamp(deltaTime, 0f, 0.1f);
            GameInputFrame frame = _input.Read();

            if (frame.Pause)
            {
                Pause();
                return;
            }

            if (frame.Restart)
            {
                RestartRequested?.Invoke();
                return;
            }

            if (frame.ToggleCamera)
            {
                ToggleCamera();
            }

            _timer.Tick(dt);
            _accumulator += dt;

            int steps = 0;
            while (_accumulator >= FixedStep && steps < MaxStepsPerFrame && !_disposed)
            {
                _accumulator -= FixedStep;
                steps++;
                if (!StepSimulation(frame.Vehicle, FixedStep))
                {
                    break;
                }
            }

            if (steps >= MaxStepsPerFrame)
            {
                _accumulator = 0f;
            }

            _vehicle.ApplyVisuals();
            _camera.Track(_vehicle.State);
            UpdateHud(frame.Vehicle);
        }

        public void Pause()
        {
            GameState state = _app.StateMachine.State;
            if (state != GameState.Playing && state != GameState.Parking)
            {
                return;
            }

            _stateBeforePause = state;
            _app.StateMachine.ResetTo(GameState.Paused);
            _timer.Stop();
            _audio.SetPaused(true);
            _hud.ShowPause(true);
            _hud.SetControlsEnabled(false);
        }

        public void Resume()
        {
            if (_app.StateMachine.State != GameState.Paused)
            {
                return;
            }

            _app.StateMachine.ResetTo(_stateBeforePause);
            _timer.Resume();
            _audio.SetPaused(false);
            _hud.ShowPause(false);
            _hud.SetControlsEnabled(true);
        }

        public void ToggleCamera()
        {
            _camera.ToggleMode();
            _hud.SetCameraMode(_camera.Mode);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _timer.Stop();
            _audio.EndGameplay();
            _world.Dispose();
        }

        private bool StepSimulation(in VehicleInput input, float step)
        {
            _vehicle.Step(input, step);
            _vehicle.ApplyVisuals();
            _camera.Track(_vehicle.State);

            for (int i = 0; i < _world.Traffic.Count; i++)
            {
                _world.Traffic[i].Step(step);
            }

            VehicleState state = _vehicle.State;
            Vector3 position = MathConversions.ToWorld(state.Position);
            Quaternion rotation = MathConversions.HeadingToRotation(state.Heading);

            if (_probe.TryDetect(position, rotation, out ObstacleType obstacle))
            {
                Crash(CrashMessages.For(obstacle));
                return false;
            }

            LevelBounds bounds = _world.Bounds;
            if (state.Position.X < bounds.MinX - 1.5f || state.Position.X > bounds.MaxX + 1.5f ||
                state.Position.Y < bounds.MinZ - 1.5f || state.Position.Y > bounds.MaxZ + 1.5f)
            {
                Crash(CrashMessages.ForOutOfBounds());
                return false;
            }

            _parkingStatus = _parking.Evaluate(state, _world.Level.Target, step);
            if (_parkingStatus.Completed)
            {
                Complete();
                return false;
            }

            if (_parkingStatus.InsideZone && _app.StateMachine.State == GameState.Playing)
            {
                _app.StateMachine.ResetTo(GameState.Parking);
            }
            else if (!_parkingStatus.InsideZone && _app.StateMachine.State == GameState.Parking)
            {
                _app.StateMachine.ResetTo(GameState.Playing);
            }

            return true;
        }

        private void UpdateHud(in VehicleInput input)
        {
            _hud.SetTimer(_timer.Elapsed);
            _hud.SetGear(_input.Gear);
            _hud.SetSpeed(Mathf.Abs(_vehicle.State.Speed));
            _hud.SetParking(_parkingStatus);

            float maxSpeed = Mathf.Max(0.1f, _vehicle.Simulation.Config.MaxForwardSpeed);
            _audio.UpdateVehicle(Mathf.Abs(_vehicle.State.Speed) / maxSpeed, input.Throttle, input.Brake);
        }

        private void Crash(string reason)
        {
            _vehicle.Halt();
            _app.StateMachine.ResetTo(GameState.Crashed);
            _timer.Stop();
            _audio.PlayCrash();
            Haptics.Vibrate(_app.Settings);
            _camera.Shake(0.45f, 0.35f);
            _hud.SetControlsEnabled(false);
            _hud.ShowCrash(reason);
        }

        private void Complete()
        {
            _vehicle.Halt();
            _audio.PlayWin();
            LevelCompletionResult result = _app.Save.RecordCompletion(_world.Level, _timer.Elapsed);
            _app.StateMachine.ResetTo(GameState.LevelCompleted);
            _timer.Stop();

            LevelDefinition next = _app.Levels.GetNext(_world.Level.Id);
            _hud.SetControlsEnabled(false);
            _hud.ShowWin(result, next != null);
        }
    }
}
