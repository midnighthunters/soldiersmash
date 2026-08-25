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
        return zeroBasedLevel < WarfestLevelCatalog.AuthoredLevelCount ? LevelOneBalls : DefaultBalls;
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
