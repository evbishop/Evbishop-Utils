using UnityEngine;

namespace Evbishop.Runtime.Singletons
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        enum PersistenceType : byte
        {
            DestroyOldest = 1,
            DestroyNewest = 2,
        }

        [SerializeField] PersistenceType persistenceType = PersistenceType.DestroyNewest;
        [SerializeField] bool dontDestroyOnLoad;

        public static T Instance { get; private set; }

        protected bool TryInitializeSingleton()
        {
            if (Instance is null)
            {
                Instance = this as T;
                if (dontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
                return true;
            }

            if (persistenceType == PersistenceType.DestroyOldest)
            {
                Destroy(Instance.gameObject);
                Instance = this as T;
                if (dontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
                return true;
            }

            if (persistenceType == PersistenceType.DestroyNewest)
                Destroy(gameObject);

            return false;
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
