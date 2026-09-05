using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// The four combat boosters shown in the bottom corners of the gameplay HUD. The integer values
// map directly onto the sliced sprite names in Resources/boosters.png (boosters_0..boosters_3).
public enum WarfestBooster
{
    InfiniteBalls = 0, // boosters_0 - unlimited balls for the rest of the level
    SkullShot = 1,     // boosters_1 - a heavy skull ball that smashes through everything
    SpreadShot = 2,    // boosters_2 - fires a three-ball fan
    Missile = 3,       // boosters_3 - an explosive shell that detonates on impact
}

public static class WarfestSession
{
    public const int LevelCount = 100;
    public const int DefaultBalls = 20;
    public const int LevelOneBalls = 60;
    public const int MaxLives = 5;
    public const int LifeRefillSeconds = 30 * 60;
    public const int BoosterCount = 4;

    // Every booster starts the campaign with a small free stock so the flow is playable out of
    // the box. A store / rewarded-ad hook would top these up in a shipping build.
    private const int DefaultBoosterStock = 3;
    private const string SelectedLevelKey = "Warfest.SelectedLevel";
    private const string LivesKey = "Warfest.Lives";
    private const string NextLifeUtcKey = "Warfest.NextLifeUtc";
    private const string CampaignCompleteKey = "Warfest.CampaignComplete";
    private const string BoosterCountKeyPrefix = "Warfest.Booster.";
    private static int selectedLevel = -1;
    public static bool HasShownSplash { get; set; } = false;

    // True once the player has cleared the final level. The menu uses this to swap the
    // "Deploy Mission" card into a disabled "Coming Soon" state instead of replaying level 100.
    public static bool CampaignComplete
    {
        get { return PlayerPrefs.GetInt(CampaignCompleteKey, 0) == 1; }
    }

    // Current number of lives the player holds. Defaults to a full bar and is clamped so the
    // UI can safely show "5" and switch its label to "FULL" when the player is topped up.
    public static int Lives
    {
        get
        {
            RefreshLives();
            return Mathf.Clamp(PlayerPrefs.GetInt(LivesKey, MaxLives), 0, MaxLives);
        }
    }

    public static bool LivesFull
    {
        get { return Lives >= MaxLives; }
    }

    // Removes one life for a failed attempt. The first missing life starts a shared thirty-minute
    // regeneration clock; losing another life does not reset progress toward the next refill.
    public static bool ConsumeLife()
    {
        RefreshLives();
        int lives = Mathf.Clamp(PlayerPrefs.GetInt(LivesKey, MaxLives), 0, MaxLives);
        if (lives <= 0) return false;

        lives--;
        PlayerPrefs.SetInt(LivesKey, lives);
        if (!TryGetNextLifeUtc(out long nextLifeUtc) || nextLifeUtc <= 0)
        {
            PlayerPrefs.SetString(NextLifeUtcKey, (UtcNowSeconds() + LifeRefillSeconds).ToString());
        }
        PlayerPrefs.Save();
        return true;
    }

    // Seconds remaining until the next life is restored. Returns zero while the life bar is full.
    public static int SecondsUntilNextLife
    {
        get
        {
            RefreshLives();
            int lives = Mathf.Clamp(PlayerPrefs.GetInt(LivesKey, MaxLives), 0, MaxLives);
            if (lives >= MaxLives || !TryGetNextLifeUtc(out long nextLifeUtc)) return 0;
            return Mathf.Max(0, (int)Math.Min(int.MaxValue, nextLifeUtc - UtcNowSeconds()));
        }
    }

