using UnityEngine;

namespace Evbishop.Runtime.UIUtils.Dragging
{
    [RequireComponent(typeof(RectTransform))]
    public class DraggableElementContainer : MonoBehaviour
    {
        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }
    }
}
