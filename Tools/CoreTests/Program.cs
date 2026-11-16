using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MasterParking.Core;

namespace MasterParking.Core.Tests
{
    /// <summary>
    /// Dependency-free test runner for the pure gameplay core. Runs with:
    ///   dotnet run --project Tools/CoreTests
    /// The same behaviours are mirrored in Unity EditMode tests for CI inside Unity.
    /// </summary>
    internal static class Program
    {
        private static int _passed;
        private static readonly List<string> Failures = new List<string>();

        private static int Main()
        {
            Run("Vehicle accelerates and respects forward speed cap", VehicleAccelerates);
            Run("Brake stops the car and never reverses direction", BrakeStopsWithoutReversing);
            Run("Reverse gear moves backwards with a lower cap", ReverseGear);
            Run("Park gear acts as a handbrake", ParkGearHandbrake);
            Run("Steering ramps, clamps and returns to centre", SteeringBehaviour);
            Run("Touch steering is applied as an absolute angle", AbsoluteSteering);
            Run("Yaw follows speed and steering angle", YawFollowsSteering);
            Run("Simulation is frame-rate independent within tolerance", FrameRateIndependence);
            Run("Parking requires alignment, low speed and hold time", ParkingHoldCompletes);
            Run("Parking progress decays when the car creeps", ParkingDecaysWhenMoving);
            Run("Outside the bay resets progress", ParkingOutsideResets);
            Run("Park gear doubles parking progress", ParkingParkGearMultiplier);
            Run("Stars follow par and 145% thresholds", StarThresholds);
            Run("Progression unlocks sequentially and keeps best results", ProgressionBehaviour);
            Run("Save data sanitises corrupted values", ProgressionSanitises);
            Run("Level timer honours pause and resume", LevelTimerBehaviour);
            Run("State machine allows legal transitions only", StateMachineBehaviour);
            Run("Traffic mover ping-pongs within its bounds", TrafficMoverBounces);
            Run("All 18 extracted levels are valid and solvable geometry", AllLevelsValid);
            Run("Level metadata matches the source game", LevelMetadata);
            Run("Level bounds enclose the target", LevelBoundsEncloseTarget);

            Console.WriteLine();
            Console.WriteLine($"Passed: {_passed}");
            Console.WriteLine($"Failed: {Failures.Count}");
            foreach (string failure in Failures)
            {
                Console.WriteLine("  FAIL " + failure);
            }

            return Failures.Count == 0 ? 0 : 1;
        }

        private static void Run(string name, Action test)
        {
            try
            {
                test();
                _passed++;
                Console.WriteLine("PASS  " + name);
            }
            catch (Exception ex)
            {
                Failures.Add(name + " -> " + ex.Message);
                Console.WriteLine("FAIL  " + name);
            }
        }

        private static void AssertTrue(bool condition, string message = "Expected true")
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void AssertFalse(bool condition, string message = "Expected false") =>
            AssertTrue(!condition, message);

        private static void AssertNear(float expected, float actual, float tolerance, string message = "")
        {
            if (Math.Abs(expected - actual) > tolerance)
            {
                throw new InvalidOperationException(
                    $"{message} expected {expected:0.####} but was {actual:0.####} (tol {tolerance})");
            }
        }

        private static VehicleSimulation CreateDrivingCar()
        {
            var simulation = new VehicleSimulation(new VehicleConfig());
            simulation.Reset(Vec2.Zero, 0f, Gear.Drive);
            return simulation;
        }

        private static void Step(VehicleSimulation simulation, int frames, float dt, VehicleInput input)
        {
            for (int i = 0; i < frames; i++)
            {
                simulation.Step(input, dt);
            }
        }

        private static void VehicleAccelerates()
        {
            VehicleSimulation simulation = CreateDrivingCar();
            var input = new VehicleInput { Gear = Gear.Drive, Throttle = true };
            Step(simulation, 600, 1f / 60f, input);

            AssertNear(9.3f, simulation.State.Speed, 0.001f, "speed cap");
            AssertTrue(simulation.State.Position.X > 50f, "car should have moved forward");
            AssertNear(0f, simulation.State.Position.Y, 0.001f, "no lateral drift while straight");
        }

        private static void BrakeStopsWithoutReversing()
        {
            VehicleSimulation simulation = CreateDrivingCar();
            Step(simulation, 120, 1f / 60f, new VehicleInput { Gear = Gear.Drive, Throttle = true });
            AssertTrue(simulation.State.Speed > 5f, "should be moving before braking");

            Step(simulation, 120, 1f / 60f, new VehicleInput { Gear = Gear.Drive, Brake = true });
            AssertNear(0f, simulation.State.Speed, 0.0001f, "brake should stop, not invert");
        }

