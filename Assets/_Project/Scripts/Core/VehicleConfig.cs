using System;

namespace MasterParking.Core
{
    /// <summary>
    /// Tuning values for the casual parking car. All speeds are metres/second,
    /// accelerations metres/second² and angles radians.
    /// The original game simulated per frame; these values are the frame-rate
    /// independent equivalents, with slightly smoother acceleration for touch play.
    /// </summary>
    [Serializable]
    public sealed class VehicleConfig
    {
        public float MaxForwardSpeed = 9.3f;
        public float MaxReverseSpeed = 5.7f;
        public float Acceleration = 7.0f;
        public float ReverseAccelerationFactor = 0.9f;
        public float BrakeDeceleration = 18.0f;
        public float ParkBrakeDeceleration = 25.0f;
        public float RollingResistance = 5.0f;

        public float MaxSteerAngle = 0.62f;
        public float WheelBase = 5.2f;

        /// <summary>Steering ratio units per second while a direction is held.</summary>
        public float SteeringRate = 3.1f;

        /// <summary>Steering ratio units per second while the wheel is released.</summary>
        public float SteeringReturnRate = 9.0f;

        public void Validate()
        {
            MaxForwardSpeed = Math.Max(0.1f, MaxForwardSpeed);
            MaxReverseSpeed = Math.Max(0.1f, MaxReverseSpeed);
            Acceleration = Math.Max(0.1f, Acceleration);
            BrakeDeceleration = Math.Max(0.1f, BrakeDeceleration);
            ParkBrakeDeceleration = Math.Max(0.1f, ParkBrakeDeceleration);
            RollingResistance = Math.Max(0f, RollingResistance);
            MaxSteerAngle = Math.Min(1.2f, Math.Max(0.05f, MaxSteerAngle));
            WheelBase = Math.Max(0.5f, WheelBase);
            SteeringRate = Math.Max(0.1f, SteeringRate);
            SteeringReturnRate = Math.Max(0.1f, SteeringReturnRate);
            ReverseAccelerationFactor = Math.Max(0f, ReverseAccelerationFactor);
        }
    }
}
