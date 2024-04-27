#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Evbishop.Editor.Texture_Batch_Editor
{
    [CreateAssetMenu(menuName = "Evbishop/Editor/Build Report Container", fileName = "BuildReportContainer")]
    public class BuildReportContainer : ScriptableObject
    {
        #region Singleton

        private static string s_assetName => nameof(BuildReportContainer);
        private static string s_path => $"Assets/Evbishop/Editor/Texture Batch Editor/{s_assetName}.asset";

        private static BuildReportContainer s_instance;

        public static BuildReportContainer Instance
        {
            get
            {
                if (s_instance != null) return s_instance;
                s_instance = AssetDatabase.LoadAssetAtPath(s_path, typeof(BuildReportContainer)) as BuildReportContainer;
                if (s_instance != null) return s_instance;
                s_instance = CreateInstance<BuildReportContainer>();
                AssetDatabase.CreateAsset(s_instance, s_path);
                return s_instance;
            }
        }

        #endregion

        [SerializeField, TextArea(5, 10)] private string filesInBuild;
        [field: SerializeField, ReadOnly] public string[] FilesInBuild { get; private set; }

        public void SetFilesInBuild(string files)
        {
            filesInBuild = files;
            ConvertFilesStringToFilesArray();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

        [Button]
        private void ConvertFilesStringToFilesArray()
        {
            FilesInBuild = filesInBuild.GetFilesFromBuildReport().ToArray();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }

    public static class StringExtensions
    {
        public static IEnumerable<string> GetFilesFromBuildReport(this string input)
        {
            if (input is null)
                yield break;

            using (System.IO.StringReader reader = new System.IO.StringReader(input))
            {
                string line;
                while ((line = reader.ReadLine()) is not null)
                {
                    int startIndex = line.IndexOf("Assets/");
                    if (startIndex < 0)
                        continue;
                    line = line.Substring(startIndex);
                    yield return line;
                }
            }
        }
    }
}
#endif