        private static void ReverseGear()
        {
            VehicleSimulation simulation = CreateDrivingCar();
            var input = new VehicleInput { Gear = Gear.Reverse, Throttle = true };
            Step(simulation, 600, 1f / 60f, input);

            AssertNear(-5.7f, simulation.State.Speed, 0.001f, "reverse cap");
            AssertTrue(simulation.State.Position.X < -10f, "car should move backwards");
        }

        private static void ParkGearHandbrake()
        {
            VehicleSimulation simulation = CreateDrivingCar();
            Step(simulation, 120, 1f / 60f, new VehicleInput { Gear = Gear.Drive, Throttle = true });
            Step(simulation, 120, 1f / 60f, new VehicleInput { Gear = Gear.Park, Throttle = true });
            AssertNear(0f, simulation.State.Speed, 0.0001f, "park gear must hold the car");
        }

        private static void SteeringBehaviour()
        {
            var config = new VehicleConfig();
            VehicleSimulation simulation = CreateDrivingCar();
            var input = new VehicleInput
            {
                Gear = Gear.Drive,
                SteeringHeld = true,
                SteeringTarget = 1f
            };

            Step(simulation, 60, 1f / 60f, input);
            AssertNear(1f, simulation.State.SteeringRatio, 0.001f, "full lock after one second");
            AssertNear(config.MaxSteerAngle, simulation.State.SteerAngle, 0.001f);

            Step(simulation, 60, 1f / 60f, new VehicleInput { Gear = Gear.Drive });
            AssertNear(0f, simulation.State.SteeringRatio, 0.001f, "wheel should return");
        }

        private static void AbsoluteSteering()
        {
            VehicleSimulation simulation = CreateDrivingCar();
            var input = new VehicleInput
            {
                Gear = Gear.Drive,
                SteeringHeld = true,
                SteeringTarget = -0.42f,
                SteeringIsAbsolute = true
            };

            simulation.Step(input, 1f / 60f);
            AssertNear(-0.42f, simulation.State.SteeringRatio, 0.0001f, "touch wheel is immediate");
        }

        private static void YawFollowsSteering()
        {
            VehicleSimulation simulation = CreateDrivingCar();
            var input = new VehicleInput
            {
                Gear = Gear.Drive,
                Throttle = true,
                SteeringHeld = true,
                SteeringTarget = 1f,
                SteeringIsAbsolute = true
            };

            Step(simulation, 60, 1f / 60f, input);
            AssertTrue(simulation.State.Heading > 0.1f, "positive steering should turn toward +Z");
            AssertTrue(simulation.State.Position.Y > 0.1f, "car should curve to +Z");
        }

        private static void FrameRateIndependence()
        {
            var input = new VehicleInput { Gear = Gear.Drive, Throttle = true };
            VehicleSimulation sixty = CreateDrivingCar();
            VehicleSimulation thirty = CreateDrivingCar();

            Step(sixty, 60, 1f / 60f, input);
            Step(thirty, 30, 1f / 30f, input);

            AssertNear(sixty.State.Speed, thirty.State.Speed, 0.35f, "speed after one second");
            AssertNear(sixty.State.Position.X, thirty.State.Position.X, 0.6f, "distance after one second");
        }

        private static ParkingTarget Bay() => new ParkingTarget
        {
            X = 10f,
            Z = 5f,
            Angle = 0f,
            Width = 10f,
            Depth = 5f,
            ToleranceDistance = 2f,
            ToleranceAngle = 0.2f
        };

        private static VehicleState ParkedState(Gear gear = Gear.Park) => new VehicleState
        {
            Position = new Vec2(10.2f, 5.1f),
            Heading = 0.05f,
            Speed = 0f,
            Gear = gear
        };

        private static void ParkingHoldCompletes()
        {
            var evaluator = new ParkingEvaluator(new ParkingSettings());
            ParkingStatus status = default;
            for (int i = 0; i < 200 && !status.Completed; i++)
            {
                status = evaluator.Evaluate(ParkedState(Gear.Drive), Bay(), 1f / 60f);
            }

            AssertTrue(status.Completed, "parking should complete");
            AssertNear(1f, status.Progress, 0.0001f);
            AssertNear(1.4f, status.HoldTime, 0.001f);
        }

