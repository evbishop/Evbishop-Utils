using UnityEditor.Toolbars;
using UnityEngine;

namespace Evbishop.Editor
{
    public static class ClearPrefsButton
    {
        [MainToolbarElement("Evbishop/Clear Saves", defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement ButtonClearSaves()
        {
            var content = new MainToolbarContent("C");
            return new MainToolbarButton(content, ClearSaves);
        }

        private static void ClearSaves()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
