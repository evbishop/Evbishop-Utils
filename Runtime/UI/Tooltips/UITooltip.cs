using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Evbishop.Runtime.Pooling;
using JEGStudios.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Evbishop.Runtime.UI.Tooltips
{
    [RequireComponent(typeof(PoolItem))]
    [DisallowMultipleComponent]
    public class UITooltip : UIContainer
    {
        public const int MAX_SORT_ORDER = 32767;

        public static Vector2 PointerPosition => Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

        [SerializeField] public List<TextMeshProUGUI> Texts = new();
        [SerializeField] public List<Image> Images = new();
        [SerializeField] public List<UIButton> Buttons = new();
        [SerializeField] private bool _keepInScreen = true;
        [SerializeField] private bool _setMaxSortOrder = true;
        [SerializeField] private bool _hideOnAnyButton = true;
        [SerializeField] private bool _hideOnBackButton = true;
        [field: SerializeField] public float MaximumWidth { get; set; }

        private PoolItem _poolItem;
        private bool _addedHideEventToButtons;
        private LayoutElement _layoutElement;
        private UITooltipTrigger _trigger;
        private Canvas _tooltipRootCanvas;
        private RectTransform _targetRectTransform;
        private Vector3 _originalScale;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private float _originalMaximumWidth;
        private RectTransform _triggerRectTransform;
        private RectTransform _followTarget;

        public ETooltipTracking TrackingMode { get; set; }
        internal ETooltipPositioning PositionMode { get; set; }
        internal Vector3 PositionOffset { get; set; }
        internal RectTransform ParentRectTransform { get; set; }
        internal RectTransform TooltipRootCanvasRectTransform { get; private set; }
        internal Canvas TargetRootCanvas { get; private set; }
        internal RectTransform TargetRootCanvasRectTransform { get; private set; }

        private float Width => RectTransform.rect.width;
        private float Height => RectTransform.rect.height;
        private float PivotX => RectTransform.pivot.x;
        private float PivotY => RectTransform.pivot.y;
        private LayoutElement LayoutElement =>
            _layoutElement ?
                _layoutElement :
                _layoutElement = GetComponent<LayoutElement>() ??
                    gameObject.AddComponent<LayoutElement>();

        internal UITooltipTrigger Trigger
        {
            get => _trigger;
            set
            {
                _triggerRectTransform = null;
                _trigger = value;
                if (value == null)
                    return;
                _triggerRectTransform = value.GetComponent<RectTransform>();
            }
        }

        internal Canvas TooltipRootCanvas
        {
            get => _tooltipRootCanvas;
            set
            {
                TooltipRootCanvasRectTransform = null;
                _tooltipRootCanvas = value;
                if (value == null)
                    return;
                TooltipRootCanvasRectTransform = value.GetComponent<RectTransform>();
            }
        }

        internal RectTransform TargetRectTransform
        {
            get => _targetRectTransform;
            set
            {
                TargetRootCanvas = null;
                TargetRootCanvasRectTransform = null;
                _targetRectTransform = value;
                if (value == null)
                    return;
                TargetRootCanvas = value.GetComponentInParent<Canvas>().rootCanvas;
                TargetRootCanvasRectTransform = TargetRootCanvas.GetComponent<RectTransform>();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _originalScale = transform.localScale;
            _originalPosition = transform.localPosition;
            _originalRotation = transform.localRotation;
            _originalMaximumWidth = MaximumWidth;
            _poolItem = GetComponent<PoolItem>();
            _poolItem.OnGet += HandleGet;
            _addedHideEventToButtons = false;
            OnShow.AddListener(HandleShow);
            OnHidden.AddListener(HandleHidden);
            ApplyHideOnAnyButton();
            if (_setMaxSortOrder)
                ApplyOverrideSortingAsync().Forget();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            OnShow.RemoveListener(HandleShow);
            OnHidden.RemoveListener(HandleHidden);
        }

        private void LateUpdate()
        {
            if (!IsVisible)
                return;

            ApplyTracking();
            ApplyPositioning();
            ApplyKeepInScreen();
        }

        private void HandleGet()
        {
            MaximumWidth = _originalMaximumWidth;
        }

        private void HandleShow()
        {
            TooltipRootCanvas = Canvas.rootCanvas;
            ResetTransformProperties();
            UpdateTarget();
            ApplyMaximumWidth();
            if (_hideOnBackButton)
            {
                UIHelper.Instance.OnBackButtonPressed += HandleBackButtonPressed;
            }
            ApplyTracking();
            ApplyPositioning();
            ApplyKeepInScreen();
            LayoutRebuilder.ForceRebuildLayoutImmediate(Canvas.transform as RectTransform);
        }

        protected override void HandleHide()
        {
            base.HandleHide();

            if (_hideOnBackButton)
            {
                UIHelper.Instance.OnBackButtonPressed -= HandleBackButtonPressed;
            }
        }

        private void HandleHidden()
        {
            _poolItem.Release();
        }

        internal void SetFollowTarget(RectTransform followTarget)
        {
            _followTarget = followTarget;
        }

        private void HandleBackButtonPressed()
        {
            Hide();
        }

        private void ResetTransformProperties()
        {
            transform.localScale = _originalScale;
            transform.localPosition = _originalPosition;
            transform.localRotation = _originalRotation;
        }

        private async UniTaskVoid ApplyOverrideSortingAsync()
        {
            await UniTask.Yield();
            Canvas.overrideSorting = true;
            Canvas.sortingOrder = MAX_SORT_ORDER;
        }

        public void UpdateTarget()
        {
            TargetRectTransform = null;

            switch (TrackingMode)
            {
                case ETooltipTracking.Disabled:
                case ETooltipTracking.FollowPointer:
                    return;
                case ETooltipTracking.FollowTrigger:
                    TargetRectTransform = _triggerRectTransform;
                    break;
                case ETooltipTracking.FollowTarget:
                    TargetRectTransform = _followTarget;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ApplyHideOnAnyButton()
        {
            if (!_hideOnAnyButton ||
                _addedHideEventToButtons ||
                Buttons.Count == 0)
                return;

            foreach (var button in Buttons)
            {
                if (button != null)
                {
                    button.AddOnClickAction(() => Hide());
                }
            }

            _addedHideEventToButtons = true;
        }

        private void ApplyTracking()
        {
            Vector3 targetPosition;

            switch (TrackingMode)
            {
                case ETooltipTracking.Disabled:
                    return;
                case ETooltipTracking.FollowPointer:
                    targetPosition = PointerPosition;
                    break;
                case ETooltipTracking.FollowTrigger:
                    if (Trigger == null)
                    {
                        TrackingMode = ETooltipTracking.Disabled;
                        return;
                    }
                    targetPosition = Trigger.transform.position;
                    break;
                case ETooltipTracking.FollowTarget:
                    if (_followTarget == null)
                    {
                        TrackingMode = ETooltipTracking.Disabled;
                        return;
                    }
                    targetPosition = _followTarget.transform.position;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            transform.position = targetPosition;
        }

        private void ApplyPositioning()
        {
            if (transform.parent == null || TooltipRootCanvas == null)
                return;
            var calculatedTargetPosition = TrackingMode switch
            {
                ETooltipTracking.Disabled => CalculatePositioningWhenTrackingIsDisabled(),
                ETooltipTracking.FollowPointer => CalculatePositioningWhenTrackingIsFollowPointer(),
                ETooltipTracking.FollowTrigger or ETooltipTracking.FollowTarget => CalculatePositioningWhenTrackingIsEnabled(),
                _ => throw new ArgumentOutOfRangeException(),
            };
            calculatedTargetPosition += PositionOffset;

            calculatedTargetPosition.x = float.IsNaN(calculatedTargetPosition.x) ? 0 : calculatedTargetPosition.x;
            calculatedTargetPosition.y = float.IsNaN(calculatedTargetPosition.y) ? 0 : calculatedTargetPosition.y;
            calculatedTargetPosition.z = float.IsNaN(calculatedTargetPosition.z) ? 0 : calculatedTargetPosition.z;

            calculatedTargetPosition.x = float.IsInfinity(calculatedTargetPosition.x) ? 0 : calculatedTargetPosition.x;
            calculatedTargetPosition.y = float.IsInfinity(calculatedTargetPosition.y) ? 0 : calculatedTargetPosition.y;
            calculatedTargetPosition.z = float.IsInfinity(calculatedTargetPosition.z) ? 0 : calculatedTargetPosition.z;

            RectTransform.anchoredPosition3D = calculatedTargetPosition;
        }

        private void ApplyKeepInScreen()
        {
            if (!_keepInScreen || TooltipRootCanvasRectTransform == null)
                return;

            var cCorners = new Vector3[4];
            TooltipRootCanvasRectTransform.GetWorldCorners(cCorners);
            Vector3 cBottomLeft = cCorners[0];
            Vector3 cTopRight = cCorners[2];
            Vector3 cSize = cTopRight - cBottomLeft;
            RectTransform.GetWorldCorners(cCorners);
            Vector3 tBottomLeft = cCorners[0];
            Vector3 tTopRight = cCorners[2];
            Vector3 tSize = tTopRight - tBottomLeft;
            Vector3 tPosition = RectTransform.position;
            Vector3 deltaBottomLeft = tPosition - tBottomLeft;
            Vector3 deltaTopRight = tTopRight - tPosition;
            tPosition.x = tSize.x < cSize.x
                ? Mathf.Clamp(tPosition.x, cBottomLeft.x + deltaBottomLeft.x, cTopRight.x - deltaTopRight.x)
                : Mathf.Clamp(tPosition.x, cTopRight.x - deltaTopRight.x, cBottomLeft.x + deltaBottomLeft.x);
            tPosition.y = tSize.y < cSize.y
                ? Mathf.Clamp(tPosition.y, cBottomLeft.y + deltaBottomLeft.y, cTopRight.y - deltaTopRight.y)
                : Mathf.Clamp(tPosition.y, cTopRight.y - deltaTopRight.y, cBottomLeft.y + deltaBottomLeft.y);
            RectTransform.position = tPosition;
        }

        private void ApplyMaximumWidth()
        {
            if (MaximumWidth <= 0)
                return;
            LayoutElement.preferredWidth = -1;
            LayoutElement.enabled = false;
            RectTransform.ForceUpdateRectTransforms();
            foreach (TextMeshProUGUI label in Texts)
            {
                if (label != null)
                    label.ForceMeshUpdate();
            }
            float maxLabelWidth = Texts.Where(l => l != null).Max(label => label.preferredWidth);
            if (maxLabelWidth < MaximumWidth)
                return;
            LayoutElement.enabled = true;
            LayoutElement.preferredWidth = MaximumWidth;
            RectTransform.ForceUpdateRectTransforms();
        }

        private Vector3 CalculatePositioningWhenTrackingIsDisabled()
        {
            if (transform.parent == null || ParentRectTransform == null)
                return RectTransform.anchoredPosition3D;
            float z = RectTransform.anchoredPosition3D.z;
            Rect parentRect = ParentRectTransform.rect;
            float parentWidth = parentRect.width;
            float parentHeight = parentRect.height;

            return PositionMode switch
            {
                ETooltipPositioning.TopLeft => new Vector3(-parentWidth / 2f + Width * PivotX, parentHeight / 2f - Height * PivotY, z),
                ETooltipPositioning.TopCenter => new Vector3(0f, parentHeight / 2f - Height * PivotY, z),
                ETooltipPositioning.TopRight => new Vector3(parentWidth / 2f - Width * (1 - PivotX), parentHeight / 2f - Height * PivotY, z),
                ETooltipPositioning.MiddleLeft => new Vector3(-parentWidth / 2f + Width * PivotX, 0f, z),
                ETooltipPositioning.MiddleCenter => new Vector3(0f, 0f, z),
                ETooltipPositioning.MiddleRight => new Vector3(parentWidth / 2f - Width * (1 - PivotX), 0f, z),
                ETooltipPositioning.BottomLeft => new Vector3(-parentWidth / 2f + Width * PivotX, -parentHeight / 2f + Height * (1 - PivotY), z),
                ETooltipPositioning.BottomCenter => new Vector3(0f, -parentHeight / 2f + Height * (1 - PivotY), z),
                ETooltipPositioning.BottomRight => new Vector3(parentWidth / 2f - Width * (1 - PivotX), -parentHeight / 2f + Height * (1 - PivotY), z),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private Vector3 CalculatePositioningWhenTrackingIsFollowPointer()
        {
            Vector2 point;

            switch (TooltipRootCanvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    point = ParentRectTransform.InverseTransformPoint(PointerPosition);
                    break;
                case RenderMode.ScreenSpaceCamera:
                case RenderMode.WorldSpace:
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentRectTransform, PointerPosition, TooltipRootCanvas.worldCamera, out point);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            point -= ParentRectTransform.rect.center;

            point = PositionMode switch
            {
                ETooltipPositioning.TopLeft => new Vector3(point.x - Width * PivotX, point.y + Height * PivotY, 0),
                ETooltipPositioning.TopCenter => new Vector3(point.x, point.y + Height * PivotY, 0),
                ETooltipPositioning.TopRight => new Vector3(point.x + Width * (1 - PivotX), point.y + Height * PivotY, 0),
                ETooltipPositioning.MiddleLeft => new Vector3(point.x - Width * PivotX, point.y, 0),
                ETooltipPositioning.MiddleCenter => new Vector3(point.x, point.y, 0),
                ETooltipPositioning.MiddleRight => new Vector3(point.x + Width * (1 - PivotX), point.y, 0),
                ETooltipPositioning.BottomLeft => new Vector3(point.x - Width * PivotX, point.y - Height * (1 - PivotY), 0),
                ETooltipPositioning.BottomCenter => new Vector3(point.x, point.y - Height * (1 - PivotY), 0),
                ETooltipPositioning.BottomRight => new Vector3(point.x + Width * (1 - PivotX), point.y - Height * (1 - PivotY), 0),
                _ => throw new ArgumentOutOfRangeException()
            };

            return point;
        }

        private Vector3 CalculatePositioningWhenTrackingIsEnabled()
        {
            Vector3 point = ParentRectTransform.InverseTransformPoint(TargetRectTransform.position);
            Vector3 targetOffset = GetPositionOffset(TargetRectTransform, PositionMode);
            Vector3 localScale = TargetRectTransform.localScale;
            targetOffset.x *= localScale.x;
            targetOffset.y *= localScale.y;
            Vector3 targetCanvasScale = TargetRectTransform.lossyScale;
            Vector3 tooltipCanvasScale = RectTransform.lossyScale;
            var scaleDiff = new Vector3(targetCanvasScale.x / tooltipCanvasScale.x, targetCanvasScale.y / tooltipCanvasScale.y, targetCanvasScale.z / tooltipCanvasScale.z);
            targetOffset.x *= scaleDiff.x;
            targetOffset.y *= scaleDiff.y;
            targetOffset.z *= scaleDiff.z;
            point += targetOffset;
            Vector3 tooltipOffset = GetPositionOffset(RectTransform, PositionMode);
            point += tooltipOffset;
            return point;
        }

        private static Vector3 GetPositionOffset(RectTransform rectTransform, ETooltipPositioning positionMode)
        {
            Rect rect = rectTransform.rect;
            return positionMode switch
            {
                ETooltipPositioning.TopLeft => new Vector3(rect.xMin, rect.yMax, 0),
                ETooltipPositioning.TopCenter => new Vector3(rect.center.x, rect.yMax, 0),
                ETooltipPositioning.TopRight => new Vector3(rect.xMax, rect.yMax, 0),
                ETooltipPositioning.MiddleLeft => new Vector3(rect.xMin, rect.center.y, 0),
                ETooltipPositioning.MiddleCenter => new Vector3(rect.center.x, rect.center.y, 0),
                ETooltipPositioning.MiddleRight => new Vector3(rect.xMax, rect.center.y, 0),
                ETooltipPositioning.BottomLeft => new Vector3(rect.xMin, rect.yMin, 0),
                ETooltipPositioning.BottomCenter => new Vector3(rect.center.x, rect.yMin, 0),
                ETooltipPositioning.BottomRight => new Vector3(rect.xMax, rect.yMin, 0),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        internal void SetTexts(params string[] texts)
        {
            int textsCount = texts.Length;
            if (textsCount == 0)
                return;
            for (int i = 0; i < Texts.Count; i++)
            {
                TextMeshProUGUI label = Texts[i];
                if (label == null)
                    continue;
                label.SetText(i < textsCount ? texts[i] : string.Empty);
                label.ForceMeshUpdate();
            }
        }

        internal void SetSprites(params Sprite[] sprites)
        {
            int spritesCount = sprites.Length;
            if (spritesCount == 0)
                return;
            for (int i = 0; i < Images.Count; i++)
            {
                Image image = Images[i];
                if (image == null)
                    continue;
                image.sprite = i < spritesCount ? sprites[i] : null;
            }
        }

        internal void SetEvents(params UnityEvent[] events)
        {
            if (events == null || events.Length == 0)
                return;
            bool hasGraphicRaycaster = GetComponentInChildren<GraphicRaycaster>();
            for (int i = 0; i < Buttons.Count; i++)
            {
                var button = Buttons[i];
                if (button == null)
                    continue;
                if (!hasGraphicRaycaster && !button.GetComponent<GraphicRaycaster>())
                    button.gameObject.AddComponent<GraphicRaycaster>();
                if (events.Length <= i)
                    continue;
                var evt = events[i];
                if (evt == null)
                    continue;
                button.AddOnClickAction(evt.Invoke);
            }
        }

        internal void SetEvents(params UnityAction[] actions)
        {
            int actionsCount = actions.Length;
            if (actionsCount == 0)
                return;
            bool hasGraphicRaycaster = GetComponentInChildren<GraphicRaycaster>();
            for (int i = 0; i < Buttons.Count; i++)
            {
                var button = Buttons[i];
                if (button == null)
                    continue;
                if (!hasGraphicRaycaster && !button.GetComponent<GraphicRaycaster>())
                    button.gameObject.AddComponent<GraphicRaycaster>();
                if (actionsCount <= i)
                    continue;
                var action = actions[i];
                if (action == null)
                    continue;
                button.AddOnClickAction(action.Invoke);
            }
        }
    }
}
