using Evbishop.Runtime.Singletons;

namespace Evbishop.Runtime.Pooling
{
    public partial class Pools : MonoSingleton<Pools>
    {
        private void Awake()
        {
            TryInitializeSingleton();
        }
    }
}
