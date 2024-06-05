using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Evbishop.Runtime.SceneUtils
{
    [CreateAssetMenu(fileName = "SceneHelper", menuName = "Scriptable Objects/Singletons/Scene Helper")]
    public class SceneHelper : SerializedScriptableObject
    {
        #region Singleton
        private static string s_assetName => nameof(SceneHelper);
        private static string s_loadPath => $"Singletons/{s_assetName}";
        private static string s_savePath => $"Assets/Resources/{s_loadPath}.asset";

        private static SceneHelper s_instance;

        public static SceneHelper Instance
        {
            get
            {
                if (s_instance != null)
                    return s_instance;

                s_instance = Resources.Load<SceneHelper>(s_loadPath);
                if (s_instance != null)
                    return s_instance;

                s_instance = CreateInstance<SceneHelper>();
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(s_instance, s_savePath);
#endif
                return s_instance;
            }
        }
        #endregion

        [field: SerializeField] public Dictionary<SceneDesignation, SceneInfo> Scenes { get; private set; }
    }
}