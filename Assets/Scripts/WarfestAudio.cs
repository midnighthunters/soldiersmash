using UnityEngine;

/// <summary>
/// Centralized audio manager and preference controller for Warfest.
/// Coordinates playback, clip loading, and persistent mute/unmute states across scenes.
/// </summary>
public static class WarfestAudio
{
    public const string SoundEnabledKey = "Warfest.SoundEnabled";
    public const string MusicEnabledKey = "Warfest.MusicEnabled";

    private static AudioClip everytimeClip;
    private static AudioClip levelClip;
    private static AudioClip victoryClip;
    private static AudioClip matchClip;

    public static bool SoundEnabled
    {
        get => PlayerPrefs.GetInt(SoundEnabledKey, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(SoundEnabledKey, value ? 1 : 0);
            PlayerPrefs.Save();
            ApplyAudioPreferences();
        }
    }

    public static bool MusicEnabled
    {
        get => PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(MusicEnabledKey, value ? 1 : 0);
            PlayerPrefs.Save();
            ApplyAudioPreferences();
        }
    }

    public static AudioClip GetEverytimeClip()
    {
        if (everytimeClip == null)
        {
            everytimeClip = Resources.Load<AudioClip>("audio/everytime");
        }
        return everytimeClip;
    }

    public static AudioClip GetLevelClip()
    {
        if (levelClip == null)
        {
            levelClip = Resources.Load<AudioClip>("audio/level");
        }
        return levelClip;
    }

    public static AudioClip GetVictoryClip()
    {
        if (victoryClip == null)
        {
            victoryClip = Resources.Load<AudioClip>("audio/victory");
        }
        return victoryClip;
    }

    public static AudioClip GetMatchClip()
    {
        if (matchClip == null)
        {
            matchClip = Resources.Load<AudioClip>("audio/match");
        }
        return matchClip;
    }

    public static bool IsMusicSource(AudioSource source)
    {
        if (source == null) return false;
        string name = source.gameObject.name.ToLowerInvariant();
        return source.loop ||
               name.Contains("music") ||
               name.Contains("theme") ||
               name.Contains("bgm") ||
               name.Contains("everytime") ||
               name.Contains("victory");
    }

    public static void ApplyAudioPreferences()
    {
        bool musicOn = MusicEnabled;
        bool soundOn = SoundEnabled;

        AudioSource[] sources = Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (AudioSource source in sources)
        {
            if (source == null) continue;
            bool isMusic = IsMusicSource(source);
            source.mute = isMusic ? !musicOn : !soundOn;
        }
    }

    /// <summary>
    /// Stops any music sources in the active scene that belong to gameplay or victory screens.
    /// Used when transitioning between scenes or to the main menu.
    /// </summary>
    public static void StopGameplayAudio()
    {
        AudioSource[] sources = Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (AudioSource source in sources)
        {
            if (source == null) continue;
            string name = source.gameObject.name.ToLowerInvariant();
            if (name.Contains("gameplay") || name.Contains("level") || name.Contains("victory") || source.loop)
            {
                if (!name.Contains("main menu"))
                {
                    source.Stop();
                }
            }
        }
    }

    private static Sprite soundIconSprite;
    private static Sprite musicIconSprite;
    private static Sprite leaveIconSprite;
    private static Sprite settingsEnabledSprite;
    private static Sprite settingsDisabledSprite;

    public static Sprite GetSoundIconSprite()
    {
        if (soundIconSprite == null)
        {
            Texture2D icons = Resources.Load<Texture2D>("settings_icons");
            if (icons != null)
            {
                float sx = icons.width / 1536f;
                float sy = icons.height / 1024f;
                // Center precisely around speaker artwork (Top-Right quadrant)
                soundIconSprite = Sprite.Create(icons, new Rect(864f * sx, 520.5f * sy, 466f * sx, 466f * sy), new Vector2(0.5f, 0.5f), 100f);
            }
        }
        return soundIconSprite;
    }

    public static Sprite GetMusicIconSprite()
    {
        if (musicIconSprite == null)
        {
            Texture2D icons = Resources.Load<Texture2D>("settings_icons");
            if (icons != null)
            {
                float sx = icons.width / 1536f;
                float sy = icons.height / 1024f;
                // Center precisely around music note artwork (Bottom-Left quadrant)
                musicIconSprite = Sprite.Create(icons, new Rect(223.5f * sx, 26f * sy, 430f * sx, 430f * sy), new Vector2(0.5f, 0.5f), 100f);
            }
        }
        return musicIconSprite;
    }

    public static Sprite GetLeaveIconSprite()
    {
        if (leaveIconSprite == null)
        {
            Texture2D icons = Resources.Load<Texture2D>("settings_icons");
            if (icons != null)
            {
                float sx = icons.width / 1536f;
                float sy = icons.height / 1024f;
                // Center precisely around exit arrow artwork (Top-Left quadrant)
                leaveIconSprite = Sprite.Create(icons, new Rect(242.5f * sx, 548f * sy, 420f * sx, 420f * sy), new Vector2(0.5f, 0.5f), 100f);
            }
        }
        return leaveIconSprite;
    }

    public static Sprite GetSettingsEnabledSprite()
    {
        if (settingsEnabledSprite == null)
        {
            Texture2D bg = Resources.Load<Texture2D>("settings_background");
            if (bg != null)
            {
                float sx = bg.width / 500f;
                float sy = bg.height / 295f;
                // Center precisely around circular green button plate
                settingsEnabledSprite = Sprite.Create(bg, new Rect(254f * sx, 36.5f * sy, 230f * sx, 230f * sy), new Vector2(0.5f, 0.5f), 100f);
            }
        }
        return settingsEnabledSprite;
    }

    public static Sprite GetSettingsDisabledSprite()
    {
        if (settingsDisabledSprite == null)
        {
            Texture2D bg = Resources.Load<Texture2D>("settings_background");
            if (bg != null)
            {
                float sx = bg.width / 500f;
                float sy = bg.height / 295f;
                // Center precisely around circular red button plate
                settingsDisabledSprite = Sprite.Create(bg, new Rect(15f * sx, 36.5f * sy, 230f * sx, 230f * sy), new Vector2(0.5f, 0.5f), 100f);
            }
        }
        return settingsDisabledSprite;
    }
}
