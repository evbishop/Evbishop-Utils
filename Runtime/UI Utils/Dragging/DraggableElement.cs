using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Evbishop.Runtime.UIUtils.Dragging
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class DraggableElement : MonoBehaviour
    {
        protected RectTransform rectTransform;
        [field: SerializeField] public Image DraggableImage { get; protected set; }
        [SerializeField] protected float draggableImageWidth;
        [SerializeField] protected float draggableImageHeight;

        protected Vector2 initialPosition;
        protected Transform initialParent;
        bool isMouseDragOverASpot = false;

        public Action<DraggableElement> OnStartDrag;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        protected virtual void Start()
        {
            initialPosition = rectTransform.anchoredPosition;
            initialParent = rectTransform.transform.parent;
        }

        public void OnPointerDown()
        {
            OnStartDrag?.Invoke(this);
        }

        public virtual void OnMouseReleased()
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            var spots = GetDraggableElementContainers();
            foreach (var spot in spots)
            {
                RectTransform spotRT = spot.RectTransform;
                Vector2 spotSize = Vector2.Scale(spotRT.rect.size, spotRT.lossyScale);
                Rect spotInCameraCoords = new((Vector2)spotRT.position - (spotSize * 0.5f), spotSize);

                if (mousePos.x >= spotInCameraCoords.xMin
                    && mousePos.x <= spotInCameraCoords.xMax
                    && mousePos.y >= spotInCameraCoords.yMin
                    && mousePos.y <= spotInCameraCoords.yMax)
                {
                    HandleDragFinishedOnAContainer(spot);
                    break;
                }
            }
        }

        public abstract void HandleDragFinishedOnAContainer(DraggableElementContainer container);

        public abstract IEnumerable<DraggableElementContainer> GetDraggableElementContainers();

        public virtual void StartDrag()
        {
            rectTransform.transform.SetParent(CanvasMain.Instance.transform);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, draggableImageWidth);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, draggableImageHeight);
            StartCoroutine(Drag());
        }

        protected IEnumerator Drag()
        {
            while (Mouse.current.leftButton.isPressed)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();

                rectTransform.anchoredPosition = new Vector2(
                    mousePos.x * CanvasMain.Instance.RectTransform.rect.width / Screen.width,
                    mousePos.y * CanvasMain.Instance.RectTransform.rect.height / Screen.height);

                bool isMouseDraggedOverASpotThisFrame = false;
                if (!isMouseDraggedOverASpotThisFrame && isMouseDragOverASpot)
                {
                    isMouseDragOverASpot = false;
                }

                yield return null;
            }
        }
    }
}