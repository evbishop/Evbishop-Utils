using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Evbishop.Runtime.UIUtils.Dragging
{
    public class DraggingOutOfScrollController : DraggingController
    {
        [SerializeField] float scrollFreezeOffset;
        [SerializeField] ScrollRect scrollRect;

        Vector2 scrollPositionOnDragStart;

        void Update()
        {
            HandleMouseRelease();

            if (!dragging)
                return;

            FreezeScrollRect();

            if (Mouse.current.position.ReadValue().y > dragStartPosition.y + popOffOffsetY &&
                Mouse.current.position.ReadValue().x - dragStartPosition.x < popOffOffsetX)
            {
                if (!poppedOut && selectedUnit != null)
                {
                    selectedUnit.StartDrag();
                    poppedOut = true;
                }
            }
        }

        void FreezeScrollRect()
        {
            if (Mouse.current.position.ReadValue().y > dragStartPosition.y + scrollFreezeOffset)
            {
                if (scrollPositionOnDragStart == Vector2.zero)
                {
                    scrollPositionOnDragStart = scrollRect.content.localPosition;
                }
                scrollRect.content.localPosition = scrollPositionOnDragStart;
            }
            else
            {
                scrollPositionOnDragStart = Vector2.zero;
            }
        }

        public override void StartDragging(DraggableElement unit)
        {
            base.StartDragging(unit);
            scrollPositionOnDragStart = scrollRect.content.localPosition;
        }
    }
}