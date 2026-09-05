using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Enforces a fixed iPhone aspect ratio (390 x 844) on wide-screen devices like iPads and tablets.
/// On iPad, the game is centered at the exact iPhone resolution and aspect ratio with pure black
/// pillarbox bars on the left and right, preventing any distortion, stretching, or viewport expansion.
/// </summary>
public static class WarfestDeviceViewport
{
    public const float TargetWidth = 390f;
    public const float TargetHeight = 844f;
    public const float TargetAspect = TargetWidth / TargetHeight; // ~0.4620853f

    public static Rect GetNormalizedViewport()
    {
        return CalculateViewport(Screen.width, Screen.height);
    }

    public static Rect CalculateViewport(float w, float h)
    {
        if (w <= 0f || h <= 0f) return new Rect(0f, 0f, 1f, 1f);

        float currentAspect = w / h;

        // Tolerance to prevent subpixel jitter on exact iPhone screens
        if (currentAspect > TargetAspect + 0.002f)
        {
            // Screen is wider than iPhone (e.g. iPad 3:4, tablets, desktop) -> Pillarbox (black bars on left/right)
            float insetWidth = TargetAspect / currentAspect;
            float insetX = (1f - insetWidth) * 0.5f;
            return new Rect(insetX, 0f, insetWidth, 1f);
        }
        else if (currentAspect < TargetAspect - 0.002f)
        {
            // Screen is taller than iPhone -> Letterbox (black bars on top/bottom)
            float insetHeight = currentAspect / TargetAspect;
            float insetY = (1f - insetHeight) * 0.5f;
            return new Rect(0f, insetY, 1f, insetHeight);
        }
        else
        {
            // Standard iPhone display -> Full screen
            return new Rect(0f, 0f, 1f, 1f);
        }
    }

    public static Rect GetPixelViewport()
    {
        Rect norm = GetNormalizedViewport();
        return new Rect(
            norm.x * Screen.width,
            norm.y * Screen.height,
            norm.width * Screen.width,
            norm.height * Screen.height
        );
    }

    public static bool IsPointerInViewport(Vector2 screenPosition)
    {
        Rect pixelRect = GetPixelViewport();
        return pixelRect.Contains(screenPosition);
    }
}

[ExecuteAlways]
public sealed class WarfestViewportManager : MonoBehaviour
{
    private static WarfestViewportManager instance;
    public static WarfestViewportManager Instance => instance;

    private Camera backgroundBlackCamera;
    private Canvas blackBarCanvas;
    private RectTransform leftBarRect;
    private RectTransform rightBarRect;
    private RectTransform topBarRect;
    private RectTransform bottomBarRect;

    private int lastScreenWidth = -1;
    private int lastScreenHeight = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void EnforcePortraitOrientation()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void EnsureManagerExists()
    {
        EnforcePortraitOrientation();
        if (instance != null) return;
        var existing = FindAnyObjectByType<WarfestViewportManager>();
        if (existing != null)
        {
            instance = existing;
            return;
        }

        GameObject go = new GameObject("Warfest Viewport Manager");
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(go);
        }
#if UNITY_EDITOR
        else
        {
            go.hideFlags = HideFlags.DontSave;
        }
#endif
        instance = go.AddComponent<WarfestViewportManager>();
    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    private static void EditorInitialize()
    {
        EditorApplication.delayCall += () =>
        {
            EnsureManagerExists();
        };
    }
#endif

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this && Application.isPlaying)
        {
            Destroy(gameObject);
            return;
        }

        EnforcePortraitOrientation();
        EnsureComponents();
        ApplyViewport();
    }

    private void OnEnable()
    {
        EnsureComponents();
        ApplyViewport();
#if UNITY_EDITOR
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= OnEditorUpdate;
#endif
    }

#if UNITY_EDITOR
    private void OnEditorUpdate()
    {
        if (this == null) return;
        CheckScreenChange();
    }
