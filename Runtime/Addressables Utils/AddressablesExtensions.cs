using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Evbishop.Runtime.AddressablesUtils
{
    public static class AddressablesExtensions
    {
        private static readonly AssetReference s_EmptyAssetReference = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanInstantiate(this AssetReference targetAssetReference)
        {
            return !IsEmpty(targetAssetReference) && HasRuntimeKey(targetAssetReference) &&
                targetAssetReference.RuntimeKeyIsValid();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanRelease(this AsyncOperationHandle<GameObject> targetAsyncOperationHandle,
            bool autoRelease = false)
        {
            if (!targetAsyncOperationHandle.IsValid())
                return false;

            if (targetAsyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                if (autoRelease)
                    Addressables.ReleaseInstance(targetAsyncOperationHandle);
                return true;
            }

            if (targetAsyncOperationHandle.Result == null)
                return false;

            if (autoRelease)
                Addressables.ReleaseInstance(targetAsyncOperationHandle);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasRuntimeKey(this AssetReference targetAssetReference)
        {
            if (targetAssetReference == null)
                return false;

            return !string.IsNullOrEmpty((string)targetAssetReference.RuntimeKey);
        }

        public static bool IsEmpty(this AssetReference targetAssetReference)
        {
            return targetAssetReference.AssetGUID == s_EmptyAssetReference.AssetGUID;
        }
    }
}