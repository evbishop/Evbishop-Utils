using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Evbishop.Runtime.AddressablesUtils
{
    public static class AddressableHelper
    {
        public static AsyncOperationHandle<T> Get<T>(AssetReference assetRef, System.Action<T> cb, bool autoReleaseAfterCb = true)
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetRef);
            handle.Completed += handle =>
            {
                cb?.Invoke(handle.Result);

                if (autoReleaseAfterCb)
                    Addressables.Release(handle);
            };
            return handle;
        }

        public static async UniTask<T> InstantiateAsync<T>(AssetReferenceGameObject prefab, Transform parent)
            where T : Component
        {
            var op = Addressables.InstantiateAsync(prefab, parent);
            await op;
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                return op.Result.GetComponent<T>();
            }
            else
            {
                throw new Exception($"Error loading addressable prefab. OperationStatus:{op.Status}. RuntimeKey:{prefab.RuntimeKey}.");
            }
        }

        public static async UniTask<GameObject> InstantiateAsync(AssetReferenceGameObject prefab, Transform parent)
        {
            var op = Addressables.InstantiateAsync(prefab, parent);
            await op;
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                return op.Result;
            }
            else
            {
                throw new Exception($"Error loading addressable prefab. OperationStatus:{op.Status}. RuntimeKey:{prefab.RuntimeKey}.");
            }
        }
    }
}
