using System;
using UnityEngine;

namespace Evbishop.Runtime.UIUtils
{
    [RequireComponent(typeof(RectTransform))]
    public class ScrollViewElement : MonoBehaviour
    {
        RectTransform rectTransform;

        public static Action<RectTransform> OnSelectedRectTransform;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public virtual void Select()
        {
            OnSelectedRectTransform?.Invoke(rectTransform);
        }

        public virtual void Deselect()
        {

        }
    }
}
