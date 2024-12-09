using UnityEngine;

namespace Evbishop.Runtime.ModulesSystem
{
    public interface IModuleAwakable : IModule
    {
        void HandleAwake();
    }

    public interface IModuleAwakable<T> : IModule where T : Component
    {
        void HandleAwake(T component);
    }
}
