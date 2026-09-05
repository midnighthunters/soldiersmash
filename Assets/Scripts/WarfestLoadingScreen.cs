using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class WarfestLoadingScreen : MonoBehaviour
{
    private const string CanvasName = "Warfest Loading Canvas";
    private static WarfestLoadingScreen instance;

    private CanvasGroup canvasGroup;
    private RectTransform characterRect;
    private Image progressFill;
    private Text progressText;
    private Text tipText;
    private RectTransform shimmerRect;
    private float displayTimer;
    private const float MinDisplayDuration = 1.4f;
    private const float FadeDuration = 0.35f;

    private static readonly string[] TacticalTips =
    {
        "TIP: Strike lower supports to topple the entire fortress!",
        "TIP: Cannon balls bounce off surfaces — angle your shots!",
        "TIP: Explosive bombs can trigger devastating chain reactions!",
        "TIP: Eliminate all targets on the table to achieve victory!"
    };

    public static void ShowAndLoad(int zeroBasedLevel)
    {
        if (instance != null) return;

        if (WarfestSession.Lives <= 0) return;

        WarfestSession.SelectLevel(zeroBasedLevel);

        GameObject canvasObj = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        DontDestroyOnLoad(canvasObj);

        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9998;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1.0f;
        scaler.referenceResolution = new Vector2(WarfestDeviceViewport.TargetWidth, WarfestDeviceViewport.TargetHeight);

        instance = canvasObj.AddComponent<WarfestLoadingScreen>();
        instance.BuildUI(canvasObj.transform as RectTransform, zeroBasedLevel);
        instance.StartCoroutine(instance.LoadSceneRoutine());
    }

    private void BuildUI(RectTransform root, int zeroBasedLevel)
    {
        canvasGroup = GetComponent<CanvasGroup>();
        Font font = WarfestMiscArt.GetCartoonFont();

        Rect viewport = WarfestDeviceViewport.GetNormalizedViewport();
        GameObject frameObj = new GameObject("iPhone Frame", typeof(RectTransform));
        frameObj.transform.SetParent(root, false);
        RectTransform frameRect = frameObj.GetComponent<RectTransform>();
        frameRect.anchorMin = new Vector2(viewport.xMin, viewport.yMin);
        frameRect.anchorMax = new Vector2(viewport.xMax, viewport.yMax);
        frameRect.offsetMin = Vector2.zero;
        frameRect.offsetMax = Vector2.zero;

        // 1. Background Pattern Image
        Sprite bgSprite = WarfestMiscArt.GetLoadingBackground();
        GameObject bgObj = new GameObject("Loading Background", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
        bgObj.transform.SetParent(frameRect, false);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.color = Color.white;
        bgImg.raycastTarget = false;

        if (bgSprite != null)
        {
            bgImg.sprite = bgSprite;
            bgImg.preserveAspect = true;
            AspectRatioFitter fitter = bgObj.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = (float)bgSprite.rect.width / bgSprite.rect.height;
        }

        // 2. Soft Edge Vignette / Dim for readability
        GameObject vignetteObj = new GameObject("Vignette", typeof(RectTransform), typeof(Image));
        vignetteObj.transform.SetParent(frameRect, false);
        RectTransform vigRect = vignetteObj.GetComponent<RectTransform>();
        vigRect.anchorMin = Vector2.zero;
        vigRect.anchorMax = Vector2.one;
        vigRect.offsetMin = Vector2.zero;
        vigRect.offsetMax = Vector2.zero;
        Image vigImg = vignetteObj.GetComponent<Image>();
        vigImg.color = new Color(0.04f, 0.08f, 0.16f, 0.25f);
        vigImg.raycastTarget = false;

        // 3. Top Mission Header Badge
        GameObject headerObj = new GameObject("Mission Badge", typeof(RectTransform), typeof(Text));
        headerObj.transform.SetParent(frameRect, false);
        RectTransform headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0.5f, 0.82f);
        headerRect.anchorMax = new Vector2(0.5f, 0.82f);
        headerRect.sizeDelta = new Vector2(340f, 40f);
        headerRect.anchoredPosition = Vector2.zero;
        Text headerText = headerObj.GetComponent<Text>();
        headerText.font = font;
        headerText.text = "LEVEL " + (zeroBasedLevel + 1).ToString("00");
        headerText.fontSize = 28;
        headerText.fontStyle = FontStyle.Bold;
        headerText.color = new Color(1f, 0.98f, 0.88f, 1f);
        headerText.alignment = TextAnchor.MiddleCenter;
        headerText.raycastTarget = false;
        Outline headerOutline = headerObj.AddComponent<Outline>();
        headerOutline.effectColor = new Color(0.08f, 0.16f, 0.24f, 1f);
        headerOutline.effectDistance = new Vector2(2f, -2f);

        // Mission Sub-heading
        string missionName = WarfestLevelCatalog.CampaignName(zeroBasedLevel);
        GameObject subObj = new GameObject("Mission Name", typeof(RectTransform), typeof(Text));
        subObj.transform.SetParent(frameRect, false);
        RectTransform subRect = subObj.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0.5f, 0.77f);
        subRect.anchorMax = new Vector2(0.5f, 0.77f);
        subRect.sizeDelta = new Vector2(320f, 24f);
        subRect.anchoredPosition = Vector2.zero;
        Text subText = subObj.GetComponent<Text>();
        subText.font = font;
        subText.text = missionName.ToUpper();
        subText.fontSize = 14;
        subText.color = new Color(0.75f, 0.90f, 1f, 0.9f);
        subText.alignment = TextAnchor.MiddleCenter;
        subText.raycastTarget = false;

        // 4. Character Sprite (Randomly chosen load sprite: load0, load2, load3, load4)
        Sprite loadSprite = WarfestMiscArt.GetRandomLoadingSprite();
        GameObject charObj = new GameObject("Character Image", typeof(RectTransform), typeof(Image));
        charObj.transform.SetParent(frameRect, false);
        characterRect = charObj.GetComponent<RectTransform>();
        characterRect.anchorMin = new Vector2(0.5f, 0.54f);
        characterRect.anchorMax = new Vector2(0.5f, 0.54f);
        characterRect.sizeDelta = new Vector2(290f, 290f);
        characterRect.anchoredPosition = Vector2.zero;

        Image charImg = charObj.GetComponent<Image>();
        charImg.color = Color.white;
        charImg.preserveAspect = true;
        charImg.raycastTarget = false;
        if (loadSprite != null)
        {
            charImg.sprite = loadSprite;
        }

        // 5. Progress Text (Positioned clearly above loading bar)
        GameObject progObj = new GameObject("Progress Text", typeof(RectTransform), typeof(Text));
        progObj.transform.SetParent(frameRect, false);
        RectTransform progRect = progObj.GetComponent<RectTransform>();
        progRect.anchorMin = new Vector2(0.5f, 0.332f);
        progRect.anchorMax = new Vector2(0.5f, 0.332f);
        progRect.sizeDelta = new Vector2(320f, 26f);
        progRect.anchoredPosition = Vector2.zero;

        progressText = progObj.GetComponent<Text>();
        progressText.font = font;
        progressText.text = "PREPARING MISSION... 0%";
        progressText.fontSize = 16;
        progressText.fontStyle = FontStyle.Bold;
        progressText.color = Color.white;
        progressText.alignment = TextAnchor.MiddleCenter;
        progressText.raycastTarget = false;
        Outline progOutline = progObj.AddComponent<Outline>();
        progOutline.effectColor = new Color(0.06f, 0.12f, 0.18f, 0.95f);
        progOutline.effectDistance = new Vector2(1.5f, -1.5f);

        // Hypercasual 3D Capsule Loading Bar Track
        GameObject trackObj = new GameObject("Loading Bar Track", typeof(RectTransform), typeof(Image));
        trackObj.transform.SetParent(frameRect, false);
        RectTransform trackRect = trackObj.GetComponent<RectTransform>();
        trackRect.anchorMin = new Vector2(0.5f, 0.285f);
        trackRect.anchorMax = new Vector2(0.5f, 0.285f);
        trackRect.sizeDelta = new Vector2(320f, 34f);
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

        // Vibrant 3D Candy Fill Bar (Candy Emerald Green)
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
        progressFill.color = new Color(0.08f, 0.88f, 0.42f, 1f); // Vibrant Candy Emerald
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

        // 6. Tactical Tip Text
        GameObject tipObj = new GameObject("Tactical Tip", typeof(RectTransform), typeof(Text));
        tipObj.transform.SetParent(frameRect, false);
        RectTransform tipRect = tipObj.GetComponent<RectTransform>();
        tipRect.anchorMin = new Vector2(0.5f, 0.20f);
        tipRect.anchorMax = new Vector2(0.5f, 0.20f);
        tipRect.sizeDelta = new Vector2(320f, 44f);
        tipRect.anchoredPosition = Vector2.zero;

        tipText = tipObj.GetComponent<Text>();
        tipText.font = font;
        tipText.text = TacticalTips[Random.Range(0, TacticalTips.Length)];
        tipText.fontSize = 12;
        tipText.color = new Color(0.90f, 0.95f, 1f, 0.85f);
        tipText.alignment = TextAnchor.MiddleCenter;
        tipText.horizontalOverflow = HorizontalWrapMode.Wrap;
        tipText.verticalOverflow = VerticalWrapMode.Overflow;
        tipText.raycastTarget = false;
    }

    private void Update()
    {
        // Character floating bob & breathing idle
        if (characterRect != null)
        {
            float bob = Mathf.Sin(Time.unscaledTime * 3.2f) * 8f;
            characterRect.anchoredPosition = new Vector2(0f, bob);

            float breath = 1f + Mathf.Sin(Time.unscaledTime * 2.2f) * 0.02f;
            characterRect.localScale = new Vector3(breath, breath, 1f);
        }

        // Shimmer beam continuous sweep
        if (shimmerRect != null)
        {
            float shimPos = Mathf.Repeat(Time.unscaledTime * 1.6f, 1.5f) - 0.25f;
            shimmerRect.anchorMin = new Vector2(shimPos - 0.25f, 0f);
            shimmerRect.anchorMax = new Vector2(shimPos, 1f);
        }

        // Subtly pulse tip text
        if (tipText != null)
        {
            float alpha = 0.75f + Mathf.Sin(Time.unscaledTime * 3f) * 0.2f;
            Color c = tipText.color;
            tipText.color = new Color(c.r, c.g, c.b, alpha);
        }
    }

    private IEnumerator LoadSceneRoutine()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync("Game");
        op.allowSceneActivation = false;

        float visualProgress = 0f;
        displayTimer = 0f;

        while (displayTimer < MinDisplayDuration || visualProgress < 0.99f)
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            displayTimer += dt;

            // Target progress: op.progress reaches 0.9 when loaded
            float targetProgress = Mathf.Clamp01(op.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(displayTimer / MinDisplayDuration);
            float target = Mathf.Min(targetProgress, timeProgress);

            visualProgress = Mathf.MoveTowards(visualProgress, target, dt * 1.5f);

            if (progressFill != null)
            {
                progressFill.rectTransform.anchorMax = new Vector2(visualProgress, 1f);
            }

            if (progressText != null)
            {
                int pct = Mathf.RoundToInt(visualProgress * 100f);
                progressText.text = "DEPLOYING... " + pct + "%";
            }

            yield return null;
        }

        if (progressFill != null) progressFill.rectTransform.anchorMax = Vector2.one;
        if (progressText != null) progressText.text = "DEPLOYING... 100%";

        yield return new WaitForSecondsRealtime(0.1f);

        // Activate scene
        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            yield return null;
        }

        // Smoothly fade out the loading overlay in the new scene
        float fadeTimer = 0f;
        while (fadeTimer < FadeDuration)
        {
            fadeTimer += Time.unscaledDeltaTime;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f - (fadeTimer / FadeDuration);
            }
            yield return null;
        }

        instance = null;
        Destroy(gameObject);
    }
}
