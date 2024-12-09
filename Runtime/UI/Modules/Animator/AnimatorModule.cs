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

        public UnityEvent<AnimatorModule> OnPlayFinished { get; set; }
        public UnityEvent<AnimatorModule> OnPlayReverseFinished { get; set; }

        public virtual void HandleAwake()
        {
            OnPlayFinished = new();
            OnPlayReverseFinished = new();
        }

        [Button, ButtonGroup, GUIColor("cyan")]
        public virtual void Play()
        {

        }

        [Button, ButtonGroup, GUIColor(1f, 0.7f, 0.7f)]
        public virtual void PlayReverse()
        {

        }

        public virtual void Cancel()
        {

        }

        public virtual void ReverseInstant()
        {

        }
    }
}
