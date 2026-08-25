using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class WarfestMainMenu : MonoBehaviour
{
    private static readonly Color Navy = new Color(0.08f, 0.16f, 0.24f, 1f);
    private static readonly Color DeepGreen = new Color(0.13f, 0.23f, 0.08f, 1f);
    private static readonly Color Cream = new Color(1f, 0.98f, 0.88f, 1f);
    private static readonly Color Gold = new Color(1f, 0.77f, 0.12f, 1f);

    [SerializeField] private Font headingFont;
    [SerializeField] private Font bodyFont;

    private Font fallbackFont;
    private Texture2D panelSheet;
    private RectTransform safeAreaRoot;
    private CanvasScaler canvasScaler;
    private Vector2 appliedReferenceResolution;
    private Rect appliedSafeArea;

    private void Start()
    {
        fallbackFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        panelSheet = Resources.Load<Texture2D>("pnl");

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.63f, 0.78f, 0.49f, 1f);
        }

        EnsureEventSystem();
        BuildMenu();
    }

    private void Update()
    {
        ApplyCanvasScale();
        ApplySafeArea();
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
    }

    private void BuildMenu()
    {
        Canvas canvas = CreateCanvas("Main Menu Canvas");
        RectTransform root = canvas.transform as RectTransform;
        CreateBackground(root);
        safeAreaRoot = CreateSafeAreaRoot(root);

        WarfestLevelCatalog.LevelDefinition level = WarfestLevelCatalog.Get(WarfestSession.SelectedLevel);
        int balls = WarfestSession.GetBallAllowance(WarfestSession.SelectedLevel);

        BuildTopStatus(level, balls);
        BuildWorldDecorations();
        BuildMissionCard(level, balls);
        BuildBottomNavigation();
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
        CreateSheetImage(safeAreaRoot, "Commander", new Rect(8f, 7f, 195f, 184f),
            new Vector2(0.10f, 0.91f), new Vector2(0.18f, 0.102f));

        CreateSheetImage(safeAreaRoot, "Coin Bar", new Rect(508f, 48f, 270f, 108f),
            new Vector2(0.425f, 0.92f), new Vector2(0.32f, 0.072f));
        CreateSheetImage(safeAreaRoot, "Coin", new Rect(384f, 38f, 132f, 132f),
            new Vector2(0.29f, 0.92f), new Vector2(0.105f, 0.064f));
        CreateOutlinedText(safeAreaRoot, "Campaign Value", (level.number * 125).ToString(), 24, Navy,
            TextAnchor.MiddleCenter, new Vector2(0.425f, 0.92f), new Vector2(0.17f, 0.046f), bodyFont, Color.white, 1.4f);

        CreateSheetImage(safeAreaRoot, "Energy Bar", new Rect(775f, 43f, 276f, 119f),
            new Vector2(0.715f, 0.92f), new Vector2(0.27f, 0.071f));
        CreateOutlinedText(safeAreaRoot, "Ball Count", balls.ToString(), 25, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.665f, 0.922f), new Vector2(0.07f, 0.046f), headingFont, Navy, 1.5f);
        CreateText(safeAreaRoot, "Ready Status", "READY", 18, DeepGreen, TextAnchor.MiddleCenter,
            new Vector2(0.775f, 0.92f), new Vector2(0.10f, 0.04f), bodyFont);

        CreateSheetImage(safeAreaRoot, "Settings", new Rect(1144f, 46f, 100f, 108f),
            new Vector2(0.925f, 0.92f), new Vector2(0.09f, 0.062f));

        CreateOutlinedText(safeAreaRoot, "Game Title", "WARFEST", 42, Cream, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.80f), new Vector2(0.68f, 0.075f), headingFont, DeepGreen, 2.4f);
        CreateOutlinedText(safeAreaRoot, "Location", "BANGALORE COMMAND", 17, Gold, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.755f), new Vector2(0.6f, 0.036f), bodyFont, DeepGreen, 1.2f);
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
        RectTransform card = CreateContainer(safeAreaRoot, "Mission Card", new Vector2(0.5f, 0.285f), new Vector2(0.86f, 0.34f));
        CreateSheetImage(card, "Cream Frame", new Rect(8f, 540f, 441f, 305f), new Vector2(0.5f, 0.5f), Vector2.one);

        CreateSheetImage(card, "Progress Track", new Rect(458f, 763f, 365f, 82f),
            new Vector2(0.47f, 0.91f), new Vector2(0.72f, 0.16f));
        Image progress = CreateSheetImage(card, "Progress Fill", new Rect(216f, 866f, 550f, 60f),
            new Vector2(0.18f, 0.91f), new Vector2(0.29f, 0.075f));
        progress.type = Image.Type.Filled;
        progress.fillMethod = Image.FillMethod.Horizontal;
        progress.fillOrigin = 0;
        progress.fillAmount = Mathf.Clamp01((float)level.number / WarfestSession.LevelCount);
        CreateOutlinedText(card, "Campaign Progress", level.number + "/" + WarfestSession.LevelCount, 21, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.49f, 0.91f), new Vector2(0.25f, 0.09f), bodyFont, Navy, 1.4f);
        CreateSheetImage(card, "Reward Chest", new Rect(1065f, 751f, 128f, 115f),
            new Vector2(0.86f, 0.91f), new Vector2(0.18f, 0.23f));

        Button deploy = CreateSheetButton(card, "Deploy Mission", new Rect(443f, 560f, 410f, 174f),
            new Vector2(0.5f, 0.52f), new Vector2(0.91f, 0.54f));
        deploy.onClick.AddListener(() => WarfestSession.LoadLevel(WarfestSession.SelectedLevel));

        string difficulty = level.difficulty <= 2 ? "EASY" : level.difficulty <= 4 ? "HARD" : "ELITE";
        CreateText(deploy.transform, "Difficulty", difficulty, 17, Gold, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.18f), bodyFont);
        CreateOutlinedText(deploy.transform, "Level Number", "LEVEL " + level.number.ToString("00"), 43, Cream,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.53f), new Vector2(0.86f, 0.32f), headingFont, DeepGreen, 2f);
        CreateText(deploy.transform, "Mission Name", level.title.ToUpperInvariant(), 18, Cream, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.25f), new Vector2(0.84f, 0.18f), bodyFont);

        CreateSheetImage(card, "Rounds", new Rect(24f, 837f, 120f, 125f),
            new Vector2(0.16f, 0.105f), new Vector2(0.18f, 0.25f));
        CreateOutlinedText(card, "Rounds Count", balls + " SHOTS", 19, Navy, TextAnchor.MiddleCenter,
            new Vector2(0.43f, 0.105f), new Vector2(0.34f, 0.13f), bodyFont, Color.white, 1.1f);
        CreateSheetImage(card, "Star Medal", new Rect(814f, 830f, 256f, 140f),
            new Vector2(0.84f, 0.11f), new Vector2(0.22f, 0.27f));
    }

    private void BuildBottomNavigation()
    {
        RectTransform nav = CreateContainer(safeAreaRoot, "Bottom Navigation", new Vector2(0.5f, 0.075f), new Vector2(0.96f, 0.15f));

        CreateSheetImage(nav, "Left Tile", new Rect(5f, 963f, 210f, 178f),
            new Vector2(0.17f, 0.46f), new Vector2(0.33f, 0.92f));
        CreateSheetImage(nav, "Armory", new Rect(237f, 960f, 205f, 181f),
            new Vector2(0.17f, 0.52f), new Vector2(0.22f, 0.72f));
        CreateOutlinedText(nav, "Armory Label", "ARMORY", 13, Cream, TextAnchor.MiddleCenter,
            new Vector2(0.17f, 0.15f), new Vector2(0.25f, 0.18f), bodyFont, DeepGreen, 1f);

        Button home = CreateSheetButton(nav, "Home Deploy", new Rect(442f, 930f, 370f, 224f),
            new Vector2(0.5f, 0.53f), new Vector2(0.42f, 1.05f));
        home.onClick.AddListener(() => WarfestSession.LoadLevel(WarfestSession.SelectedLevel));
        CreateOutlinedText(home.transform, "Deploy Label", "DEPLOY", 19, Cream, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.16f), new Vector2(0.7f, 0.18f), bodyFont, DeepGreen, 1.4f);

        CreateSheetImage(nav, "Right Tile", new Rect(1038f, 963f, 211f, 178f),
            new Vector2(0.83f, 0.46f), new Vector2(0.33f, 0.92f));
        CreateSheetImage(nav, "Trophy", new Rect(824f, 962f, 204f, 185f),
            new Vector2(0.83f, 0.52f), new Vector2(0.22f, 0.72f));
        CreateOutlinedText(nav, "Ranks Label", "RANKS", 13, Cream, TextAnchor.MiddleCenter,
            new Vector2(0.83f, 0.15f), new Vector2(0.25f, 0.18f), bodyFont, DeepGreen, 1f);
    }

    private Canvas CreateCanvas(string name)
    {
        GameObject gameObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
}