        private static void ParkingDecaysWhenMoving()
        {
            var evaluator = new ParkingEvaluator(new ParkingSettings());
            for (int i = 0; i < 40; i++)
            {
                evaluator.Evaluate(ParkedState(Gear.Drive), Bay(), 1f / 60f);
            }

            AssertTrue(evaluator.HoldTime > 0.5f, "should have accumulated some hold time");

            VehicleState moving = ParkedState(Gear.Drive);
            moving.Speed = 2f;
            ParkingStatus status = evaluator.Evaluate(moving, Bay(), 1f / 60f);
            AssertTrue(status.Phase == ParkingPhase.Aligning, "phase should be Aligning while creeping");
            AssertTrue(evaluator.HoldTime < 0.7f, "progress should decay while creeping");
        }

        private static void ParkingOutsideResets()
        {
            var evaluator = new ParkingEvaluator(new ParkingSettings());
            for (int i = 0; i < 40; i++)
            {
                evaluator.Evaluate(ParkedState(Gear.Drive), Bay(), 1f / 60f);
            }

            VehicleState outside = ParkedState(Gear.Drive);
            outside.Position = new Vec2(30f, 30f);
            ParkingStatus status = evaluator.Evaluate(outside, Bay(), 1f / 60f);
            AssertTrue(status.Phase == ParkingPhase.Outside, "phase should be Outside");
            AssertNear(0f, status.Progress, 0f);
        }

        private static void ParkingParkGearMultiplier()
        {
            var evaluator = new ParkingEvaluator(new ParkingSettings());
            for (int i = 0; i < 30; i++)
            {
                evaluator.Evaluate(ParkedState(Gear.Park), Bay(), 1f / 60f);
            }

            AssertNear(1f, evaluator.HoldTime, 0.02f, "P should complete in 0.7 s");
        }

        private static void StarThresholds()
        {
            AssertNear(3, ScoreCalculator.CalculateStars(10f, 10f), 0f);
            AssertNear(2, ScoreCalculator.CalculateStars(10.01f, 10f), 0f);
            AssertNear(2, ScoreCalculator.CalculateStars(14.5f, 10f), 0f);
            AssertNear(1, ScoreCalculator.CalculateStars(14.51f, 10f), 0f);
            AssertNear(3, ScoreCalculator.CalculateStars(1f, 0f), 0f);
        }

        private static void ProgressionBehaviour()
        {
            var state = new ProgressionState();
            LevelCompletionResult first = ProgressionRules.ApplyCompletion(state, 1, 18, 8f, 10f);
            AssertNear(3, first.Stars, 0f);
            AssertTrue(first.UnlockedNextLevel, "level 1 should unlock 2");
            AssertNear(2, state.UnlockedLevel, 0f);
            AssertNear(8f, state.GetOrCreate(1).BestTime, 0.001f);

            LevelCompletionResult slower = ProgressionRules.ApplyCompletion(state, 1, 18, 12f, 10f);
            AssertNear(2, slower.Stars, 0f);
            AssertFalse(slower.NewBestTime, "slower run must not replace best time");
            AssertNear(8f, state.GetOrCreate(1).BestTime, 0.001f);
            AssertNear(3, state.GetOrCreate(1).Stars, 0f, "stars keep the best value");

            LevelCompletionResult replay = ProgressionRules.ApplyCompletion(state, 1, 18, 3f, 10f);
            AssertFalse(replay.UnlockedNextLevel, "replaying a cleared level must not unlock again");
            AssertNear(2, state.UnlockedLevel, 0f);

            for (int level = 2; level <= 18; level++)
            {
                ProgressionRules.ApplyCompletion(state, level, 18, 5f, 10f);
            }

            AssertNear(18, state.UnlockedLevel, 0f, "final level never unlocks 19");
        }

        private static void ProgressionSanitises()
        {
            var state = new ProgressionState
            {
                UnlockedLevel = 99,
                Levels = new List<LevelProgress>
                {
                    new LevelProgress { LevelId = 1, Stars = 42, BestTime = -3f },
                    new LevelProgress { LevelId = 55, Stars = 1, BestTime = 4f }
                }
            };

            state.ClampToLevelCount(18);
            AssertNear(18, state.UnlockedLevel, 0f);
            AssertNear(3, state.GetOrCreate(1).Stars, 0f);
            AssertNear(0f, state.GetOrCreate(1).BestTime, 0f);
            AssertFalse(state.TryGet(55, out _), "level outside the game should be removed");
        }

        private static void LevelTimerBehaviour()
        {
            var timer = new LevelTimer();
            timer.Start();
            timer.Tick(1f);
            timer.Stop();
            timer.Tick(1f);
            AssertNear(1f, timer.Elapsed, 0.0001f);
            timer.Resume();
            timer.Tick(0.5f);
            AssertNear(1.5f, timer.Elapsed, 0.0001f);
            timer.Reset();
            AssertNear(0f, timer.Elapsed, 0f);
        }

