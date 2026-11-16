using System;

namespace MasterParking.Core
{
    /// <summary>
    /// Deterministic kinematic car model. It deliberately does not use Rigidbody
    /// dynamics: the original is a casual arcade parking game, and stable, readable
    /// movement matters more than vehicle simulation. Unity handles authoring
    /// colliders and overlap queries around this model.
    /// </summary>
    public sealed class VehicleSimulation
    {
        public VehicleConfig Config { get; }
        public VehicleState State { get; private set; }

        public VehicleSimulation(VehicleConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Config.Validate();
        }

        public void Reset(Vec2 position, float heading, Gear gear = Gear.Drive)
        {
            State = new VehicleState
            {
                Position = position,
                Heading = MathUtil.WrapAngle(heading),
                Speed = 0f,
                SteeringRatio = 0f,
                SteerAngle = 0f,
                Gear = gear
            };
        }

        /// <summary>Stops the car immediately after a crash without moving it.</summary>
        public void Halt()
        {
            VehicleState state = State;
            state.Speed = 0f;
            state.SteeringRatio = 0f;
            state.SteerAngle = 0f;
            state.Gear = Gear.Park;
            State = state;
        }

        /// <summary>Advances the car by one fixed physics step.</summary>
        public void Step(VehicleInput input, float deltaTime)
        {
            // A long hitch (app resume, GC spike) must not teleport the car through walls.
            float dt = MathUtil.Clamp(deltaTime, 0f, 0.05f);
            if (dt <= 0f)
            {
                return;
            }

            VehicleState state = State;
            UpdateSteering(ref state, input, dt);
            UpdateSpeed(ref state, input, dt);

            if (Math.Abs(state.Speed) > 0.001f)
            {
                float angularVelocity = state.Speed / Config.WheelBase * (float)Math.Tan(state.SteerAngle);
                state.Heading = MathUtil.WrapAngle(state.Heading + angularVelocity * dt);
                state.Position += Vec2.FromAngle(state.Heading) * (state.Speed * dt);
            }

            state.Gear = input.Gear;
            State = state;
        }

        private void UpdateSteering(ref VehicleState state, VehicleInput input, float dt)
        {
            float target = MathUtil.Clamp(input.SteeringTarget, -1f, 1f);

            if (input.SteeringHeld)
            {
                state.SteeringRatio = input.SteeringIsAbsolute
                    ? target
                    : MathUtil.MoveTowards(state.SteeringRatio, target, Config.SteeringRate * dt);
            }
            else
            {
                state.SteeringRatio = MathUtil.MoveTowards(state.SteeringRatio, 0f, Config.SteeringReturnRate * dt);
            }

            state.SteerAngle = state.SteeringRatio * Config.MaxSteerAngle;
        }

        private void UpdateSpeed(ref VehicleState state, VehicleInput input, float dt)
        {
            if (input.Gear == Gear.Park)
            {
                state.Speed = MathUtil.MoveTowards(state.Speed, 0f, Config.ParkBrakeDeceleration * dt);
                return;
            }

            if (input.Brake)
            {
                state.Speed = MathUtil.MoveTowards(state.Speed, 0f, Config.BrakeDeceleration * dt);
                return;
            }

            if (input.Throttle)
            {
                if (input.Gear == Gear.Drive)
                {
                    state.Speed = Math.Min(Config.MaxForwardSpeed, state.Speed + Config.Acceleration * dt);
                }
                else
                {
                    float reverseAcceleration = Config.Acceleration * Config.ReverseAccelerationFactor;
                    state.Speed = Math.Max(-Config.MaxReverseSpeed, state.Speed - reverseAcceleration * dt);
                }

                return;
            }

            state.Speed = MathUtil.MoveTowards(state.Speed, 0f, Config.RollingResistance * dt);
        }
    }
}
