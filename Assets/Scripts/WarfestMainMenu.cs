#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
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
    private const string PrivacyPolicyUrl = "https://sites.google.com/view/ssmashprivacypolicy/home";
    private const string TermsOfUseUrl = "https://sites.google.com/view/sstou/home";

    private static readonly Color Navy = new Color(0.08f, 0.16f, 0.24f, 1f);
    private static readonly Color DeepGreen = new Color(0.13f, 0.23f, 0.08f, 1f);
    private static readonly Color Cream = new Color(1f, 0.98f, 0.88f, 1f);

    [SerializeField] private Font headingFont;
    [SerializeField] private Font bodyFont;

    private Font fallbackFont;
    private Texture2D panelSheet;
    private Dictionary<string, Sprite> panelSprites;
    private RectTransform iphoneFrameRoot;
    private RectTransform safeAreaRoot;
    private CanvasScaler canvasScaler;
    private Vector2 appliedReferenceResolution;
    private Rect appliedSafeArea;
    private Canvas menuCanvas;
    private GameObject createdEventSystem;
    private Text lifeCountText;
    private Text lifeStatusText;
    private Text coinValueText;
    private Button deployButton;
    private int displayedLives = -1;
    private int displayedLifeSeconds = -1;
    private float nextLifeHudRefreshTime;
    private AudioSource menuMusicSource;
    private GameObject settingsFlyout;
    private Image soundButtonBackground;
    private Image musicButtonBackground;
    private bool settingsOpen;
    private GameObject buyLivesFlyout;
    private Button buyLivesButton;
    private Image buyLivesButtonBg;
    private Text buyLivesButtonText;
    private Image buyLivesCoinIcon;
    private Text buyLivesStatusText;
    private bool buyLivesOpen;

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
        Camera mainCamera = Camera.main;
        Rect targetRect = WarfestDeviceViewport.GetNormalizedViewport();
        if (mainCamera != null && mainCamera.rect != targetRect)
        {
            mainCamera.rect = targetRect;
        }

        if (iphoneFrameRoot != null)
        {
            Vector2 curMin = iphoneFrameRoot.anchorMin;
            Vector2 curMax = iphoneFrameRoot.anchorMax;
            if (curMin.x != targetRect.xMin || curMax.x != targetRect.xMax)
            {
                iphoneFrameRoot.anchorMin = new Vector2(targetRect.xMin, targetRect.yMin);
                iphoneFrameRoot.anchorMax = new Vector2(targetRect.xMax, targetRect.yMax);
                iphoneFrameRoot.offsetMin = Vector2.zero;
                iphoneFrameRoot.offsetMax = Vector2.zero;
            }
        }

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

        if (headingFont == null) headingFont = WarfestFontResolver.HeadingFont;
        if (bodyFont == null) bodyFont = WarfestFontResolver.BodyFont;
        fallbackFont = WarfestFontResolver.FallbackFont;
        LoadPanelSprites();

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
            mainCamera.rect = WarfestDeviceViewport.GetNormalizedViewport();
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
        iphoneFrameRoot = null;
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

        if (buyLivesFlyout != null)
        {
            SafeDestroy(buyLivesFlyout);
            buyLivesFlyout = null;
        }
        coinValueText = null;
        buyLivesButton = null;
        buyLivesButtonBg = null;
        buyLivesButtonText = null;
        buyLivesCoinIcon = null;
        buyLivesStatusText = null;
        buyLivesOpen = false;
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

        Rect viewport = WarfestDeviceViewport.GetNormalizedViewport();
        GameObject frameObj = new GameObject("iPhone Frame", typeof(RectTransform));
        frameObj.transform.SetParent(root, false);
        iphoneFrameRoot = frameObj.GetComponent<RectTransform>();
        iphoneFrameRoot.anchorMin = new Vector2(viewport.xMin, viewport.yMin);
        iphoneFrameRoot.anchorMax = new Vector2(viewport.xMax, viewport.yMax);
        iphoneFrameRoot.offsetMin = Vector2.zero;
        iphoneFrameRoot.offsetMax = Vector2.zero;

        CreateBackground(iphoneFrameRoot);
        safeAreaRoot = CreateSafeAreaRoot(iphoneFrameRoot);

        WarfestLevelCatalog.LevelDefinition level = WarfestLevelCatalog.Get(WarfestSession.SelectedLevel);
        int balls = WarfestSession.GetBallAllowance(WarfestSession.SelectedLevel);

        BuildTopStatus(level, balls);
        BuildMissionCard(level, balls);
        BuildBottomNavigation();
        BuildSettingsFlyout();
        BuildBuyLivesPanel();
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
        const float topBarY = 0.932f;
        const float barHeight = 0.046f;

        // 1. Commander Avatar: circular portrait on left
        CreatePanelImage(safeAreaRoot, "Commander", "avatar_soldier",
            new Vector2(0.075f, topBarY), new Vector2(0.115f, 0.054f));

        // 2. Coin Bar: Pill container at topBarY
        Image coinBar = CreateSlicedPanelImage(safeAreaRoot, "Coin Bar", "panel_pill_1",
            new Vector2(0.320f, topBarY), new Vector2(0.285f, barHeight));

        // Coin Icon inside coinBar on left
        CreatePanelImage(coinBar.transform, "Coin Icon", "icon_star_coin",
            new Vector2(0.14f, 0.50f), new Vector2(0.24f, 0.82f));

        // Coin Value text centered in coinBar
        coinValueText = CreateText(coinBar.transform, "Coin Value", WarfestSession.Coins.ToString(), 17, Navy,
            TextAnchor.MiddleCenter, new Vector2(0.52f, 0.50f), new Vector2(0.46f, 0.70f), bodyFont);

        // Coin Plus Button on right
        Button coinPlus = CreatePanelButton(coinBar.transform, "Coin Plus", "btn_plus",
            new Vector2(0.88f, 0.50f), new Vector2(0.21f, 0.78f));
        coinPlus.onClick.AddListener(OpenBuyLivesPanel);

        // 3. Lives Bar: Pill container at topBarY - tap to open Buy Lives panel
        int lives = WarfestSession.Lives;
        Button livesBar = CreateSlicedPanelButton(safeAreaRoot, "Lives Bar", "panel_pill_2",
            new Vector2(0.665f, topBarY), new Vector2(0.295f, barHeight));
        livesBar.onClick.AddListener(OpenBuyLivesPanel);

        // Heart icon inside livesBar on left
        CreatePanelImage(livesBar.transform, "Heart Icon", "icon_heart",
            new Vector2(0.14f, 0.50f), new Vector2(0.24f, 0.82f));

        bool isFull = WarfestSession.LivesFull;
        lifeCountText = CreateOutlinedText(livesBar.transform, "Life Count", isFull ? "FULL" : lives.ToString(), 18, Cream,
            TextAnchor.MiddleCenter, isFull ? new Vector2(0.50f, 0.50f) : new Vector2(0.36f, 0.50f), isFull ? new Vector2(0.52f, 0.70f) : new Vector2(0.22f, 0.70f), headingFont, Navy, 1.4f);
        lifeStatusText = CreateText(livesBar.transform, "Life Status", isFull ? "" : WarfestSession.LifeTimerText, 13, DeepGreen,
            TextAnchor.MiddleCenter, new Vector2(0.64f, 0.50f), new Vector2(0.30f, 0.70f), bodyFont);

        // Plus button inside livesBar on right
        Button livesPlus = CreatePanelButton(livesBar.transform, "Lives Plus", "btn_plus",
            new Vector2(0.88f, 0.50f), new Vector2(0.21f, 0.78f));
        livesPlus.onClick.AddListener(OpenBuyLivesPanel);

        // 4. Settings Gear Icon at topBarY on far right
        Button settingsBtn = CreatePanelButton(safeAreaRoot, "Settings", "icon_settings",
            new Vector2(0.925f, topBarY), new Vector2(0.095f, 0.046f));
        settingsBtn.onClick.AddListener(ToggleSettingsFlyout);
    }

    private void BuildMissionCard(WarfestLevelCatalog.LevelDefinition level, int balls)
    {
        RectTransform card = CreateContainer(safeAreaRoot, "Mission Card", new Vector2(0.5f, 0.48f), new Vector2(0.88f, 0.52f));

        // Level pill banner header
        Image levelPill = CreateSlicedPanelImage(card, "Level Pill", "panel_pill_3",
            new Vector2(0.5f, 0.82f), new Vector2(0.86f, 0.15f));

        int displayLevel = WarfestSession.CampaignComplete ? WarfestSession.LevelCount : level.number;
        string levelTitle = WarfestSession.CampaignComplete ? "REPLAY " + displayLevel : "LEVEL " + displayLevel.ToString("00");
        string diffText = level.difficulty <= 2 ? "EASY" : level.difficulty <= 4 ? "HARD" : "ELITE";
        if (WarfestSession.CampaignComplete) diffText = "MASTER";

        // Unified Level and Difficulty header for clean centered alignment without overlapping
        CreateOutlinedText(levelPill.transform, "Level Header", $"{levelTitle}  •  {diffText}", 22, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.50f, 0.50f), new Vector2(0.88f, 0.75f), headingFont, DeepGreen, 2f);

        // Campaign progress indicator
        CreateOutlinedText(card, "Campaign Progress", "Campaign: " + displayLevel + " / " + WarfestSession.LevelCount + " Completed", 16, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.66f), new Vector2(0.92f, 0.08f), bodyFont, Navy, 1.4f);

        // Center Big Juicy Green Play Button from panel_new
        Button deploy = CreatePanelButton(card, "Deploy Mission", "btn_play",
            new Vector2(0.5f, 0.35f), new Vector2(0.78f, 0.28f));
        deployButton = deploy;
        deploy.interactable = true;

        deploy.onClick.AddListener(() =>
        {
            if (WarfestSession.Lives <= 0)
            {
                OpenBuyLivesPanel();
                return;
            }
            StopMenuAudio();
            int lvl = WarfestSession.CampaignComplete ? WarfestSession.LevelCount - 1 : WarfestSession.SelectedLevel;
            WarfestLoadingScreen.ShowAndLoad(lvl);
        });

        // "BATTLE" label on the play button (left-center aligned before the triangle)
        CreateOutlinedText(deploy.transform, "Play Text", "BATTLE", 30, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.36f, 0.52f), new Vector2(0.46f, 0.60f), headingFont, DeepGreen, 2.5f);
    }

    private void RefreshLifeHud()
    {
        if (lifeCountText == null || lifeStatusText == null) return;

        int lives = WarfestSession.Lives;
        int seconds = WarfestSession.SecondsUntilNextLife;
        if (lives == displayedLives && seconds == displayedLifeSeconds) return;

        displayedLives = lives;
        displayedLifeSeconds = seconds;
        if (lives >= WarfestSession.MaxLives)
        {
            lifeCountText.text = "FULL";
            lifeStatusText.text = "";
            SetRect(lifeCountText.rectTransform, new Vector2(0.50f, 0.50f), new Vector2(0.52f, 0.70f));
        }
        else
        {
            lifeCountText.text = lives.ToString();
            lifeStatusText.text = WarfestSession.LifeTimerText;
            SetRect(lifeCountText.rectTransform, new Vector2(0.36f, 0.50f), new Vector2(0.22f, 0.70f));
            SetRect(lifeStatusText.rectTransform, new Vector2(0.64f, 0.50f), new Vector2(0.30f, 0.70f));
        }
        RefreshCoinHud();
        RefreshBuyLivesPanelState();
    }

    private void RefreshCoinHud()
    {
        if (coinValueText != null)
        {
            coinValueText.text = WarfestSession.Coins.ToString();
        }
    }

    private void BuildBottomNavigation()
    {
        RectTransform nav = CreateContainer(safeAreaRoot, "Bottom Navigation", new Vector2(0.5f, 0.08f), new Vector2(0.90f, 0.12f));

        // Only keep the home button in bottom bar and replace sprites with the panel_new spritesheet
        Button home = CreatePanelButton(nav, "Home Tab", "btn_home",
            new Vector2(0.5f, 0.5f), new Vector2(0.24f, 0.85f));
        home.onClick.AddListener(WarfestSession.ReturnToMenu);
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
        canvasScaler.matchWidthOrHeight = 1.0f; // Lock scale strictly to height to prevent wide-screen stretching
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
        safeAreaRoot.anchorMin = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
        safeAreaRoot.anchorMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);
        safeAreaRoot.offsetMin = Vector2.zero;
        safeAreaRoot.offsetMax = Vector2.zero;
        appliedSafeArea = safeArea;
    }

    private RectTransform CreateContainer(Transform parent, string name, Vector2 center, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        SetRect(rect, center, size);
        return rect;
    }

    private void LoadPanelSprites()
    {
        if (panelSprites != null && panelSprites.Count > 0) return;
        panelSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("panel_new");
        if (sprites != null)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] != null && !panelSprites.ContainsKey(sprites[i].name))
                {
                    panelSprites[sprites[i].name] = sprites[i];
                }
            }
        }
    }

    private Sprite GetPanelSprite(string spriteName)
    {
        LoadPanelSprites();
        if (panelSprites != null && panelSprites.TryGetValue(spriteName, out Sprite s))
        {
            return s;
        }
        return null;
    }

    private Image CreatePanelImage(Transform parent, string name, string spriteName, Vector2 center, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        gameObject.transform.SetParent(parent, false);
        SetRect(gameObject.GetComponent<RectTransform>(), center, size);
        Image image = gameObject.GetComponent<Image>();
        image.sprite = GetPanelSprite(spriteName);
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private Image CreateSlicedPanelImage(Transform parent, string name, string spriteName, Vector2 center, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        gameObject.transform.SetParent(parent, false);
        SetRect(gameObject.GetComponent<RectTransform>(), center, size);
        Image image = gameObject.GetComponent<Image>();
        image.sprite = GetPanelSprite(spriteName);
        image.type = Image.Type.Sliced;
        image.raycastTarget = false;
        return image;
    }

    private Button CreatePanelButton(Transform parent, string name, string spriteName, Vector2 center, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        gameObject.transform.SetParent(parent, false);
        SetRect(gameObject.GetComponent<RectTransform>(), center, size);
        Image image = gameObject.GetComponent<Image>();
        image.sprite = GetPanelSprite(spriteName);
        image.preserveAspect = true;
        Button button = gameObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 0.90f, 1f);
        colors.pressedColor = new Color(0.85f, 0.95f, 0.85f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        return button;
    }

    private Button CreateSlicedPanelButton(Transform parent, string name, string spriteName, Vector2 center, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        gameObject.transform.SetParent(parent, false);
        SetRect(gameObject.GetComponent<RectTransform>(), center, size);
        Image image = gameObject.GetComponent<Image>();
        image.sprite = GetPanelSprite(spriteName);
        image.type = Image.Type.Sliced;
        Button button = gameObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 0.90f, 1f);
        colors.pressedColor = new Color(0.85f, 0.95f, 0.85f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        return button;
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
        Font chosenFont = requestedFont != null ? requestedFont : (headingFont != null ? headingFont : WarfestFontResolver.HeadingFont);
        text.font = chosenFont;
        text.text = value;
        text.fontSize = fontSize;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 8;
        text.resizeTextMaxSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
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
            menuMusicSource.volume = 0.22f;
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
        RectTransform card = CreateContainer(flyout, "Settings Card", new Vector2(0.5f, 0.50f), new Vector2(0.80f, 0.256f));
        CreateSlicedPanelImage(card, "Card Frame", "panel_pill_1", new Vector2(0.5f, 0.5f), Vector2.one);
        CreateSlicedPanelImage(card, "Title Header", "panel_pill_3", new Vector2(0.5f, 0.865f), new Vector2(0.65f, 0.18f));

        // Title: "SETTINGS" - cleanly nested within the top tab
        CreateOutlinedText(card, "Settings Title", "SETTINGS", 22, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.865f), new Vector2(0.60f, 0.14f), headingFont, DeepGreen, 2f);

        // Close "✕" button in top-right tab
        GameObject closeObj = new GameObject("Close Button", typeof(RectTransform), typeof(Image), typeof(Button));
        closeObj.transform.SetParent(card, false);
        SetRect(closeObj.GetComponent<RectTransform>(), new Vector2(0.90f, 0.865f), new Vector2(0.12f, 0.12f));
        Image closeImg = closeObj.GetComponent<Image>();
        closeImg.color = Color.clear;
        Button closeBtn = closeObj.GetComponent<Button>();
        closeBtn.targetGraphic = closeImg;
        closeBtn.onClick.AddListener(ToggleSettingsFlyout);
        CreateOutlinedText(closeObj.transform, "Close Text", "✕", 18, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.one, headingFont, DeepGreen, 1.5f);

        // Sound Toggle Button (prominent circular button, aligned symmetrically on left)
        Button sound = CreateAudioToggleButton(card, "Sound Toggle",
            WarfestAudio.GetSoundIconSprite(), new Vector2(0.34f, 0.550f), new Vector2(0.23f, 0.332f), out soundButtonBackground);
        sound.onClick.AddListener(ToggleSound);

        // Music Toggle Button (prominent circular button, aligned symmetrically on right)
        Button music = CreateAudioToggleButton(card, "Music Toggle",
            WarfestAudio.GetMusicIconSprite(), new Vector2(0.66f, 0.550f), new Vector2(0.23f, 0.332f), out musicButtonBackground);
        music.onClick.AddListener(ToggleMusic);

        // Legal Links: Privacy Policy & Terms of Use (symmetrically centered around middle dot)
        CreateLinkButton(card, "Privacy Policy Link", "<b>Privacy Policy</b>",
            PrivacyPolicyUrl, new Vector2(0.27f, 0.205f), new Vector2(0.38f, 0.15f), TextAnchor.MiddleRight);

        CreateText(card, "Link Separator", "<b>•</b>", 16, new Color(Navy.r, Navy.g, Navy.b, 0.50f),
            TextAnchor.MiddleCenter, new Vector2(0.50f, 0.205f), new Vector2(0.06f, 0.15f), bodyFont);

        CreateLinkButton(card, "Terms of Use Link", "<b>Terms of Use</b>",
            TermsOfUseUrl, new Vector2(0.73f, 0.205f), new Vector2(0.38f, 0.15f), TextAnchor.MiddleLeft);

        settingsFlyout.SetActive(settingsOpen);
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

    private Button CreateLinkButton(Transform parent, string name, string labelText, string url,
        Vector2 center, Vector2 size, TextAnchor textAlignment = TextAnchor.MiddleCenter)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        gameObject.transform.SetParent(parent, false);
        SetRect(gameObject.GetComponent<RectTransform>(), center, size);

        Image hitGraphic = gameObject.GetComponent<Image>();
        hitGraphic.color = Color.clear;
        hitGraphic.raycastTarget = true;

        Button button = gameObject.GetComponent<Button>();
        button.targetGraphic = hitGraphic;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.clear;
        colors.highlightedColor = new Color(0.08f, 0.16f, 0.24f, 0.08f);
        colors.pressedColor = new Color(0.08f, 0.16f, 0.24f, 0.18f);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.06f;
        button.colors = colors;

        button.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
            }
        });

        Text text = CreateText(gameObject.transform, "Label", labelText, 16, Navy,
            textAlignment, new Vector2(0.5f, 0.5f), Vector2.one, bodyFont);
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        return button;
    }

    private void ToggleSettingsFlyout()
    {
        if (settingsFlyout == null) return;
        if (!settingsOpen && buyLivesOpen)
        {
            CloseBuyLivesPanel();
        }
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

    private void BuildBuyLivesPanel()
    {
        GameObject flyoutObj = new GameObject("Buy Lives Flyout", typeof(RectTransform));
        flyoutObj.transform.SetParent(safeAreaRoot, false);
        RectTransform flyout = flyoutObj.GetComponent<RectTransform>();
        SetRect(flyout, new Vector2(0.5f, 0.5f), Vector2.one);
        buyLivesFlyout = flyoutObj;

        // Dimmed backdrop tap to close
        GameObject backdropObj = new GameObject("Buy Lives Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
        backdropObj.transform.SetParent(flyout, false);
        SetRect(backdropObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.one);
        Image backdropImage = backdropObj.GetComponent<Image>();
        backdropImage.color = new Color(0.02f, 0.05f, 0.10f, 0.65f);
        Button backdropBtn = backdropObj.GetComponent<Button>();
        backdropBtn.onClick.AddListener(CloseBuyLivesPanel);

        // Dialog container card
        RectTransform card = CreateContainer(flyout, "Buy Lives Card", new Vector2(0.5f, 0.50f), new Vector2(0.80f, 0.256f));
        CreateSlicedPanelImage(card, "Card Frame", "panel_pill_1", new Vector2(0.5f, 0.5f), Vector2.one);
        CreateSlicedPanelImage(card, "Title Header", "panel_pill_3", new Vector2(0.5f, 0.865f), new Vector2(0.65f, 0.18f));

        // Title: "LIVES" - cleanly nested within top tab
        CreateOutlinedText(card, "Buy Lives Title", "LIVES", 22, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.865f), new Vector2(0.60f, 0.14f), headingFont, DeepGreen, 2f);

        // Close "✕" button in top-right tab
        GameObject closeObj = new GameObject("Close Button", typeof(RectTransform), typeof(Image), typeof(Button));
        closeObj.transform.SetParent(card, false);
        SetRect(closeObj.GetComponent<RectTransform>(), new Vector2(0.90f, 0.865f), new Vector2(0.12f, 0.12f));
        Image closeImg = closeObj.GetComponent<Image>();
        closeImg.color = Color.clear;
        Button closeBtn = closeObj.GetComponent<Button>();
        closeBtn.targetGraphic = closeImg;
        closeBtn.onClick.AddListener(CloseBuyLivesPanel);
        CreateOutlinedText(closeObj.transform, "Close Text", "✕", 18, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.one, headingFont, DeepGreen, 1.5f);

        // Text: "Buy 2 lives"
        CreateOutlinedText(card, "Prompt Text", "Buy 2 lives", 26, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.620f), new Vector2(0.85f, 0.18f), headingFont, DeepGreen, 2.2f);

        // Button: Green button with "200 coins"
        Button buyBtn = CreatePanelButton(card, "Buy Button", "btn_play",
            new Vector2(0.5f, 0.320f), new Vector2(0.60f, 0.280f));
        buyLivesButton = buyBtn;
        buyLivesButtonBg = buyBtn.GetComponent<Image>();
        buyBtn.onClick.AddListener(OnBuyLivesClicked);

        // Coin Icon inside button (left side)
        buyLivesCoinIcon = CreatePanelImage(buyBtn.transform, "Coin Icon", "icon_star_coin",
            new Vector2(0.22f, 0.50f), new Vector2(0.20f, 0.60f));

        // Button Text: "200 COINS"
        buyLivesButtonText = CreateOutlinedText(buyBtn.transform, "Cost Text", "200 COINS", 20, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.60f, 0.50f), new Vector2(0.64f, 0.60f), headingFont, DeepGreen, 1.8f);

        // Status text below button (e.g. "NOT ENOUGH COINS" or current balance)
        buyLivesStatusText = CreateText(card, "Status Text", "", 14, new Color(Navy.r, Navy.g, Navy.b, 0.65f),
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.120f), new Vector2(0.85f, 0.12f), bodyFont);

        buyLivesFlyout.SetActive(buyLivesOpen);
        RefreshBuyLivesPanelState();
    }

    private void OnBuyLivesClicked()
    {
        if (WarfestSession.BuyTwoLives())
        {
            displayedLives = -1; // force HUD refresh
            RefreshLifeHud();
            RefreshCoinHud();
            RefreshBuyLivesPanelState();
        }
    }

    private void RefreshBuyLivesPanelState()
    {
        if (buyLivesButton == null) return;

        bool hasCoins = WarfestSession.Coins >= WarfestSession.BuyTwoLivesCoinCost;
        bool canBuy = WarfestSession.Lives < WarfestSession.MaxLives && hasCoins;

        buyLivesButton.interactable = canBuy;

        if (buyLivesButtonBg != null)
        {
            buyLivesButtonBg.color = canBuy ? Color.white : new Color(0.52f, 0.55f, 0.52f, 1f);
        }
        if (buyLivesButtonText != null)
        {
            buyLivesButtonText.color = canBuy ? Cream : new Color(0.85f, 0.85f, 0.85f, 0.85f);
        }
        if (buyLivesCoinIcon != null)
        {
            buyLivesCoinIcon.color = canBuy ? Color.white : new Color(0.70f, 0.70f, 0.70f, 0.8f);
        }

        if (buyLivesStatusText != null)
        {
            if (WarfestSession.Lives >= WarfestSession.MaxLives)
            {
                buyLivesStatusText.text = "LIVES ARE FULL";
                buyLivesStatusText.color = DeepGreen;
            }
            else if (!hasCoins)
            {
                buyLivesStatusText.text = "NOT ENOUGH COINS (" + WarfestSession.Coins + " / 200)";
                buyLivesStatusText.color = new Color(0.78f, 0.18f, 0.15f, 1f);
            }
            else
            {
                buyLivesStatusText.text = "Coins Available: " + WarfestSession.Coins;
                buyLivesStatusText.color = new Color(0.20f, 0.35f, 0.22f, 1f);
            }
        }
    }

    public void OpenBuyLivesPanel()
    {
        if (settingsOpen)
        {
            settingsOpen = false;
            if (settingsFlyout != null) settingsFlyout.SetActive(false);
        }
        if (buyLivesFlyout == null) return;
        buyLivesOpen = true;
        RefreshBuyLivesPanelState();
        buyLivesFlyout.SetActive(true);
    }

    public void CloseBuyLivesPanel()
    {
        if (buyLivesFlyout == null) return;
        buyLivesOpen = false;
        buyLivesFlyout.SetActive(false);
    }
}
