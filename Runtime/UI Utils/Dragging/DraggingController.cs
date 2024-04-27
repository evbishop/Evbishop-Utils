using UnityEngine;
using UnityEngine.InputSystem;

namespace Evbishop.Runtime.UIUtils.Dragging
{
    public class DraggingController : MonoBehaviour
    {
        [SerializeField] protected float popOffOffsetY;
        [SerializeField] protected float popOffOffsetX;

        protected Vector2 dragStartPosition;
        protected DraggableElement selectedUnit;
        protected bool dragging;
        protected bool poppedOut;

        void Update()
        {
            HandleMouseRelease();

            if (!dragging)
                return;

            if (Mathf.Abs(Mouse.current.position.ReadValue().y - dragStartPosition.y) > popOffOffsetY &&
                Mathf.Abs(Mouse.current.position.ReadValue().x - dragStartPosition.x) > popOffOffsetX)
            {
                if (!poppedOut && selectedUnit != null)
                {
                    selectedUnit.StartDrag();
                    poppedOut = true;
                }
            }
        }

        protected void HandleMouseRelease()
        {
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                dragging = false;
                if (selectedUnit)
                {
                    if (poppedOut)
                        selectedUnit.OnMouseReleased();
                    poppedOut = false;
                    selectedUnit = null;
                }
            }
        }

        public virtual void StartDragging(DraggableElement unit)
        {
            dragStartPosition = Mouse.current.position.ReadValue();
            selectedUnit = unit;
            dragging = true;
        }
    }
}
