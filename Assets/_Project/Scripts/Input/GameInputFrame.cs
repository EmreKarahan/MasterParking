using MasterParking.Core;

namespace MasterParking.Input
{
    /// <summary>One frame of driver intent plus UI-level actions.</summary>
    public struct GameInputFrame
    {
        public VehicleInput Vehicle;
        public bool ToggleCamera;
        public bool Pause;
        public bool Restart;

        public static GameInputFrame Neutral => new GameInputFrame
        {
            Vehicle = VehicleInput.Neutral
        };
    }
}
