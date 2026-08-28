using UnityEngine;
using UnityEngine.SceneManagement;

public static class WarfestSession
{
    public const int LevelCount = 100;
    public const int DefaultBalls = 20;
    public const int LevelOneBalls = 60;
    public const int MaxLives = 5;
    private const string SelectedLevelKey = "Warfest.SelectedLevel";
    private const string LivesKey = "Warfest.Lives";
    private const string CampaignCompleteKey = "Warfest.CampaignComplete";
    private static int selectedLevel = -1;

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
        get { return Mathf.Clamp(PlayerPrefs.GetInt(LivesKey, MaxLives), 0, MaxLives); }
    }

    public static bool LivesFull
    {
        get { return Lives >= MaxLives; }
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

    // The authored campaign scales its block budget from 20 (level 1) to 57 (level 100). Every
    // block is a target, so the ball allowance is derived from that budget with a generous,
    // stage-tapered margin above a competent clear (per the brief's shot-budget philosophy:
    // ~25-40% extra early, easing toward ~15% late). This is a pre-playtest starting point.
    public static int GetBallAllowance(int zeroBasedLevel)
    {
        int levelIndex = Mathf.Clamp(zeroBasedLevel, 0, LevelCount - 1);
        int blocks = WarfestLevelCatalog.CampaignBlockCount(levelIndex);
        float factor =
            levelIndex < 10 ? 1.6f :
            levelIndex < 30 ? 1.4f :
            levelIndex < 60 ? 1.25f : 1.15f;
        return Mathf.Clamp(Mathf.CeilToInt(blocks * factor) + 4, 20, 90);
    }

public static void SelectLevel(int zeroBasedLevel)
    {
        selectedLevel = Mathf.Clamp(zeroBasedLevel, 0, LevelCount - 1);
        PlayerPrefs.SetInt(SelectedLevelKey, selectedLevel);
        PlayerPrefs.Save();
    }

    public static void LoadLevel(int zeroBasedLevel)
    {
        // Starting any real level means the player is back in the campaign, so clear the
        // "finished" flag in case they replayed the final level from a completed state.
        SetCampaignComplete(false);
        SelectLevel(zeroBasedLevel);
        SceneManager.LoadScene("Game");
    }

public static void CompleteLevel(int zeroBasedLevel)
    {
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
        SceneManager.LoadScene("MainMenu");
    }
}
