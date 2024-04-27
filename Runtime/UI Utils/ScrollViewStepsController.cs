using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Evbishop.Runtime.UIUtils
{
    public class ScrollViewStepsController : MonoBehaviour
    {
        [SerializeField] ScrollRectSnap scrollRectSnap;
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] bool isVertical = true;
        [SerializeField] int itemsInRow;

        [InfoBox("Only one of these should be filled")]
        [SerializeField, BoxGroup("Layout")] GridLayoutGroup grid;
        [SerializeField, BoxGroup("Layout")] VerticalLayoutGroup verticalLayout;
        [SerializeField, BoxGroup("Layout")] HorizontalLayoutGroup horizontalLayout;

        private void Awake()
        {
            ScrollViewElement.OnSelectedRectTransform += ScrollToChild;
        }

        private void OnDestroy()
        {
            ScrollViewElement.OnSelectedRectTransform -= ScrollToChild;
        }

        public void UpdateScrollStepsCount()
        {
            if (grid != null)
            {
                scrollRect.verticalScrollbar.numberOfSteps = Mathf.Max(1,
                    Mathf.CeilToInt((float)grid.transform.childCount / itemsInRow));
                scrollRectSnap.Screens = scrollRect.verticalScrollbar.numberOfSteps;
            }
            else if (verticalLayout != null)
            {
                // not tested
                scrollRect.verticalScrollbar.numberOfSteps = Mathf.Max(1,
                    verticalLayout.transform.childCount - 1);
                scrollRectSnap.Screens = scrollRect.verticalScrollbar.numberOfSteps;
            }
            else if (horizontalLayout != null)
            {
                scrollRect.horizontalScrollbar.numberOfSteps = Mathf.Max(1,
                    Mathf.CeilToInt((float)horizontalLayout.transform.childCount / itemsInRow));
                scrollRectSnap.Screens = scrollRect.horizontalScrollbar.numberOfSteps;
            }

            scrollRectSnap.Init();
        }

        public void ScrollToChild(RectTransform child)
        {
            if (this.enabled)
            {
                if (isVertical)
                {
                    float upperPadding = 0;
                    float lowerPadding = 0;
                    if (grid)
                    {
                        upperPadding = grid.padding.top;
                        lowerPadding = grid.padding.bottom;
                    }
                    else if (verticalLayout)
                    {
                        upperPadding = verticalLayout.padding.top;
                        lowerPadding = verticalLayout.padding.bottom;
                    }
                    scrollRect.FitScrollAreaToChildVertically(child,
                        upperPadding, lowerPadding);
                }
                else
                {
                    scrollRect.FitScrollAreaToChildHorizontally(child,
                        horizontalLayout.padding.right, horizontalLayout.padding.left);
                }
            }
        }

        public void StepUp()
        {
            scrollRectSnap.StepUp();
        }

        public void StepDown()
        {
            scrollRectSnap.StepDown();
        }

        public void StepRight()
        {
            scrollRectSnap.StepRight();
        }

        public void StepLeft()
        {
            scrollRectSnap.StepLeft();
        }
    }
}