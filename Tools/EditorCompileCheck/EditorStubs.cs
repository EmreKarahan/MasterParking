// Compile-time stubs for the small slice of UnityEditor, NUnit and scene APIs
// used by the editor tooling and EditMode tests. Never part of a build.
using System;
using UnityEngine;

namespace UnityEngine.SceneManagement
{
    public struct Scene
    {
        public string path;
        public string name;
        public bool IsValid() => true;
    }
}

namespace UnityEditor
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MenuItem : Attribute
    {
        public int priority;

        public MenuItem(string itemName) { }
        public MenuItem(string itemName, bool isValidateFunction) { }
        public MenuItem(string itemName, bool isValidateFunction, int priority) { this.priority = priority; }
    }

    public static class AssetDatabase
    {
        public static T LoadAssetAtPath<T>(string path) where T : UnityEngine.Object => null;
        public static void CreateAsset(UnityEngine.Object asset, string path) { }
        public static bool IsValidFolder(string path) => true;
        public static string CreateFolder(string parent, string newFolder) => string.Empty;
        public static void SaveAssets() { }
        public static void Refresh() { }
        public static bool DeleteAsset(string path) => true;
        public static UnityEngine.Object[] LoadAllAssetsAtPath(string path) => new UnityEngine.Object[0];
    }

    public static class EditorUtility
    {
        public static void SetDirty(UnityEngine.Object target) { }
    }

    public sealed class SerializedObject
    {
        public SerializedObject(UnityEngine.Object target) { }
        public SerializedProperty FindProperty(string propertyPath) => null;
        public void ApplyModifiedProperties() { }
    }

    public sealed class SerializedProperty
    {
        public bool isArray => true;
        public int arraySize => 0;
        public string stringValue { get; set; }
        public UnityEngine.Object objectReferenceValue { get; set; }
        public SerializedProperty GetArrayElementAtIndex(int index) => new SerializedProperty();
        public void InsertArrayElementAtIndex(int index) { }
    }

    public sealed class EditorBuildSettingsScene
    {
        public string path;
        public bool enabled;

        public EditorBuildSettingsScene(string path, bool enabled)
        {
            this.path = path;
            this.enabled = enabled;
        }
    }

    public static class EditorBuildSettings
    {
        public static EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[0];
    }

    public sealed class SceneAsset : UnityEngine.Object { }

    public sealed class AndroidSettings
    {
        public AndroidSdkVersions minSdkVersion;
        public AndroidSdkVersions targetSdkVersion;
        public AndroidArchitecture targetArchitectures;
    }

    public sealed class IosSettings
    {
        public string targetOSVersionString;
        public iOSTargetDevice targetDevice;
    }

    public static class PlayerSettings
    {
        public static string companyName;
        public static string productName;
        public static string bundleVersion;
        public static UIOrientation defaultInterfaceOrientation;
        public static bool allowedAutorotateToPortrait;
        public static bool allowedAutorotateToPortraitUpsideDown;
        public static bool allowedAutorotateToLandscapeLeft;
        public static bool allowedAutorotateToLandscapeRight;

        public static AndroidSettings Android { get; } = new AndroidSettings();
        public static IosSettings iOS { get; } = new IosSettings();

        public static void SetApplicationIdentifier(Build.NamedBuildTarget buildTarget, string identifier) { }
        public static void SetScriptingBackend(Build.NamedBuildTarget buildTarget, ScriptingImplementation implementation) { }
    }

    public enum UIOrientation
    {
        Portrait,
        PortraitUpsideDown,
        LandscapeRight,
        LandscapeLeft,
        AutoRotation
    }

    public enum AndroidSdkVersions
    {
        AndroidApiLevel24 = 24,
        AndroidApiLevelAuto = 0
    }

    [Flags]
    public enum AndroidArchitecture
    {
        None = 0,
        ARMv7 = 1,
        ARM64 = 2,
        X86 = 4,
        X86_64 = 8
    }

    public enum ScriptingImplementation
    {
        Mono2x,
        IL2CPP
    }

    public enum iOSTargetDevice
    {
        iPhoneOnly,
        iPadOnly,
        iPhoneAndiPad
    }

    public enum BuildTarget
    {
        Android,
        iOS,
        StandaloneWindows64
    }

    public enum BuildTargetGroup
    {
        Android,
        iOS,
        Standalone
    }

    public enum BuildOptions
    {
        None,
        Development
    }

    public struct BuildPlayerOptions
    {
        public string[] scenes;
        public string locationPathName;
        public BuildTarget target;
        public BuildOptions options;
    }

    public static class BuildPipeline
    {
        public static Build.Reporting.BuildReport BuildPlayer(BuildPlayerOptions options) =>
            new Build.Reporting.BuildReport();
        public static bool IsBuildTargetSupported(BuildTargetGroup group, BuildTarget target) => false;
    }

    public static class EditorUserBuildSettings
    {
        public static bool buildAppBundle;
    }
}

