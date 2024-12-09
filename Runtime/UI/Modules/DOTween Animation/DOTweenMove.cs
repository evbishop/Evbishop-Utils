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

        public override void HandleAwake()
        {
            base.HandleAwake();

            if (playFrom == EStart.StartValue)
            {
                _from = Component.anchoredPosition3D;
            }
            else if (playFrom == EStart.CustomValue)
            {
                _from = AnimationUtils.GetTargetPosition(
                    Component,
                    _fromDirection,
                    _to,
                    Component.localScale,
                    Component.localEulerAngles)
                    + _fromOffset;
            }

            if (playTo == EStart.StartValue)
            {
                _to = Component.anchoredPosition3D;
            }
            else if (playTo == EStart.CustomValue)
            {
                _to = AnimationUtils.GetTargetPosition(
                    Component,
                    _toDirection,
                    _from,
                    Component.localScale,
                    Component.localEulerAngles)
                    + _toOffset;
            }
        }

        public override void Play()
        {
            base.Play();

            if (playFrom != EStart.CurrentValue)
                Component.anchoredPosition3D = _from;
            sequence
                .Append(Component.DOAnchorPos3D(_to, duration).SetEase(easePlay))
                .OnComplete(HandleTweenComplete);
        }

        public override void PlayReverse()
        {
            base.PlayReverse();

            sequence
                .Append(Component.DOAnchorPos3D(_from, duration).SetEase(easeReversePlay))
                .OnComplete(HandleTweenRewind);
        }

        public override void ReverseInstant()
        {
            base.ReverseInstant();

            Component.anchoredPosition3D = _from;
        }
    }
}