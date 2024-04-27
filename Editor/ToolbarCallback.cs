using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evbishop.Editor
{
    public static class ToolbarCallback
    {
        private static ScriptableObject currentToolbar;

        private static Type toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");

        public static Action OnToolbarGUILeft;
        public static Action OnToolbarGUIRight;

        static ToolbarCallback()
        {
            EditorApplication.update -= OnToolbarUpdate;
            EditorApplication.update += OnToolbarUpdate;
        }

        private static void OnToolbarUpdate()
        {
            if (currentToolbar == null)
            {
                var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);

                currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;

                if (currentToolbar != null)
                {
                    FieldInfo root = currentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                    VisualElement visualElementRoot = root.GetValue(currentToolbar) as VisualElement;

                    RegisterCallback("ToolbarZoneLeftAlign", OnToolbarGUILeft);
                    RegisterCallback("ToolbarZoneRightAlign", OnToolbarGUIRight);

                    void RegisterCallback(string root, Action action)
                    {
                        VisualElement toolbarZone = visualElementRoot.Q(root);
                        VisualElement parent = new VisualElement()
                        {
                            style =
                            {
                                flexGrow = 1,
                                flexDirection = FlexDirection.Row,
                            }
                        };

                        IMGUIContainer container = new IMGUIContainer();
                        container.style.flexGrow = 1;
                        container.onGUIHandler += () =>
                        {
                            action?.Invoke();
                        };

                        parent.Add(container);
                        toolbarZone.Add(parent);
                    }
                }
            }
        }
    }
}