namespace UnityEditor.Build
{
    public struct NamedBuildTarget
    {
        public static NamedBuildTarget Android => default;
        public static NamedBuildTarget iOS => default;
    }
}

namespace UnityEditor.Build.Reporting
{
    public enum BuildResult
    {
        Unknown,
        Succeeded,
        Failed,
        Cancelled
    }

    public sealed class BuildReport
    {
        public BuildSummary summary;
    }

    public struct BuildSummary
    {
        public BuildResult result;
        public ulong totalSize;
        public int totalErrors;
    }
}

namespace UnityEditor.SceneManagement
{
    public enum NewSceneSetup
    {
        EmptyScene,
        DefaultGameObjects
    }

    public enum NewSceneMode
    {
        Single,
        Additive
    }

    public enum OpenSceneMode
    {
        Single,
        Additive,
        AdditiveWithoutLoading
    }

    public static class EditorSceneManager
    {
        public static UnityEngine.SceneManagement.Scene NewScene(NewSceneSetup setup, NewSceneMode mode) =>
            new UnityEngine.SceneManagement.Scene();

        public static UnityEngine.SceneManagement.Scene OpenScene(string path, OpenSceneMode mode) =>
            new UnityEngine.SceneManagement.Scene();

        public static bool SaveScene(UnityEngine.SceneManagement.Scene scene, string path) => true;

        public static bool SaveCurrentModifiedScenesIfUserWantsTo() => true;

        public static void MarkSceneDirty(UnityEngine.SceneManagement.Scene scene) { }
    }
}

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SetUpAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TearDownAttribute : Attribute { }

    public static class Assert
    {
        private static void FailInternal(string message) =>
            throw new InvalidOperationException(message ?? "Assertion failed");

        public static void IsTrue(bool condition, string message = null)
        {
            if (!condition) FailInternal(message);
        }

        public static void IsFalse(bool condition, string message = null)
        {
            if (condition) FailInternal(message);
        }

        public static void IsNull(object value, string message = null)
        {
            if (value != null) FailInternal(message);
        }

        public static void IsNotNull(object value, string message = null)
        {
            if (value == null) FailInternal(message);
        }

        public static void AreEqual<T>(T expected, T actual, string message = null)
        {
            if (!Equals(expected, actual)) FailInternal(message ?? "Expected " + expected + " but was " + actual);
        }

        public static void AreEqual(float expected, float actual, float delta, string message = null)
        {
            if (Math.Abs(expected - actual) > delta)
            {
                FailInternal(message ?? "Expected " + expected + " +/- " + delta + " but was " + actual);
            }
        }

        public static void AreNotEqual<T>(T expected, T actual, string message = null)
        {
            if (Equals(expected, actual)) FailInternal(message ?? "Expected a value other than " + expected);
        }

        public static void Greater(IComparable greater, IComparable less, string message = null)
        {
            if (greater == null || less == null || greater.CompareTo(less) <= 0) FailInternal(message);
        }

        public static void GreaterOrEqual(IComparable greater, IComparable less, string message = null)
        {
            if (greater == null || less == null || greater.CompareTo(less) < 0) FailInternal(message);
        }

        public static void Fail(string message) => throw new InvalidOperationException(message);
    }
}
