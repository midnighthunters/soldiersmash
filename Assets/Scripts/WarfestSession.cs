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

    // Twice the maximum successful shot count recorded in
    // Assets/QA/Level-01-100-repeat-report.csv. The report currently contains complete
    // ten-attempt data for levels 1-46 and one successful attempt for level 47.
    //
    // Levels without a successful report row use the legacy allowance below rather than zero,
    // so untested levels remain playable until their repeat data is available.
    private static readonly int[] ReportBasedBallAllowances =
    {
        30, 12, 26, 30, 12, 16, 18, 22, 16, 8,
        26, 36, 42, 18, 24, 18, 58, 36, 22, 44,
        48, 10, 22, 82, 38, 38, 16, 28, 26, 22,
        22, 32, 52, 10, 16, 66, 36, 26, 14, 18,
        34, 14, 16, 28, 26, 10, 16
    };

    public static int GetBallAllowance(int zeroBasedLevel)
    {
        int levelIndex = Mathf.Clamp(zeroBasedLevel, 0, LevelCount - 1);
        if (levelIndex < ReportBasedBallAllowances.Length)
        {
            return ReportBasedBallAllowances[levelIndex];
        }

        // Fallback for levels not yet represented by a successful report attempt.
        return Mathf.Clamp(52 - levelIndex / 2, 28, 52);
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
