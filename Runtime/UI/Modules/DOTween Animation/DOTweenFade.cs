using DG.Tweening;
using Evbishop.Runtime.ModulesSystem;
using Evbishop.Runtime.UI.Modules.Animator;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Evbishop.Runtime.UI.Modules.DOTween_Animation
{
    [Title("Fade")]
    public class DOTweenFade : ModuleDOTween, INeedComponent<CanvasGroup>
    {
        [SerializeField, VerticalGroup(GROUP_FROM), ShowIf(nameof(playFrom), EStart.CustomValue), LabelText("From")] private float _customFrom = 0;
        [SerializeField, VerticalGroup(GROUP_TO), ShowIf(nameof(playTo), EStart.CustomValue), LabelText("To")] private float _customTo = 1;
        [OdinSerialize] public CanvasGroup Component { get; set; }

        private float _from;
        private float _to;

        public override void HandleAwake()
        {
            base.HandleAwake();

            if (playFrom == EStart.StartValue)
            {
                _from = Component.alpha;
            }
            else if (playFrom == EStart.CustomValue)
            {
                _from = _customFrom;
            }

            if (playTo == EStart.StartValue)
            {
                _to = Component.alpha;
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
                Component.alpha = _from;
            sequence
                .Append(Component.DOFade(_to, duration).SetEase(easePlay))
                .OnComplete(HandleTweenComplete);
        }

        public override void PlayReverse()
        {
            base.PlayReverse();

            sequence
                .Append(Component.DOFade(_from, duration).SetEase(easeReversePlay))
                .OnComplete(HandleTweenRewind);
        }

        public override void ReverseInstant()
        {
            base.ReverseInstant();

            Component.alpha = _from;
        }
    }
}