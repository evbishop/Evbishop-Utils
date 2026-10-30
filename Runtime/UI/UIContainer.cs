using System;
using Evbishop.Runtime.ModulesSystem;
using Evbishop.Runtime.UI.Modules;
using Evbishop.Runtime.UI.Modules.Animator;
using Evbishop.Runtime.UI.Modules.Container;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Evbishop.Runtime.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UIContainer : UIBehaviour
    {
        private const string SELECTION = "Selection";
        private const string SELECTION_HORIZONTAL = "Selection/Horizontal";
        private const string ON_START = "On Start";
        private const string ON_START_HORIZONTAL = "On Start/Horizontal";
        private const string ON_HIDE = "On Hide";
        private const string ON_HIDE_HORIZONTAL = "On Hide/Horizontal";

        [field: SerializeField, ReadOnly] public EVisibility Visibility { get; private set; }

        [SerializeField, BoxGroup(ON_START), HorizontalGroup(ON_START_HORIZONTAL, Width = 200), HideLabel] private EStartBehaviour _startBehaviour = EStartBehaviour.None;
        [SerializeField, HorizontalGroup(ON_START_HORIZONTAL)] private bool _notify = false;

        [SerializeField, BoxGroup(ON_HIDE), HorizontalGroup(ON_HIDE_HORIZONTAL, Width = 200), LabelText("Disable game object on hide"), LabelWidth(160)] private bool _isDisablingGameObjectOnHide = true;
        [SerializeField, HorizontalGroup(ON_HIDE_HORIZONTAL), LabelText("Disable canvas on hide"), LabelWidth(160)] private bool _isDisablingCanvasOnHide = true;

        [SerializeField, BoxGroup(SELECTION), HorizontalGroup(SELECTION_HORIZONTAL, Width = 200), LabelText("Clear selected on show"), LabelWidth(160)] private bool _isClearingSelectedOnShow = false;
        [SerializeField, HorizontalGroup(SELECTION_HORIZONTAL), LabelText("Clear selected on hide"), LabelWidth(160)] private bool _isClearingSelectedOnHide = false;

        [SerializeField, FoldoutGroup(CALLBACKS)] public UnityEvent OnShow;
        [SerializeField, FoldoutGroup(CALLBACKS)] public UnityEvent OnShown;
        [SerializeField, FoldoutGroup(CALLBACKS)] public UnityEvent OnHide;
        [SerializeField, FoldoutGroup(CALLBACKS)] public UnityEvent OnHidden;

        [OdinSerialize] public UIModulesSystem Modules { get; private set; } = new();

        private int _workingAnimators;

        public Canvas Canvas { get; private set; }

        public bool IsVisible => Visibility == EVisibility.Showing || Visibility == EVisibility.Shown;
        public bool HasAnimations => Modules.HasInterface<IModuleAnimator>();

        protected UnityEvent _eventCleanup = new();

        protected override void Awake()
        {
            base.Awake();

            Canvas = GetComponent<Canvas>();
            foreach (var module in Modules.GetModulesWithInterface<IModuleAwakable>())
            {
                module.HandleAwake();
            }
            foreach (var module in Modules.GetModulesWithInterface<IModuleAwakable<UIContainer>>())
            {
                module.HandleAwake(this);
            }
            OnHide.AddListener(HandleHide);
        }

        protected override void Start()
        {
            base.Start();

            if (Modules.TryGetModule(out ContainerModuleCustomStartPosition moduleStartPosition))
                moduleStartPosition.SetPosition(transform);
            switch (_startBehaviour)
            {
                case EStartBehaviour.None:
                    Visibility = EVisibility.Shown;
                    break;
                case EStartBehaviour.InstantHide:
                    Hide(true, _notify);
                    break;
                case EStartBehaviour.InstantShow:
                    Show(true, _notify);
                    break;
                case EStartBehaviour.Hide:
                    Hide(false, _notify);
                    break;
                case EStartBehaviour.Show:
                    Show(false, _notify);
                    break;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var module in Modules.GetModulesWithInterface<IModuleDisposable>())
            {
                module.Dispose();
            }
            OnHide.RemoveListener(HandleHide);
        }

        protected virtual void HandleHide()
        {
            _eventCleanup?.Invoke();
            _eventCleanup.RemoveAllListeners();
        }

        [Button, ButtonGroup, GUIColor("cyan"), ShowIf("@UnityEngine.Application.isPlaying")]
        public void Show(bool instant = false)
        {
            Show(instant, true);
        }

        public void Show(bool instant, bool notify)
        {
            if (_isClearingSelectedOnShow)
                EventSystem.current.SetSelectedGameObject(null);

            _workingAnimators = 0;
            Visibility = EVisibility.Showing;
            if (notify)
                OnShow?.Invoke();

            gameObject.SetActive(true);
            if (_isDisablingCanvasOnHide)
            {
                if (Canvas == null)
                    Canvas = GetComponent<Canvas>();
                Canvas.enabled = true;
            }
            if (Modules.TryGetModule(out ContainerModuleGraphicRaycaster grModule) &&
                grModule.IsDisablingGraphicRaycasterOnHide)
                grModule.Component.enabled = true;

            if (instant)
            {
                Visibility = EVisibility.Shown;
                if (notify)
                    OnShown?.Invoke();
            }
            else
            {
                if (Modules.HasInterface<IModuleAnimator>())
                {
                    foreach (var animator in Modules.GetModulesWithInterface<IModuleAnimator>())
                    {
                        _workingAnimators++;
                        if (animator.OnPlayFinished == null)
                        {
                            animator.OnPlayFinished = new();
                        }
                        animator.OnPlayFinished.AddListener(HandleAnimatorShowFinished);
                        animator.Play(notify);
                    }
                }
                else
                {
                    Visibility = EVisibility.Shown;
                    if (notify)
                        OnShown?.Invoke();
                }
            }
        }

        [Button, ButtonGroup, GUIColor(1f, 0.7f, 0.7f), ShowIf("@UnityEngine.Application.isPlaying")]
        public void Hide(bool instant = false)
        {
            Hide(instant, true);
        }

        public void Hide(bool instant, bool notify)
        {
            if (_isClearingSelectedOnHide && EventSystem.current)
                EventSystem.current.SetSelectedGameObject(null);

            _workingAnimators = 0;
            Visibility = EVisibility.Hiding;
            if (notify)
                OnHide?.Invoke();

            if (instant)
            {
                InstantHide(notify);
                if (Modules.HasInterface<IModuleAnimator>())
                {
                    foreach (var animator in Modules.GetModulesWithInterface<IModuleAnimator>())
                    {
                        animator.ReverseInstant(notify);
                    }
                }
            }
            else
            {
                if (Modules.HasInterface<IModuleAnimator>())
                {
                    foreach (var animator in Modules.GetModulesWithInterface<IModuleAnimator>())
                    {
                        _workingAnimators++;
                        if (animator.OnPlayReverseFinished == null)
                        {
                            animator.OnPlayReverseFinished = new();
                        }
                        animator.OnPlayReverseFinished.AddListener(HandleAnimatorHideFinished);
                        animator.PlayReverse(notify);
                    }
                }
                else
                {
                    InstantHide(notify);
                }
            }
        }

        private void InstantHide(bool notify = true)
        {
            if (_isDisablingCanvasOnHide)
            {
                if (Canvas == null)
                    Canvas = GetComponent<Canvas>();
                Canvas.enabled = false;
            }
            if (_isDisablingGameObjectOnHide)
                gameObject.SetActive(false);
            if (Modules.TryGetModule(out ContainerModuleGraphicRaycaster grModule) &&
                grModule.IsDisablingGraphicRaycasterOnHide)
                grModule.Component.enabled = false;

            Visibility = EVisibility.Hidden;
            if (notify)
                OnHidden?.Invoke();
        }

        public void HandleAnimatorShowFinished(AnimatorModule module)
        {
            module.OnPlayFinished.RemoveListener(HandleAnimatorShowFinished);
            _workingAnimators--;
            if (_workingAnimators < 1)
            {
                Visibility = EVisibility.Shown;
                if (module.Notify)
                    OnShown?.Invoke();
            }
        }

        public void HandleAnimatorHideFinished(AnimatorModule module)
        {
            module.OnPlayReverseFinished.RemoveListener(HandleAnimatorHideFinished);
            _workingAnimators--;
            if (_workingAnimators < 1)
            {
                InstantHide(module.Notify);
            }
        }

        protected void AddToCleanup(UnityEvent eventToRemoveFrom, UnityAction actionToRemove)
        {
            _eventCleanup.AddListener(() => eventToRemoveFrom.RemoveListener(actionToRemove));
        }

        public void AddListenerUntilHidden(UnityEvent eventToAddTo, UnityAction actionToAdd)
        {
            eventToAddTo.AddListener(actionToAdd);
            AddToCleanup(eventToAddTo, actionToAdd);
        }
    }
}
