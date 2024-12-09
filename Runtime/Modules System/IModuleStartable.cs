using UnityEngine;

namespace Evbishop.Runtime.ModulesSystem
{
    public interface IModuleStartable : IModule
    {
        void HandleStart();
    }

    public interface IModuleStartable<T> : IModule where T : Component
    {
        void HandleStart(T component);
    }
}
