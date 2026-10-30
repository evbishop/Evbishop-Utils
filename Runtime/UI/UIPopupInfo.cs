using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;

namespace Evbishop.Runtime.UI
{
    [CreateAssetMenu(fileName = "Popup Info - ", menuName = "Scriptable Objects/Popup Info")]
    public class UIPopupInfo : SerializedScriptableObject
    {
        [field: SerializeField, Required] public AssetReferenceGameObject PrefabReference { get; private set; }
        [field: SerializeField] public string[] Texts { get; private set; }
        [field: SerializeField] public Sprite[] Sprites { get; private set; }

        public string GetLocalizedText(int index)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString("Popups", Texts[index]);
        }

        public string[] GetLocalizedTexts()
        {
            string[] result = new string[Texts.Length];
            for (int i = 0; i < Texts.Length; i++)
            {
                result[i] = GetLocalizedText(i);
            }
            return result;
        }
    }
}