using MasterParking.CameraSystem;
using MasterParking.Core;

namespace MasterParking.Gameplay
{
    /// <summary>
    /// View contract for the running session. Keeping the HUD behind an interface
    /// lets the session stay free of uGUI construction details.
    /// </summary>
    public interface IGameplayHud
    {
        void SetLevelInfo(string levelName, int levelId, int levelCount);
        void SetTimer(float seconds);
        void SetGear(Gear gear);
        void SetSpeed(float metresPerSecond);
        void SetParking(in ParkingStatus status);
        void SetCameraMode(CameraMode mode);
        void SetControlsEnabled(bool enabled);
        void ShowCrash(string reason);
        void ShowWin(LevelCompletionResult result, bool hasNextLevel);
        void ShowPause(bool visible);
    }
}
