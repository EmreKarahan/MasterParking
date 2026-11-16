using MasterParking.App;
using MasterParking.CameraSystem;
using MasterParking.Core;
using MasterParking.Gameplay;
using MasterParking.Input;
using MasterParking.UI;
using MasterParking.Utilities;
using MasterParking.Vehicle;
using UnityEngine;
using UnityEngine.UI;

namespace MasterParking.UI
{
    /// <summary>
    /// Builds and owns one gameplay attempt: world, player car, camera, HUD and
    /// session. Closing it tears everything down synchronously so a restart does
    /// not leave the previous attempt ticking.
    /// </summary>
    public sealed class GameplayScreen : MonoBehaviour
    {
        private AppRoot _app;
        private AppFlow _flow;
        private LevelDefinition _level;
        private UiRoot _ui;
        private GameObject _gameplayRoot;
        private GameObject _cameraObject;
        private LevelWorld _world;
        private VehicleController _vehicle;
        private GameCameraRig _cameraRig;
        private TouchGameInput _touch;
        private CompositeGameInput _input;
        private HudView _hud;
        private GameplaySession _session;
        private bool _closed;

        public void Initialize(AppRoot app, AppFlow flow, LevelDefinition level)
        {
            _app = app;
            _flow = flow;
            _level = level;
            _ui = GetComponent<UiRoot>();

            BuildWorld();
            BuildCamera();
            BuildVehicle();
            BuildInputAndHud();
            StartSession();
        }

        private void Update()
        {
            _session?.Tick(Time.deltaTime);
        }

        public void PauseForLifecycle()
        {
            _session?.Pause();
        }

        public void Close()
        {
            if (_closed)
            {
                return;
            }

            _closed = true;
            _session?.Dispose();
            _session = null;
            _hud?.Dispose();
            _hud = null;

            if (_cameraObject != null)
            {
                Destroy(_cameraObject);
                _cameraObject = null;
            }

            if (_gameplayRoot != null)
            {
                Destroy(_gameplayRoot);
                _gameplayRoot = null;
            }
        }

        private void OnDestroy()
        {
            Close();
        }

        private void BuildWorld()
        {
            _gameplayRoot = new GameObject("GameplayWorld");
            _world = LevelBuilder.Build(_level, _gameplayRoot.transform);
        }

        private void BuildCamera()
        {
            _cameraObject = new GameObject("CameraRig");
            _cameraRig = _cameraObject.AddComponent<GameCameraRig>();
            _cameraRig.Initialize(_world.Bounds);
        }

        private void BuildVehicle()
        {
            GameObject car = VehicleFactory.Create(
                _gameplayRoot.transform,
                "PlayerCar",
                new Vector3(_level.Start.X, 0f, _level.Start.Z),
                MathConversions.HeadingToRotation(_level.Start.Angle),
                Color.white,
                true);

            var view = car.GetComponent<VehicleView>();
            _vehicle = new VehicleController(new VehicleConfig(), _level.Start, view);
            _cameraRig.Track(_vehicle.State);
        }

        private void BuildInputAndHud()
        {
            Image dragArea = UiFactory.Image(_ui.SafeArea, "CameraDrag", UiTheme.Transparent, null, true);
            UiFactory.Stretch(dragArea.rectTransform);
            var cameraDrag = dragArea.gameObject.AddComponent<CameraDragArea>();
            cameraDrag.Dragged += delta => _cameraRig.AddDrag(delta);

            _touch = new TouchGameInput();
            _input = new CompositeGameInput(_touch);
            _hud = new HudView(
                _ui.SafeArea,
                _app,
                _level,
                _app.Levels.Count,
                _cameraRig.Mode,
                _app.Audio);

            _hud.GasChanged += _touch.SetThrottle;
            _hud.BrakeChanged += _touch.SetBrake;
            _hud.SteeringChanged += _touch.SetSteering;
            _hud.GearSelected += gear =>
            {
                _touch.SetGear(gear);
                _hud.SetGear(gear);
            };

            _hud.PausePressed += () => _session?.Pause();
            _hud.ResumePressed += () => _session?.Resume();
            _hud.CameraPressed += () => _session?.ToggleCamera();
            _hud.RestartPressed += () => _flow.RestartLevel();
            _hud.NextPressed += () => _flow.StartNextLevel();
            _hud.LevelsPressed += () => _flow.ShowLevelSelect();
            _hud.MenuPressed += () => _flow.ShowMainMenu();
        }

        private void StartSession()
        {
            _session = new GameplaySession(_app, _world, _vehicle, _cameraRig, _input, _hud);
            _session.RestartRequested += () => _flow.RestartLevel();
        }
    }
}
