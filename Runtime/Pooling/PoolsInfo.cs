using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Evbishop.Runtime.Pooling
{
    [CreateAssetMenu(fileName = "Pools Info", menuName = "Scriptable Objects/PoolsInfo")]
    public class PoolsInfo : SerializedScriptableObject
    {
        [field: SerializeField, TableList(ShowIndexLabels = true)
#if UNITY_EDITOR
        , ValidateInput(nameof(ValidatePreloadedPrefabs), "Duplicate prefabs detected")
#endif
        ]
        public PreloadedAddressablePrefab[] PreloadedAddressablePrefabs { get; private set; } = null;

        [field: SerializeField, TableList(ShowIndexLabels = true), ValidateInput(nameof(ValidatePooledObjects), "Duplicate prefabs detected")]
        public PooledNetworkObject[] PooledNetworkObjects { get; private set; } = null;

        [ShowInInspector, ReadOnly, DisplayAsString]
        private string PreloadedAddressableTotals => PreloadedAddressablePrefabs == null || PreloadedAddressablePrefabs.Length == 0
            ? "Total Count: 0, Total Max Size: 0"
            : $"Total Count: {PreloadedAddressablePrefabs.Sum(p => p.Count)}, Total Max Size: {PreloadedAddressablePrefabs.Sum(p => p.MaxSize)}";

        [ShowInInspector, ReadOnly, DisplayAsString]
        private string PooledNetworkObjectsTotals => PooledNetworkObjects == null || PooledNetworkObjects.Length == 0
            ? "Total Count: 0"
            : $"Total Count: {PooledNetworkObjects.Sum(p => p.Count)}";

        [ShowInInspector, ReadOnly, DisplayAsString]
        private string CombinedTotals
        {
            get
            {
                int preloadedCount = PreloadedAddressablePrefabs?.Sum(p => p.Count) ?? 0;
                int preloadedMaxSize = PreloadedAddressablePrefabs?.Sum(p => p.MaxSize) ?? 0;
                int pooledCount = PooledNetworkObjects?.Sum(p => p.Count) ?? 0;
                return $"Combined Count: {preloadedCount + pooledCount}, Combined Max Size + Count: {preloadedMaxSize + pooledCount}";
            }
        }

#if UNITY_EDITOR
        private bool ValidatePreloadedPrefabs(PreloadedAddressablePrefab[] prefabs, ref string errorMessage)
        {
            if (prefabs == null || prefabs.Length == 0)
                return true;

            var indexMap = new Dictionary<string, List<int>>();

            for (int i = 0; i < prefabs.Length; i++)
            {
                var prefab = prefabs[i];
                if (prefab.Prefab == null || string.IsNullOrEmpty(prefab.Prefab.AssetGUID))
                    continue;

                if (!indexMap.TryGetValue(prefab.Prefab.AssetGUID, out var indices))
                {
                    indices = new List<int>();
                    indexMap.Add(prefab.Prefab.AssetGUID, indices);
                }
                indices.Add(i);
            }

            var duplicates = new List<string>();
            foreach (var kvp in indexMap)
            {
                if (kvp.Value.Count > 1)
                {
                    for (int i = 0; i < prefabs.Length; i++)
                    {
                        if (prefabs[i].Prefab?.AssetGUID == kvp.Key)
                        {
                            var assetName = prefabs[i].Prefab.editorAsset != null ? prefabs[i].Prefab.editorAsset.name : kvp.Key;
                            duplicates.Add($"{assetName} (indices {string.Join(", ", kvp.Value)})");
                            break;
                        }
                    }
                }
            }

            if (duplicates.Count > 0)
            {
                errorMessage = $"Duplicate prefabs: {string.Join("; ", duplicates)}";
                return false;
            }

            return true;
        }
#endif

        private bool ValidatePooledObjects(PooledNetworkObject[] objects, ref string errorMessage)
        {
            if (objects == null || objects.Length == 0)
                return true;

            var indexMap = new Dictionary<GameObject, List<int>>();

            for (int i = 0; i < objects.Length; i++)
            {
                var obj = objects[i];
                if (obj.Prefab == null)
                    continue;

                if (!indexMap.TryGetValue(obj.Prefab, out var indices))
                {
                    indices = new List<int>();
                    indexMap.Add(obj.Prefab, indices);
                }
                indices.Add(i);
            }

            var duplicates = new List<string>();
            foreach (var kvp in indexMap)
            {
                if (kvp.Value.Count > 1)
                    duplicates.Add($"{kvp.Key.name} (indices {string.Join(", ", kvp.Value)})");
            }

            if (duplicates.Count > 0)
            {
                errorMessage = $"Duplicate prefabs: {string.Join("; ", duplicates)}";
                return false;
            }

            return true;
        }
    }
}
