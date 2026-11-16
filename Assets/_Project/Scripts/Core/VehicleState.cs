namespace MasterParking.Core
{
    /// <summary>Snapshot of the simulated vehicle pose, consumed by views and rules.</summary>
    public struct VehicleState
    {
        public Vec2 Position;

        /// <summary>Heading in radians. 0 = world +X, positive turns toward +Z.</summary>
        public float Heading;

        /// <summary>Signed speed in metres/second.</summary>
        public float Speed;

        /// <summary>Smoothed steering wheel position (-1..1).</summary>
        public float SteeringRatio;

        public float SteerAngle;
        public Gear Gear;
    }
}
