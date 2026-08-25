using UnityEngine;
using UnityEngine.SceneManagement;

public static class WarfestSession
{
    public const int LevelCount = 50;
    public const int DefaultBalls = 20;
    public const int LevelOneBalls = 60;
    private const string SelectedLevelKey = "Warfest.SelectedLevel";
    private static int selectedLevel = -1;

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
