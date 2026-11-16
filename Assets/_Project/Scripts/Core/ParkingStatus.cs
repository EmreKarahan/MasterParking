namespace MasterParking.Core
{
    public enum ParkingPhase
    {
        /// <summary>The car is outside the bay.</summary>
        Outside = 0,

        /// <summary>Inside the bay but moving (or settling) — progress is decaying.</summary>
        Aligning = 1,

        /// <summary>Inside, stationary and accumulating hold time.</summary>
        Holding = 2,

        /// <summary>Hold time satisfied.</summary>
        Completed = 3
    }

    /// <summary>Per-tick parking feedback for HUD and audio, with no Unity dependency.</summary>
    public struct ParkingStatus
    {
        public ParkingPhase Phase;
        public float Progress;
        public float HoldTime;
        public float DistanceError;
        public float AngleError;
        public float Speed;
        public bool InsideZone;
        public bool IsSlowEnough;
        public bool Completed;

        public bool IsVisible => Phase != ParkingPhase.Outside;
    }
}
