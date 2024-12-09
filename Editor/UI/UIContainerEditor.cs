using Evbishop.Runtime.UI;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Evbishop.Editor.UI
{
    [CustomEditor(typeof(UIContainer))]
    public class UIContainerEditor : OdinEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            var container = target as UIContainer;
            if (container != null && container.Modules != null)
            {
                container.Modules.SetEditorBehaviour(container);
            }
        }
    }
}
