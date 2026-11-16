using System;
using MasterParking.Core;
using UnityEngine;

namespace MasterParking.SaveSystem
{
    /// <summary>
    /// Owns the in-memory save document and writes through ISaveStorage.
    /// Gameplay calls RecordCompletion; UI calls SetSettings/ResetProgress.
    /// </summary>
    public sealed class SaveManager
    {
        private readonly ISaveStorage _storage;
        private readonly int _levelCount;
        private SaveData _data;

        public SaveManager(ISaveStorage storage, int levelCount)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _levelCount = Math.Max(1, levelCount);
            Load();
        }

        public ProgressionState Progression => _data.Progression;
        public SettingsState Settings => _data.Settings;
        public int LastPlayedLevelId => _data.LastPlayedLevelId;
        public string StorageDescription => _storage.Description;

        public void Load()
        {
            string json = _storage.Load();
            _data = null;

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    _data = JsonUtility.FromJson<SaveData>(json);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[Save] Invalid save payload, starting fresh: " + exception.Message);
                }
            }

            if (_data == null)
            {
                _data = new SaveData();
            }

            if (_data.Version <= 0)
            {
                _data.Version = SaveData.CurrentVersion;
            }

            // JsonUtility can leave missing nested objects null when reading an
            // older payload; repair before any gameplay reads them.
            _data.Progression ??= new ProgressionState();
            _data.Settings ??= new SettingsState();
            _data.Settings.Sanitize();
            _data.Progression.ClampToLevelCount(_levelCount);

            if (_data.LastPlayedLevelId < 1 || _data.LastPlayedLevelId > _levelCount)
            {
                _data.LastPlayedLevelId = 1;
            }
        }

        public void Save()
        {
            if (_data == null)
            {
                return;
            }

            _data.Version = SaveData.CurrentVersion;
            _data.Settings.Sanitize();
            _data.Progression.ClampToLevelCount(_levelCount);

            try
            {
                string json = JsonUtility.ToJson(_data, true);
                _storage.Save(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Save] Could not serialize save data: " + exception.Message);
            }
        }

        public void SetLastPlayedLevel(int levelId)
        {
            _data.LastPlayedLevelId = (int)MathUtil.Clamp(levelId, 1, _levelCount);
            Save();
        }

        public void ApplySettings(SettingsState settings)
        {
            if (settings == null)
            {
                return;
            }

            _data.Settings = settings.Clone();
            _data.Settings.Sanitize();
            Save();
        }

        public LevelCompletionResult RecordCompletion(LevelDefinition level, float elapsedSeconds)
        {
            if (level == null)
            {
                return default;
            }

            LevelCompletionResult result = ProgressionRules.ApplyCompletion(
                _data.Progression,
                level.Id,
                _levelCount,
                elapsedSeconds,
                level.ParTime);

            _data.LastPlayedLevelId = (int)MathUtil.Clamp(level.Id, 1, _levelCount);
            Save();
            return result;
        }

        public void ResetProgress()
        {
            _data.Progression = new ProgressionState();
            _data.LastPlayedLevelId = 1;
            Save();
        }

        public void ResetEverything()
        {
            _data = new SaveData();
            _storage.Delete();
            Save();
        }
    }
}
