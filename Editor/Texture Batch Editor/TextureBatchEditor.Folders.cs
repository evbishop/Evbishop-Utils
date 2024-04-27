#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Evbishop.Editor.Texture_Batch_Editor
{
    public partial class TextureBatchEditor
    {
        private const string GROUP_FOLDERS = "Search from folders";

        [SerializeField, TabGroup(GROUP_FOLDERS)] private string[] searchFolders;

        private void CrawlTexturesInFolders(TextureAction action, bool isOn = false, int value = 0)
        {
            string[] guids = AssetDatabase.FindAssets("t:texture", searchFolders);
            int count = 0;
            try
            {
                AssetDatabase.StartAssetEditing();
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (textureImporter)
                    {
                        if (!textureImporterTypes.Contains(textureImporter.textureType))
                            continue;

                        action(isOn, value, textureImporter, path, ref count);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                Debug.Log($"{count} textures processed");
            }
        }

        [Button(COUNT_TEXTURES), TabGroup(GROUP_FOLDERS)]
        private void CountTexturesFolders()
        {
            CrawlTexturesInFolders((bool _, int _, TextureImporter _, string _, ref int count) =>
            {
                count++;
            });
        }

        [Button(SET_CRUNCH_COMPRESSION), TabGroup(GROUP_FOLDERS)]
        private void SetCrunchCompressionFolders(bool isOn, int quality = 75)
        {
            CrawlTexturesInFolders(SetCrunchCompression, isOn, quality);
        }

        [Button(SET_MIP_STREAMING), TabGroup(GROUP_FOLDERS)]
        private void SetMipStreamingFolder(bool isOn, int priority = 0)
        {
            CrawlTexturesInFolders(SetMipStreaming, isOn, priority);
        }
    }
}
#endif
