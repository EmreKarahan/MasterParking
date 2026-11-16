using System.Collections.Generic;
using System.IO;
using MasterParking.App;
using MasterParking.Utilities;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MasterParking.EditorTools
{
    /// <summary>
    /// One-click project preparation for a fresh checkout: custom layers, mobile
    /// player settings, generated level assets, the bootstrap scene and build
    /// settings. Every step is idempotent and safe to run repeatedly.
    /// </summary>
    public static class ProjectSetupTool
    {
        public const string BootstrapScenePath = "Assets/_Project/Scenes/Bootstrap.unity";
        private const string ScenesFolder = "Assets/_Project/Scenes";
        private const string AndroidApplicationId = "com.masterparking.game";
        private const string IosApplicationId = "com.masterparking.game";

        [MenuItem("Master Parking/Setup/Prepare Project", false, 0)]
        public static void PrepareProject()
        {
            EnsureLayer(LayerNames.Vehicle, 8);
            EnsureLayer(LayerNames.Obstacle, 9);
            ConfigurePlayerSettings();
            EnsureAlwaysIncludedShaders();
            LevelImportTool.ImportAll();
            EnsureBootstrapScene();
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("[MasterParking] Project ready. Open " + BootstrapScenePath + " and press Play.");
        }

        [MenuItem("Master Parking/Setup/Create Or Repair Bootstrap Scene", false, 1)]
        public static void CreateBootstrapSceneMenu()
        {
            EnsureBootstrapScene();
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
        }

        public static void EnsureBootstrapScene()
        {
            EnsureFolder(ScenesFolder);

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            SceneAsset existing = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScenePath);
            if (existing != null)
            {
                Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
                if (Object.FindAnyObjectByType<AppRoot>() != null)
                {
                    return;
                }

                var root = new GameObject("AppRoot");
                root.AddComponent<AppRoot>();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, BootstrapScenePath);
                Debug.Log("[MasterParking] Added AppRoot to the existing bootstrap scene.");
                return;
            }

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var appRoot = new GameObject("AppRoot");
            appRoot.AddComponent<AppRoot>();
            EditorSceneManager.MarkSceneDirty(newScene);
            EditorSceneManager.SaveScene(newScene, BootstrapScenePath);
            Debug.Log("[MasterParking] Created " + BootstrapScenePath + ".");
        }

        public static void ConfigureBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(scene => scene.path == BootstrapScenePath);
            scenes.Insert(0, new EditorBuildSettingsScene(BootstrapScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        public static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "Master Parking";
            PlayerSettings.productName = "Master Parking";
            PlayerSettings.bundleVersion = "1.0.0";

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AndroidApplicationId);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, IosApplicationId);

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

            PlayerSettings.iOS.targetOSVersionString = "13.0";
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);

            // One 60 Hz quality target for mobile; the runtime also enforces it.
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            AssetDatabase.SaveAssets();
            Debug.Log("[MasterParking] Player settings configured for landscape Android/iOS.");
        }

        /// <summary>
        /// Runtime-generated materials use Shader.Find, so the shaders must be
        /// present even when no asset references them in a player build.
        /// </summary>
        private static void EnsureAlwaysIncludedShaders()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
            if (assets == null || assets.Length == 0)
            {
                Debug.LogWarning("[MasterParking] GraphicsSettings.asset not found; shader stripping is unchanged.");
                return;
            }

            var graphics = new SerializedObject(assets[0]);
            SerializedProperty shaders = graphics.FindProperty("m_AlwaysIncludedShaders");
            if (shaders == null || !shaders.isArray)
            {
                Debug.LogWarning("[MasterParking] Always-included shader list not found.");
                return;
            }

            string[] required = { "Standard", "Sprites/Default", "Unlit/Color", "Legacy Shaders/Diffuse" };
            for (int nameIndex = 0; nameIndex < required.Length; nameIndex++)
            {
                Shader shader = Shader.Find(required[nameIndex]);
                if (shader == null)
                {
                    continue;
                }

                bool present = false;
                for (int i = 0; i < shaders.arraySize; i++)
                {
                    if (shaders.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                    {
                        present = true;
                        break;
                    }
                }

                if (present)
                {
                    continue;
                }

                int index = shaders.arraySize;
                shaders.InsertArrayElementAtIndex(index);
                shaders.GetArrayElementAtIndex(index).objectReferenceValue = shader;
            }

            graphics.ApplyModifiedProperties();
        }

        /// <summary>Adds a custom layer to TagManager without relying on project YAML.</summary>
        private static void EnsureLayer(string layerName, int preferredIndex)
        {
            if (LayerMask.NameToLayer(layerName) >= 0)
            {
                return;
            }

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                Debug.LogWarning("[MasterParking] TagManager.asset not found; using the default layer.");
                return;
            }

            var tagManager = new SerializedObject(assets[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            if (layers == null || !layers.isArray)
            {
                Debug.LogWarning("[MasterParking] TagManager layers were not found; using the default layer.");
                return;
            }

            int target = preferredIndex;
            if (target < 8 || target >= layers.arraySize ||
                !string.IsNullOrEmpty(layers.GetArrayElementAtIndex(target).stringValue))
            {
                target = -1;
                for (int i = 8; i < layers.arraySize; i++)
                {
                    if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue))
                    {
                        target = i;
                        break;
                    }
                }
            }

            if (target < 0)
            {
                Debug.LogWarning("[MasterParking] No free user layer for '" + layerName + "'.");
                return;
            }

            layers.GetArrayElementAtIndex(target).stringValue = layerName;
            tagManager.ApplyModifiedProperties();
            Debug.Log("[MasterParking] Added layer '" + layerName + "' at index " + target + ".");
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
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
