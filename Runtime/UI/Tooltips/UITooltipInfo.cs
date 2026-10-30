using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Evbishop.Runtime.UI.Tooltips
{
    [CreateAssetMenu(fileName = "Tooltip Info - ", menuName = "Scriptable Objects/Tooltip Info")]
    public class UITooltipInfo : SerializedScriptableObject
    {
        [field: SerializeField, Required] public AssetReferenceGameObject PrefabReference { get; private set; }
        [field: SerializeField] public string[] Texts { get; private set; }
        [field: SerializeField] public Sprite[] Sprites { get; private set; }
    }
}
