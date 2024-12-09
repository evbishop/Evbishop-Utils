using Evbishop.Runtime.ModulesSystem;
using System;
#if UNITY_EDITOR
using UnityEngine;
using DG.Tweening;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using UnityEngine.UI;
#endif

namespace Evbishop.Runtime.UI.Modules
{
    [Serializable]
    public class UIModulesSystem : ModulesSystem<UIModule>
    {
#if UNITY_EDITOR
        [NonSerialized] private UIBehaviour _editorBehaviour;

        public void SetEditorBehaviour(UIBehaviour behaviour)
        {
            _editorBehaviour = behaviour;
        }

        public override void AfterModulesChanged(CollectionChangeInfo info, object value)
        {
            base.AfterModulesChanged(info, value);

            if (Modules == null)
                return;

            if (_editorBehaviour != null &&
                (info.ChangeType == CollectionChangeType.Add ||
                info.ChangeType == CollectionChangeType.Insert))
            {
                if (HasInterface<INeedComponent<GraphicRaycaster>>())
                {
                    PassComponent<GraphicRaycaster>();
                }
                if (HasInterface<INeedComponent<CanvasGroup>>())
                {
                    PassComponent<CanvasGroup>();
                }
                if (HasInterface<INeedComponent<RectTransform>>())
                {
                    PassComponent<RectTransform>();
                }
            }
        }

        private void PassComponent<T>() where T : Component
        {
            var component = _editorBehaviour.GetComponent<T>();
            if (component == null)
            {
                component = Undo.AddComponent<T>(_editorBehaviour.gameObject);
                EditorUtility.SetDirty(_editorBehaviour.gameObject);
            }

            var modulesNeedingComponent =
                GetModulesWithInterface<INeedComponent<T>>();
            foreach (var module in modulesNeedingComponent)
            {
                Undo.RecordObject(_editorBehaviour, "Assign component to module");
                module.Component = component;
                EditorUtility.SetDirty(_editorBehaviour);
            }
        }
#endif
    }
}