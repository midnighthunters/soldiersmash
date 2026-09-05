using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class WarfestCampaignQaLauncher
{
    private const string MarkerPath = "Assets/QA/.run-campaign-qa";
    private const string RestartMarkerPath = "Assets/QA/.restart-campaign-qa";
    private static bool playRequested;

    static WarfestCampaignQaLauncher()
    {
        Debug.Log("[Warfest QA] campaign launcher loaded");
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        string absoluteMarkerPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
            MarkerPath.Replace('/', Path.DirectorySeparatorChar));
        string absoluteRestartMarkerPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
            RestartMarkerPath.Replace('/', Path.DirectorySeparatorChar));

        if (Application.isPlaying)
        {
            if (File.Exists(absoluteRestartMarkerPath))
            {
                File.Delete(absoluteRestartMarkerPath);
                playRequested = false;
                EditorApplication.isPlaying = false;
                return;
            }
            if (EditorApplication.isPaused) EditorApplication.isPaused = false;
            if (WarfestMcpCampaignRunner.Completed)
            {
                if (File.Exists(absoluteMarkerPath)) File.Delete(absoluteMarkerPath);
                playRequested = false;
                EditorApplication.isPlaying = false;
            }
            return;
        }

        if (!playRequested && File.Exists(absoluteMarkerPath))
        {
            Debug.Log("[Warfest QA] campaign marker found; entering play mode");
            playRequested = true;
            EditorApplication.isPlaying = true;
        }
    }
}
