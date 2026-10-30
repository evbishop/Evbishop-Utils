using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Evbishop.Runtime.AddressablesUtils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Evbishop.Runtime.Pooling
{
    public partial class Pools
    {
        [SerializeField] private PoolsInfo _poolsInfo = null;

        public Dictionary<string, AsyncObjectPool<PoolItem>> AddressablePool { get; private set; } = new();
        public Dictionary<EntityId, string> InstantiatedAddressables { get; private set; } = new();

        public async UniTask PreloadAddressablePrefabs()
        {
            if (_poolsInfo == null ||
                _poolsInfo.PreloadedAddressablePrefabs == null ||
                _poolsInfo.PreloadedAddressablePrefabs.Length == 0)
                return;

            var preloadTasks = new List<UniTask>();

            for (int i = 0; i < _poolsInfo.PreloadedAddressablePrefabs.Length; ++i)
            {
                var preloadedPrefab = _poolsInfo.PreloadedAddressablePrefabs[i];
                if (preloadedPrefab.Prefab == null ||
                    !preloadedPrefab.Prefab.CanInstantiate() ||
                    preloadedPrefab.Count <= 0)
                    continue;

                var runtimeKey = (string)preloadedPrefab.Prefab.RuntimeKey;
                if (string.IsNullOrEmpty(runtimeKey))
                    continue;

                if (!AddressablePool.ContainsKey(runtimeKey))
                {
                    AddPool(runtimeKey, preloadedPrefab.Count, preloadedPrefab.MaxSize);
                }

                preloadTasks.Add(PreloadPoolItems(runtimeKey, preloadedPrefab.Count));
            }

            await UniTask.WhenAll(preloadTasks);
        }

        private async UniTask PreloadPoolItems(string runtimeKey, int count)
        {
            if (!AddressablePool.ContainsKey(runtimeKey))
                return;

            for (int i = 0; i < count; i++)
            {
                var poolItem = await CreatePoolItemAsync(runtimeKey);
                if (poolItem != null)
                {
                    poolItem.Pools = Instance;
                    poolItem.Pool = AddressablePool[runtimeKey];
                    InstantiatedAddressables.TryAdd(poolItem.GetEntityId(), runtimeKey);
                    Release(poolItem);
                }
            }
        }

        public static void Release(PoolItem poolItem)
        {
            if (Instance == null || poolItem == null)
                return;

            var instantiatedInstanceID = poolItem.GetEntityId();
            if (!Instance.InstantiatedAddressables.TryGetValue(instantiatedInstanceID, out string runtimeKey))
            {
                Debug.LogError($"Unable to pool {poolItem.name} (instance {instantiatedInstanceID}): the GameObject was not instantiated with ObjectPool.InstantiateAsync {Time.time}");
                Destroy(poolItem);
                return;
            }

            if (Instance.AddressablePool.TryGetValue(runtimeKey, out var pool))
            {
                pool.Release(poolItem);
            }
            else
            {
                pool = Instance.AddPool(runtimeKey);
                pool.Release(poolItem);
            }
        }

        private AsyncObjectPool<PoolItem> AddPool(string runtimeKey,
            int defaultCapacity = 1, int maxSize = 50)
        {
            if (string.IsNullOrEmpty(runtimeKey))
            {
                Debug.LogError("Cannot create pool with null or empty runtime key");
                return null;
            }

            if (AddressablePool.TryGetValue(runtimeKey, out var existingPool))
            {
                return existingPool;
            }

            var pool = new AsyncObjectPool<PoolItem>(
                    asyncCreateFunc: async () => await CreatePoolItemAsync(runtimeKey),
                    actionOnGet: poolItem => poolItem?.HandleGet(),
                    actionOnRelease: poolItem => poolItem?.HandleRelease(),
                    actionOnDestroy: poolItem => poolItem?.HandleDestroy(),
                    collectionCheck: false,
                    defaultCapacity: defaultCapacity,
                    maxSize: maxSize
                );
            AddressablePool.Add(runtimeKey, pool);
            return pool;
        }

        private async UniTask<PoolItem> CreatePoolItemAsync(string runtimeKey)
        {
            if (string.IsNullOrEmpty(runtimeKey))
            {
                Debug.LogError("Cannot instantiate addressable asset with null or empty runtime key");
                return null;
            }

            AssetReferenceGameObject original = null;
            original = new AssetReferenceGameObject(runtimeKey);
            if (!original.CanInstantiate())
            {
                Debug.LogError($"Cannot instantiate addressable asset with key: {runtimeKey}");
                return null;
            }

            var instance = await Addressables.InstantiateAsync(original).ToUniTask();
            if (instance == null)
            {
                Debug.LogError($"Failed to instantiate addressable asset with key: {runtimeKey}");
                return null;
            }

            if (instance.TryGetComponent(out PoolItem poolItem))
            {
                poolItem.Pools = Instance;
                return poolItem;
            }
            else
            {
                Debug.LogError($"No {nameof(PoolItem)} component found on the instantiated addressable asset with key: {runtimeKey}");
                Addressables.ReleaseInstance(instance);
                return null;
            }
        }

        public static async UniTask<PoolItem> InstantiateAsync(AssetReferenceGameObject original)
        {
            return await InstantiateAsync(original, Vector3.zero, Quaternion.identity, null);
        }

        public static async UniTask<PoolItem> InstantiateAsync(AssetReferenceGameObject original, Transform parent)
        {
            return await InstantiateAsync(original, Vector3.zero, Quaternion.identity, parent);
        }

        public static async UniTask<PoolItem> InstantiateAsync(AssetReferenceGameObject original, Vector3 position, Quaternion rotation)
        {
            return await InstantiateAsync(original, position, rotation, null);
        }

        public static async UniTask<PoolItem> InstantiateAsync(AssetReferenceGameObject original, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (Instance == null)
                return null;

            if (original == null || !original.CanInstantiate())
            {
                Debug.LogError("Cannot instantiate null or invalid AssetReferenceGameObject");
                return null;
            }

            var runtimeKey = (string)original.RuntimeKey;
            if (string.IsNullOrEmpty(runtimeKey))
            {
                Debug.LogError("AssetReferenceGameObject has null or empty runtime key");
                return null;
            }

            var poolItem = await Instance.GetFromPool(runtimeKey);
            if (poolItem == null)
                return null;

            poolItem.transform.SetPositionAndRotation(position, rotation);
            poolItem.transform.SetParent(parent, worldPositionStays: false);
            poolItem.gameObject.SetActive(true);
            Instance.InstantiatedAddressables.TryAdd(poolItem.GetEntityId(), runtimeKey);
            return poolItem;
        }

        private async UniTask<PoolItem> GetFromPool(string runtimeKey)
        {
            if (!AddressablePool.TryGetValue(runtimeKey, out var pool))
            {
                pool = AddPool(runtimeKey);
                if (pool == null)
                    return null;
            }

            var poolItem = await pool.GetAsync();
            if (poolItem == null)
            {
                Debug.LogError($"Failed to get pool item for runtime key: {runtimeKey}");
                return null;
            }

            poolItem.Pools = Instance;
            poolItem.Pool = pool;
            return poolItem;
        }
    }
}