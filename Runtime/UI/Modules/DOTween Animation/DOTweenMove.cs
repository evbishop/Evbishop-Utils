using DG.Tweening;
using Evbishop.Runtime.ModulesSystem;
using Evbishop.Runtime.UI.Modules.Animator;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Evbishop.Runtime.UI.Modules.DOTween_Animation
{
    public class DOTweenMove : ModuleDOTween, INeedComponent<RectTransform>, IModuleAwakable
    {
        [SerializeField, VerticalGroup(GROUP_FROM), ShowIf(nameof(playFrom), EStart.CustomValue)] private EDirection _fromDirection;
        [SerializeField, VerticalGroup(GROUP_FROM), ShowIf(nameof(playFrom), EStart.CustomValue)] private Vector3 _fromOffset;
        [SerializeField, VerticalGroup(GROUP_TO), ShowIf(nameof(playTo), EStart.CustomValue)] private EDirection _toDirection;
        [SerializeField, VerticalGroup(GROUP_TO), ShowIf(nameof(playTo), EStart.CustomValue)] private Vector3 _toOffset;
        [OdinSerialize] public RectTransform Component { get; set; }

        private Vector3 _from;
        private Vector3 _to;
        private Vector3 _initialPosition;

        public override void HandleAwake()
        {
            base.HandleAwake();

            // Store the initial position - this will be used as a reference for maintaining
            // the horizontal/vertical alignment when using cardinal directions
            _initialPosition = Component.anchoredPosition3D;

            if (playFrom == EStart.StartValue)
            {
                _from = _initialPosition;
            }
            else if (playFrom == EStart.CustomValue)
            {
                // For cardinal directions (Left, Right, Top, Bottom):
                // - Maintains the object's vertical/horizontal alignment
                // - Positions it outside the parent's bounds
                //
                // For compound directions (TopLeft, MiddleCenter, etc.):
                // - Positions it relative to the parent's boundaries
                _from = AnimationUtils.GetTargetPosition(
                    Component,
                    _fromDirection,
                    _initialPosition,
                    Component.localScale,
                    Component.localEulerAngles)
                    + _fromOffset;
            }

            if (playTo == EStart.StartValue)
            {
                _to = _initialPosition;
            }
            else if (playTo == EStart.CustomValue)
            {
                // Calculate the target position using the same logic as above
                // We always use _initialPosition as the reference to maintain alignment
                _to = AnimationUtils.GetTargetPosition(
                    Component,
                    _toDirection,
                    _initialPosition,
                    Component.localScale,
                    Component.localEulerAngles)
                    + _toOffset;
            }
        }

        public override void Play(bool notify = true)
        {
            base.Play(notify);

            if (playFrom != EStart.CurrentValue)
                Component.anchoredPosition3D = _from;
            sequence
                .Append(Component.DOAnchorPos3D(_to, duration).SetEase(easePlay))
                .OnComplete(HandleTweenComplete);
        }

        public override void PlayReverse(bool notify = true)
        {
            base.PlayReverse(notify);

            sequence
                .Append(Component.DOAnchorPos3D(_from, duration).SetEase(easeReversePlay))
                .OnComplete(HandleTweenRewind);
        }

        public override void ReverseInstant(bool notify = true)
        {
            base.ReverseInstant(notify);

            Component.anchoredPosition3D = _from;
        }
    }
}