using UnityEngine;
using UnityEngine.UI;

namespace Evbishop.Runtime.UIUtils
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectSnap : MonoBehaviour
    {
        public int Screens { get; set; }

        private ScrollRect scroll;
        private float[] points;
        private float stepSize;

        public void Init()
        {
            scroll = GetComponent<ScrollRect>();
            points = new float[Screens];
            stepSize = 1f / (Screens - 1);
            for (int i = 0; i < Screens; i++)
            {
                points[i] = i * stepSize;
            }
        }

        public void StepUp()
        {
            int target = Mathf.Min(Screens - 1, Mathf.RoundToInt(scroll.verticalNormalizedPosition / stepSize) + 1);
            scroll.verticalNormalizedPosition = points[target];
        }

        public void StepDown()
        {
            int target = Mathf.Max(0, Mathf.RoundToInt(scroll.verticalNormalizedPosition / stepSize) - 1);
            scroll.verticalNormalizedPosition = points[target];
        }

        public void StepRight()
        {
            int target = Mathf.Min(Screens - 1, Mathf.RoundToInt(scroll.horizontalNormalizedPosition / stepSize) + 1);
            scroll.horizontalNormalizedPosition = points[target];
        }

        public void StepLeft()
        {
            int target = Mathf.Max(0, Mathf.RoundToInt(scroll.horizontalNormalizedPosition / stepSize) - 1);
            scroll.horizontalNormalizedPosition = points[target];
        }
    }
}