using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MasterParking.EditorTools
{
    /// <summary>
    /// Thin build entry points. They intentionally use the standard Unity build
    /// pipeline so CI can call the same static methods.
    /// </summary>
    public static class MobileBuildMenu
    {
        private const string BuildsFolder = "Builds";

        [MenuItem("Master Parking/Build/Android APK", false, 40)]
        public static void BuildAndroid()
        {
            ProjectSetupTool.PrepareProject();
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            EditorUserBuildSettings.buildAppBundle = false;

            string output = Path.Combine(BuildsFolder, "Android", "MasterParking.apk");
            Directory.CreateDirectory(Path.GetDirectoryName(output) ?? BuildsFolder);

            RunBuild("Android", output, BuildTarget.Android, BuildTargetGroup.Android);
        }

        [MenuItem("Master Parking/Build/iOS Xcode Project", false, 41)]
        public static void BuildIos()
        {
            ProjectSetupTool.PrepareProject();

            string output = Path.Combine(BuildsFolder, "iOS");
            Directory.CreateDirectory(output);

            RunBuild("iOS", output, BuildTarget.iOS, BuildTargetGroup.iOS);
        }

        private static void RunBuild(string label, string output, BuildTarget target, BuildTargetGroup group)
        {
            if (!BuildPipeline.IsBuildTargetSupported(group, target))
            {
                Debug.LogError("[MasterParking] " + label +
                               " build support is not installed in this Unity editor.");
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ProjectSetupTool.BootstrapScenePath },
                locationPathName = output,
                target = target,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[MasterParking] " + label + " build succeeded (" +
                          report.summary.totalSize + " bytes): " + output);
            }
            else
            {
                Debug.LogError("[MasterParking] " + label + " build failed: " +
                               report.summary.result + " (" + report.summary.totalErrors + " errors)");
            }
        }
    }
}
