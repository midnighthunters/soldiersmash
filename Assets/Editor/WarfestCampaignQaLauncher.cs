using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class WarfestCampaignQaLauncher
{
    private const string MarkerPath = "Assets/QA/.run-campaign-qa";
    private const string RestartMarkerPath = "Assets/QA/.restart-campaign-qa";
    private static bool playRequested;
    private static Thread pumpThread;
    private static volatile bool pumpRunning;

    static WarfestCampaignQaLauncher()
    {
        EditorPrefs.SetBool("FreeCPUWhenUnfocused", false);
        Debug.Log("[Warfest QA] campaign launcher loaded");
        EditorApplication.update += Update;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            StartPump();
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            StopPump();
        }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool PostMessage(System.IntPtr hWnd, uint Msg, System.IntPtr wParam, System.IntPtr lParam);

    private static void StartPump()
    {
        if (pumpRunning) return;
        pumpRunning = true;
        System.IntPtr hwnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
        pumpThread = new Thread(() =>
        {
            while (pumpRunning)
            {
                try
                {
                    if (hwnd != System.IntPtr.Zero)
                    {
                        PostMessage(hwnd, 0x0000, System.IntPtr.Zero, System.IntPtr.Zero);
                    }
                    EditorApplication.QueuePlayerLoopUpdate();
                }
                catch {}
                Thread.Sleep(8);
            }
        });
        pumpThread.IsBackground = true;
        pumpThread.Start();
    }

    private static void StopPump()
    {
        pumpRunning = false;
        pumpThread = null;
    }

    private static void Update()
    {
        EditorPrefs.SetBool("FreeCPUWhenUnfocused", false);
        string absoluteMarkerPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
            MarkerPath.Replace('/', Path.DirectorySeparatorChar));
        string absoluteRestartMarkerPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
            RestartMarkerPath.Replace('/', Path.DirectorySeparatorChar));

        if (Application.isPlaying)
        {
            if (!pumpRunning) StartPump();
            if (File.Exists(absoluteRestartMarkerPath))
            {
                File.Delete(absoluteRestartMarkerPath);
                playRequested = false;
                StopPump();
                EditorApplication.isPlaying = false;
                return;
            }
            if (EditorApplication.isPaused) EditorApplication.isPaused = false;
            if (WarfestMcpCampaignRunner.Completed)
            {
                if (File.Exists(absoluteMarkerPath)) File.Delete(absoluteMarkerPath);
                playRequested = false;
                StopPump();
                EditorApplication.isPlaying = false;
            }
            return;
        }

        StopPump();
        if (!playRequested && File.Exists(absoluteMarkerPath))
        {
            Debug.Log("[Warfest QA] campaign marker found; entering play mode");
            playRequested = true;
            EditorApplication.isPlaying = true;
        }
    }
}
