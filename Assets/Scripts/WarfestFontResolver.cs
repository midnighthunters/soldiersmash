using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Centralized font resolver and text configuration manager for Warfest.
/// Guarantees that UI text elements have a valid font asset across all iOS standalone builds,
/// iPads, iPhones, and editor environments, and enforces non-destructive overflow settings.
/// </summary>
public static class WarfestFontResolver
{
    private static Font _headingFont;
    private static Font _bodyFont;
    private static Font _fallbackFont;
    private static bool _initialized;

    public static Font HeadingFont
    {
        get
        {
            if (_headingFont != null) return _headingFont;
            ResolveFonts();
            return _headingFont;
        }
    }

    public static Font BodyFont
    {
        get
        {
            if (_bodyFont != null) return _bodyFont;
            ResolveFonts();
            return _bodyFont != null ? _bodyFont : HeadingFont;
        }
    }

    public static Font FallbackFont
    {
        get
        {
            if (_fallbackFont != null) return _fallbackFont;
            ResolveFonts();
            return _fallbackFont != null ? _fallbackFont : HeadingFont;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Warmup()
    {
        ResolveFonts();
    }

    public static void ResolveFonts()
    {
        if (_headingFont != null && _bodyFont != null && _fallbackFont != null)
            return;

        // Tier 1: Resources.Load from packaged Resources/Fonts/
        if (_headingFont == null)
            _headingFont = Resources.Load<Font>("Fonts/LuckiestGuy-Regular") ?? Resources.Load<Font>("LuckiestGuy-Regular");

        if (_bodyFont == null)
            _bodyFont = Resources.Load<Font>("Fonts/Fredoka-Bold") ?? Resources.Load<Font>("Fredoka-Bold");

        // Tier 2: Loaded font objects in current domain
        if (_headingFont == null || _bodyFont == null)
        {
            try
            {
                Font[] allFonts = Resources.FindObjectsOfTypeAll<Font>();
                if (allFonts != null)
                {
                    foreach (Font f in allFonts)
                    {
                        if (f == null) continue;
                        string fName = f.name;
                        if (_headingFont == null && fName.IndexOf("Luckiest", StringComparison.OrdinalIgnoreCase) >= 0)
                            _headingFont = f;
                        if (_bodyFont == null && fName.IndexOf("Fredoka", StringComparison.OrdinalIgnoreCase) >= 0)
                            _bodyFont = f;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[WarfestFontResolver] Exception searching loaded fonts: " + ex.Message);
            }
        }

        // Tier 3: Unity builtin resources
        if (_fallbackFont == null)
        {
            try
            {
                _fallbackFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch { }
        }

        if (_fallbackFont == null)
        {
            try
            {
                _fallbackFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            catch { }
        }

        // Tier 4: OS Dynamic Fonts
        if (_fallbackFont == null)
        {
            try
            {
                string[] osFontNames = new string[] { "Helvetica-Bold", "Arial-BoldMT", "Helvetica", "Arial" };
                _fallbackFont = Font.CreateDynamicFontFromOSFont(osFontNames, 32);
            }
            catch { }
        }

        // Cross-assign fallbacks so no property ever returns null
        if (_headingFont == null)
            _headingFont = _bodyFont ?? _fallbackFont;

        if (_bodyFont == null)
            _bodyFont = _headingFont ?? _fallbackFont;

        if (_fallbackFont == null)
            _fallbackFont = _headingFont ?? _bodyFont;

        _initialized = true;
    }

    /// <summary>
    /// Configures a Text component with safe overflow settings that prevent Unity from
    /// discarding or truncating text on iPads, small screens, or scaled canvases.
    /// </summary>
    public static void ConfigureSafeText(Text text, int minSize = 8, bool overflow = true)
    {
        if (text == null) return;

        if (text.font == null)
        {
            text.font = HeadingFont;
        }

        if (overflow)
        {
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
        }

        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = Mathf.Max(6, minSize);
        if (text.resizeTextMaxSize < text.resizeTextMinSize)
        {
            text.resizeTextMaxSize = text.fontSize > 0 ? text.fontSize : text.resizeTextMinSize;
        }
    }
}
