using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class WarfestGameController : MonoBehaviour
{
    private static readonly Color LightBackground = new Color(0.93f, 0.96f, 0.98f, 1f);
    private static readonly Color Ink = new Color(0.08f, 0.14f, 0.22f, 1f);
    private static readonly Color LightPanel = new Color(1f, 1f, 1f, 0.95f);
    private static readonly Color DisabledBall = new Color(0.70f, 0.76f, 0.82f, 1f);

    private readonly List<GameObject> blocks = new List<GameObject>();
    private readonly List<Image> ballSlots = new List<Image>();
    private Font font;
    private Camera gameplayCamera;
    private Transform worldRoot;
    private Transform pistolPivot;
    private Transform pistolVisual;
    private Transform muzzle;
    private Text ballsText;
    private Text targetText;
    private WarfestLevelCatalog.LevelDefinition level;
    private Sprite pistolSprite;
    private Sprite tableSprite;
    private Sprite cannonBaseSprite;
    private Sprite[] blockSprites;
    private int ballCapacity;
    private int remainingBalls;
    private int targetsRemaining;
    private bool levelEnded;
    private Vector3 pistolRestLocalPosition;
    private Canvas hudCanvas;
    private RectTransform safeAreaRoot;
    private CanvasScaler hudScaler;
    private Vector2 appliedReferenceResolution;
    private Rect appliedSafeArea;

    private const float TableColliderWidth = 16.10f;
    private const float TableColliderHeight = 0.52f;
    private const float TableColliderOffsetY = 1.64f;
    private const float ShotRadius = 0.16f;
    private const float ShotSpeed = 17f;
    private const float ShotLifetime = 4f;
    private float recoilTime;

private void Start()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        LoadOriginalSprites();
        EnsureEventSystem();
        EnsureCamera();
        level = WarfestLevelCatalog.Get(WarfestSession.SelectedLevel);
        ballCapacity = WarfestSession.GetBallAllowance(level.number - 1);
        remainingBalls = ballCapacity;
        BuildWorld();
        BuildHud();
        RefreshHud();
    }

    private void LoadOriginalSprites()
    {
        Sprite[] pistolSprites = Resources.LoadAll<Sprite>("pistol");
        Sprite[] tables = Resources.LoadAll<Sprite>("table");
        blockSprites = Resources.LoadAll<Sprite>("blocks");
        pistolSprite = pistolSprites.Length > 0 ? pistolSprites[0] : null;
        tableSprite = tables.Length > 0 ? tables[0] : null;
        if (pistolSprite == null || tableSprite == null || blockSprites == null || blockSprites.Length == 0)
        {
            Debug.LogError("Warfest requires the original Resources/pistol, Resources/table, and Resources/blocks sprites.");
        }
    }

private void Update()
    {
        ApplyCanvasScale();
        ApplySafeArea();
        if (levelEnded || gameplayCamera == null || muzzle == null) return;
        if (TryGetPointer(out Vector2 position, out bool pressed) && pressed)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            AimAt(position);
            Fire();
        }
        UpdateRecoil();
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
    }

private void EnsureCamera()
    {
        gameplayCamera = Camera.main;
        if (gameplayCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            gameplayCamera = cameraObject.GetComponent<Camera>();
        }
        gameplayCamera.transform.position = new Vector3(0f, 1.85f, -10f);
        gameplayCamera.orthographic = true;
        gameplayCamera.orthographicSize = 6.35f;
        gameplayCamera.clearFlags = CameraClearFlags.SolidColor;
        gameplayCamera.backgroundColor = LightBackground;
    }

    private void BuildWorld()
    {
        worldRoot = new GameObject("Runtime Level // " + level.number.ToString("00")).transform;
        CreateTable();
        List<WarfestLevelCatalog.BlockSpec> specs = new List<WarfestLevelCatalog.BlockSpec>();
        WarfestLevelCatalog.FillLayout(level.number - 1, specs);
        targetsRemaining = specs.Count;
        for (int i = 0; i < specs.Count; i++) CreateBlock(specs[i], i);
        CreatePistol();
    }

    private void CreateTable()
    {
        if (tableSprite == null) return;

        GameObject table = CreateSprite("Table", tableSprite, new Vector3(0f, -0.95f, 0f), new Vector2(0.315f, 0.315f), 0);
        int tableLayer = LayerMask.NameToLayer("WarfestTable");
        if (tableLayer >= 0) table.layer = tableLayer;

        BoxCollider2D surface = table.AddComponent<BoxCollider2D>();
        // The sprite contains transparent space above the physical tabletop. This offset
        // aligns the collision top with the visible metal surface rather than the image bounds.
        surface.size = new Vector2(TableColliderWidth, TableColliderHeight);
        surface.offset = new Vector2(0f, TableColliderOffsetY);
    }

    private void CreateBlock(WarfestLevelCatalog.BlockSpec spec, int index)
    {
        if (blockSprites == null || blockSprites.Length == 0) return;
        int spriteIndex = spec.spriteIndex >= 0 ? spec.spriteIndex % blockSprites.Length : index % blockSprites.Length;
        Sprite sprite = blockSprites[spriteIndex];

        Vector2 spriteSize = sprite.bounds.size;
        Vector2 localScale = new Vector2(
            spriteSize.x > 0.0001f ? spec.size.x / spriteSize.x : 1f,
            spriteSize.y > 0.0001f ? spec.size.y / spriteSize.y : 1f);

        GameObject block = CreateSprite("Target " + (index + 1).ToString("00"), sprite, new Vector3(spec.position.x, spec.position.y, 0f), localScale, 2);
        block.transform.rotation = Quaternion.Euler(0f, 0f, spec.rotation);
        block.GetComponent<SpriteRenderer>().color = spec.color;

        BoxCollider2D collider = block.AddComponent<BoxCollider2D>();
        collider.size = spriteSize;
        Rigidbody2D body = block.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.mass = 0.7f;
        body.gravityScale = 1f;
        body.linearDamping = 0.12f;

        body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        body.angularDamping = 0.4f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        WarfestTarget target = block.AddComponent<WarfestTarget>();
        target.Initialize(this);
        blocks.Add(block);
    }

