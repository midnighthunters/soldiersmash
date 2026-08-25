using UnityEngine;
using UnityEngine.SceneManagement;

public static class WarfestSession
{
    public const int LevelCount = 50;
    public const int DefaultBalls = 20;
    public const int LevelOneBalls = 60;
    public static int SelectedLevel { get; private set; }

    public static int GetBallAllowance(int zeroBasedLevel)
    {
        return zeroBasedLevel == 0 ? LevelOneBalls : DefaultBalls;
    }

    public static void SelectLevel(int zeroBasedLevel)
    {
        SelectedLevel = Mathf.Clamp(zeroBasedLevel, 0, LevelCount - 1);
    }

    public static void LoadLevel(int zeroBasedLevel)
    {
        SelectLevel(zeroBasedLevel);
        SceneManager.LoadScene("Game");
    }

    public static void CompleteLevel(int zeroBasedLevel)
    {
        SelectedLevel = Mathf.Clamp(zeroBasedLevel + 1, 0, LevelCount - 1);
        SceneManager.LoadScene("MainMenu");
    }

    public static void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
