using System.Collections.Generic;
using UnityEngine;

public static class WarfestMiscArt
{
    private static readonly Dictionary<string, Sprite> cachedSprites = new Dictionary<string, Sprite>();

    private static readonly string[] SplashNames = { "misc/splash1", "misc/splash2", "misc/splash3" };
    private static readonly string[] LoadCandidateNames = { "misc/load0", "misc/load1", "misc/load2", "misc/load3", "misc/load4" };

    public static Sprite GetRandomSplashScreen()
    {
        int index = Random.Range(0, SplashNames.Length);
        return LoadSprite(SplashNames[index]);
    }

    public static Sprite GetRandomLoadingSprite()
    {
        List<string> valid = new List<string>();
        for (int i = 0; i < LoadCandidateNames.Length; i++)
        {
            Sprite s = LoadSprite(LoadCandidateNames[i]);
            if (s != null) valid.Add(LoadCandidateNames[i]);
        }

        if (valid.Count == 0) return null;
        int picked = Random.Range(0, valid.Count);
        return LoadSprite(valid[picked]);
    }

    public static Sprite GetLoadingBackground()
    {
        return LoadSprite("misc/background");
    }

    public static Sprite GetVictorySprite()
    {
        return LoadSprite("misc/victory1");
    }

    public static Sprite GetLogoSprite()
    {
        return LoadSprite("misc/logo");
    }

    public static Sprite GetGreenButtonSprite()
    {
        const string key = "green_button_sprite";
        if (cachedSprites.TryGetValue(key, out Sprite cached) && cached != null) return cached;

        Texture2D panelSheet = Resources.Load<Texture2D>("pnl");
        if (panelSheet == null) return null;
        Rect topLeftRect = new Rect(443f, 560f, 410f, 174f);
        float scaleX = panelSheet.width / 1254f;
        float scaleY = panelSheet.height / 1254f;
        Rect unityRect = new Rect(topLeftRect.x * scaleX,
            panelSheet.height - (topLeftRect.y + topLeftRect.height) * scaleY,
            topLeftRect.width * scaleX, topLeftRect.height * scaleY);
        Sprite s = Sprite.Create(panelSheet, unityRect, new Vector2(0.5f, 0.5f), 100f);
        cachedSprites[key] = s;
        return s;
    }

