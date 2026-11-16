using System;
using MasterParking.Core;

namespace MasterParking.SaveSystem
{
    /// <summary>Player preferences. Kept flat so it serializes with JsonUtility.</summary>
    [Serializable]
    public sealed class SettingsState
    {
        public float MusicVolume = 0.32f;
        public float SfxVolume = 0.85f;
        public float EngineVolume = 0.55f;
        public bool Haptics = true;
        public bool ShowTutorialHints = true;

        public void Sanitize()
        {
            MusicVolume = MathUtil.Clamp01(MusicVolume);
            SfxVolume = MathUtil.Clamp01(SfxVolume);
            EngineVolume = MathUtil.Clamp01(EngineVolume);
        }

        public SettingsState Clone() => new SettingsState
        {
            MusicVolume = MusicVolume,
            SfxVolume = SfxVolume,
            EngineVolume = EngineVolume,
            Haptics = Haptics,
            ShowTutorialHints = ShowTutorialHints
        };
    }
}