#endif

    private void Update()
    {
        CheckScreenChange();
    }

    private void CheckScreenChange()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            ApplyViewport();
        }
    }

    public void ApplyViewport()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        Rect viewport = WarfestDeviceViewport.GetNormalizedViewport();

        // 1. Update Main Camera rect
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.rect = viewport;
        }

        // 2. Ensure Background Black Camera clears everything behind
        if (backgroundBlackCamera != null)
        {
            bool needsPillarbox = viewport.xMin > 0.001f || viewport.yMin > 0.001f;
            backgroundBlackCamera.enabled = needsPillarbox;
        }

        // 3. Update Black Bar Overlay RectTransforms
        UpdateBlackBars(viewport);
    }

    private void EnsureComponents()
    {
        // 1. Background Black Camera (Depth = -100, Solid Black, Culling Mask = 0)
        if (backgroundBlackCamera == null)
        {
            Transform existingCam = transform.Find("Pillarbox Black Camera");
            if (existingCam != null) backgroundBlackCamera = existingCam.GetComponent<Camera>();
            if (backgroundBlackCamera == null)
            {
                GameObject camObj = new GameObject("Pillarbox Black Camera", typeof(Camera));
                camObj.transform.SetParent(transform, false);
                backgroundBlackCamera = camObj.GetComponent<Camera>();
                backgroundBlackCamera.clearFlags = CameraClearFlags.SolidColor;
                backgroundBlackCamera.backgroundColor = Color.black;
                backgroundBlackCamera.cullingMask = 0;
                backgroundBlackCamera.depth = -100;
                backgroundBlackCamera.rect = new Rect(0f, 0f, 1f, 1f);
            }
        }

        // 2. Black Bar Overlay Canvas (Sorting Order = 32767)
        if (blackBarCanvas == null)
        {
            Transform existingCanvas = transform.Find("Pillarbox Overlay Canvas");
            if (existingCanvas != null) blackBarCanvas = existingCanvas.GetComponent<Canvas>();
            if (blackBarCanvas == null)
            {
                GameObject canvasObj = new GameObject("Pillarbox Overlay Canvas", typeof(Canvas), typeof(GraphicRaycaster));
                canvasObj.transform.SetParent(transform, false);
                blackBarCanvas = canvasObj.GetComponent<Canvas>();
                blackBarCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                blackBarCanvas.sortingOrder = 32767;

                leftBarRect = CreateBar(canvasObj.transform, "Left Pillarbox Bar");
                rightBarRect = CreateBar(canvasObj.transform, "Right Pillarbox Bar");
                topBarRect = CreateBar(canvasObj.transform, "Top Letterbox Bar");
                bottomBarRect = CreateBar(canvasObj.transform, "Bottom Letterbox Bar");
            }
            else
            {
                leftBarRect = blackBarCanvas.transform.Find("Left Pillarbox Bar") as RectTransform;
                rightBarRect = blackBarCanvas.transform.Find("Right Pillarbox Bar") as RectTransform;
                topBarRect = blackBarCanvas.transform.Find("Top Letterbox Bar") as RectTransform;
                bottomBarRect = blackBarCanvas.transform.Find("Bottom Letterbox Bar") as RectTransform;
            }
        }
    }

    private RectTransform CreateBar(Transform parent, string name)
    {
        GameObject bar = new GameObject(name, typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(parent, false);
        RectTransform rt = bar.GetComponent<RectTransform>();
        Image img = bar.GetComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = true;
        return rt;
    }

    private void UpdateBlackBars(Rect viewport)
    {
        if (leftBarRect == null || rightBarRect == null || topBarRect == null || bottomBarRect == null) return;

        bool hasPillarbox = viewport.xMin > 0.001f;
        bool hasLetterbox = viewport.yMin > 0.001f;

        // Left Bar
        leftBarRect.gameObject.SetActive(hasPillarbox);
        if (hasPillarbox)
        {
            leftBarRect.anchorMin = new Vector2(0f, 0f);
            leftBarRect.anchorMax = new Vector2(viewport.xMin, 1f);
            leftBarRect.offsetMin = Vector2.zero;
            leftBarRect.offsetMax = Vector2.zero;
        }

        // Right Bar
        rightBarRect.gameObject.SetActive(hasPillarbox);
        if (hasPillarbox)
        {
            rightBarRect.anchorMin = new Vector2(viewport.xMax, 0f);
            rightBarRect.anchorMax = new Vector2(1f, 1f);
            rightBarRect.offsetMin = Vector2.zero;
            rightBarRect.offsetMax = Vector2.zero;
        }

        // Top Bar
        topBarRect.gameObject.SetActive(hasLetterbox);
        if (hasLetterbox)
        {
            topBarRect.anchorMin = new Vector2(0f, viewport.yMax);
            topBarRect.anchorMax = new Vector2(1f, 1f);
            topBarRect.offsetMin = Vector2.zero;
            topBarRect.offsetMax = Vector2.zero;
        }

        // Bottom Bar
        bottomBarRect.gameObject.SetActive(hasLetterbox);
        if (hasLetterbox)
        {
            bottomBarRect.anchorMin = new Vector2(0f, 0f);
            bottomBarRect.anchorMax = new Vector2(1f, viewport.yMin);
            bottomBarRect.offsetMin = Vector2.zero;
            bottomBarRect.offsetMax = Vector2.zero;
        }
    }
}