        private static void StateMachineBehaviour()
        {
            var machine = new GameStateMachine();
            AssertTrue(machine.TryTransition(GameState.MainMenu));
            AssertFalse(machine.TryTransition(GameState.Paused), "cannot pause from the main menu");
            AssertTrue(machine.TryTransition(GameState.Playing));
            AssertTrue(machine.TryTransition(GameState.Parking));
            AssertTrue(machine.TryTransition(GameState.LevelCompleted));
            AssertFalse(machine.TryTransition(GameState.Paused), "cannot pause the results screen");
            AssertTrue(machine.TryTransition(GameState.Playing));
            AssertTrue(machine.TryTransition(GameState.Paused));
            AssertTrue(machine.TryTransition(GameState.Playing));
        }

        private static void TrafficMoverBounces()
        {
            var definition = new MoverDefinition
            {
                X = 0f, Z = 0f,
                DirectionX = 5f, DirectionZ = 0f,
                MinX = -10f, MaxX = 10f,
                MinZ = -10f, MaxZ = 10f
            };
            var mover = new TrafficMoverModel(definition);
            for (int i = 0; i < 300; i++)
            {
                mover.Step(1f / 60f);
                AssertTrue(mover.Position.X >= -10.001f && mover.Position.X <= 10.001f, "must stay in bounds");
            }

            AssertTrue(mover.VelocityX < 0f, "should have bounced off the max bound");
        }

        private static string FindLevelsPath()
        {
            string current = Directory.GetCurrentDirectory();
            for (int i = 0; i < 8 && current != null; i++)
            {
                string candidate = Path.Combine(current, "Assets", "_Project", "Resources", "Levels", "levels.json");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                current = Directory.GetParent(current)?.FullName;
            }

            throw new FileNotFoundException("levels.json not found from " + Directory.GetCurrentDirectory());
        }

        private static LevelCollection LoadLevels()
        {
            var options = new JsonSerializerOptions { IncludeFields = true, PropertyNameCaseInsensitive = true };
            LevelCollection collection = JsonSerializer.Deserialize<LevelCollection>(File.ReadAllText(FindLevelsPath()), options);
            if (collection?.Levels == null)
            {
                throw new InvalidOperationException("Levels failed to deserialize");
            }

            return collection;
        }

        private static void AllLevelsValid()
        {
            LevelCollection collection = LoadLevels();
            AssertNear(18, collection.Levels.Count, 0f, "level count");

            for (int i = 0; i < collection.Levels.Count; i++)
            {
                LevelDefinition level = collection.Levels[i];
                AssertNear(i + 1, level.Id, 0f, "sequential ids");
                List<string> errors = LevelGeometry.Validate(level);
                AssertTrue(errors.Count == 0, $"level {level.Id}: {string.Join("; ", errors)}");
                AssertFalse(LevelGeometry.IsInsideAnyWall(level, level.Start.Position, 0.2f),
                    $"level {level.Id} start is inside a wall");
                AssertFalse(LevelGeometry.IsInsideAnyWall(level, level.Target.Center, 0.2f),
                    $"level {level.Id} target is inside a wall");
            }
        }

        private static void LevelMetadata()
        {
            LevelCollection collection = LoadLevels();
            AssertTrue(collection.Levels[0].Name == "İlk Adım", "first level name");
            AssertTrue(collection.Levels[17].Name == "Büyük Final", "last level name");
            AssertNear(10f, collection.Levels[0].ParTime, 0f);
            AssertNear(30f, collection.Levels[17].ParTime, 0f);
            AssertNear(10.5f, collection.Levels[0].Target.Width, 0.0001f);
            AssertNear(9.2f, collection.Levels[17].Target.Width, 0.0001f);
            AssertTrue(collection.Levels[13].ParkedCars.Count == 20, "dense parking level keeps all cars");
        }

        private static void LevelBoundsEncloseTarget()
        {
            LevelCollection collection = LoadLevels();
            foreach (LevelDefinition level in collection.Levels)
            {
                LevelBounds bounds = LevelGeometry.CalculateBounds(level);
                AssertTrue(level.Target.X >= bounds.MinX && level.Target.X <= bounds.MaxX,
                    $"level {level.Id} target X outside bounds");
                AssertTrue(level.Target.Z >= bounds.MinZ && level.Target.Z <= bounds.MaxZ,
                    $"level {level.Id} target Z outside bounds");
            }
        }
    }
}
