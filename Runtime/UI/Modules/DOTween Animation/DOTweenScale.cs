using DG.Tweening;
using Evbishop.Runtime.ModulesSystem;
using Evbishop.Runtime.UI.Modules.Animator;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace Evbishop.Runtime.UI.Modules.DOTween_Animation
{
    [Title("Scale")]
    public class DOTweenScale : ModuleDOTween, INeedComponent<RectTransform>
    {
        [SerializeField, VerticalGroup(GROUP_FROM), ShowIf(nameof(playFrom), EStart.CustomValue), FormerlySerializedAs("_from")] private Vector3 _customFrom = new(1, 1, 1);
        [SerializeField, VerticalGroup(GROUP_TO), ShowIf(nameof(playTo), EStart.CustomValue), FormerlySerializedAs("_to")] private Vector3 _customTo = new(1, 1, 1);
        [OdinSerialize] public RectTransform Component { get; set; }

        private Vector3 _from;
        private Vector3 _to;

        public override void HandleAwake()
        {
            base.HandleAwake();

            if (playFrom == EStart.StartValue)
            {
                _from = Component.localScale;
            }
            else if (playFrom == EStart.CustomValue)
            {
                _from = _customFrom;
            }

            if (playTo == EStart.StartValue)
            {
                _to = Component.localScale;
            }
            else if (playTo == EStart.CustomValue)
            {
                _to = _customTo;
            }
        }

        public override void Play()
        {
            base.Play();

            if (playFrom != EStart.CurrentValue)
                Component.localScale = _from;
            sequence
                .Append(Component.DOScale(_to, duration).SetEase(easePlay))
                .OnComplete(HandleTweenComplete);
        }

        public override void PlayReverse()
        {
            base.PlayReverse();

            sequence
                .Append(Component.DOScale(_from, duration).SetEase(easeReversePlay))
                .OnComplete(HandleTweenRewind);
        }

        public override void ReverseInstant()
        {
            base.ReverseInstant();

            Component.localScale = _from;
        }
    }
}