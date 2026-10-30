using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Evbishop.Runtime.Pooling
{
    [Serializable]
    public struct PreloadedAddressablePrefab
    {
        [field: SerializeField] public AssetReferenceGameObject Prefab { get; private set; }
        [field: SerializeField, GUIColor(nameof(GetCountColor)), MinValue(0)] public int Count { get; private set; }
        [field: SerializeField, GUIColor(nameof(GetSizeColor)), MinValue(0)] public int MaxSize { get; private set; }

        private Color GetCountColor()
        {
            if (Count <= 0)
            {
                return Color.red;
            }
            else
            {
                return Color.white;
            }
        }

        private Color GetSizeColor()
        {
            if (Count <= 0)
            {
                return Color.red;
            }
            else
            {
                return Color.white;
            }
        }
    }
}