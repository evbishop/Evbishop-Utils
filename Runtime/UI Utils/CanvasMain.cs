using Evbishop.Runtime.Singletons;
using UnityEngine;

namespace Evbishop.Runtime.UIUtils
{
    [RequireComponent(typeof(RectTransform))]
    public class CanvasMain : MonoSingleton<CanvasMain>
    {
        public RectTransform RectTransform { get; private set; }

        void Awake()
        {
            if (TryInitializeSingleton())
            {
                RectTransform = GetComponent<RectTransform>();
            }
        }
    }
}
