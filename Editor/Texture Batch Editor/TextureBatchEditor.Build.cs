#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Evbishop.Editor.Texture_Batch_Editor
{
    public partial class TextureBatchEditor
    {
        private const string GROUP_BUILD = "Search from build report";
        private const string BUILD_REPORT_PATH_LIBRARY = "Library/LastBuild.buildreport";
        private const string BUILD_REPORT_PATH_ASSETS = "Assets/BuildReports";

        [SerializeField, TabGroup(GROUP_BUILD), Tooltip("If enabled, the tool" +
            " will get build data from the BuildReportContainer scriptable object")]
        private bool useManuallyCopiedBuildReport;
        [SerializeField, TabGroup(GROUP_BUILD)] private string[] filterFolders;

        private void CrawlTexturesInLastBuildReport(TextureAction action, bool isOn = false, int value = 0)
        {
            int count = 0;

            string[] paths = null;
            PackedAssets[] files = null;

            if (useManuallyCopiedBuildReport)
            {
                paths = BuildReportContainer.Instance.FilesInBuild;
            }
            else
            {
                if (!Directory.Exists(BUILD_REPORT_PATH_ASSETS))
                    Directory.CreateDirectory(BUILD_REPORT_PATH_ASSETS);
                DateTime date = File.GetLastWriteTime(BUILD_REPORT_PATH_LIBRARY);
                string assetPath = BUILD_REPORT_PATH_ASSETS + "/Build_" + date.ToString("yyyy-dd-MMM-HH-mm-ss") + ".buildreport";
                File.Copy(BUILD_REPORT_PATH_LIBRARY, assetPath, true);
                AssetDatabase.ImportAsset(assetPath);
                files = AssetDatabase.LoadAssetAtPath<BuildReport>(assetPath).packedAssets;
            }

            HashSet<string> filePathsInBuild = new();

            int iterations = useManuallyCopiedBuildReport ? paths.Length : files.Length;
            for (int i = 0; i < iterations; i++)
            {
                if (useManuallyCopiedBuildReport)
                {
                    TryGetFile(paths[i], filePathsInBuild);
                }
                else
                {
                    int contentsIterations = files[i].contents.Length;
                    for (int j = 0; j < contentsIterations; j++)
                    {
                        TryGetFile(files[i].contents[j].sourceAssetPath, filePathsInBuild);
                    }
                }
            }

            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (string path in filePathsInBuild)
                {
                    AssetImporter assetImporter = AssetImporter.GetAtPath(path);
                    if (assetImporter is null ||
                        assetImporter is not TextureImporter textureImporter ||
                        !textureImporterTypes.Contains(textureImporter.textureType))
                        continue;

                    action(isOn, value, textureImporter, path, ref count);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                Debug.Log($"{count} textures processed");
            }
        }

        private void TryGetFile(string path, HashSet<string> filePathsInBuild)
        {
            if (filterFolders is not null && filterFolders.Length > 0)
            {
                for (int j = 0; j < filterFolders.Length; j++)
                {
                    if (path.Contains(filterFolders[j]))
                    {
                        filePathsInBuild.Add(path);
                        break;
                    }
                }
            }
            else
            {
                filePathsInBuild.Add(path);
            }
        }

        [Button(COUNT_TEXTURES), TabGroup(GROUP_BUILD)]
        private void CountTexturesInBuild()
        {
            CrawlTexturesInLastBuildReport((bool _, int _, TextureImporter _, string _, ref int count) =>
            {
                count++;
            });
        }

        [Button(SET_CRUNCH_COMPRESSION), TabGroup(GROUP_BUILD)]
        private void SetCrunchCompressionBuild(bool isOn, int quality = 75)
        {
            CrawlTexturesInLastBuildReport(SetCrunchCompression, isOn, quality);
        }

        [Button(SET_MIP_STREAMING), TabGroup(GROUP_BUILD)]
        private void SetMipStreamingBuild(bool isOn, int priority = 0)
        {
            CrawlTexturesInLastBuildReport(SetMipStreaming, isOn, priority);
        }
    }
}
#endif
