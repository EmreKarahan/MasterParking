using MasterParking.SaveSystem;
using UnityEngine;

namespace MasterParking.Utilities
{
    /// <summary>Optional haptics, respecting the player setting and platform.</summary>
    public static class Haptics
    {
        public static void Vibrate(SettingsState settings)
        {
            if (settings == null || !settings.Haptics)
            {
                return;
            }

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
