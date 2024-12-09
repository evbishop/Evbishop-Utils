using Evbishop.Runtime.ModulesSystem;
using UnityEngine.Events;

namespace Evbishop.Runtime.UI.Modules.Animator
{
    public interface IModuleAnimator : IModule
    {
        void Play();
        void PlayReverse();
        void ReverseInstant();
        void Cancel();
        UnityEvent<AnimatorModule> OnPlayFinished { get; set; }
        UnityEvent<AnimatorModule> OnPlayReverseFinished { get; set; }
    }
}