    public static Sprite GetRadialGlowSprite()
    {
        const string key = "generated_radial_glow";
        if (cachedSprites.TryGetValue(key, out Sprite cached) && cached != null) return cached;

        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.name = "Soft Radial Glow";
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - (size - 1) * 0.5f) / (size * 0.5f);
                float dy = (y - (size - 1) * 0.5f) / (size * 0.5f);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(1f - dist);
                alpha = Mathf.Pow(alpha, 1.8f);
                pixels[y * size + x] = new Color(1f, 0.88f, 0.40f, alpha * 0.50f);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        Sprite s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        cachedSprites[key] = s;
        return s;
    }

    public static Sprite GetHypercasualTrackSprite()
    {
        const string key = "hypercasual_track_sprite";
        if (cachedSprites.TryGetValue(key, out Sprite cached) && cached != null) return cached;

        const int tw = 96;
        const int th = 34;
        const float radius = 11.5f;
        const float bevel = 4.0f;

        Texture2D trackTex = new Texture2D(tw, th, TextureFormat.RGBA32, false);
        trackTex.name = "Hypercasual Track";
        trackTex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[tw * th];
        Vector2 lightDir = new Vector2(-0.25f, 0.96f).normalized;

        float minX = radius;
        float maxX = tw - 1 - radius;
        float minY = radius;
        float maxY = th - 1 - radius;

        for (int y = 0; y < th; y++)
        {
            for (int x = 0; x < tw; x++)
            {
                float cx = Mathf.Clamp((float)x, minX, maxX);
                float cy = Mathf.Clamp((float)y, minY, maxY);
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > radius)
                {
                    float sDist = Mathf.Sqrt(dx * dx + (dy + 1.5f) * (dy + 1.5f));
                    float sAlpha = Mathf.Clamp01((radius + 1.5f - sDist) / 2f) * 0.40f;
                    pixels[y * tw + x] = new Color(0, 0, 0, sAlpha);
                }
                else if (dist > radius - bevel)
                {
                    Vector2 n = new Vector2(dx, dy) / (dist > 0.001f ? dist : 1f);
                    float dot = Vector2.Dot(n, lightDir);
                    float normDot = (dot + 1f) * 0.5f;

                    if (dist > radius - 1.2f)
                    {
                        Color contour = new Color(0.18f, 0.12f, 0.05f, 1f);
                        float aa = Mathf.Clamp01(radius - dist);
                        contour.a = aa;
                        pixels[y * tw + x] = contour;
                    }
                    else
                    {
                        Color shadowCol = new Color(0.35f, 0.22f, 0.08f, 1f);
                        Color midCol = new Color(0.90f, 0.72f, 0.40f, 1f);
                        Color highlightCol = new Color(1f, 0.98f, 0.86f, 1f);
                        Color rimCol = normDot > 0.5f
                            ? Color.Lerp(midCol, highlightCol, (normDot - 0.5f) * 2f)
                            : Color.Lerp(shadowCol, midCol, normDot * 2f);
                        pixels[y * tw + x] = rimCol;
                    }
                }
                else if (dist > radius - bevel - 1.5f)
                {
                    float innerT = (dist - (radius - bevel - 1.5f)) / 1.5f;
                    pixels[y * tw + x] = Color.Lerp(new Color(0.04f, 0.06f, 0.09f, 1f), new Color(0.20f, 0.14f, 0.07f, 1f), innerT);
                }
                else
                {
                    float ny = (float)y / th;
                    Color topCav = new Color(0.05f, 0.07f, 0.11f, 1f);
                    Color botCav = new Color(0.09f, 0.12f, 0.17f, 1f);
                    pixels[y * tw + x] = Color.Lerp(botCav, topCav, ny);
                }
            }
        }
        trackTex.SetPixels(pixels);
        trackTex.Apply();
        Sprite s = Sprite.Create(trackTex, new Rect(0, 0, tw, th), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(12, 12, 12, 12));
        cachedSprites[key] = s;
        return s;
    }

    public static Sprite GetHypercasualFillSprite()
    {
        const string key = "hypercasual_fill_sprite";
        if (cachedSprites.TryGetValue(key, out Sprite cached) && cached != null) return cached;

        const int fw = 64;
        const int fh = 26;
        const float radius = 7.5f;

        Texture2D fillTex = new Texture2D(fw, fh, TextureFormat.RGBA32, false);
        fillTex.name = "Hypercasual Fill";
        fillTex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[fw * fh];

        float minX = radius;
        float maxX = fw - 1 - radius;
        float minY = radius;
        float maxY = fh - 1 - radius;

        for (int y = 0; y < fh; y++)
        {
            for (int x = 0; x < fw; x++)
            {
                float cx = Mathf.Clamp((float)x, minX, maxX);
                float cy = Mathf.Clamp((float)y, minY, maxY);
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > radius)
                {
                    pixels[y * fw + x] = Color.clear;
                }
                else
                {
                    float aa = Mathf.Clamp01(radius - dist);
                    float ny = (float)y / fh;
                    Color col = Color.white;
                    if (ny < 0.30f)
                    {
                        float shadowT = 1f - (ny / 0.30f);
                        col = Color.Lerp(col, new Color(0.60f, 0.60f, 0.60f, 1f), shadowT * 0.75f);
                    }
                    else if (ny > 0.45f)
                    {
                        float glossT = (ny - 0.45f) / 0.55f;
                        float glossPeak = Mathf.Pow(Mathf.Sin(glossT * Mathf.PI), 0.9f);
                        col = Color.Lerp(col, new Color(1.5f, 1.5f, 1.5f, 1f), glossPeak * 0.6f);
                    }
                    col.a = aa;
                    pixels[y * fw + x] = col;
                }
            }
        }
        fillTex.SetPixels(pixels);
        fillTex.Apply();
        Sprite s = Sprite.Create(fillTex, new Rect(0, 0, fw, fh), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(8, 8, 8, 8));
        cachedSprites[key] = s;
        return s;
    }

    public static Sprite GetShimmerSprite()
    {
        const string key = "hypercasual_shimmer_sprite";
        if (cachedSprites.TryGetValue(key, out Sprite cached) && cached != null) return cached;

        const int sw = 64;
        const int sh = 64;
        Texture2D shimTex = new Texture2D(sw, sh, TextureFormat.RGBA32, false);
        shimTex.name = "Hypercasual Shimmer";
        shimTex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[sw * sh];

        for (int y = 0; y < sh; y++)
        {
            for (int x = 0; x < sw; x++)
            {
                float diag = ((float)x / sw) * 0.7f + ((float)y / sh) * 0.3f;
                float distToBeam = Mathf.Abs(diag - 0.5f) / 0.5f;
                float beam = Mathf.Clamp01(1f - distToBeam);
                beam = Mathf.Pow(beam, 2.5f);
                pixels[y * sw + x] = new Color(1f, 1f, 1f, beam * 0.55f);
            }
        }
        shimTex.SetPixels(pixels);
        shimTex.Apply();
        Sprite s = Sprite.Create(shimTex, new Rect(0, 0, sw, sh), new Vector2(0.5f, 0.5f), 100f);
        cachedSprites[key] = s;
        return s;
    }

    public static Font GetCartoonFont()
    {
        return WarfestFontResolver.HeadingFont;
    }

    public static Sprite LoadSprite(string resourcePath)
    {
        if (string.IsNullOrEmpty(resourcePath)) return null;

        if (cachedSprites.TryGetValue(resourcePath, out Sprite cached) && cached != null)
        {
            return cached;
        }

        // Try direct sprite load
        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite != null)
        {
            cachedSprites[resourcePath] = sprite;
            return sprite;
        }

        // Fallback: load as Texture2D and wrap in Sprite
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture != null)
        {
            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0u,
                SpriteMeshType.FullRect
            );
            sprite.name = texture.name;
            cachedSprites[resourcePath] = sprite;
            return sprite;
        }

        return null;
    }

    private static Sprite cachedVerticalGradientSprite;
    public static Sprite GetVerticalGradientSprite()
    {
        if (cachedVerticalGradientSprite != null) return cachedVerticalGradientSprite;
        Texture2D tex = new Texture2D(1, 32, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < 32; y++)
        {
            float a = 1f - (y / 31f); // 1 at bottom, 0 at top
            a = a * a * (3f - 2f * a); // smooth cubic fade
            tex.SetPixel(0, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        cachedVerticalGradientSprite = Sprite.Create(
            tex,
            new Rect(0, 0, 1, 32),
            new Vector2(0.5f, 0.5f),
            100f,
            0u,
            SpriteMeshType.FullRect
        );
        return cachedVerticalGradientSprite;
    }
}