private void CreatePistol()
    {
        if (pistolSprite == null) return;
        CreateCannonBase();
        pistolPivot = new GameObject("Pistol Pivot").transform;
        pistolPivot.SetParent(worldRoot, false);
        pistolPivot.position = new Vector3(0f, -3.35f, -1f);
        pistolVisual = CreateSprite("Pistol", pistolSprite, new Vector3(0f, 0.34f, 0f), new Vector2(0.165f, 0.165f), 5).transform;
        pistolVisual.SetParent(pistolPivot, false);
        pistolRestLocalPosition = pistolVisual.localPosition;
        muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(pistolPivot, false);
        muzzle.localPosition = new Vector3(0f, 0.60f, 0f);
    }

private void CreateCannonBase()
    {
        CreateBasePad("Cannon Base Shadow", new Vector3(0f, -3.72f, 0f), new Vector2(2.95f, 0.58f), new Color(0.14f, 0.19f, 0.16f, 0.72f));
        CreateBasePad("Cannon Base Outer Ring", new Vector3(0f, -3.64f, 0f), new Vector2(2.78f, 0.72f), new Color(0.43f, 0.31f, 0.15f, 1f));
        CreateBasePad("Cannon Base Inner Ring", new Vector3(0f, -3.56f, 0f), new Vector2(2.34f, 0.56f), new Color(0.67f, 0.52f, 0.27f, 1f));
        CreateBasePad("Cannon Base Hub", new Vector3(0f, -3.48f, 0f), new Vector2(1.40f, 0.38f), new Color(0.28f, 0.36f, 0.25f, 1f));
    }

    private void CreateBasePad(string name, Vector3 position, Vector2 scale, Color color)
    {
        GameObject pad = CreateSprite(name, GetCannonBaseSprite(), position, scale, 1);
        pad.GetComponent<SpriteRenderer>().color = color;
    }

    private Sprite GetCannonBaseSprite()
    {
        if (cannonBaseSprite != null) return cannonBaseSprite;

        const int textureSize = 64;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[textureSize * textureSize];
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float normalizedX = (x + 0.5f) / textureSize * 2f - 1f;
                float normalizedY = (y + 0.5f) / textureSize * 2f - 1f;
                pixels[y * textureSize + x] = normalizedX * normalizedX + normalizedY * normalizedY <= 1f ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        cannonBaseSprite = Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
        return cannonBaseSprite;
    }

    private bool TryGetPointer(out Vector2 screenPosition, out bool pressed)
    {
        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
        {
            screenPosition = touchscreen.primaryTouch.position.ReadValue();
            pressed = touchscreen.primaryTouch.press.wasPressedThisFrame;
            return true;
        }
        Pointer pointer = Pointer.current;
        if (pointer == null)
        {
            screenPosition = default;
            pressed = false;
            return false;
        }
        screenPosition = pointer.position.ReadValue();
        pressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        return true;
    }

    private void AimAt(Vector2 screenPosition)
    {
        Vector3 worldPosition = gameplayCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
        Vector2 direction = (Vector2)(worldPosition - pistolPivot.position);
        if (direction.sqrMagnitude < 0.0001f) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        pistolPivot.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void Fire()
    {
        if (remainingBalls <= 0 || levelEnded) return;

        remainingBalls--;
        recoilTime = 0.11f;
        RefreshHud();
        CreateBall(muzzle.position, muzzle.up);

        if (remainingBalls <= 0 && targetsRemaining > 0) StartCoroutine(ShowFailureAfterDelay());
    }

    private void CreateBall(Vector2 position, Vector2 direction)
    {
        GameObject ball = new GameObject("Shot Ball", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D), typeof(WarfestBall));

        int shotLayer = LayerMask.NameToLayer("WarfestShot");
        if (shotLayer >= 0) ball.layer = shotLayer;
        ball.transform.SetParent(worldRoot, false);
        ball.transform.position = position;
        ball.transform.localScale = Vector3.one * ShotRadius;

        SpriteRenderer renderer = ball.GetComponent<SpriteRenderer>();
        renderer.sprite = GetCannonBaseSprite();
        renderer.color = new Color(1f, 0.82f, 0.22f, 1f);
        renderer.sortingOrder = 6;

        CircleCollider2D collider = ball.GetComponent<CircleCollider2D>();
        collider.radius = 0.5f;
        Rigidbody2D body = ball.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.mass = 0.22f;
        body.gravityScale = 0.35f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.linearVelocity = direction.normalized * ShotSpeed;
        ball.GetComponent<WarfestBall>().Initialize(direction, 5.8f + level.difficulty * 0.5f, ShotLifetime);
    }

    private IEnumerator ShowFailureAfterDelay()
    {
        yield return new WaitForSeconds(0.7f);
        if (!levelEnded && targetsRemaining > 0) ShowFailure();
    }

