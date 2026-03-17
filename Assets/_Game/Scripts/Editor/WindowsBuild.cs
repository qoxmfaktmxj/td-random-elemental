using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TdRandomElemental.Editor
{
    public static class WindowsBuild
    {
        private const string DefaultBuildDirectory = "Builds/Windows";
        private const string DefaultExecutableName = "TDRandomElemental.exe";

        [MenuItem("TD Random Elemental/Build/Windows x64")]
        public static void BuildWindows64Menu()
        {
            string outputPath = GetOutputPathFromCommandLine() ?? Path.Combine(DefaultBuildDirectory, DefaultExecutableName);
            BuildWindows64(outputPath);
        }

        public static void BuildWindows64FromCommandLine()
        {
            string outputPath = GetOutputPathFromCommandLine() ?? Path.Combine(DefaultBuildDirectory, DefaultExecutableName);
            BuildWindows64(outputPath);
        }

        private static void BuildWindows64(string outputPath)
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("No enabled scenes were found in Build Settings.");
            }

            string fullOutputPath = Path.GetFullPath(outputPath);
            string outputDirectory = Path.GetDirectoryName(fullOutputPath);
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new InvalidOperationException($"Invalid output path: '{outputPath}'.");
            }

            Directory.CreateDirectory(outputDirectory);

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = fullOutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log($"WindowsBuild: Building Windows player to '{fullOutputPath}'.");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Windows build failed. Result={summary.result}, errors={summary.totalErrors}, warnings={summary.totalWarnings}.");
            }

            Debug.Log($"WindowsBuild: Build succeeded. Size={summary.totalSize} bytes, duration={summary.totalTime}.");
        }

        private static string GetOutputPathFromCommandLine()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length - 1; i++)
            {
                if (string.Equals(arguments[i], "-customBuildPath", StringComparison.OrdinalIgnoreCase))
                {
                    return arguments[i + 1];
                }
            }

            return null;
        }
    }
}
