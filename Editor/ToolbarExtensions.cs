#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Evbishop.Editor
{
    [InitializeOnLoad]
    public static class ToolbarExtensions
    {
        private static SceneAsset _openedScene;
        static ToolbarExtensions()
        {
            ToolbarExtender.LeftToolbarGUI.Add(DrawLeftGUI);
            EditorApplication.playModeStateChanged += PlayModeStateChangedHandler;
        }

        private static void DrawLeftGUI()
        {
            GUILayout.FlexibleSpace();

            PlaySceneButton("Main Menu", "Assets/Scenes/MainMenu.unity");
        }

        private static void PlaySceneButton(string btnName, string pathScene)
        {
            if (EditorApplication.isPlaying)
                return;

            if (GUILayout.Button($"Play {btnName}"))
            {
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = false;
                    if (_openedScene != null)
                        EditorSceneManager.playModeStartScene = _openedScene;
                }
                else
                {
                    PlayScene(pathScene);
                }
            }
        }

        private static void PlayScene(string path)
        {
            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (scene != null)
            {
                _openedScene = EditorSceneManager.playModeStartScene;
                EditorSceneManager.playModeStartScene = scene;
                EditorApplication.isPlaying = true;
            }
            else
            {
                Debug.LogError($"Can't find scene: {path}");
            }
        }

        private static void PlayModeStateChangedHandler(PlayModeStateChange stateChange)
        {
            if (stateChange == PlayModeStateChange.ExitingPlayMode || _openedScene != null)
            {
                EditorSceneManager.playModeStartScene = _openedScene;
            }
        }
    }
}
#endif