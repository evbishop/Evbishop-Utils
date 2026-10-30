using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Evbishop.Runtime.Pooling;
using JEGStudios.UI;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Evbishop.Runtime.UI
{
    [RequireComponent(typeof(PoolItem))]
    public class UIPopup : UIContainer
    {
        private const int MAX_SORT_ORDER = 32766;
        private const string SETTINGS = "Popup Settings";
        private const string REFERENCES = "Popup References";

        [SerializeField, TabGroup(SETTINGS)] private bool _setMaxSortOrder = true;
        [SerializeField, TabGroup(SETTINGS)] private bool _blockBackButton = true;
        [SerializeField, TabGroup(SETTINGS)] private bool _hideByAnyButton = true;
        [field: SerializeField, TabGroup(SETTINGS)] public bool HideByBackButton { get; private set; }

        [field: SerializeField, TabGroup(REFERENCES)] public List<TMP_Text> Texts { get; private set; } = new();
        [field: SerializeField, TabGroup(REFERENCES)] public List<Image> Images { get; private set; } = new();
        [field: SerializeField, TabGroup(REFERENCES)] public List<UIButton> Buttons { get; private set; } = new();
        [field: SerializeField, TabGroup(REFERENCES)] public List<TMP_InputField> InputFields { get; private set; } = new();

        private PoolItem _poolItem;
        private Vector3 _originalScale;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        protected override void Awake()
        {
            base.Awake();

            _originalScale = transform.localScale;
            _originalPosition = transform.localPosition;
            _originalRotation = transform.localRotation;
            _poolItem = GetComponent<PoolItem>();
            OnShow.AddListener(HandleShow);
            OnHidden.AddListener(HandleHidden);
            if (_hideByAnyButton)
            {
                foreach (var button in Buttons)
                {
                    button.AddOnClickAction(() => Hide());
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            OnShow.RemoveListener(HandleShow);
            OnHide.RemoveListener(HandleHidden);
        }

        private void HandleShow()
        {
            ResetTransformProperties();
            if (_setMaxSortOrder)
                ApplyOverrideSortingAsync().Forget();
            if (_blockBackButton)
            {
                UIHelper.Instance.BlockBackButton(this);
            }
            else if (HideByBackButton)
            {
                UIHelper.Instance.OnBackButtonPressed += HandleBackButtonPressed;
            }
        }

        protected override void HandleHide()
        {
            base.HandleHide();

            if (_blockBackButton)
            {
                UIHelper.Instance.UnblockBackButton(this);
            }
            else if (HideByBackButton)
            {
                UIHelper.Instance.OnBackButtonPressed -= HandleBackButtonPressed;
            }
        }

        private void HandleHidden()
        {
            _poolItem.Release();
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

        protected void AddButtonClickToCleanup(int i, UnityAction action)
        {
            _eventCleanup.AddListener(() => Buttons[i].RemoveOnClickAction(action));
        }

        public void AddListenerToButtonUntilHidden(int buttonIndex, UnityAction action)
        {
            Buttons[buttonIndex].AddOnClickAction(action);
            AddButtonClickToCleanup(buttonIndex, action);
        }
    }
}