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
    private static AudioClip shootClip;

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
            everytimeClip = Resources.Load<AudioClip>("audio/warfest_ambient") ?? Resources.Load<AudioClip>("audio/everytime");
        }
        return everytimeClip;
    }

    public static AudioClip GetLevelClip()
    {
        if (levelClip == null)
        {
            levelClip = Resources.Load<AudioClip>("audio/warfest_battle") ?? Resources.Load<AudioClip>("audio/level");
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

    public static AudioClip GetShootClip()
    {
        if (shootClip == null)
        {
            shootClip = Resources.Load<AudioClip>("audio/shoot");
        }
        return shootClip;
    }

    public static bool IsMusicSource(AudioSource source)
    {
        if (source == null) return false;
        string name = source.gameObject.name.ToLowerInvariant();
        return source.loop ||
               name.Contains("music") ||
               name.Contains("theme") ||
               name.Contains("bgm") ||
               name.Contains("ambient") ||
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
    private static Sprite gearIconSprite;
    private static Sprite settingsEnabledSprite;
    private static Sprite settingsDisabledSprite;

    private static Sprite FindSprite(string resourcePath, string spriteName)
    {
        Sprite[] all = Resources.LoadAll<Sprite>(resourcePath);
        if (all != null)
        {
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].name == spriteName)
                    return all[i];
            }
            if (all.Length > 0 && all[0] != null) return all[0];
        }
        return null;
    }

    public static Sprite GetSoundIconSprite()
    {
        if (soundIconSprite == null)
        {
            soundIconSprite = FindSprite("warfest_settings", "icon_sound") ?? FindSprite("settings", "icon_sound");
        }
        return soundIconSprite;
    }

    public static Sprite GetMusicIconSprite()
    {
        if (musicIconSprite == null)
        {
            musicIconSprite = FindSprite("warfest_settings", "icon_music") ?? FindSprite("settings", "icon_music");
        }
        return musicIconSprite;
    }

    public static Sprite GetLeaveIconSprite()
    {
        if (leaveIconSprite == null)
        {
            leaveIconSprite = FindSprite("warfest_settings", "icon_leave") ?? FindSprite("settings", "icon_leave");
        }
        return leaveIconSprite;
    }

    public static Sprite GetGearIconSprite()
    {
        if (gearIconSprite == null)
        {
            gearIconSprite = FindSprite("warfest_settings", "icon_gear") ?? FindSprite("settings", "icon_gear");
        }
        return gearIconSprite;
    }

    public static Sprite GetSettingsEnabledSprite()
    {
        if (settingsEnabledSprite == null)
        {
            settingsEnabledSprite = FindSprite("warfest_plates", "plate_green") ?? FindSprite("set_back", "plate_green");
        }
        return settingsEnabledSprite;
    }

    public static Sprite GetSettingsDisabledSprite()
    {
        if (settingsDisabledSprite == null)
        {
            settingsDisabledSprite = FindSprite("warfest_plates", "plate_red") ?? FindSprite("set_back", "plate_red");
        }
        return settingsDisabledSprite;
    }

    private static Sprite dialogCardSprite;
    public static Sprite GetDialogCardSprite()
    {
        if (dialogCardSprite == null)
        {
            dialogCardSprite = FindSprite("pnl", "dialog_card");
        }
        return dialogCardSprite;
    }
}
