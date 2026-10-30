using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Evbishop.Runtime.Pooling
{
    public partial class Pools
    {
        public Dictionary<Type, IObjectPool<IPoolable>> GenericPools { get; set; } = new();

        public static T Get<T>() where T : class, IPoolable, new()
        {
            if (Instance == null)
                return null;

            var type = typeof(T);
            if (!Instance.GenericPools.TryGetValue(type, out var pool))
            {
                pool = Instance.CreatePool<T>();
                Instance.GenericPools[type] = pool;
            }

            var poolable = (T)pool.Get();
            poolable.Pools = Instance;
            poolable.Pool = pool;
            return poolable;
        }

        private IObjectPool<IPoolable> CreatePool<T>() where T : class, IPoolable, new()
        {
            void OnGetFromPool(IPoolable poolable)
            {
                poolable.HandleGet();
            }

            void OnReleaseToPool(IPoolable poolable)
            {
                poolable.HandleRelease();
            }

            void OnDestroyPoolObject(IPoolable poolable)
            {
                poolable.HandleDestroy();
            }

            return new ObjectPool<IPoolable>(
                createFunc: () => new T(),
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReleaseToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: false,
                defaultCapacity: 10,
                maxSize: 100
            );
        }

        public static void Release<T>(T obj) where T : class, IPoolable
        {
            if (Instance == null || obj == null)
                return;

            var type = typeof(T);
            if (Instance.GenericPools.TryGetValue(type, out var pool))
            {
                pool.Release(obj);
            }
        }
    }
}