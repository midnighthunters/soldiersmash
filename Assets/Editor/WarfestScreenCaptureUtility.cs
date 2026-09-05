using System.IO;
using UnityEditor;
using UnityEngine;

public static class WarfestScreenCaptureUtility
{
    private const int Width = 390;
    private const int Height = 844;

    [MenuItem("Warfest/Capture All UI Previews (Including Settings)")]
    public static void CaptureAllPreviews()
    {
        string dir = Path.Combine(Application.dataPath, "Screenshots");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        CaptureSplashPreview(Path.Combine(dir, "preview_splash.png"));
        CaptureLoadingPreview(Path.Combine(dir, "preview_loading.png"));
        CaptureVictoryPreview(Path.Combine(dir, "preview_victory.png"));
        CaptureSettingsFlyoutPreview(Path.Combine(dir, "preview_settings_flyout.png"));

        AssetDatabase.Refresh();
        Debug.Log("[Warfest] Captured all UI previews to Assets/Screenshots/");
    }

    [MenuItem("Warfest/Capture Settings Flyout Preview")]
    public static void CaptureSettingsFlyout()
    {
        string dir = Path.Combine(Application.dataPath, "Screenshots");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        CaptureSettingsFlyoutPreview(Path.Combine(dir, "preview_settings_flyout.png"));
        AssetDatabase.Refresh();
        Debug.Log("[Warfest] Captured settings flyout preview to Assets/Screenshots/preview_settings_flyout.png");
    }

