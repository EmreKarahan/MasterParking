using System;
using System.Collections.Generic;
using MasterParking.Core;
using UnityEngine;

namespace MasterParking.LevelSystem
{
    /// <summary>
    /// Read-only level access for gameplay. Prefers the generated ScriptableObject
    /// catalog and falls back to the migrated JSON so a fresh checkout is playable
    /// before editor tooling has run.
    /// </summary>
    public sealed class LevelRepository
    {
        public const string CatalogResourcePath = "Levels/LevelCatalog";
        public const string JsonResourcePath = "Levels/levels";

        private readonly List<LevelDefinition> _levels;

        private LevelRepository(List<LevelDefinition> levels)
        {
            _levels = levels ?? new List<LevelDefinition>();
        }

        public int Count => _levels.Count;
        public IReadOnlyList<LevelDefinition> Levels => _levels;

        public static LevelRepository LoadDefault()
        {
            var definitions = new List<LevelDefinition>();

            LevelCatalog catalog = Resources.Load<LevelCatalog>(CatalogResourcePath);
            if (catalog != null && catalog.Levels != null)
            {
                for (int i = 0; i < catalog.Levels.Count; i++)
                {
                    LevelData data = catalog.Levels[i];
                    if (data != null && data.Definition != null)
                    {
                        definitions.Add(data.Definition);
                    }
                }
            }

            if (definitions.Count == 0)
            {
                TextAsset json = Resources.Load<TextAsset>(JsonResourcePath);
                if (json != null)
                {
                    LevelCollection collection = JsonUtility.FromJson<LevelCollection>(json.text);
                    if (collection?.Levels != null)
                    {
                        definitions.AddRange(collection.Levels);
                    }
                }
            }

            return new LevelRepository(Sanitize(definitions));
        }

        public static LevelRepository FromDefinitions(IEnumerable<LevelDefinition> definitions) =>
            new LevelRepository(Sanitize(new List<LevelDefinition>(definitions ?? Array.Empty<LevelDefinition>())));

        public LevelDefinition GetByIndex(int index)
        {
            if (index < 0 || index >= _levels.Count)
            {
                return null;
            }

            return _levels[index];
        }

        public LevelDefinition GetById(int id)
        {
            int index = IndexOfId(id);
            return index >= 0 ? _levels[index] : null;
        }

        public int IndexOfId(int id)
        {
            for (int i = 0; i < _levels.Count; i++)
            {
                if (_levels[i].Id == id)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Returns the next level after <paramref name="id"/>, or null at the end.</summary>
        public LevelDefinition GetNext(int id)
        {
            int index = IndexOfId(id);
            return index >= 0 && index + 1 < _levels.Count ? _levels[index + 1] : null;
        }

        public LevelBounds GetBounds(int index) => LevelGeometry.CalculateBounds(GetByIndex(index));

        private static List<LevelDefinition> Sanitize(List<LevelDefinition> source)
        {
            var result = new List<LevelDefinition>(source.Count);
            var seen = new HashSet<int>();

            source.Sort((a, b) =>
            {
                if (a == null)
                {
                    return b == null ? 0 : 1;
                }

                return b == null ? -1 : a.Id.CompareTo(b.Id);
            });

            for (int i = 0; i < source.Count; i++)
            {
                LevelDefinition level = source[i];
                if (level == null || level.Id <= 0 || !seen.Add(level.Id))
                {
                    continue;
                }

                level.Target?.Validate();
                result.Add(level);
            }

            return result;
        }
    }
}
