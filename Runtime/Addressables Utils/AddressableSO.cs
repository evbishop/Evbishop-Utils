using Sirenix.OdinInspector;
using UnityEngine;

namespace Evbishop.Runtime.AddressablesUtils
{
    public class AddressableSO : SerializedScriptableObject
    {
        [SerializeField, ReadOnly] private string _guid;
        public string Guid => _guid;

        private long _fileId;
        public long FileId => _fileId;

#if UNITY_EDITOR
        private void OnEnable()
        {
            // re-evaluates guid when duplicating SOs
            ForceGuidUpdate();
        }

        protected virtual void OnValidate()
        {
            ForceGuidUpdate();
        }

        public void ForceGuidUpdate()
        {
            // prevent instantiation of these trying to update the guid
            // instances will not have a valid guid and will become 0's in the editor
            if (Application.isPlaying) return;

            UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(GetInstanceID(), out _guid, out _fileId);
        }
#endif
    }
}
