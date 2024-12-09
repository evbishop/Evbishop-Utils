using Evbishop.Runtime.UI;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Evbishop.Editor.UI
{
    [CustomEditor(typeof(UIButton))]
    public class UIButtonEditor : OdinEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            var button = target as UIButton;
            if (button == null)
                return;

            button.ModulesNormal.SetEditorBehaviour(button);
            button.ModulesHighlighted.SetEditorBehaviour(button);
            button.ModulesSelected.SetEditorBehaviour(button);
            button.ModulesPressed.SetEditorBehaviour(button);
            button.ModulesDisabled.SetEditorBehaviour(button);
        }
    }
}