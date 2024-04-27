using UnityEditor;
using UnityEngine;

namespace Evbishop.Editor
{
    [InitializeOnLoad]
    public static class ClearPrefsButton
    {
        static ClearPrefsButton()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }

        private static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            OnClearPlayerPrefsButtonClick();
        }

        private static void OnClearPlayerPrefsButtonClick()
        {
            var testLevelContent = new GUIContent("C", "Clear All Player Datas");

            if (GUILayout.Button(testLevelContent, ToolbarStyles.CommandButtonStyle))
            {
                PlayerPrefs.DeleteAll();
            }
        }
    }
}
