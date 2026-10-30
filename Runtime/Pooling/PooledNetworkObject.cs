using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Evbishop.Runtime.Pooling
{
    [Serializable]
    public struct PooledNetworkObject
    {
        [field: SerializeField] public GameObject Prefab { get; private set; }
        [field: SerializeField, GUIColor(nameof(GetCountColor)), MinValue(0)] public int Count { get; private set; }

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
    }
}