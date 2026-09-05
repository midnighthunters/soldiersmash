using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Warfest.Editor
{
    [CustomEditor(typeof(WarfestGameController))]
    public class WarfestGameScenePreviewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Edit Mode 2D Scene Preview", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                if (GUILayout.Button("Build / Refresh Edit-Mode Preview", GUILayout.Height(32)))
                {
                    BuildPreview();
                }

                if (GUILayout.Button("Clear Edit-Mode Preview", GUILayout.Height(24)))
                {
                    ClearPreview();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Scene is currently running in Play Mode.", MessageType.Info);
            }
        }

        [MenuItem("Tools/Warfest/Setup Edit Mode Preview for Game Scene", priority = 10)]
        public static void BuildPreviewFromMenu()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != "Game")
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
                }
                else
                {
                    return;
                }
            }

            BuildPreview();
        }

        private static void BuildPreview()
        {
            WarfestGameController controller = Object.FindAnyObjectByType<WarfestGameController>();
            if (controller == null)
            {
                Debug.LogWarning("[Warfest] WarfestGameController not found in active scene.");
                return;
            }

            controller.BuildEditModePreview();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Warfest] Edit-mode preview successfully built for Game scene.");
        }

        private static void ClearPreview()
        {
            WarfestGameController controller = Object.FindAnyObjectByType<WarfestGameController>();
            if (controller != null)
            {
                controller.ClearEditModePreview();
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                Debug.Log("[Warfest] Edit-mode preview cleared.");
            }
        }
    }
}
