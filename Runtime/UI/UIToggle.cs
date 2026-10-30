using System;
using TheraBytes.BetterUi;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Evbishop.Runtime.UI
{
    [RequireComponent(typeof(BetterToggle))]
    public class UIToggle : UIBehaviour, IPointerUpHandler
    {
        [SerializeField] private bool _deselectAfterPress = false;

        private BetterToggle _toggle;

        public ToggleGroup Group
        {
            get
            {
                _toggle ??= GetComponent<BetterToggle>();
                return _toggle.group;
            }
        }

        public bool IsOn
        {
            get
            {
                _toggle ??= GetComponent<BetterToggle>();
                return _toggle.isOn;
            }
        }

        public bool IsInteractable
        {
            get => _toggle.interactable;
            set => _toggle.interactable = value;
        }

        public event Action<bool> OnValueChanged;

        protected override void Awake()
        {
            base.Awake();

            _toggle ??= GetComponent<BetterToggle>();
            _toggle.onValueChanged.AddListener((value) =>
            {
                if (_deselectAfterPress)
                {
                    EventSystem.current?.SetSelectedGameObject(null);
                }
                OnValueChanged?.Invoke(value);
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_toggle != null && _toggle.group != null && _toggle.group is UIToggleGroup toggleGroup)
            {
                toggleGroup.RemoveToggle(this);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_deselectAfterPress && !eventData.hovered.Contains(gameObject))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void SetIsOn(bool state, bool notify = true)
        {
            if (_toggle == null)
                return;
            if (notify)
            {
                _toggle.isOn = state;
            }
            else
            {
                _toggle.SetIsOnWithoutNotify(state);
            }
        }

        public void AddToToggleGroup(UIToggleGroup toggleGroup)
        {
            _toggle ??= GetComponent<BetterToggle>();
            _toggle.group = toggleGroup;
            toggleGroup.AddToggle(this);
        }
    }
}