public void RegisterTargetBroken(WarfestTarget target)
    {
        if (levelEnded) return;
        targetsRemaining = Mathf.Max(0, targetsRemaining - 1);
        if (targetText != null) targetText.text = "TARGETS\n" + targetsRemaining.ToString("00");
        if (targetsRemaining == 0)
        {
            levelEnded = true;
            WarfestSession.CompleteLevel(level.number - 1);
        }
        else
        {
            Destroy(target.gameObject, 0.65f);
        }
    }

    private void UpdateRecoil()
    {
        if (pistolVisual == null) return;
        if (recoilTime <= 0f)
        {
            pistolVisual.localPosition = pistolRestLocalPosition;
            return;
        }
        recoilTime -= Time.deltaTime;
        float offset = Mathf.Sin(Mathf.Clamp01(recoilTime / 0.11f) * Mathf.PI) * 0.14f;
        pistolVisual.localPosition = pistolRestLocalPosition - Vector3.up * offset;
    }

private void BuildHud()
    {
        hudCanvas = CreateCanvas("Game HUD Canvas");
        RectTransform root = hudCanvas.transform as RectTransform;
        safeAreaRoot = CreateSafeAreaRoot(root);

        CreateImage(safeAreaRoot, "HUD Panel", LightPanel, new Vector2(0.5f, 0.91f), new Vector2(0.96f, 0.18f));
        Button menu = CreateButton(safeAreaRoot, "Menu", "MENU", Ink, new Vector2(0.10f, 0.92f), new Vector2(0.15f, 0.10f), 14);
        menu.onClick.AddListener(WarfestSession.ReturnToMenu);
        CreateText(safeAreaRoot, "Level Label", "LEVEL " + level.number.ToString("00"), 18, Ink, TextAnchor.MiddleLeft, new Vector2(0.32f, 0.945f), new Vector2(0.24f, 0.05f));
        CreateText(safeAreaRoot, "Level Subtitle", level.title.ToUpperInvariant(), 14, new Color(0.30f, 0.40f, 0.50f), TextAnchor.MiddleLeft, new Vector2(0.32f, 0.885f), new Vector2(0.24f, 0.045f));
        targetText = CreateText(safeAreaRoot, "Target Label", "TARGETS\n" + targetsRemaining.ToString("00"), 15, Ink, TextAnchor.MiddleCenter, new Vector2(0.64f, 0.925f), new Vector2(0.16f, 0.10f));
        ballsText = CreateText(safeAreaRoot, "Balls Label", "BALLS\n" + remainingBalls.ToString("00"), 15, Ink, TextAnchor.MiddleCenter, new Vector2(0.84f, 0.945f), new Vector2(0.18f, 0.055f));

        RectTransform slotsRoot = CreateRect(safeAreaRoot, "Ball Slots", new Vector2(0.84f, 0.875f), new Vector2(0.18f, 0.035f));
        HorizontalLayoutGroup layout = slotsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 4f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        int visibleSlotCount = Mathf.Min(ballCapacity, 10);
        for (int i = 0; i < visibleSlotCount; i++)
        {
            GameObject slot = new GameObject("Ball " + (i + 1), typeof(RectTransform), typeof(Image));
            slot.transform.SetParent(slotsRoot, false);
            slot.GetComponent<Image>().color = Ink;
            ballSlots.Add(slot.GetComponent<Image>());
        }

        if (level.number != 1)
        {
            CreateText(safeAreaRoot, "Instruction", "TAP A TARGET TO FIRE", 15, Ink, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.76f), new Vector2(0.58f, 0.05f));
        }
    }

