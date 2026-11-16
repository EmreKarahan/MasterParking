using System;
using MasterParking.Core;

namespace MasterParking.SaveSystem
{
    /// <summary>
    /// Root persisted document. Version exists so a future build can migrate
    /// instead of silently discarding player progress.
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;
        public ProgressionState Progression = new ProgressionState();
        public SettingsState Settings = new SettingsState();
        public int LastPlayedLevelId = 1;
    }
}
