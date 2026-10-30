using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;

namespace Evbishop.Runtime.Pooling
{
    public class AsyncObjectPool<T> : IDisposable, IObjectPool<T> where T : class
    {
        internal readonly List<T> m_List;

        private readonly Func<UniTask<T>> m_AsyncCreateFunc;

        private readonly Action<T> m_ActionOnGet;

        private readonly Action<T> m_ActionOnRelease;

        private readonly Action<T> m_ActionOnDestroy;

        private readonly int m_MaxSize;

        internal bool m_CollectionCheck;

        private T m_FreshlyReleased;

        public int CountAll { get; private set; }

        public int CountActive => CountAll - CountInactive;

        public int CountInactive => m_List.Count + ((m_FreshlyReleased != null) ? 1 : 0);

        public AsyncObjectPool(Func<UniTask<T>> asyncCreateFunc, Action<T> actionOnGet = null, Action<T> actionOnRelease = null, Action<T> actionOnDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (asyncCreateFunc == null)
            {
                throw new ArgumentNullException("asyncCreateFunc");
            }

            if (maxSize <= 0)
            {
                throw new ArgumentException("Max Size must be greater than 0", "maxSize");
            }

            m_List = new List<T>(defaultCapacity);
            m_AsyncCreateFunc = asyncCreateFunc;
            m_MaxSize = maxSize;
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
            m_ActionOnDestroy = actionOnDestroy;
            m_CollectionCheck = collectionCheck;
        }

        public T Get()
        {
            throw new InvalidOperationException("Cannot use synchronous Get() with AsyncObjectPool. Use GetAsync() instead.");
        }

        public PooledObject<T> Get(out T v)
        {
            throw new InvalidOperationException("Cannot use synchronous Get(out T) with AsyncObjectPool. Use GetAsyncWithPooledObject() instead.");
        }

        public async UniTask<T> GetAsync()
        {
            T val;
            if (m_FreshlyReleased != null)
            {
                val = m_FreshlyReleased;
                m_FreshlyReleased = null;
            }
            else if (m_List.Count == 0)
            {
                val = await m_AsyncCreateFunc();
                if (val == null)
                {
                    return null;
                }
                CountAll++;
            }
            else
            {
                int index = m_List.Count - 1;
                val = m_List[index];
                m_List.RemoveAt(index);
            }

            m_ActionOnGet?.Invoke(val);
            return val;
        }

        public async UniTask<(T value, PooledObject<T> pooledObject)> GetAsyncWithPooledObject()
        {
            var value = await GetAsync();
            return (value, new PooledObject<T>(value, this));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(T element)
        {
            if (element == null)
            {
                return;
            }

            if (m_CollectionCheck && (m_List.Count > 0 || m_FreshlyReleased != null))
            {
                if (element == m_FreshlyReleased)
                {
                    throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
                }

                for (int i = 0; i < m_List.Count; i++)
                {
                    if (element == m_List[i])
                    {
                        throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
                    }
                }
            }

            m_ActionOnRelease?.Invoke(element);
            if (m_FreshlyReleased == null)
            {
                m_FreshlyReleased = element;
                return;
            }

            if (CountInactive < m_MaxSize)
            {
                m_List.Add(element);
                return;
            }

            CountAll--;
            m_ActionOnDestroy?.Invoke(element);
        }

        public void Clear()
        {
            if (m_ActionOnDestroy != null)
            {
                foreach (T item in m_List)
                {
                    m_ActionOnDestroy(item);
                }

                if (m_FreshlyReleased != null)
                {
                    m_ActionOnDestroy(m_FreshlyReleased);
                }
            }

            m_FreshlyReleased = null;
            m_List.Clear();
            CountAll = 0;
        }

        public void Dispose()
        {
            Clear();
        }

        internal bool HasElement(T element)
        {
            return m_FreshlyReleased == element || m_List.Contains(element);
        }
    }
}