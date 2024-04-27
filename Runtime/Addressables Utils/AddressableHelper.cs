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
    }
}
