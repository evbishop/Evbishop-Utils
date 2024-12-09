using Sirenix.OdinInspector;
using UnityEngine;

namespace Evbishop.Runtime.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIBehaviour : SerializedMonoBehaviour
    {
        protected const string CALLBACKS = "Callbacks";

        public RectTransform RectTransform { get; private set; }

        protected virtual void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        protected virtual void Start()
        {

        }

        protected virtual void OnDestroy()
        {

        }
    }
}