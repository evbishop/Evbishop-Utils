using UnityEngine.Pool;

namespace Evbishop.Runtime.Pooling
{
    public interface IPoolable<T> where T : class, IPoolable<T>
    {
        Pools Pools { get; set; }
        IObjectPool<T> Pool { get; set; }

        void HandleGet();
        void HandleRelease();
        void HandleDestroy();
    }

    public interface IPoolable
    {
        Pools Pools { get; set; }
        IObjectPool<IPoolable> Pool { get; set; }

        void HandleGet();
        void HandleRelease();
        void HandleDestroy();
    }
}