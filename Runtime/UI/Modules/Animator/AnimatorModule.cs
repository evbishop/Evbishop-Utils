using Evbishop.Runtime.ModulesSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Evbishop.Runtime.UI.Modules.Animator
{
    public abstract class AnimatorModule : UIModule, IModuleAnimator, IModuleAwakable
    {
        protected const string GROUP_FROM_TO = "FromTo";
        protected const string GROUP_FROM = "FromTo/From";
        protected const string GROUP_TO = "FromTo/To";

        [SerializeField, HorizontalGroup(GROUP_FROM_TO), VerticalGroup(GROUP_FROM)] protected EStart playFrom = EStart.CustomValue;
        [SerializeField, VerticalGroup(GROUP_TO)] protected EStart playTo;

        private bool _notify = true;
        public bool Notify => _notify;

        public UnityEvent<AnimatorModule> OnPlayFinished { get; set; } = new();
        public UnityEvent<AnimatorModule> OnPlayReverseFinished { get; set; } = new();

        public virtual void HandleAwake()
        {
            OnPlayFinished = new();
            OnPlayReverseFinished = new();
        }

        [Button, ButtonGroup, GUIColor("cyan"), ShowIf("@UnityEngine.Application.isPlaying")]
        public virtual void Play(bool notify = true)
        {
            _notify = notify;
        }

        [Button, ButtonGroup, GUIColor(1f, 0.7f, 0.7f), ShowIf("@UnityEngine.Application.isPlaying")]
        public virtual void PlayReverse(bool notify = true)
        {
            _notify = notify;
        }

        public virtual void Cancel()
        {

        }

        public virtual void ReverseInstant(bool notify = true)
        {
            _notify = notify;
        }
    }
}
