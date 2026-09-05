#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Runs in the editor as well as at runtime so the fully generated menu is previewable in edit
// mode. Everything it builds is regenerated on demand and flagged DontSave, so the scene file
// stays lean (just the camera and this controller) while the Game view matches play mode.
[ExecuteAlways]
public sealed class WarfestMainMenu : MonoBehaviour
{
    private const string GeneratedCanvasName = "Main Menu Canvas";

    private static readonly Color Navy = new Color(0.08f, 0.16f, 0.24f, 1f);
    private static readonly Color DeepGreen = new Color(0.13f, 0.23f, 0.08f, 1f);
    private static readonly Color Cream = new Color(1f, 0.98f, 0.88f, 1f);

    [SerializeField] private Font headingFont;
    [SerializeField] private Font bodyFont;

    private Font fallbackFont;
    private Texture2D panelSheet;
    private RectTransform safeAreaRoot;
    private CanvasScaler canvasScaler;
    private Vector2 appliedReferenceResolution;
    private Rect appliedSafeArea;
    private Canvas menuCanvas;
    private GameObject createdEventSystem;
    private Text lifeCountText;
    private Text lifeStatusText;
    private Button deployButton;
    private int displayedLives = -1;
    private int displayedLifeSeconds = -1;
    private float nextLifeHudRefreshTime;
    private AudioSource menuMusicSource;
    private GameObject settingsFlyout;
    private Image soundButtonBackground;
    private Image musicButtonBackground;
    private bool settingsOpen;

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            Application.targetFrameRate = 60;
            Rebuild();
            StartMenuAudio();
            if (!WarfestSession.HasShownSplash)
            {
                WarfestSession.HasShownSplash = true;
                WarfestSplashScreen.Show(transform);
            }
            return;
        }

#if UNITY_EDITOR
        // Building a full UI tree while the scene is still loading / the inspector is refreshing
        // can trip Unity's "don't create objects during OnEnable" guard, so defer one tick.
        EditorApplication.delayCall -= EditorDeferredBuild;
        EditorApplication.delayCall += EditorDeferredBuild;
        // Drop the editor preview the instant we leave edit mode so its objects can never carry
        // over into play mode (and from there into another scene such as the Game scene).
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.delayCall -= EditorDeferredBuild;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        StopMenuAudio();
        // Tear the editor preview down cleanly; at runtime Unity handles scene teardown for us.
        if (!Application.isPlaying)
        {
            Teardown();
        }
    }

    private void OnDestroy()
    {
        StopMenuAudio();
    }

#if UNITY_EDITOR
    private void EditorDeferredBuild()
    {
        if (this == null || Application.isPlaying || !isActiveAndEnabled)
        {
            return;
        }

        Rebuild();
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            Teardown();
        }
    }