private void RefreshHud()
    {
        if (ballsText != null) ballsText.text = "BALLS\n" + remainingBalls.ToString("00");
        for (int i = 0; i < ballSlots.Count; i++)
        {
            ballSlots[i].color = i < remainingBalls ? Ink : DisabledBall;
        }
    }

private void ShowFailure()
    {
        levelEnded = true;
        Transform parent = safeAreaRoot != null ? safeAreaRoot : (hudCanvas != null ? hudCanvas.transform : null);
        if (parent == null) return;

        CreateImage(parent, "Failure Panel", new Color(1f, 1f, 1f, 0.97f), new Vector2(0.5f, 0.5f), new Vector2(0.50f, 0.54f));
        CreateText(parent, "Failure Title", "OUT OF BALLS", 34, Ink, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.65f), new Vector2(0.42f, 0.10f));
        Button retry = CreateButton(parent, "Retry", "RETRY LEVEL", Ink, new Vector2(0.5f, 0.49f), new Vector2(0.34f, 0.12f), 15);
        retry.onClick.AddListener(() => WarfestSession.LoadLevel(level.number - 1));
        Button menu = CreateButton(parent, "Back to Menu", "MAIN MENU", new Color(0.35f, 0.48f, 0.58f), new Vector2(0.5f, 0.33f), new Vector2(0.34f, 0.12f), 15);
        menu.onClick.AddListener(WarfestSession.ReturnToMenu);
    }

private Canvas CreateCanvas(string name)
    {
        GameObject gameObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = gameObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudScaler = gameObject.GetComponent<CanvasScaler>();
        hudScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        hudScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        hudScaler.matchWidthOrHeight = 0.5f;
        ApplyCanvasScale();
        return canvas;
    }

    private void ApplyCanvasScale()
    {
        if (hudScaler == null) return;

        // Use iPhone point dimensions rather than Retina pixel dimensions so text and touch targets
        // retain a readable physical size on both device orientations.
        Vector2 referenceResolution = Screen.width >= Screen.height
            ? new Vector2(844f, 390f)
            : new Vector2(390f, 844f);
        if (referenceResolution == appliedReferenceResolution) return;

        appliedReferenceResolution = referenceResolution;
        hudScaler.referenceResolution = referenceResolution;
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

    private GameObject CreateSprite(string name, Sprite sprite, Vector3 position, Vector2 scale, int sortingOrder)
    {
        GameObject gameObject = new GameObject(name, typeof(SpriteRenderer));
        gameObject.transform.SetParent(worldRoot, false);
        gameObject.transform.localPosition = position;
        gameObject.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = Color.white;
        renderer.sortingOrder = sortingOrder;
        return gameObject;
    }

    private Image CreateImage(Transform parent, string name, Color color, Vector2 center, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        gameObject.transform.SetParent(parent, false);
        ApplyRect(gameObject.GetComponent<RectTransform>(), parent, center, size);
        Image image = gameObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private RectTransform CreateRect(Transform parent, string name, Vector2 center, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        ApplyRect(rect, parent, center, size);
        return rect;
    }

private Text CreateText(Transform parent, string name, string value, int size, Color color, TextAnchor alignment, Vector2 center, Vector2 dimensions)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        gameObject.transform.SetParent(parent, false);
        ApplyRect(gameObject.GetComponent<RectTransform>(), parent, center, dimensions);
        Text text = gameObject.GetComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = Mathf.Max(10, size - 12);
        text.resizeTextMaxSize = size;
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
        ApplyRect(gameObject.GetComponent<RectTransform>(), parent, center, size);
        Image image = gameObject.GetComponent<Image>();
        image.color = color;
        Button button = gameObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.16f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        CreateText(gameObject.transform, "Label", label, fontSize, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.one);
        return button;
    }

private static void ApplyRect(RectTransform rect, Transform parent, Vector2 center, Vector2 size)
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

public sealed class WarfestTarget : MonoBehaviour
{
    private WarfestGameController controller;
    private bool broken;

    public void Initialize(WarfestGameController owner)
    {
        controller = owner;
    }

    public void Break()
    {
        if (broken) return;
        broken = true;
        controller.RegisterTargetBroken(this);
    }
}