    public static string LifeTimerText
    {
        get
        {
            int seconds = SecondsUntilNextLife;
            return string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);
        }
    }

    // Restores every life earned while the app was closed and keeps any partial progress toward
    // the following life. This makes the timer independent of frame rate and device suspension.
    public static void RefreshLives()
    {
        int lives = Mathf.Clamp(PlayerPrefs.GetInt(LivesKey, MaxLives), 0, MaxLives);
        if (lives >= MaxLives)
        {
            if (PlayerPrefs.HasKey(NextLifeUtcKey))
            {
                PlayerPrefs.DeleteKey(NextLifeUtcKey);
                PlayerPrefs.Save();
            }
            return;
        }

        long now = UtcNowSeconds();
        if (!TryGetNextLifeUtc(out long nextLifeUtc) || nextLifeUtc <= 0)
        {
            PlayerPrefs.SetString(NextLifeUtcKey, (now + LifeRefillSeconds).ToString());
            PlayerPrefs.Save();
            return;
        }

        if (now < nextLifeUtc) return;

        long elapsedIntervals = 1L + (now - nextLifeUtc) / LifeRefillSeconds;
        int restored = (int)Math.Min(MaxLives - lives, elapsedIntervals);
        lives += restored;
        PlayerPrefs.SetInt(LivesKey, lives);
        if (lives >= MaxLives)
        {
            PlayerPrefs.DeleteKey(NextLifeUtcKey);
        }
        else
        {
            PlayerPrefs.SetString(NextLifeUtcKey, (nextLifeUtc + restored * (long)LifeRefillSeconds).ToString());
        }
        PlayerPrefs.Save();
    }

    private static bool TryGetNextLifeUtc(out long nextLifeUtc)
    {
        return long.TryParse(PlayerPrefs.GetString(NextLifeUtcKey, string.Empty), out nextLifeUtc);
    }

    private static long UtcNowSeconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    // How many uses of a booster the player currently owns. Persisted per booster so a stock
    // spent on one level carries over to the next, mirroring the Lives persistence pattern.
    public static int GetBoosterCount(WarfestBooster booster)
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(BoosterCountKeyPrefix + (int)booster, DefaultBoosterStock));
    }

    private static void SetBoosterCount(WarfestBooster booster, int count)
    {
        PlayerPrefs.SetInt(BoosterCountKeyPrefix + (int)booster, Mathf.Max(0, count));
        PlayerPrefs.Save();
    }

    // Spends a single use. Returns false (and changes nothing) when the player owns none.
    public static bool ConsumeBooster(WarfestBooster booster)
    {
        int count = GetBoosterCount(booster);
        if (count <= 0) return false;
        SetBoosterCount(booster, count - 1);
        return true;
    }

    // Adds uses to a booster - the entry point a shop or rewarded-ad reward would call.
    public static void GrantBooster(WarfestBooster booster, int amount = 1)
    {
        SetBoosterCount(booster, GetBoosterCount(booster) + Mathf.Max(1, amount));
    }

    public static int SelectedLevel
    {
        get
        {
            if (selectedLevel < 0)
            {
                selectedLevel = Mathf.Clamp(PlayerPrefs.GetInt(SelectedLevelKey, 0), 0, LevelCount - 1);
            }

            return selectedLevel;
        }
    }

    // Specific level ball reductions where QA validation recorded remaining balls > 20.
    // Each target level has its starting ball allowance reduced by exactly 15.
    private static readonly Dictionary<int, int> BallAllowanceAdjustments = new Dictionary<int, int>
    {
        { 12, -15 }, // Level 12 (Hollow Bunker): 43 -> 28
        { 22, -15 }, // Level 22 (Twin Depots): 41 -> 26
        { 43, -15 }, // Level 43 (Three-Post Trial): 46 -> 31
        { 74, -15 }, // Level 74 (Four Watchtowers): 40 -> 25
        { 86, -15 }, // Level 86 (High-Low Diamond): 51 -> 36
        { 92, -15 }, // Level 92 (Twin Front Bastions): 51 -> 36
        { 94, -15 }, // Level 94 (Crown Labyrinth): 46 -> 31
        { 98, -15 }, // Level 98 (Last Barricade): 51 -> 36
        { 100, -15 } // Level 100 (Grand King's Fortress): 51 -> 36
    };

    // The authored campaign scales its block budget from 20 (level 1) to 57 (level 100). Every
    // block is a target, so the ball allowance is derived from that budget with a generous,
    // stage-tapered margin above a competent clear (per the brief's shot-budget philosophy:
    // ~25-40% extra early, easing toward ~15% late). The live game uses half of that original
    // allowance, rounded up so an odd allowance never costs an additional full shot.
    public static int GetBallAllowance(int zeroBasedLevel)
    {
        int levelIndex = Mathf.Clamp(zeroBasedLevel, 0, LevelCount - 1);
        int levelNumber = levelIndex + 1;
        int blocks = WarfestLevelCatalog.CampaignBlockCount(levelIndex);
        float factor =
            levelIndex < 10 ? 1.6f :
            levelIndex < 30 ? 1.4f :
            levelIndex < 60 ? 1.25f : 1.15f;
        int originalAllowance = Mathf.Clamp(Mathf.CeilToInt(blocks * factor) + 4, 20, 90);
        int allowance = Mathf.CeilToInt(originalAllowance * 0.5f) + 6;

        if (BallAllowanceAdjustments.TryGetValue(levelNumber, out int adjustment))
        {
            allowance = Mathf.Max(10, allowance + adjustment);
        }

        return allowance;
    }

public static void SelectLevel(int zeroBasedLevel)
    {
        selectedLevel = Mathf.Clamp(zeroBasedLevel, 0, LevelCount - 1);
        PlayerPrefs.SetInt(SelectedLevelKey, selectedLevel);
        PlayerPrefs.Save();
    }

    public static void LoadLevel(int zeroBasedLevel)
    {
        if (Lives <= 0) return;

        // Starting any real level means the player is back in the campaign, so clear the
        // "finished" flag in case they replayed the final level from a completed state.
        SetCampaignComplete(false);
        SelectLevel(zeroBasedLevel);
        SceneManager.LoadScene("Game");
    }

    public static void CompleteLevel(int zeroBasedLevel)
    {
        WarfestAudio.StopGameplayAudio();
        int nextLevel = zeroBasedLevel + 1;
        if (nextLevel >= LevelCount)
        {
            // The player just cleared the last authored level. Park them on it and mark the
            // campaign complete so the menu shows "Coming Soon" instead of looping level 100.
            SetCampaignComplete(true);
            SelectLevel(LevelCount - 1);
        }
        else
        {
            SetCampaignComplete(false);
            SelectLevel(nextLevel);
        }

        SceneManager.LoadScene("MainMenu");
    }

    private static void SetCampaignComplete(bool complete)
    {
        PlayerPrefs.SetInt(CampaignCompleteKey, complete ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void ReturnToMenu()
    {
        WarfestAudio.StopGameplayAudio();
        SceneManager.LoadScene("MainMenu");
    }
}