    public static void CaptureSplashPreview(string outputPath)
    {
        GameObject splashObj = new GameObject("Test Splash");
        WarfestSplashScreen splash = WarfestSplashScreen.Show(splashObj.transform);
        Canvas canvas = splash.GetComponent<Canvas>();

        var fillField = typeof(WarfestSplashScreen).GetField("progressFill", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var fillImg = fillField?.GetValue(splash) as UnityEngine.UI.Image;
        if (fillImg != null) fillImg.rectTransform.anchorMax = new Vector2(0.65f, 1f);

        var statusField = typeof(WarfestSplashScreen).GetField("statusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var statusTxt = statusField?.GetValue(splash) as UnityEngine.UI.Text;
        if (statusTxt != null) statusTxt.text = "LOADING... 65%";

        RenderCanvasToFile(canvas, outputPath);
        Object.DestroyImmediate(splashObj);
    }

    public static void CaptureLoadingPreview(string outputPath)
    {
        GameObject loadObj = new GameObject("Test Loading", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster), typeof(CanvasGroup));
        Canvas canvas = loadObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9998;

        UnityEngine.UI.CanvasScaler scaler = loadObj.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referenceResolution = new Vector2(390f, 844f);

        WarfestLoadingScreen loading = loadObj.AddComponent<WarfestLoadingScreen>();
        System.Reflection.MethodInfo build = typeof(WarfestLoadingScreen).GetMethod("BuildUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        build?.Invoke(loading, new object[] { loadObj.transform as RectTransform, 0 });

        var fillField = typeof(WarfestLoadingScreen).GetField("progressFill", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var textField = typeof(WarfestLoadingScreen).GetField("progressText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var fillImg = fillField?.GetValue(loading) as UnityEngine.UI.Image;
        var progTxt = textField?.GetValue(loading) as UnityEngine.UI.Text;
        if (fillImg != null) fillImg.rectTransform.anchorMax = new Vector2(0.72f, 1f);
        if (progTxt != null) progTxt.text = "DEPLOYING... 72%";

        RenderCanvasToFile(canvas, outputPath);
        Object.DestroyImmediate(loadObj);
    }

    public static void CaptureVictoryPreview(string outputPath)
    {
        GameObject vicObj = new GameObject("Test Victory", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster), typeof(CanvasGroup), typeof(AudioSource));
        Canvas canvas = vicObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9990;

        UnityEngine.UI.CanvasScaler scaler = vicObj.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referenceResolution = new Vector2(390f, 844f);

        WarfestVictoryScreen victory = vicObj.AddComponent<WarfestVictoryScreen>();
        System.Reflection.MethodInfo build = typeof(WarfestVictoryScreen).GetMethod("BuildUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        build?.Invoke(victory, new object[] { vicObj.transform as RectTransform });

        // Simulate settled animation state so victory tank and logo are centered
        var vicRectField = typeof(WarfestVictoryScreen).GetField("victoryRect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var logoRectField = typeof(WarfestVictoryScreen).GetField("logoRect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        RectTransform vicRect = vicRectField?.GetValue(victory) as RectTransform;
        RectTransform logoRect = logoRectField?.GetValue(victory) as RectTransform;

        if (vicRect != null) { vicRect.anchoredPosition = new Vector2(0f, 70f); vicRect.localScale = Vector3.one; }
        if (logoRect != null) { logoRect.anchoredPosition = new Vector2(0f, -120f); logoRect.localScale = Vector3.one; }

        RenderCanvasToFile(canvas, outputPath);
        Object.DestroyImmediate(vicObj);
    }

    public static void CaptureSettingsFlyoutPreview(string outputPath)
    {
        GameObject rootObj = new GameObject("Test Game HUD", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        Canvas canvas = rootObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        UnityEngine.UI.CanvasScaler scaler = rootObj.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referenceResolution = new Vector2(390f, 844f);

        GameObject safeObj = new GameObject("Safe Area", typeof(RectTransform));
        safeObj.transform.SetParent(rootObj.transform, false);
        RectTransform safeRect = safeObj.GetComponent<RectTransform>();
        safeRect.anchorMin = Vector2.zero;
        safeRect.anchorMax = Vector2.one;
        safeRect.offsetMin = Vector2.zero;
        safeRect.offsetMax = Vector2.zero;

        Texture2D panelTexture = Resources.Load<Texture2D>("panel");
        Sprite settingsPanelSprite = null;
        if (panelTexture != null)
        {
            float sx = panelTexture.width / 1536f;
            float sy = panelTexture.height / 1024f;
            settingsPanelSprite = Sprite.Create(panelTexture, new Rect(930f * sx, 0f * sy, 410f * sx, 404f * sy), new Vector2(0.5f, 0.5f), 100f);
        }

        GameObject gearObj = new GameObject("Settings Menu", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        gearObj.transform.SetParent(safeRect, false);
        RectTransform gearRect = gearObj.GetComponent<RectTransform>();
        Vector2 gearCenter = new Vector2(0.865f, 0.937f);
        Vector2 gearSize = new Vector2(0.18f, 0.092f);
        gearRect.anchorMin = gearCenter - gearSize * 0.5f;
        gearRect.anchorMax = gearCenter + gearSize * 0.5f;
        gearRect.offsetMin = Vector2.zero;
        gearRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image gearImg = gearObj.GetComponent<UnityEngine.UI.Image>();
        gearImg.sprite = settingsPanelSprite;
        gearImg.preserveAspect = true;

        GameObject flyoutObj = new GameObject("Settings Flyout", typeof(RectTransform));
        flyoutObj.transform.SetParent(safeRect, false);
        RectTransform flyoutRect = flyoutObj.GetComponent<RectTransform>();
        flyoutRect.anchorMin = Vector2.zero;
        flyoutRect.anchorMax = Vector2.one;
        flyoutRect.offsetMin = Vector2.zero;
        flyoutRect.offsetMax = Vector2.zero;

        Sprite leaveIcon = WarfestAudio.GetLeaveIconSprite();
        Sprite soundIcon = WarfestAudio.GetSoundIconSprite();
        Sprite musicIcon = WarfestAudio.GetMusicIconSprite();
        Sprite disabledPlate = WarfestAudio.GetSettingsDisabledSprite();
        Sprite enabledPlate = WarfestAudio.GetSettingsEnabledSprite();

        Vector2 btnSize = new Vector2(0.145f, 0.070f);
        const float flyoutX = 0.865f;

        System.Action<string, Sprite, Sprite, float> makeButton = (name, bgSprite, iconSprite, yPos) => {
            GameObject btn = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            btn.transform.SetParent(flyoutRect, false);
            RectTransform rt = btn.GetComponent<RectTransform>();
            Vector2 center = new Vector2(flyoutX, yPos);
            rt.anchorMin = center - btnSize * 0.5f;
            rt.anchorMax = center + btnSize * 0.5f;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            UnityEngine.UI.Image bg = btn.GetComponent<UnityEngine.UI.Image>();
            bg.sprite = bgSprite;
            bg.preserveAspect = true;

            GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            icon.transform.SetParent(btn.transform, false);
            RectTransform iconRt = icon.GetComponent<RectTransform>();
            Vector2 iconSize = new Vector2(0.56f, 0.56f);
            iconRt.anchorMin = new Vector2(0.5f, 0.5f) - iconSize * 0.5f;
            iconRt.anchorMax = new Vector2(0.5f, 0.5f) + iconSize * 0.5f;
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;
            UnityEngine.UI.Image ic = icon.GetComponent<UnityEngine.UI.Image>();
            ic.sprite = iconSprite;
            ic.preserveAspect = true;
        };

        makeButton("Leave Level", disabledPlate, leaveIcon, 0.852f);
        makeButton("Sound Toggle", enabledPlate, soundIcon, 0.772f);
        makeButton("Music Toggle", enabledPlate, musicIcon, 0.692f);

        RenderCanvasToFile(canvas, outputPath);
        Object.DestroyImmediate(rootObj);
    }

    private static void RenderCanvasToFile(Canvas canvas, string outputPath)
    {
        // Create temporary capture camera
        GameObject camObj = new GameObject("Capture Camera", typeof(Camera));
        Camera cam = camObj.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.15f, 0.2f, 1f);
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = 100f;

        RenderTexture rt = new RenderTexture(Width * 2, Height * 2, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 1f;

        Canvas.ForceUpdateCanvases();
        cam.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        RenderTexture.active = null;
        cam.targetTexture = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(camObj);

        byte[] pngData = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);

        File.WriteAllBytes(outputPath, pngData);
    }
}
