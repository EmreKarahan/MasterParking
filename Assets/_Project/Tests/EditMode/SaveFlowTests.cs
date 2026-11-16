using MasterParking.Core;
using MasterParking.SaveSystem;
using NUnit.Framework;

namespace MasterParking.Tests
{
    /// <summary>
    /// Exercises persistence and progression through the replaceable storage
    /// seam, including a simulated app restart and a corrupt payload.
    /// </summary>
    public sealed class SaveFlowTests
    {
        private sealed class MemorySaveStorage : ISaveStorage
        {
            public string Payload;

            public string Load() => Payload;

            public bool Save(string payload)
            {
                Payload = payload;
                return true;
            }

            public void Delete() => Payload = null;

            public string Description => "memory";
        }

        [Test]
        public void Completion_PersistsStarsBestTimeAndUnlock()
        {
            var storage = new MemorySaveStorage();
            var manager = new SaveManager(storage, 18);
            var level = new LevelDefinition { Id = 1, ParTime = 10f };

            LevelCompletionResult result = manager.RecordCompletion(level, 8f);

            Assert.AreEqual(3, result.Stars);
            Assert.IsTrue(result.UnlockedNextLevel);
            Assert.AreEqual(2, manager.Progression.UnlockedLevel);
            Assert.IsNotNull(storage.Payload);

            // Simulate an app restart: the same storage must hydrate progress.
            var reloaded = new SaveManager(storage, 18);
            Assert.AreEqual(2, reloaded.Progression.UnlockedLevel);
            Assert.IsTrue(reloaded.Progression.TryGet(1, out LevelProgress progress));
            Assert.AreEqual(3, progress.Stars);
            Assert.AreEqual(8f, progress.BestTime, 0.001f);
            Assert.IsTrue(progress.Completed);
        }

        [Test]
        public void Completion_KeepsBestTimeAndStars()
        {
            var storage = new MemorySaveStorage();
            var manager = new SaveManager(storage, 18);
            var level = new LevelDefinition { Id = 3, ParTime = 10f };

            manager.RecordCompletion(level, 9f);
            LevelCompletionResult slower = manager.RecordCompletion(level, 14f);

            Assert.AreEqual(2, slower.Stars);
            Assert.IsFalse(slower.NewBestTime);
            Assert.IsTrue(manager.Progression.TryGet(3, out LevelProgress progress));
            Assert.AreEqual(9f, progress.BestTime, 0.001f);
            Assert.AreEqual(3, progress.Stars);
        }

        [Test]
        public void CorruptPayload_StartsFromCleanProgress()
        {
            var storage = new MemorySaveStorage { Payload = "{ this is not json" };
            var manager = new SaveManager(storage, 18);

            Assert.AreEqual(1, manager.Progression.UnlockedLevel);
            Assert.AreEqual(1, manager.LastPlayedLevelId);
            Assert.AreEqual(0, manager.Progression.Levels.Count);
        }

        [Test]
        public void ResetProgress_KeepsSettingsButClearsLevels()
        {
            var storage = new MemorySaveStorage();
            var manager = new SaveManager(storage, 18);
            var settings = new SettingsState { MusicVolume = 0.1f, SfxVolume = 0.2f, Haptics = false };
            manager.ApplySettings(settings);

            manager.RecordCompletion(new LevelDefinition { Id = 1, ParTime = 10f }, 5f);
            manager.ResetProgress();

            Assert.AreEqual(1, manager.Progression.UnlockedLevel);
            Assert.AreEqual(0, manager.Progression.Levels.Count);
            Assert.AreEqual(0.1f, manager.Settings.MusicVolume, 0.001f);
            Assert.IsFalse(manager.Settings.Haptics);

            var reloaded = new SaveManager(storage, 18);
            Assert.AreEqual(1, reloaded.Progression.UnlockedLevel);
            Assert.AreEqual(0.2f, reloaded.Settings.SfxVolume, 0.001f);
        }

        [Test]
        public void Settings_AreClampedBeforeSaving()
        {
            var storage = new MemorySaveStorage();
            var manager = new SaveManager(storage, 18);
            manager.ApplySettings(new SettingsState
            {
                MusicVolume = 4f,
                SfxVolume = -2f,
                EngineVolume = 0.5f
            });

            Assert.AreEqual(1f, manager.Settings.MusicVolume, 0.001f);
            Assert.AreEqual(0f, manager.Settings.SfxVolume, 0.001f);
            Assert.AreEqual(0.5f, manager.Settings.EngineVolume, 0.001f);
        }
    }
}
