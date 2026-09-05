using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class WarfestSplashScreen : MonoBehaviour
{
    private const string CanvasName = "Warfest Splash Canvas";
    private CanvasGroup canvasGroup;
    private RectTransform imageRect;
    private Image progressFill;
    private Text statusText;
    private RectTransform shimmerRect;
    private Action onFinished;
    private float elapsedTime;
    private bool isFinishing;
    private const float DisplayDuration = 2.2f;
    private const float FadeDuration = 0.5f;
    private const float MinSkipTime = 0.4f;

    public static WarfestSplashScreen Show(Transform parent = null, Action onComplete = null)
    {
        // The splash screen must strictly only appear once when the app is first opened
        if (WarfestSession.HasShownSplash)
        {
            onComplete?.Invoke();
            return null;
        }
        WarfestSession.HasShownSplash = true;

        // Avoid duplicate splash screens
        WarfestSplashScreen existing = FindAnyObjectByType<WarfestSplashScreen>();
        if (existing != null) return existing;

        GameObject canvasObj = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        if (parent != null)
        {
            canvasObj.transform.SetParent(parent, false);
        }

        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1.0f;
        scaler.referenceResolution = Screen.width >= Screen.height ? new Vector2(844f, 390f) : new Vector2(390f, 844f);

        WarfestSplashScreen splash = canvasObj.AddComponent<WarfestSplashScreen>();
        splash.onFinished = onComplete;
        splash.BuildUI(canvasObj.transform as RectTransform);
        return splash;
    }

    private void BuildUI(RectTransform root)
    {
        canvasGroup = GetComponent<CanvasGroup>();

        Rect viewport = WarfestDeviceViewport.GetNormalizedViewport();
        GameObject frameObj = new GameObject("iPhone Frame", typeof(RectTransform));
        frameObj.transform.SetParent(root, false);
        RectTransform frameRect = frameObj.GetComponent<RectTransform>();
        frameRect.anchorMin = new Vector2(viewport.xMin, viewport.yMin);
        frameRect.anchorMax = new Vector2(viewport.xMax, viewport.yMax);
        frameRect.offsetMin = Vector2.zero;
        frameRect.offsetMax = Vector2.zero;

        // 1. Splash Background Image
        Sprite splashSprite = WarfestMiscArt.GetRandomSplashScreen();
        GameObject imgObj = new GameObject("Splash Image", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
        imgObj.transform.SetParent(frameRect, false);
        imageRect = imgObj.GetComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        Image img = imgObj.GetComponent<Image>();
        img.color = Color.white;
        img.raycastTarget = false;

        if (splashSprite != null)
        {
            img.sprite = splashSprite;
            img.preserveAspect = true;
            AspectRatioFitter fitter = imgObj.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = (float)splashSprite.rect.width / splashSprite.rect.height;
        }

        // 2. Subtle Bottom Vignette for UI readability
        GameObject vignetteObj = new GameObject("Bottom Vignette", typeof(RectTransform), typeof(Image));
        vignetteObj.transform.SetParent(frameRect, false);
        RectTransform vigRect = vignetteObj.GetComponent<RectTransform>();
        vigRect.anchorMin = new Vector2(0f, 0f);
        vigRect.anchorMax = new Vector2(1f, 0.40f);
        vigRect.offsetMin = Vector2.zero;
        vigRect.offsetMax = Vector2.zero;
        Image vigImg = vignetteObj.GetComponent<Image>();
        vigImg.sprite = WarfestMiscArt.GetVerticalGradientSprite();
        vigImg.color = new Color(0.02f, 0.05f, 0.10f, 0.85f);
        vigImg.raycastTarget = false;

        Font font = WarfestMiscArt.GetCartoonFont();

        // Brand Subtitle Text
        GameObject brandObj = new GameObject("Brand Label", typeof(RectTransform), typeof(Text));
        brandObj.transform.SetParent(frameRect, false);
        RectTransform brandRect = brandObj.GetComponent<RectTransform>();
        brandRect.anchorMin = new Vector2(0.5f, 0.165f);
        brandRect.anchorMax = new Vector2(0.5f, 0.165f);
        brandRect.sizeDelta = new Vector2(320f, 32f);
        brandRect.anchoredPosition = Vector2.zero;
        Text brandText = brandObj.GetComponent<Text>();
        brandText.font = font;
        brandText.text = "SOLDIER SMASH";
        brandText.fontSize = 24;
        brandText.fontStyle = FontStyle.Bold;
        brandText.color = new Color(1f, 0.96f, 0.85f, 1f);
        brandText.alignment = TextAnchor.MiddleCenter;
        brandText.raycastTarget = false;
        Outline brandOutline = brandObj.AddComponent<Outline>();
        brandOutline.effectColor = new Color(0.08f, 0.14f, 0.22f, 1f);
        brandOutline.effectDistance = new Vector2(1.5f, -1.5f);

        // Status Label Text (Positioned clearly above loading bar)
        GameObject statusObj = new GameObject("Status Label", typeof(RectTransform), typeof(Text));
        statusObj.transform.SetParent(frameRect, false);
        RectTransform statusRect = statusObj.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.5f, 0.122f);
        statusRect.anchorMax = new Vector2(0.5f, 0.122f);
        statusRect.sizeDelta = new Vector2(320f, 24f);
        statusRect.anchoredPosition = Vector2.zero;
        statusText = statusObj.GetComponent<Text>();
        statusText.font = font;
        statusText.text = "LOADING... 0%";
        statusText.fontSize = 15;
        statusText.fontStyle = FontStyle.Bold;
        statusText.color = Color.white;
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.raycastTarget = false;
        Outline statusOutline = statusObj.AddComponent<Outline>();
        statusOutline.effectColor = new Color(0.08f, 0.12f, 0.18f, 0.95f);
        statusOutline.effectDistance = new Vector2(1.5f, -1.5f);

        // 3. Hypercasual 3D Capsule Loading Track
        GameObject trackObj = new GameObject("Loading Track", typeof(RectTransform), typeof(Image));
        trackObj.transform.SetParent(frameRect, false);
        RectTransform trackRect = trackObj.GetComponent<RectTransform>();
        trackRect.anchorMin = new Vector2(0.5f, 0.085f);
        trackRect.anchorMax = new Vector2(0.5f, 0.085f);
        trackRect.sizeDelta = new Vector2(310f, 32f);
        trackRect.anchoredPosition = Vector2.zero;
        Image trackImg = trackObj.GetComponent<Image>();
        trackImg.sprite = WarfestMiscArt.GetHypercasualTrackSprite();
        trackImg.type = Image.Type.Sliced;
        trackImg.color = Color.white;
        trackImg.raycastTarget = false;

        // Inner Slot with RectMask2D (4px inset inside the beveled rim)
        GameObject slotObj = new GameObject("Slot", typeof(RectTransform), typeof(RectMask2D));
        slotObj.transform.SetParent(trackRect, false);
        RectTransform slotRect = slotObj.GetComponent<RectTransform>();
        slotRect.anchorMin = Vector2.zero;
        slotRect.anchorMax = Vector2.one;
        slotRect.offsetMin = new Vector2(4f, 4f);
        slotRect.offsetMax = new Vector2(-4f, -4f);

        // Vibrant 3D Candy Fill Bar (Vibrant Gold)
        GameObject fillObj = new GameObject("Progress Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(slotRect, false);
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        progressFill = fillObj.GetComponent<Image>();
        progressFill.sprite = WarfestMiscArt.GetHypercasualFillSprite();
        progressFill.type = Image.Type.Sliced;
        progressFill.color = new Color(1f, 0.76f, 0.05f, 1f); // Vibrant Candy Gold
        progressFill.raycastTarget = false;

        // Liquid Shimmer Beam
        GameObject shimmerObj = new GameObject("Shimmer Beam", typeof(RectTransform), typeof(Image));
        shimmerObj.transform.SetParent(slotRect, false);
        shimmerRect = shimmerObj.GetComponent<RectTransform>();
        shimmerRect.anchorMin = new Vector2(-0.3f, 0f);
        shimmerRect.anchorMax = new Vector2(0f, 1f);
        shimmerRect.offsetMin = Vector2.zero;
        shimmerRect.offsetMax = Vector2.zero;
        Image shimmerImg = shimmerObj.GetComponent<Image>();
        shimmerImg.sprite = WarfestMiscArt.GetShimmerSprite();
        shimmerImg.color = new Color(1f, 1f, 1f, 0.45f);
        shimmerImg.raycastTarget = false;

        // Interactive Fullscreen Button for Tap-to-Skip
        GameObject btnObj = new GameObject("Skip Trigger", typeof(RectTransform), typeof(Button), typeof(Image));
        btnObj.transform.SetParent(frameRect, false);
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = Vector2.zero;
        btnRect.anchorMax = Vector2.one;
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;
        Image btnImg = btnObj.GetComponent<Image>();
        btnImg.color = Color.clear;
        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(OnTapSkip);
    }

    private void Update()
    {
        if (isFinishing) return;

        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
        elapsedTime += dt;

        // Cinematic subtle Ken Burns slow zoom
        if (imageRect != null)
        {
            float zoom = Mathf.Lerp(1.0f, 1.05f, elapsedTime / DisplayDuration);
            imageRect.localScale = new Vector3(zoom, zoom, 1f);
        }

        // Progress bar smooth fill
        float progress = Mathf.Clamp01(elapsedTime / (DisplayDuration * 0.75f));
        if (progressFill != null)
        {
            progressFill.rectTransform.anchorMax = new Vector2(progress, 1f);
        }

        // Shimmer beam continuous sweep
        if (shimmerRect != null)
        {
            float shimPos = Mathf.Repeat(elapsedTime * 1.6f, 1.5f) - 0.25f;
            shimmerRect.anchorMin = new Vector2(shimPos - 0.25f, 0f);
            shimmerRect.anchorMax = new Vector2(shimPos, 1f);
        }

        if (statusText != null)
        {
            if (progress >= 1f)
            {
                statusText.text = "READY!";
            }
            else
            {
                int pct = Mathf.RoundToInt(progress * 100f);
                statusText.text = string.Format("LOADING... {0}%", pct);
            }
        }

        if (elapsedTime >= DisplayDuration)
        {
            StartFinish();
        }
    }

    private void OnTapSkip()
    {
        if (elapsedTime >= MinSkipTime && !isFinishing)
        {
            StartFinish();
        }
    }

    private void StartFinish()
    {
        if (isFinishing) return;
        isFinishing = true;
        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        float timer = 0f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

        while (timer < FadeDuration)
        {
            timer += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float t = timer / FadeDuration;
            // Smooth ease out
            float alpha = Mathf.Lerp(startAlpha, 0f, t * t);
            if (canvasGroup != null) canvasGroup.alpha = alpha;
            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 0f;

        onFinished?.Invoke();
        Destroy(gameObject);
    }
}
