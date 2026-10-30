using Evbishop.Runtime.ModulesSystem;
using UnityEngine.Events;

namespace Evbishop.Runtime.UI.Modules.Animator
{
    public interface IModuleAnimator : IModule
    {
        bool Notify { get; }
        void Play(bool notify = true);
        void PlayReverse(bool notify = true);
        void ReverseInstant(bool notify = true);
        void Cancel();
        UnityEvent<AnimatorModule> OnPlayFinished { get; set; }
        UnityEvent<AnimatorModule> OnPlayReverseFinished { get; set; }
    }
}