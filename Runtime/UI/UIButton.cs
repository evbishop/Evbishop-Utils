﻿using Evbishop.Runtime.ModulesSystem;
using Evbishop.Runtime.UI.Modules;
using Evbishop.Runtime.UI.Modules.Animator;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Linq;
using TheraBytes.BetterUi;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Evbishop.Runtime.UI
{
    [RequireComponent(typeof(BetterButton))]
    public class UIButton : UIBehaviour, IPointerUpHandler
    {
        [SerializeField, ReadOnly] private ESelectableState _currentState;

        [SerializeField] private bool _deselectAfterPress;

        [OdinSerialize, FoldoutGroup(CALLBACKS)] public UIModulesSystem ModulesNormal { get; private set; } = new();
        [OdinSerialize, FoldoutGroup(CALLBACKS)] public UIModulesSystem ModulesHighlighted { get; private set; } = new();
        [OdinSerialize, FoldoutGroup(CALLBACKS)] public UIModulesSystem ModulesPressed { get; private set; } = new();
        [OdinSerialize, FoldoutGroup(CALLBACKS)] public UIModulesSystem ModulesSelected { get; private set; } = new();
        [OdinSerialize, FoldoutGroup(CALLBACKS)] public UIModulesSystem ModulesDisabled { get; private set; } = new();

        private BetterButton _button;

        public UIModulesSystem[] Modules { get; private set; } = new UIModulesSystem[5];

        protected override void Awake()
        {
            base.Awake();

            Modules[0] = ModulesNormal;
            Modules[1] = ModulesHighlighted;
            Modules[2] = ModulesPressed;
            Modules[3] = ModulesSelected;
            Modules[4] = ModulesDisabled;
            for (int i = 0; i < Modules.Length; i++)
            {
                foreach (var module in Modules[i].GetModulesWithInterface<IModuleAwakable>())
                {
                    module.HandleAwake();
                }
            }

            _button = GetComponent<BetterButton>();
            _button.onClick.AddListener(() =>
            {
                if (_deselectAfterPress)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            });

            var transition = _button.BetterTransitions.FirstOrDefault(t => t.Mode == Transitions.TransitionMode.CustomCallback);
            if (transition == null)
            {
                transition = new Transitions(Transitions.SelectionStateNames);
                transition.SetMode(Transitions.TransitionMode.CustomCallback);
                _button.BetterTransitions.Add(transition);
            }

            foreach (var state in ((CustomTransitions)transition.TransitionStates)
                .GetStates())
            {
                var e = state.StateObject;
                switch (state.Name)
                {
                    case nameof(ESelectableState.Normal):
                        e.AddListener(HandleStateNormal);
                        break;
                    case nameof(ESelectableState.Highlighted):
                        e.AddListener(HandleStateHighlighted);
                        break;
                    case nameof(ESelectableState.Pressed):
                        e.AddListener(HandleStatePressed);
                        break;
                    case nameof(ESelectableState.Selected):
                        e.AddListener(HandleStateSelected);
                        break;
                    case nameof(ESelectableState.Disabled):
                        e.AddListener(HandleStateDisabled);
                        break;
                }
            }
        }

        private void HandleState(UIModulesSystem modules)
        {
            for (int i = 0; i < Modules.Length; i++)
            {
                if (Modules[i] == modules)
                {
                    foreach (var module in Modules[i].GetModulesWithInterface<IModuleAnimator>())
                    {
                        module.Play();
                    }
                }
                else
                {
                    foreach (var module in Modules[i].GetModulesWithInterface<IModuleAnimator>())
                    {
                        module.Cancel();
                    }
                }
            }
        }

        private void HandleStateNormal()
        {
            if (_currentState == ESelectableState.Normal)
                return;
            _currentState = ESelectableState.Normal;
            HandleState(ModulesNormal);
        }

        private void HandleStateHighlighted()
        {
            if (_currentState == ESelectableState.Highlighted)
                return;
            _currentState = ESelectableState.Highlighted;
            HandleState(ModulesHighlighted);
        }

        private void HandleStatePressed()
        {
            if (_currentState == ESelectableState.Pressed)
                return;
            _currentState = ESelectableState.Pressed;
            HandleState(ModulesPressed);
        }

        private void HandleStateSelected()
        {
            if (_currentState == ESelectableState.Selected)
                return;
            _currentState = ESelectableState.Selected;
            HandleState(ModulesSelected);
        }

        private void HandleStateDisabled()
        {
            if (_currentState == ESelectableState.Disabled)
                return;
            _currentState = ESelectableState.Disabled;
            HandleState(ModulesDisabled);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_deselectAfterPress && !eventData.hovered.Contains(gameObject))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}