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
    private static int selectedLevel = -1;

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

    public static int GetBallAllowance(int zeroBasedLevel)
    {
        // The campaign starts generously and tightens every two levels. Later bomb-heavy
        // structures remain solvable because one precise shot can clear several targets.
        return Mathf.Clamp(52 - zeroBasedLevel / 2, 28, 52);
    }

public static void SelectLevel(int zeroBasedLevel)
    {
        selectedLevel = Mathf.Clamp(zeroBasedLevel, 0, LevelCount - 1);
        PlayerPrefs.SetInt(SelectedLevelKey, selectedLevel);
        PlayerPrefs.Save();
    }

    public static void LoadLevel(int zeroBasedLevel)
    {
        SelectLevel(zeroBasedLevel);
        SceneManager.LoadScene("Game");
    }

public static void CompleteLevel(int zeroBasedLevel)
    {
        SelectLevel(zeroBasedLevel + 1);
        SceneManager.LoadScene("MainMenu");
    }

    public static void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
