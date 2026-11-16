using System;

namespace MasterParking.Core
{
    /// <summary>
    /// Evaluates whether the car is parked correctly. Owns only the hold timer;
    /// it does not know about UI, audio or Unity.
    /// </summary>
    public sealed class ParkingEvaluator
    {
        private readonly ParkingSettings _settings;
        private float _holdTime;

        public ParkingEvaluator(ParkingSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _settings.Validate();
        }

        public float HoldTime => _holdTime;

        public void Reset()
        {
            _holdTime = 0f;
        }

        public ParkingStatus Evaluate(in VehicleState state, ParkingTarget target, float deltaTime)
        {
            if (target == null)
            {
                return default;
            }

            float dt = MathUtil.Clamp(deltaTime, 0f, 0.1f);
            float distance = Vec2.Distance(state.Position, target.Center);
            float angleError = MathUtil.PerpendicularHeadingError(state.Heading, target.Angle);
            bool inside = distance < target.ToleranceDistance && angleError < target.ToleranceAngle;
            bool slow = Math.Abs(state.Speed) < _settings.MaxParkingSpeed;

            ParkingPhase phase;
            if (inside && slow)
            {
                phase = ParkingPhase.Holding;
                float multiplier = state.Gear == Gear.Park ? _settings.ParkGearMultiplier : 1f;
                _holdTime += dt * multiplier;
            }
            else if (inside)
            {
                phase = ParkingPhase.Aligning;
                _holdTime = Math.Max(0f, _holdTime - dt * _settings.SpeedPenaltyDecay);
            }
            else
            {
                phase = ParkingPhase.Outside;
                _holdTime = 0f;
            }

            bool completed = _holdTime >= _settings.RequiredHoldTime;
            if (completed)
            {
                phase = ParkingPhase.Completed;
                _holdTime = _settings.RequiredHoldTime;
            }

            return new ParkingStatus
            {
                Phase = phase,
                Progress = MathUtil.Clamp01(_holdTime / _settings.RequiredHoldTime),
                HoldTime = _holdTime,
                DistanceError = distance,
                AngleError = angleError,
                Speed = state.Speed,
                InsideZone = inside,
                IsSlowEnough = slow,
                Completed = completed
            };
        }
    }
}