#endif

    private void Update()
    {
        ApplyCanvasScale();
        ApplySafeArea();
        if (Application.isPlaying && Time.unscaledTime >= nextLifeHudRefreshTime)
        {
            nextLifeHudRefreshTime = Time.unscaledTime + 1f;
            RefreshLifeHud();
        }
    }

    private void Rebuild()
    {
        Teardown();

        fallbackFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        panelSheet = Resources.Load<Texture2D>("pnl");

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.63f, 0.78f, 0.49f, 1f);
        }

        // The editor preview only needs to render; input (and therefore an EventSystem) is a
        // play-mode concern, so we skip it in edit mode to keep the scene uncluttered.
        if (Application.isPlaying)
        {
            EnsureEventSystem();
        }

        BuildMenu();

        if (!Application.isPlaying && menuCanvas != null)
        {
            SetHideFlagsRecursively(menuCanvas.gameObject, HideFlags.DontSave);
        }
    }

    private void Teardown()
    {
        // Destroy any menu canvas we generated earlier, including one orphaned by a domain
        // reload (the managed reference is cleared but the DontSave object can survive).
        Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas != null && canvas.gameObject.name == GeneratedCanvasName)
            {
                SafeDestroy(canvas.gameObject);
            }
        }

        menuCanvas = null;
        safeAreaRoot = null;
        canvasScaler = null;
        lifeCountText = null;
        lifeStatusText = null;
        deployButton = null;
        displayedLives = -1;
        displayedLifeSeconds = -1;
        appliedReferenceResolution = Vector2.zero;
        appliedSafeArea = new Rect();

        if (createdEventSystem != null)
        {
            SafeDestroy(createdEventSystem);
            createdEventSystem = null;
        }

        if (settingsFlyout != null)
        {
            SafeDestroy(settingsFlyout);
            settingsFlyout = null;
        }
        soundButtonBackground = null;
        musicButtonBackground = null;
        settingsOpen = false;
    }

    private static void SafeDestroy(Object target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }

    private static void SetHideFlagsRecursively(GameObject root, HideFlags flags)
    {
        root.hideFlags = flags;
        foreach (Transform child in root.transform)
        {
            SetHideFlagsRecursively(child.gameObject, flags);
        }
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        createdEventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        // Own it under this controller so it is torn down with the scene rather than lingering.
        createdEventSystem.transform.SetParent(transform, false);
    }

    private void BuildMenu()
    {
        Canvas canvas = CreateCanvas(GeneratedCanvasName);
        menuCanvas = canvas;
        RectTransform root = canvas.transform as RectTransform;
        CreateBackground(root);
        safeAreaRoot = CreateSafeAreaRoot(root);

        WarfestLevelCatalog.LevelDefinition level = WarfestLevelCatalog.Get(WarfestSession.SelectedLevel);
        int balls = WarfestSession.GetBallAllowance(WarfestSession.SelectedLevel);

        BuildTopStatus(level, balls);
        BuildWorldDecorations();
        BuildMissionCard(level, balls);
        BuildBottomNavigation();
        BuildSettingsFlyout();
    }

    private void CreateBackground(RectTransform root)
    {
        Texture2D texture = Resources.Load<Texture2D>("background");
        Image background = CreateImage(root, "Bangalore Background", Color.white, new Vector2(0.5f, 0.5f), Vector2.one);
        if (texture != null)
        {
            background.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            background.preserveAspect = true;
            AspectRatioFitter fitter = background.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = (float)texture.width / texture.height;
        }

        CreateImage(root, "Background Readability", new Color(0.08f, 0.14f, 0.02f, 0.08f), new Vector2(0.5f, 0.5f), Vector2.one);
    }

    private void BuildTopStatus(WarfestLevelCatalog.LevelDefinition level, int balls)
    {
        const float topBarY = 0.925f;
        const float barHeight = 0.070f;

        // 1. Commander Avatar Card: Clean rect without bottom-right artifact, aligned to topBarY
        CreateSheetImage(safeAreaRoot, "Commander", new Rect(18f, 14f, 178f, 163f),
            new Vector2(0.095f, topBarY), new Vector2(0.106f, barHeight));

        // 2. Coin Bar: Pill container at topBarY
        Image coinBar = CreateSheetImage(safeAreaRoot, "Coin Bar", new Rect(508f, 48f, 270f, 108f),
            new Vector2(0.380f, topBarY), new Vector2(0.265f, barHeight));

        // Coin Icon: Parented to coinBar on the left, sized to balance with heart
        CreateSheetImage(coinBar.transform, "Coin Icon", new Rect(384f, 38f, 132f, 132f),
            new Vector2(0.06f, 0.50f), new Vector2(0.38f, 1.12f));

        // Coin Value: Crisp dark navy font without outline, centered in cream area between coin and + button
        CreateText(coinBar.transform, "Coin Value", (level.number * 125).ToString(), 22, Navy,
            TextAnchor.MiddleCenter, new Vector2(0.465f, 0.50f), new Vector2(0.40f, 0.65f), bodyFont);

        // 3. Lives Bar: Pill container at topBarY
        int lives = WarfestSession.Lives;
        Image livesBar = CreateSheetImage(safeAreaRoot, "Lives Bar", new Rect(775f, 43f, 276f, 119f),
            new Vector2(0.685f, topBarY), new Vector2(0.265f, barHeight));
        lifeCountText = CreateOutlinedText(livesBar.transform, "Life Count", lives.ToString(), 24, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.208f, 0.510f), new Vector2(0.28f, 0.65f), headingFont, Navy, 1.5f);
        lifeStatusText = CreateText(livesBar.transform, "Life Status", WarfestSession.LivesFull ? "FULL" : WarfestSession.LifeTimerText, 17, DeepGreen,
            TextAnchor.MiddleCenter, new Vector2(0.672f, 0.505f), new Vector2(0.56f, 0.60f), bodyFont);

        // 4. Settings Gear Icon at topBarY
        Button settingsBtn = CreateSheetButton(safeAreaRoot, "Settings", new Rect(1144f, 46f, 100f, 108f),
            new Vector2(0.920f, topBarY), new Vector2(0.108f, barHeight));
        settingsBtn.onClick.AddListener(ToggleSettingsFlyout);
    }

    private void BuildWorldDecorations()
    {
        Image tower = CreateSheetImage(safeAreaRoot, "Watchtower", new Rect(14f, 178f, 350f, 365f),
            new Vector2(0.15f, 0.52f), new Vector2(0.36f, 0.31f));
        tower.color = new Color(1f, 1f, 1f, 0.88f);

        Image tent = CreateSheetImage(safeAreaRoot, "Command Tent", new Rect(376f, 203f, 423f, 342f),
            new Vector2(0.81f, 0.51f), new Vector2(0.44f, 0.27f));
        tent.color = new Color(1f, 1f, 1f, 0.88f);
    }

    private void BuildMissionCard(WarfestLevelCatalog.LevelDefinition level, int balls)
    {
        RectTransform card = CreateContainer(safeAreaRoot, "Mission Card", new Vector2(0.5f, 0.29f), new Vector2(0.74f, 0.30f));
        CreateSheetImage(card, "Cream Frame", new Rect(8f, 540f, 441f, 305f), new Vector2(0.5f, 0.5f), Vector2.one);

        CreateSheetImage(card, "Progress Track", new Rect(458f, 763f, 365f, 82f),
            new Vector2(0.47f, 0.91f), new Vector2(0.72f, 0.16f));
        Image progress = CreateSheetImage(card, "Progress Fill", new Rect(216f, 866f, 550f, 60f),
            new Vector2(0.47f, 0.91f), new Vector2(0.66f, 0.06f));
        progress.type = Image.Type.Filled;
        progress.fillMethod = Image.FillMethod.Horizontal;
        progress.fillOrigin = 0;
        progress.fillAmount = Mathf.Clamp01((float)level.number / WarfestSession.LevelCount);
        CreateOutlinedText(card, "Campaign Progress", level.number + "/" + WarfestSession.LevelCount, 21, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.49f, 0.91f), new Vector2(0.25f, 0.09f), bodyFont, Navy, 1.4f);
        CreateSheetImage(card, "Reward Chest", new Rect(1055f, 748f, 145f, 132f),
            new Vector2(0.86f, 0.905f), new Vector2(0.19f, 0.24f));

        Button deploy = CreateSheetButton(card, "Deploy Mission", new Rect(443f, 560f, 410f, 174f),
            new Vector2(0.5f, 0.535f), new Vector2(0.84f, 0.5f));
        deployButton = deploy;

        if (WarfestSession.CampaignComplete)
        {
            // Every authored level is cleared. The card stays visible for continuity but the
            // button is inert and simply teases the next batch of missions.
            deploy.interactable = false;
            CreateOutlinedText(deploy.transform, "Level Number", "COMING SOON", 34, Cream,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.95f, 0.5f), headingFont, DeepGreen, 2.3f);
        }
        else
        {
            deploy.interactable = WarfestSession.Lives > 0;
            deploy.onClick.AddListener(() =>
            {
                StopMenuAudio();
                WarfestLoadingScreen.ShowAndLoad(WarfestSession.SelectedLevel);
            });

            // Difficulty rides on its own recessed chip across the top of the green plate; the
            // level number then reads large and centered underneath it, matching the reference.
            string difficulty = level.difficulty <= 2 ? "EASY" : level.difficulty <= 4 ? "HARD" : "ELITE";
            Image difficultyChip = CreateSheetImage(deploy.transform, "Difficulty Chip", new Rect(868f, 712f, 185f, 58f),
                new Vector2(0.5f, 0.865f), new Vector2(0.62f, 0.24f));
            difficultyChip.color = new Color(0.56f, 0.61f, 0.5f, 1f);
            CreateOutlinedText(deploy.transform, "Difficulty", difficulty, 18, new Color(0.87f, 0.93f, 0.79f, 1f),
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.865f), new Vector2(0.5f, 0.16f), bodyFont, DeepGreen, 1.2f);
            CreateOutlinedText(deploy.transform, "Level Number", "LEVEL " + level.number.ToString("00"), 48, Cream,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.4f), new Vector2(0.9f, 0.46f), headingFont, DeepGreen, 2.3f);
        }

    }

    private void RefreshLifeHud()
    {
        if (lifeCountText == null || lifeStatusText == null) return;

        int lives = WarfestSession.Lives;
        int seconds = WarfestSession.SecondsUntilNextLife;
        if (lives == displayedLives && seconds == displayedLifeSeconds) return;

        displayedLives = lives;
        displayedLifeSeconds = seconds;
        lifeCountText.text = lives.ToString();
        lifeStatusText.text = lives >= WarfestSession.MaxLives ? "FULL" : WarfestSession.LifeTimerText;
        if (deployButton != null && !WarfestSession.CampaignComplete)
        {
            deployButton.interactable = lives > 0;
        }
    }

    private void BuildBottomNavigation()
    {
        RectTransform nav = CreateContainer(safeAreaRoot, "Bottom Navigation", new Vector2(0.5f, 0.075f), new Vector2(0.96f, 0.15f));

        // Only Home is active. The shop and trophy tiles are shown dimmed to read as disabled.
        Color disabledTint = new Color(0.5f, 0.53f, 0.5f, 0.6f);

        // Left (shop) - disabled, aligned on the shared baseline.
        Image leftTile = CreateSheetImage(nav, "Left Tile", new Rect(5f, 963f, 210f, 178f),
            new Vector2(0.19f, 0.5f), new Vector2(0.3f, 0.88f));
        leftTile.color = disabledTint;
        Image armory = CreateSheetImage(nav, "Armory", new Rect(237f, 960f, 205f, 181f),
            new Vector2(0.19f, 0.52f), new Vector2(0.19f, 0.62f));
        armory.color = disabledTint;

        // Center (home) - active, slightly larger so it reads as the selected tab.
        Button home = CreateSheetButton(nav, "Home Tab", new Rect(442f, 930f, 370f, 224f),
            new Vector2(0.5f, 0.5f), new Vector2(0.34f, 1.0f));
        home.onClick.AddListener(WarfestSession.ReturnToMenu);

        // Right (trophy) - disabled, mirrored to match the left tile exactly.
        Image rightTile = CreateSheetImage(nav, "Right Tile", new Rect(1038f, 963f, 211f, 178f),
            new Vector2(0.81f, 0.5f), new Vector2(0.3f, 0.88f));
        rightTile.color = disabledTint;
        Image trophy = CreateSheetImage(nav, "Trophy", new Rect(824f, 962f, 204f, 185f),
            new Vector2(0.81f, 0.52f), new Vector2(0.19f, 0.62f));
        trophy.color = disabledTint;
    }

    private Canvas CreateCanvas(string name)
    {
        GameObject gameObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        // Parent under this controller so the canvas is owned by (and destroyed with) this scene.
        // A scene-root object can otherwise linger across a scene load and appear in the Game scene.
        gameObject.transform.SetParent(transform, false);
        Canvas canvas = gameObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        canvasScaler = gameObject.GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;
        ApplyCanvasScale();
        return canvas;
    }

    private void ApplyCanvasScale()
    {
        if (canvasScaler == null) return;
        Vector2 referenceResolution = Screen.width >= Screen.height ? new Vector2(844f, 390f) : new Vector2(390f, 844f);
        if (referenceResolution == appliedReferenceResolution) return;
        appliedReferenceResolution = referenceResolution;
        canvasScaler.referenceResolution = referenceResolution;
    }

    private RectTransform CreateSafeAreaRoot(RectTransform parent)
    {
        GameObject gameObject = new GameObject("Safe Area", typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        safeAreaRoot = gameObject.GetComponent<RectTransform>();
        appliedSafeArea = new Rect();
        ApplySafeArea();
        return safeAreaRoot;
    }

    private void ApplySafeArea()
    {
        if (safeAreaRoot == null || Screen.width <= 0 || Screen.height <= 0) return;
        Rect safeArea = Screen.safeArea;
        if (safeArea == appliedSafeArea) return;
        appliedSafeArea = safeArea;
        safeAreaRoot.anchorMin = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
        safeAreaRoot.anchorMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);
        safeAreaRoot.offsetMin = Vector2.zero;
        safeAreaRoot.offsetMax = Vector2.zero;
    }

    private RectTransform CreateContainer(Transform parent, string name, Vector2 center, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        SetRect(rect, center, size);
        return rect;
    }

    private Image CreateSheetImage(Transform parent, string name, Rect topLeftRect, Vector2 center, Vector2 size)
    {
        Image image = CreateImage(parent, name, Color.white, center, size);
        image.sprite = CreateSheetSprite(topLeftRect);
        image.preserveAspect = true;
        return image;
    }

    private Button CreateSheetButton(Transform parent, string name, Rect topLeftRect, Vector2 center, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        gameObject.transform.SetParent(parent, false);
        SetRect(gameObject.GetComponent<RectTransform>(), center, size);
        Image image = gameObject.GetComponent<Image>();
        image.sprite = CreateSheetSprite(topLeftRect);
        image.preserveAspect = true;
        Button button = gameObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 0.88f, 1f);
        colors.pressedColor = new Color(0.82f, 0.9f, 0.72f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        return button;
    }

    private Sprite CreateSheetSprite(Rect topLeftRect)
    {
        if (panelSheet == null) return null;
        float scaleX = panelSheet.width / 1254f;
        float scaleY = panelSheet.height / 1254f;
        Rect unityRect = new Rect(topLeftRect.x * scaleX,
            panelSheet.height - (topLeftRect.y + topLeftRect.height) * scaleY,
            topLeftRect.width * scaleX, topLeftRect.height * scaleY);
        return Sprite.Create(panelSheet, unityRect, new Vector2(0.5f, 0.5f), 100f);
    }

    private Image CreateImage(Transform parent, string name, Color color, Vector2 center, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        gameObject.transform.SetParent(parent, false);
        SetRect(gameObject.GetComponent<RectTransform>(), center, size);
        Image image = gameObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private Text CreateOutlinedText(Transform parent, string name, string value, int fontSize, Color color,
        TextAnchor alignment, Vector2 center, Vector2 size, Font requestedFont, Color outlineColor, float outlineDistance)
    {
        Text text = CreateText(parent, name, value, fontSize, color, alignment, center, size, requestedFont);
        Outline outline = text.gameObject.AddComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(outlineDistance, -outlineDistance);
        outline.useGraphicAlpha = true;
        return text;
    }

    private Text CreateText(Transform parent, string name, string value, int fontSize, Color color,
        TextAnchor alignment, Vector2 center, Vector2 size, Font requestedFont)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        gameObject.transform.SetParent(parent, false);
        SetRect(gameObject.GetComponent<RectTransform>(), center, size);

        Text text = gameObject.GetComponent<Text>();
        text.font = requestedFont != null ? requestedFont : fallbackFont;
        text.text = value;
        text.fontSize = fontSize;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = Mathf.Max(10, fontSize - 10);
        text.resizeTextMaxSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    private static void SetRect(RectTransform rect, Vector2 center, Vector2 size)
    {
        if (size == Vector2.one)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return;
        }

        rect.anchorMin = center - size * 0.5f;
        rect.anchorMax = center + size * 0.5f;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void StartMenuAudio()
    {
        if (!Application.isPlaying) return;

        WarfestAudio.StopGameplayAudio();

        if (menuMusicSource == null)
        {
            GameObject audioObj = new GameObject("Main Menu Music", typeof(AudioSource));
            audioObj.transform.SetParent(transform, false);
            menuMusicSource = audioObj.GetComponent<AudioSource>();
        }

        AudioClip clip = WarfestAudio.GetEverytimeClip();
        if (clip != null)
        {
            menuMusicSource.clip = clip;
            menuMusicSource.playOnAwake = false;
            menuMusicSource.loop = true;
            menuMusicSource.spatialBlend = 0f;
            menuMusicSource.volume = 0.38f;
            menuMusicSource.mute = !WarfestAudio.MusicEnabled;
            if (!menuMusicSource.isPlaying)
            {
                menuMusicSource.Play();
            }
        }
    }

    private void StopMenuAudio()
    {
        if (menuMusicSource != null)
        {
            menuMusicSource.Stop();
        }
    }

    private void BuildSettingsFlyout()
    {
        GameObject flyoutObj = new GameObject("Settings Flyout", typeof(RectTransform));
        flyoutObj.transform.SetParent(safeAreaRoot, false);
        RectTransform flyout = flyoutObj.GetComponent<RectTransform>();
        SetRect(flyout, new Vector2(0.5f, 0.5f), Vector2.one);
        settingsFlyout = flyoutObj;

        // Dimmed backdrop tap
        GameObject backdropObj = new GameObject("Settings Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
        backdropObj.transform.SetParent(flyout, false);
        SetRect(backdropObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.one);
        Image backdropImage = backdropObj.GetComponent<Image>();
        backdropImage.color = new Color(0.02f, 0.05f, 0.10f, 0.65f);
        Button backdropBtn = backdropObj.GetComponent<Button>();
        backdropBtn.onClick.AddListener(ToggleSettingsFlyout);

        // Dialog container card
        RectTransform card = CreateContainer(flyout, "Settings Card", new Vector2(0.5f, 0.50f), new Vector2(0.74f, 0.30f));
        CreateSheetImage(card, "Cream Frame", new Rect(8f, 540f, 441f, 305f), new Vector2(0.5f, 0.5f), Vector2.one);

        // Title: "SETTINGS"
        CreateOutlinedText(card, "Settings Title", "SETTINGS", 28, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.82f), new Vector2(0.85f, 0.22f), headingFont, DeepGreen, 2f);

        // Sound Toggle Button
        Button sound = CreateAudioToggleButton(card, "Sound Toggle",
            WarfestAudio.GetSoundIconSprite(), new Vector2(0.35f, 0.44f), new Vector2(0.24f, 0.44f), out soundButtonBackground);
        sound.onClick.AddListener(ToggleSound);
        CreateOutlinedText(card, "Sound Label", "SOUND", 16, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.35f, 0.16f), new Vector2(0.30f, 0.18f), bodyFont, Navy, 1.2f);

        // Music Toggle Button
        Button music = CreateAudioToggleButton(card, "Music Toggle",
            WarfestAudio.GetMusicIconSprite(), new Vector2(0.65f, 0.44f), new Vector2(0.24f, 0.44f), out musicButtonBackground);
        music.onClick.AddListener(ToggleMusic);
        CreateOutlinedText(card, "Music Label", "MUSIC", 16, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.65f, 0.16f), new Vector2(0.30f, 0.18f), bodyFont, Navy, 1.2f);

        settingsFlyout.SetActive(false);
        RefreshSettingsButtons();
    }

    private Button CreateAudioToggleButton(Transform parent, string name, Sprite iconSprite,
        Vector2 center, Vector2 size, out Image background)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        gameObject.transform.SetParent(parent, false);
        SetRect(gameObject.GetComponent<RectTransform>(), center, size);
        background = gameObject.GetComponent<Image>();
        background.sprite = WarfestAudio.GetSettingsEnabledSprite();
        background.preserveAspect = true;

        Button button = gameObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
        colors.pressedColor = new Color(0.82f, 0.86f, 0.9f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(gameObject.transform, false);
        SetRect(iconObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.68f, 0.68f));
        Image icon = iconObj.GetComponent<Image>();
        icon.sprite = iconSprite;
        icon.color = Color.white;
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        return button;
    }

    private void ToggleSettingsFlyout()
    {
        if (settingsFlyout == null) return;
        settingsOpen = !settingsOpen;
        settingsFlyout.SetActive(settingsOpen);
        if (settingsOpen)
        {
            RefreshSettingsButtons();
        }
    }

    private void ToggleSound()
    {
        WarfestAudio.SoundEnabled = !WarfestAudio.SoundEnabled;
        RefreshSettingsButtons();
    }

    private void ToggleMusic()
    {
        WarfestAudio.MusicEnabled = !WarfestAudio.MusicEnabled;
        if (menuMusicSource != null)
        {
            menuMusicSource.mute = !WarfestAudio.MusicEnabled;
        }
        RefreshSettingsButtons();
    }

    private void RefreshSettingsButtons()
    {
        if (soundButtonBackground != null)
        {
            soundButtonBackground.sprite = WarfestAudio.SoundEnabled
                ? WarfestAudio.GetSettingsEnabledSprite()
                : WarfestAudio.GetSettingsDisabledSprite();
        }
        if (musicButtonBackground != null)
        {
            musicButtonBackground.sprite = WarfestAudio.MusicEnabled
                ? WarfestAudio.GetSettingsEnabledSprite()
                : WarfestAudio.GetSettingsDisabledSprite();
        }
    }
}
