using System;
using System.Collections.Generic;
using System.IO;
using MasterParking.Core;
using MasterParking.LevelSystem;
using UnityEditor;
using UnityEngine;

namespace MasterParking.EditorTools
{
    /// <summary>
    /// Converts the canonical migrated JSON into the ScriptableObject assets that
    /// gameplay reads. Re-running the importer updates assets in place, so level
    /// tuning never requires a gameplay code change.
    /// </summary>
    public static class LevelImportTool
    {
        public const string JsonAssetPath = "Assets/_Project/Resources/Levels/levels.json";
        public const string CatalogAssetPath = "Assets/_Project/Resources/Levels/LevelCatalog.asset";
        public const string GeneratedFolder = "Assets/_Project/Levels/Generated";
        public const int ExpectedLevelCount = 18;

        [MenuItem("Master Parking/Levels/Import JSON To ScriptableObjects", false, 20)]
        public static void ImportAll()
        {
            LevelCollection collection = LoadAndValidate();
            if (collection == null)
            {
                return;
            }

            EnsureFolder(GeneratedFolder);

            var levelAssets = new List<LevelData>(collection.Levels.Count);
            for (int i = 0; i < collection.Levels.Count; i++)
            {
                LevelDefinition definition = collection.Levels[i];
                string assetPath = GeneratedFolder + "/Level_" + definition.Id.ToString("00") + ".asset";

                LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
                if (data == null)
                {
                    data = ScriptableObject.CreateInstance<LevelData>();
                    AssetDatabase.CreateAsset(data, assetPath);
                }

                data.name = Path.GetFileNameWithoutExtension(assetPath);
                data.Definition = definition;
                EditorUtility.SetDirty(data);
                levelAssets.Add(data);
            }

            LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogAssetPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<LevelCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
            }

            if (catalog.Levels == null)
            {
                catalog.Levels = new List<LevelData>();
            }

            catalog.Levels.Clear();
            catalog.Levels.AddRange(levelAssets);
            EditorUtility.SetDirty(catalog);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MasterParking] Imported " + levelAssets.Count + " levels into " + CatalogAssetPath + ".");
        }

        [MenuItem("Master Parking/Levels/Validate JSON", false, 21)]
        public static void ValidateOnly()
        {
            LevelCollection collection = LoadAndValidate();
            if (collection != null)
            {
                Debug.Log("[MasterParking] Level JSON is valid: " + collection.Levels.Count + " levels.");
            }
        }

        /// <summary>Loads and validates the JSON, logging every problem it finds.</summary>
        public static LevelCollection LoadAndValidate()
        {
            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(JsonAssetPath);
            if (textAsset == null)
            {
                Debug.LogError("[MasterParking] Level JSON not found at " + JsonAssetPath);
                return null;
            }

            LevelCollection collection;
            try
            {
                collection = JsonUtility.FromJson<LevelCollection>(textAsset.text);
            }
            catch (Exception exception)
            {
                Debug.LogError("[MasterParking] Level JSON could not be parsed: " + exception.Message);
                return null;
            }

            if (collection?.Levels == null || collection.Levels.Count == 0)
            {
                Debug.LogError("[MasterParking] Level JSON contains no levels.");
                return null;
            }

            bool valid = true;
            if (collection.Levels.Count != ExpectedLevelCount)
            {
                Debug.LogWarning("[MasterParking] Expected " + ExpectedLevelCount + " levels but found " +
                                 collection.Levels.Count + ".");
            }

            var seenIds = new HashSet<int>();
            var seenNames = new HashSet<string>();
            for (int i = 0; i < collection.Levels.Count; i++)
            {
                LevelDefinition level = collection.Levels[i];
                string prefix = "[MasterParking] Level " + (i + 1) + ": ";

                if (level == null)
                {
                    Debug.LogError(prefix + "definition is null.");
                    valid = false;
                    continue;
                }

                if (level.Id <= 0)
                {
                    Debug.LogError(prefix + "id must be positive.");
                    valid = false;
                }
                else if (!seenIds.Add(level.Id))
                {
                    Debug.LogError(prefix + "duplicate id " + level.Id + ".");
                    valid = false;
                }

                if (string.IsNullOrWhiteSpace(level.Name))
                {
                    Debug.LogError(prefix + "name is empty.");
                    valid = false;
                }
                else if (!seenNames.Add(level.Name))
                {
                    Debug.LogWarning(prefix + "duplicate name '" + level.Name + "'.");
                }

                if (level.ParTime <= 0f)
                {
                    Debug.LogError(prefix + "par time must be positive.");
                    valid = false;
                }

                if (level.Target == null)
                {
                    Debug.LogError(prefix + "parking target is missing.");
                    valid = false;
                    continue;
                }

                level.Target.Validate();

                List<string> geometryErrors = LevelGeometry.Validate(level);
                for (int errorIndex = 0; errorIndex < geometryErrors.Count; errorIndex++)
                {
                    Debug.LogError(prefix + geometryErrors[errorIndex]);
                    valid = false;
                }
            }

            return valid ? collection : null;
        }

        private static void EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            string leaf = Path.GetFileName(folder);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf))
            {
                return;
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
