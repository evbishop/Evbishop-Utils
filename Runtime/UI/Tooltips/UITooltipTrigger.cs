using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Evbishop.Runtime.UI.Tooltips
{
    [DisallowMultipleComponent]
    public class UITooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private const string SHOW = "Show";
        private const string HIDE = "Hide";
        private const string CALLBACKS = "Callbacks";

        [SerializeField, AssetsOnly] private UITooltipInfo _tooltipInfo = null;
        [SerializeField] private float _showDelay;
        [SerializeField, HorizontalGroup(SHOW, 190), LabelWidth(150)] private bool _showOnPointerEnter = true;
        [SerializeField, HorizontalGroup(HIDE, 190), LabelWidth(150)] private bool _hideOnPointerExit = true;
        [SerializeField, HorizontalGroup(SHOW, 190), LabelWidth(150)] private bool _showOnPointerClick = false;
        [SerializeField, HorizontalGroup(HIDE, 190), LabelWidth(150)] private bool _hideOnPointerClick = false;
        [SerializeField, EnumToggleButtons] private ETooltipParenting _parentMode = ETooltipParenting.TooltipsCanvas;
        [SerializeField, EnumToggleButtons] private ETooltipTracking _trackingMode = ETooltipTracking.Disabled;
        [SerializeField, ShowIf("@_trackingMode == ETooltipTracking.FollowTarget"), Tooltip("If tracking mode is set to 'Follow Target', this is the target to follow.")] private RectTransform _followTarget = null;
        [SerializeField] private ETooltipPositioning _positionMode = ETooltipPositioning.MiddleCenter;
        [SerializeField] private Vector3 _positionOffset = Vector3.zero;
        [SerializeField, PropertySpace(5, 0)] private bool _overrideMaximumWidth = false;
        [SerializeField, ShowIf(nameof(_overrideMaximumWidth))] private float _maximumWidth = 0f;
        [SerializeField, PropertySpace(5, 0)] private bool _overrideTexts = false;
        [field: SerializeField, ShowIf(nameof(_overrideTexts)), PropertySpace(0, 5)] public string[] Texts { get; set; } = null;
        [SerializeField] private bool _overrideSprites = false;
        [field: SerializeField, ShowIf(nameof(_overrideSprites)), PropertySpace(0, 5)] public Sprite[] Sprites { get; set; } = null;
        [field: SerializeField, PropertySpace(5, 5)] public UnityEvent[] TooltipEvents { get; private set; }

        [SerializeField, FoldoutGroup(CALLBACKS)] public UnityEvent OnShow = new();
        [SerializeField, FoldoutGroup(CALLBACKS)] public UnityEvent OnHidden = new();

        private CancellationTokenSource _ctsShowDelay;

        public UITooltip Tooltip { get; private set; }

        public float ShowDelay
        {
            get => _showDelay;
            set => _showDelay = Mathf.Max(0, value);
        }

        private void OnDisable()
        {
            CancelShowDelay();
            HideTooltip();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            CancelShowDelay();

            if (!_showOnPointerEnter)
                return;

            if (ShowDelay > 0)
            {
                ShowDelayAsync().Forget();
                return;
            }
            ShowTooltipAsync().Forget();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CancelShowDelay();

            if (!_hideOnPointerExit)
                return;

            HideTooltip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            CancelShowDelay();

            if (_showOnPointerClick && Tooltip == null)
            {
                ShowTooltipAsync().Forget();
                return;
            }

            if (_hideOnPointerClick)
                HideTooltip();
        }

        private void CancelShowDelay()
        {
            if (_ctsShowDelay != null)
            {
                _ctsShowDelay.Cancel();
                _ctsShowDelay = null;
            }
        }

        private async UniTaskVoid ShowDelayAsync()
        {
            CancelShowDelay();
            _ctsShowDelay = new();

            bool isCancelled = await UniTask
                .WaitForSeconds(ShowDelay, cancellationToken: _ctsShowDelay.Token)
                .SuppressCancellationThrow();
            if (isCancelled) return;

            _ctsShowDelay = null;
            ShowTooltipAsync().Forget();
        }

        public async UniTask<UITooltip> ShowTooltipAsync()
        {
            CancelShowDelay();

            if (Tooltip != null)
            {
                if (Tooltip.IsVisible)
                    return Tooltip;

                Tooltip.Hide(true);
                Tooltip = null;
            }

            if (UICanvasTooltips.Instance == null)
                return null;
            Transform parent = transform;
            if (_parentMode == ETooltipParenting.TooltipsCanvas)
                parent = UICanvasTooltips.Instance.transform;
            Tooltip = await UICanvasTooltips.Instance.GetTooltipAsync(_tooltipInfo, parent);
            if (Tooltip == null)
                return null;

            SetupTooltip();
            Tooltip.Show();
            OnShow?.Invoke();
            return Tooltip;
        }

        private void SetupTooltip()
        {
            Tooltip.ParentRectTransform = Tooltip.transform.parent.GetComponent<RectTransform>();
            Tooltip.Trigger = this;
            Tooltip.TrackingMode = _trackingMode;
            Tooltip.PositionMode = _positionMode;
            Tooltip.PositionOffset = _positionOffset;
            if (_trackingMode == ETooltipTracking.FollowTarget)
                Tooltip.SetFollowTarget(_followTarget);
            if (_overrideMaximumWidth)
                Tooltip.MaximumWidth = _maximumWidth;
            if (_overrideTexts)
                Tooltip.SetTexts(Texts);
            else
                Tooltip.SetTexts(_tooltipInfo.Texts);
            if (_overrideSprites)
                Tooltip.SetSprites(Sprites);
            else
                Tooltip.SetSprites(_tooltipInfo.Sprites);
            Tooltip.SetEvents(TooltipEvents);
        }

        public void HideTooltip()
        {
            CancelShowDelay();

            if (Tooltip == null || Tooltip.Visibility == EVisibility.Hiding || Tooltip.Visibility == EVisibility.Hidden)
                return;

            Tooltip.OnShow.AddListener(HandleShownInsteadOfHidden);
            Tooltip.OnHidden.AddListener(HandleHidden);
            Tooltip.Hide();
        }

        public UITooltipTrigger SetTexts(params string[] texts)
        {
            Texts = texts;
            return this;
        }

        public UITooltipTrigger SetSprites(params Sprite[] sprites)
        {
            Sprites = sprites;
            return this;
        }

        public UITooltipTrigger SetFollowTarget(RectTransform followTarget)
        {
            _followTarget = followTarget;
            return this;
        }

        private void HandleHidden()
        {
            if (Tooltip == null)
                return;

            Tooltip.OnHidden.RemoveListener(HandleHidden);
            Tooltip = null;
            OnHidden?.Invoke();
        }

        private void HandleShownInsteadOfHidden()
        {
            if (Tooltip == null)
                return;

            Tooltip.OnShow.RemoveListener(HandleShownInsteadOfHidden);
            Tooltip.OnHidden.RemoveListener(HandleHidden);
        }
    }
}
