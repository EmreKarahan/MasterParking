namespace MasterParking.Core
{
    /// <summary>
    /// One physics tick of driver intent. UI and keyboard implementations both
    /// feed this structure, so the simulation never knows about input devices.
    /// </summary>
    public struct VehicleInput
    {
        public bool Throttle;
        public bool Brake;
        public Gear Gear;

        /// <summary>Steering wheel position in the -1..1 range (negative = left).</summary>
        public float SteeringTarget;

        /// <summary>True while the player is actively steering (wheel drag or key held).</summary>
        public bool SteeringHeld;

        /// <summary>
        /// True when <see cref="SteeringTarget"/> is an absolute wheel position from a
        /// touch wheel. False for keyboard input, which ramps toward the target.
        /// </summary>
        public bool SteeringIsAbsolute;

        public static VehicleInput Neutral => new VehicleInput
        {
            Gear = Gear.Park,
            SteeringTarget = 0f,
            SteeringHeld = false,
            SteeringIsAbsolute = false
        };
    }
}
