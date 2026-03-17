#if UNITY_EDITOR
using System.IO;
using TdRandomElemental.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace TdRandomElemental.Editor
{
    public static class WindowsPlayerBuild
    {
        private const string BuildDirectoryName = "Builds";
        private const string WindowsDirectoryName = "Windows";
        private const string ExecutableName = "TdRandomElemental.exe";

        [MenuItem("TD Random Elemental/Build/Windows Player")]
        public static void BuildWindowsPlayerMenu()
        {
            BuildWindowsPlayer();
        }

        public static void BuildWindowsPlayerBatch()
        {
            BuildWindowsPlayer();
        }

        private static void BuildWindowsPlayer()
        {
            BootstrapProjectSetup.RunProjectSetup();

            string[] scenes =
            {
                StartupConfig.BootstrapScenePath,
                StartupConfig.MvpArenaScenePath
            };

            for (int i = 0; i < scenes.Length; i++)
            {
                if (!File.Exists(scenes[i]))
                {
                    throw new BuildFailedException($"Missing build scene: {scenes[i]}");
                }
            }

            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath)?.FullName
                ?? Directory.GetCurrentDirectory();
            string outputDirectory = Path.Combine(projectRoot, BuildDirectoryName, WindowsDirectoryName);
            Directory.CreateDirectory(outputDirectory);

            string outputPath = Path.Combine(outputDirectory, ExecutableName);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.StrictMode
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    $"Windows build failed with result {report.summary.result}. See Unity log for details.");
            }

            UnityEngine.Debug.Log($"Windows player build completed: {outputPath}");
        }
    }
}
#endif
