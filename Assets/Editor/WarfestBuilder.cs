#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Warfest.Editor
{
    public static class WarfestBuilder
    {
        public static void BuildIOS()
        {
            string[] scenes = new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Game.unity"
            };

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Build/ios",
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[WarfestBuilder] iOS Build succeeded: {summary.totalSize} bytes, duration: {summary.totalTime}");
            }
            else
            {
                Debug.LogError($"[WarfestBuilder] iOS Build failed with result: {summary.result}, errors: {summary.totalErrors}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }
    }
}
#endif
