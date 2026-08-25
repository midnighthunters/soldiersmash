using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class JumpToLevel : EditorWindow
{
    private const string GameScenePath = "Assets/Scenes/Game.unity";
    private int levelNumber = 1;

    [MenuItem("Tools/Jump To Level")]
    private static void OpenWindow()
    {
        JumpToLevel window = GetWindow<JumpToLevel>("Jump To Level");
        window.levelNumber = WarfestSession.SelectedLevel + 1;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Jump To Level", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Enter the level number to play. The chosen number is shared by MainMenu and Game.",
            MessageType.Info);

        levelNumber = EditorGUILayout.IntField("Level Number", levelNumber);
        levelNumber = Mathf.Clamp(levelNumber, 1, WarfestSession.LevelCount);
        EditorGUILayout.LabelField($"Valid range: 1 - {WarfestSession.LevelCount}", EditorStyles.miniLabel);

        EditorGUILayout.Space();
        if (GUILayout.Button("Jump To Level"))
        {
            JumpToSelectedLevel();
        }
    }

    private void JumpToSelectedLevel()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        WarfestSession.SelectLevel(levelNumber - 1);
        EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
    }
}
