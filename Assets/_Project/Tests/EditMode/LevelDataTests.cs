using System.Collections.Generic;
using MasterParking.Core;
using MasterParking.LevelSystem;
using NUnit.Framework;
using UnityEngine;

namespace MasterParking.Tests
{
    /// <summary>
    /// Verifies the shipped level payload and the optional generated catalog.
    /// These tests run fresh from the canonical JSON even before the importer
    /// has created ScriptableObject assets.
    /// </summary>
    public sealed class LevelDataTests
    {
        [Test]
        public void CanonicalJson_ContainsAllEighteenValidLevels()
        {
            TextAsset json = Resources.Load<TextAsset>("Levels/levels");
            Assert.IsNotNull(json, "levels.json must be under Resources/Levels");

            LevelCollection collection = JsonUtility.FromJson<LevelCollection>(json.text);
            Assert.IsNotNull(collection);
            Assert.IsNotNull(collection.Levels);
            Assert.AreEqual(18, collection.Levels.Count);

            var ids = new HashSet<int>();
            for (int i = 0; i < collection.Levels.Count; i++)
            {
                LevelDefinition level = collection.Levels[i];
                Assert.IsNotNull(level, "level " + (i + 1) + " is null");
                Assert.AreEqual(i + 1, level.Id, "levels must be sequential");
                Assert.IsTrue(ids.Add(level.Id), "duplicate level id " + level.Id);
                Assert.IsFalse(string.IsNullOrWhiteSpace(level.Name), "level " + level.Id + " has no name");
                Assert.Greater(level.ParTime, 0f, "level " + level.Id + " has no par time");
                Assert.IsNotNull(level.Target, "level " + level.Id + " has no target");
                Assert.IsNotNull(level.Decor, "level " + level.Id + " has no decor root");

                List<string> errors = LevelGeometry.Validate(level);
                Assert.AreEqual(0, errors.Count, "level " + level.Id + ": " + string.Join("; ", errors));
            }
        }

        [Test]
        public void CanonicalJson_KeepsKnownMetadata()
        {
            TextAsset json = Resources.Load<TextAsset>("Levels/levels");
            LevelCollection collection = JsonUtility.FromJson<LevelCollection>(json.text);

            Assert.AreEqual("İlk Adım", collection.Levels[0].Name);
            Assert.AreEqual("Büyük Final", collection.Levels[17].Name);
            Assert.AreEqual(10f, collection.Levels[0].ParTime, 0.001f);
            Assert.AreEqual(30f, collection.Levels[17].ParTime, 0.001f);
            Assert.AreEqual(20, collection.Levels[13].ParkedCars.Count);
        }

        [Test]
        public void GeneratedCatalog_WhenPresent_MatchesCanonicalJson()
        {
            TextAsset json = Resources.Load<TextAsset>("Levels/levels");
            LevelCollection collection = JsonUtility.FromJson<LevelCollection>(json.text);
            LevelCatalog catalog = Resources.Load<LevelCatalog>("Levels/LevelCatalog");

            // The importer is optional: a fresh checkout uses the JSON fallback,
            // and a generated catalog must stay in sync with it.
            if (catalog == null)
            {
                return;
            }

            Assert.AreEqual(collection.Levels.Count, catalog.Levels.Count);
            for (int i = 0; i < collection.Levels.Count; i++)
            {
                Assert.IsNotNull(catalog.Levels[i], "catalog entry " + i + " is null");
                Assert.IsNotNull(catalog.Levels[i].Definition, "catalog entry " + i + " has no definition");
                Assert.AreEqual(collection.Levels[i].Id, catalog.Levels[i].Definition.Id);
                Assert.AreEqual(collection.Levels[i].Name, catalog.Levels[i].Definition.Name);
            }
        }

        [Test]
        public void Repository_LoadsFallbackAndOrdersLevels()
        {
            LevelRepository repository = LevelRepository.LoadDefault();

            Assert.AreEqual(18, repository.Count);
            Assert.AreEqual(1, repository.GetByIndex(0).Id);
            Assert.AreEqual(18, repository.GetByIndex(17).Id);
            Assert.IsNull(repository.GetByIndex(18));
            Assert.IsNull(repository.GetByIndex(-1));
            Assert.IsNull(repository.GetById(999));
            Assert.AreEqual(2, repository.GetNext(1).Id);
            Assert.IsNull(repository.GetNext(18));
        }

        [Test]
        public void Repository_BoundsContainEveryParkingTarget()
        {
            LevelRepository repository = LevelRepository.LoadDefault();
            for (int i = 0; i < repository.Count; i++)
            {
                LevelDefinition level = repository.GetByIndex(i);
                LevelBounds bounds = repository.GetBounds(i);
                Assert.IsTrue(level.Target.X >= bounds.MinX && level.Target.X <= bounds.MaxX,
                    "level " + level.Id + " target X outside bounds");
                Assert.IsTrue(level.Target.Z >= bounds.MinZ && level.Target.Z <= bounds.MaxZ,
                    "level " + level.Id + " target Z outside bounds");
            }
        }
    }
}
