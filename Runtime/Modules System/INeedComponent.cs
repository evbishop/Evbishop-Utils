using UnityEngine;

namespace Evbishop.Runtime.ModulesSystem
{
    public interface INeedComponent<T> : IModule where T : Component
    {
        T Component { get; set; }
    }
}