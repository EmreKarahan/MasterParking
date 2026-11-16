using System;

namespace MasterParking.Core
{
    /// <summary>Rules for a successful park. Derived from the original 1.4 s hold.</summary>
    [Serializable]
    public sealed class ParkingSettings
    {
        public float RequiredHoldTime = 1.4f;

        /// <summary>The car must be nearly stationary while the timer runs.</summary>
        public float MaxParkingSpeed = 0.72f;

        /// <summary>Holding P doubles progress, as in the original game.</summary>
        public float ParkGearMultiplier = 2f;

        /// <summary>Progress lost per second when the car creeps too fast inside the bay.</summary>
        public float SpeedPenaltyDecay = 2f;

        public void Validate()
        {
            RequiredHoldTime = Math.Max(0.1f, RequiredHoldTime);
            MaxParkingSpeed = Math.Max(0.01f, MaxParkingSpeed);
            ParkGearMultiplier = Math.Max(1f, ParkGearMultiplier);
            SpeedPenaltyDecay = Math.Max(0f, SpeedPenaltyDecay);
        }
    }
}
