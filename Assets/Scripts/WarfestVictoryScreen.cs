using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class WarfestVictoryScreen : MonoBehaviour
{
    private const string CanvasName = "Warfest Victory Canvas";

    private CanvasGroup canvasGroup;
    private RectTransform victoryRect;
    private RectTransform logoRect;
    private Image sunburstGlow;
    private Action onContinue;
    private AudioSource audioSource;
    private float animationTime;
    private bool isTransitioningOut;

    private const float VictoryStartX = -520f;
    private const float LogoStartX = 520f;
    private const float VictoryTargetY = 70f;
    private const float LogoTargetY = -120f;
    private const float AutoFinishDuration = 2.8f;
    private const float MinSkipTime = 0.5f;

    public static WarfestVictoryScreen Show(int zeroBasedLevel, Transform parent = null, Action onComplete = null)
    {
        WarfestVictoryScreen existing = FindAnyObjectByType<WarfestVictoryScreen>();
        if (existing != null) return existing;

        GameObject canvasObj = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(AudioSource));
        if (parent != null)
        {
            canvasObj.transform.SetParent(parent, false);
        }

        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9990;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1.0f;
        scaler.referenceResolution = new Vector2(WarfestDeviceViewport.TargetWidth, WarfestDeviceViewport.TargetHeight);

        WarfestAudio.StopGameplayAudio();
        WarfestVictoryScreen victory = canvasObj.AddComponent<WarfestVictoryScreen>();
        victory.onContinue = onComplete ?? (() => WarfestSession.CompleteLevel(zeroBasedLevel));
        victory.BuildUI(canvasObj.transform as RectTransform);
        victory.PlayVictoryMusic();
        return victory;
    }

    private void BuildUI(RectTransform root)
    {
        canvasGroup = GetComponent<CanvasGroup>();
        audioSource = GetComponent<AudioSource>();

        Rect viewport = WarfestDeviceViewport.GetNormalizedViewport();
        GameObject frameObj = new GameObject("iPhone Frame", typeof(RectTransform));
        frameObj.transform.SetParent(root, false);
        RectTransform frameRect = frameObj.GetComponent<RectTransform>();
        frameRect.anchorMin = new Vector2(viewport.xMin, viewport.yMin);
        frameRect.anchorMax = new Vector2(viewport.xMax, viewport.yMax);
        frameRect.offsetMin = Vector2.zero;
        frameRect.offsetMax = Vector2.zero;

        // 1. Dark Backdrop Overlay & Tap Trigger
        GameObject dimObj = new GameObject("Dark Scrim", typeof(RectTransform), typeof(Image), typeof(Button));
        dimObj.transform.SetParent(frameRect, false);
        RectTransform dimRect = dimObj.GetComponent<RectTransform>();
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = Vector2.zero;
        dimRect.offsetMax = Vector2.zero;
        Image dimImg = dimObj.GetComponent<Image>();
        dimImg.color = new Color(0.02f, 0.04f, 0.08f, 0.82f);
        Button tapBtn = dimObj.GetComponent<Button>();
        tapBtn.onClick.AddListener(OnTapContinue);

        // 2. Confetti Particle Shower
        WarfestConfetti.Create(frameRect);

        // 3. Hero Radial Glow behind Victory character
        GameObject glowObj = new GameObject("Sunburst Glow", typeof(RectTransform), typeof(Image));
        glowObj.transform.SetParent(frameRect, false);
        RectTransform glowRect = glowObj.GetComponent<RectTransform>();
        glowRect.anchorMin = new Vector2(0.5f, 0.5f);
        glowRect.anchorMax = new Vector2(0.5f, 0.5f);
        glowRect.sizeDelta = new Vector2(380f, 380f);
        glowRect.anchoredPosition = new Vector2(0f, VictoryTargetY);
        sunburstGlow = glowObj.GetComponent<Image>();
        sunburstGlow.sprite = WarfestMiscArt.GetRadialGlowSprite();
        sunburstGlow.color = Color.white;
        sunburstGlow.raycastTarget = false;

        // 4. Victory Tank Character (victory1.png) - Slides in from LEFT
        Sprite victorySprite = WarfestMiscArt.GetVictorySprite();
        GameObject vicObj = new GameObject("Victory Character", typeof(RectTransform), typeof(Image));
        vicObj.transform.SetParent(frameRect, false);
        victoryRect = vicObj.GetComponent<RectTransform>();
        victoryRect.anchorMin = new Vector2(0.5f, 0.5f);
        victoryRect.anchorMax = new Vector2(0.5f, 0.5f);
        victoryRect.sizeDelta = new Vector2(270f, 270f);
        victoryRect.anchoredPosition = new Vector2(VictoryStartX, VictoryTargetY);

        Image vicImg = vicObj.GetComponent<Image>();
        vicImg.color = Color.white;
        vicImg.preserveAspect = true;
        vicImg.raycastTarget = false;
        if (victorySprite != null) vicImg.sprite = victorySprite;

        // 5. Game Logo (logo.png) - Slides in from RIGHT, below victory character
        Sprite logoSprite = WarfestMiscArt.GetLogoSprite();
        GameObject logoObj = new GameObject("Game Logo", typeof(RectTransform), typeof(Image));
        logoObj.transform.SetParent(frameRect, false);
        logoRect = logoObj.GetComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.5f, 0.5f);
        logoRect.anchorMax = new Vector2(0.5f, 0.5f);
        logoRect.sizeDelta = new Vector2(300f, 185f);
        logoRect.anchoredPosition = new Vector2(LogoStartX, LogoTargetY);

        Image logoImg = logoObj.GetComponent<Image>();
        logoImg.color = Color.white;
        logoImg.preserveAspect = true;
        logoImg.raycastTarget = false;
        if (logoSprite != null) logoImg.sprite = logoSprite;
    }

    private void Update()
    {
        if (isTransitioningOut) return;

        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
        animationTime += dt;

        // Animate Victory Tank (Slides in from LEFT, t: 0.10s to 0.65s)
        if (victoryRect != null)
        {
            float t = Mathf.Clamp01((animationTime - 0.10f) / 0.55f);
            if (t > 0f)
            {
                float ease = EaseOutBack(t);
                float x = Mathf.Lerp(VictoryStartX, 0f, ease);

                // Add gentle joyful celebration bobbing once settled
                float idleBob = t >= 1f ? Mathf.Sin((animationTime - 0.65f) * 4f) * 6f : 0f;
                victoryRect.anchoredPosition = new Vector2(x, VictoryTargetY + idleBob);

                // Scale punch on landing
                float punch = t >= 1f ? 1f : Mathf.Lerp(1.18f, 1f, t);
                victoryRect.localScale = new Vector3(punch, punch, 1f);
            }
        }

        // Animate Game Logo (Slides in from RIGHT, t: 0.35s to 0.88s)
        if (logoRect != null)
        {
            float t = Mathf.Clamp01((animationTime - 0.35f) / 0.53f);
            if (t > 0f)
            {
                float ease = EaseOutBack(t);
                float x = Mathf.Lerp(LogoStartX, 0f, ease);
                logoRect.anchoredPosition = new Vector2(x, LogoTargetY);

                float punch = t >= 1f ? 1f : Mathf.Lerp(1.15f, 1f, t);
                logoRect.localScale = new Vector3(punch, punch, 1f);
            }
        }

        // Animate Sunburst Glow rotation
        if (sunburstGlow != null)
        {
            sunburstGlow.rectTransform.Rotate(0f, 0f, -25f * dt);
        }

        // Auto finish after celebration duration
        if (animationTime >= AutoFinishDuration)
        {
            StartTransitionOut();
        }
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private void PlayVictoryMusic()
    {
        if (audioSource == null) return;
        AudioClip clip = WarfestAudio.GetVictoryClip();
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.spatialBlend = 0f;
            audioSource.loop = false;
            audioSource.volume = 0.75f;
            audioSource.mute = !WarfestAudio.MusicEnabled;
            audioSource.Play();
        }
        else
        {
            PlayFanfareSound();
        }
    }

    private void PlayFanfareSound()
    {
        if (audioSource == null) return;

        // Synthesize a triumphant C Major fanfare arpeggio (C4 -> E4 -> G4 -> C5)
        const int sampleRate = 22050;
        const float duration = 1.4f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float[] freqs = { 261.63f, 329.63f, 392.00f, 523.25f };
        float noteDuration = 0.28f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            int noteIndex = Mathf.Clamp(Mathf.FloorToInt(t / noteDuration), 0, freqs.Length - 1);
            float noteTime = t - noteIndex * noteDuration;
            float freq = freqs[noteIndex];

            // Harmonic fanfare tone with exponential decay per note
            float envelope = Mathf.Exp(-3.8f * noteTime);
            float fundamental = Mathf.Sin(t * freq * Mathf.PI * 2f) * 0.45f;
            float octave = Mathf.Sin(t * freq * 2f * Mathf.PI * 2f) * 0.25f;
            float fifth = Mathf.Sin(t * freq * 3f * Mathf.PI * 2f) * 0.12f;

            // Warm reverb-like shimmer on the final high C5 note
            float shimmer = (noteIndex == 3) ? Mathf.Sin(t * 1046.5f * Mathf.PI * 2f) * 0.08f : 0f;

            samples[i] = (fundamental + octave + fifth + shimmer) * envelope;
        }

        AudioClip clip = AudioClip.Create("Victory Fanfare", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        audioSource.PlayOneShot(clip, 0.85f);
    }

    private void OnTapContinue()
    {
        if (animationTime >= MinSkipTime && !isTransitioningOut)
        {
            StartTransitionOut();
        }
    }

    private void StartTransitionOut()
    {
        if (isTransitioningOut) return;
        isTransitioningOut = true;
        StartCoroutine(TransitionOut());
    }

    private IEnumerator TransitionOut()
    {
        float timer = 0f;
        const float fadeOutTime = 0.45f;
        float startVol = audioSource != null ? audioSource.volume : 0.75f;

        while (timer < fadeOutTime)
        {
            timer += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f - (timer / fadeOutTime);
            }
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.volume = Mathf.Lerp(startVol, 0f, timer / fadeOutTime);
            }
            yield return null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }
        WarfestAudio.StopGameplayAudio();

        onContinue?.Invoke();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}
