using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Evbishop.Runtime.AddressablesUtils
{
    [Serializable]
    public class AssetReferenceScene : AssetReference
    {
        [SerializeField] private string _sceneName;

        public string SceneName => _sceneName;

        public AssetReferenceScene() { }

        public AssetReferenceScene(string guid) : base(guid) { }

#if UNITY_EDITOR
        public override bool SetEditorAsset(UnityEngine.Object value)
        {
            if (value != null)
            {
                if (value is SceneAsset sceneAsset)
                {
                    _sceneName = sceneAsset.name;
                }
            }
            else
            {
                _sceneName = string.Empty;
            }
            return base.SetEditorAsset(value);
        }
#endif

        public override bool ValidateAsset(string path)
        {
#if UNITY_EDITOR
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            return scene != null;
#else
            return false;
#endif
        }

        public override bool ValidateAsset(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            var scene = obj as SceneAsset;
            return scene != null;
#else
            return false;
#endif
        }
    }
}