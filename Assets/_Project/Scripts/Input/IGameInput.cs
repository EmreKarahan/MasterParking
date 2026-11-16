using MasterParking.Core;

namespace MasterParking.Input
{
    /// <summary>
    /// Single input seam for gameplay. Touch UI and the editor keyboard both
    /// implement it, so the session never branches on platform.
    /// </summary>
    public interface IGameInput
    {
        Gear Gear { get; }

        void SetGear(Gear gear);
        void SetThrottle(bool held);
        void SetBrake(bool held);
        void SetSteering(float ratio, bool held);
        void RequestToggleCamera();
        void RequestPause();
        void RequestRestart();

        GameInputFrame Read();
        void Reset();
    }
}
