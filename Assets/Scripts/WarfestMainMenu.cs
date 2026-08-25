using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class WarfestMainMenu : MonoBehaviour
{
    private static readonly Color Background = new Color(0.93f, 0.96f, 0.98f, 1f);
    private static readonly Color Ink = new Color(0.08f, 0.14f, 0.22f, 1f);
    private static readonly Color SoftPanel = new Color(1f, 1f, 1f, 0.96f);
    private static readonly Color Accent = new Color(0.13f, 0.48f, 0.68f, 1f);

    private Font font;
    private RectTransform safeAreaRoot;
    private CanvasScaler canvasScaler;
    private Vector2 appliedReferenceResolution;
    private Rect appliedSafeArea;

    private void Start()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Background;
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
        CreateImage(root, "Light Background", Background, new Vector2(0.5f, 0.5f), Vector2.one);
        safeAreaRoot = CreateSafeAreaRoot(root);

        Color secondaryInk = new Color(0.16f, 0.27f, 0.38f, 1f);
        CreateImage(safeAreaRoot, "Card", SoftPanel, new Vector2(0.5f, 0.49f), new Vector2(0.78f, 0.86f));
        CreateText(safeAreaRoot, "Title", "BLAST CIRCUIT", 30, Ink, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.78f), new Vector2(0.72f, 0.09f));
        CreateText(safeAreaRoot, "Subtitle", "CLEAR 50 FORTS", 38, secondaryInk, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.68f), new Vector2(0.72f, 0.055f));

        WarfestLevelCatalog.LevelDefinition level = WarfestLevelCatalog.Get(WarfestSession.SelectedLevel);
        string buttonLabel = "LEVEL " + level.number.ToString("00") + "\n" + level.title.ToUpperInvariant();
        Button levelButton = CreateButton(safeAreaRoot, "Level Button", buttonLabel, Accent, new Vector2(0.5f, 0.44f), new Vector2(0.62f, 0.24f), 50);
        levelButton.onClick.AddListener(() => WarfestSession.LoadLevel(WarfestSession.SelectedLevel));
        CreateText(safeAreaRoot, "Level Hint", level.subtitle, 34, secondaryInk, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.25f), new Vector2(0.72f, 0.08f));
        CreateText(safeAreaRoot, "Footer", "TAP THE LEVEL\nBUTTON TO DEPLOY", 30, secondaryInk, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.10f), new Vector2(0.72f, 0.09f));
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

        // Use iPhone point dimensions rather than Retina pixel dimensions so text and touch targets
        // retain a readable physical size on both device orientations.
        Vector2 referenceResolution = Screen.width >= Screen.height
            ? new Vector2(844f, 390f)
            : new Vector2(390f, 844f);
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

    private Text CreateText(Transform parent, string name, string value, int fontSize, Color color, TextAnchor alignment, Vector2 center, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        gameObject.transform.SetParent(parent, false);
        SetRect(gameObject.GetComponent<RectTransform>(), center, size);

        Text text = gameObject.GetComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = fontSize;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = Mathf.Max(10, fontSize - 12);
        text.resizeTextMaxSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = FontStyle.Bold;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    private Button CreateButton(Transform parent, string name, string label, Color color, Vector2 center, Vector2 size, int fontSize)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        gameObject.transform.SetParent(parent, false);
        SetRect(gameObject.GetComponent<RectTransform>(), center, size);

        Image image = gameObject.GetComponent<Image>();
        image.color = color;
        Button button = gameObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.16f);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        CreateText(gameObject.transform, "Label", label, fontSize, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.one);
        return button;
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
