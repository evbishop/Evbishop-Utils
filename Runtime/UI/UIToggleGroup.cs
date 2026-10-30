using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Evbishop.Runtime.UI
{
    /// <summary>
    /// Extended toggle group that enforces specific behaviors based on the selected control mode.
    /// </summary>
    public class UIToggleGroup : ToggleGroup
    {
        public enum ControlMode
        {
            /// <summary>
            /// Toggle values are not enforced in any way
            /// <para/> Allows for all toggles to be OFF
            /// </summary>
            Passive = 0,
            /// <summary>
            /// Only one Toggle can be ON at any given time
            /// <para/> Allows for all toggles to be OFF
            /// </summary>
            OneToggleOn = 1,
            /// <summary>
            /// Only one Toggle will to be ON at any given time
            /// <para/> One Toggle will be forced ON at all times
            /// </summary>
            OneToggleOnEnforced = 2,
            /// <summary>
            /// At least one Toggle needs to be ON at any given time
            /// <para/> Allows for multiple toggles to be ON
            /// <para/> One Toggle will be forced ON at all times
            /// </summary>
            AnyToggleOnEnforced = 3,
        }

        [SerializeField] private ControlMode _controlMode = ControlMode.Passive;

        private List<UIToggle> _cachedToggles = new();
        private Dictionary<UIToggle, bool> _toggleStates = new();
        private UIToggle _lastActiveToggle;

        public UIToggle FirstToggle => _cachedToggles.FirstOrDefault();

        public event Action<UIToggle> OnToggleActivated;
        public event Action<UIToggle> OnToggleDeactivated;

        protected override void Start()
        {
            base.Start();

            EnforceControlMode();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            InitializeToggles();
            EnforceControlMode();
        }

        public void InitializeToggles()
        {
            // Cache all toggles in this group
            _cachedToggles = GetComponentsInChildren<UIToggle>(true)
                .Where(t => t.Group == this)
                .ToList();

            foreach (var toggle in _cachedToggles)
            {
                if (!_toggleStates.TryAdd(toggle, toggle.IsOn))
                    _toggleStates[toggle] = toggle.IsOn;

                toggle.OnValueChanged += (isOn) => HandleToggleValueChanged(toggle, isOn);

                if (toggle.IsOn)
                    _lastActiveToggle = toggle;
            }

            // If we're in enforced mode, ensure a toggle is on
            if ((_controlMode == ControlMode.OneToggleOnEnforced ||
                 _controlMode == ControlMode.AnyToggleOnEnforced) &&
                !HasAnyTogglesOn())
            {
                ActivateFirstToggle();
            }
        }

        public int GetToggleIndex(UIToggle toggle)
        {
            return _cachedToggles.IndexOf(toggle);
        }

        public UIToggle GetToggleByIndex(int index)
        {
            return _cachedToggles[index];
        }

        private void HandleToggleValueChanged(UIToggle toggle, bool isOn)
        {
            if (isOn)
                _lastActiveToggle = toggle;

            _toggleStates[toggle] = isOn;
            HandleToggleChange(toggle, isOn);
        }

        private void HandleToggleChange(UIToggle changedToggle, bool isOn)
        {
            switch (_controlMode)
            {
                case ControlMode.OneToggleOn:
                    if (isOn)
                        EnsureOnlyOneToggleOn(changedToggle);
                    break;
                case ControlMode.OneToggleOnEnforced:
                    if (!isOn && !HasAnyTogglesOn())
                        changedToggle.SetIsOn(true);
                    else if (isOn)
                        EnsureOnlyOneToggleOn(changedToggle);
                    break;
                case ControlMode.AnyToggleOnEnforced:
                    if (!isOn && !HasAnyTogglesOn())
                        changedToggle.SetIsOn(true);
                    break;
            }
            if (changedToggle.IsOn)
                OnToggleActivated?.Invoke(changedToggle);
            else
                OnToggleDeactivated?.Invoke(changedToggle);
        }

        private void EnforceControlMode()
        {
            if (_cachedToggles.Count == 0)
                return;

            switch (_controlMode)
            {
                case ControlMode.OneToggleOn:
                    EnsureAtMostOneToggleOn();
                    break;
                case ControlMode.OneToggleOnEnforced:
                    EnsureExactlyOneToggleOn();
                    break;
                case ControlMode.AnyToggleOnEnforced:
                    EnsureAtLeastOneToggleOn();
                    break;
            }
        }

        private void EnsureOnlyOneToggleOn(UIToggle activeToggle)
        {
            foreach (var toggle in _cachedToggles)
            {
                if (toggle != activeToggle && toggle.IsOn)
                {
                    toggle.SetIsOn(false);
                    _toggleStates[toggle] = false;
                }
            }
        }

        private void EnsureAtMostOneToggleOn()
        {
            UIToggle activeToggle = null;

            foreach (var toggle in _cachedToggles)
            {
                if (toggle.IsOn)
                {
                    if (activeToggle == null)
                        activeToggle = toggle;
                    else
                    {
                        toggle.SetIsOn(false);
                        _toggleStates[toggle] = false;
                    }
                }
            }
        }

        private void EnsureExactlyOneToggleOn()
        {
            if (!HasAnyTogglesOn())
                ActivateFirstToggle();
            else
                EnsureAtMostOneToggleOn();
        }

        private void EnsureAtLeastOneToggleOn()
        {
            if (!HasAnyTogglesOn())
                ActivateFirstToggle();
        }

        private void ActivateFirstToggle()
        {
            if (_cachedToggles.Count > 0)
            {
                _cachedToggles[0].SetIsOn(true);
                _toggleStates[_cachedToggles[0]] = true;
                _lastActiveToggle = _cachedToggles[0];
            }
        }

        private bool HasAnyTogglesOn()
        {
            return _cachedToggles.Any(t => t.IsOn);
        }

        public UIToggle GetActiveToggle()
        {
            return _lastActiveToggle;
        }

        public List<UIToggle> GetActiveToggles()
        {
            return _cachedToggles.Where(t => t.IsOn).ToList();
        }

        public void AddToggle(UIToggle toggle)
        {
            _cachedToggles.Add(toggle);
            if (!_toggleStates.TryAdd(toggle, toggle.IsOn))
                _toggleStates[toggle] = toggle.IsOn;
            toggle.OnValueChanged += (isOn) => HandleToggleValueChanged(toggle, isOn);
            EnforceControlMode();
        }

        public void RemoveToggle(UIToggle toggle)
        {
            if (_cachedToggles.Contains(toggle))
                _cachedToggles.Remove(toggle);
            if (_toggleStates.ContainsKey(toggle))
                _toggleStates.Remove(toggle);
            EnforceControlMode();
        }

        public void SelectFirst()
        {
            var firstToggle = FirstToggle;
            if (firstToggle == null)
                return;

            firstToggle.SetIsOn(false);
            firstToggle.SetIsOn(true);
        }

        public void SelectRandom()
        {
            var randomToggle = _cachedToggles[UnityEngine.Random.Range(0, _cachedToggles.Count)];
            if (randomToggle == null)
                return;

            randomToggle.SetIsOn(false);
            randomToggle.SetIsOn(true);
        }
    }
}