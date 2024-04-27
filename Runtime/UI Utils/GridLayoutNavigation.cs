using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Evbishop.Runtime.UIUtils
{
    public class GridLayoutNavigation : MonoBehaviour
    {
        [SerializeField] GridLayoutGroup gridLayoutGroup;
        [SerializeField] bool overwriteTopRow = false;
        [ShowIf(nameof(overwriteTopRow))]
        [SerializeField] Selectable selectOnUp;

        [SerializeField] bool overwriteBottomRow = false;
        [ShowIf(nameof(overwriteBottomRow))]
        [SerializeField] Selectable selectOnDown;

        public Selectable FirstElement { get; private set; }

        public void UpdateGridNavigation()
        {
            int childCount = gridLayoutGroup.transform.childCount;
            var gridTransform = gridLayoutGroup.transform;
            int constraintCount = gridLayoutGroup.constraintCount;

            if (childCount < 2) return;
            for (var i = 0; i < childCount; i++)
            {
                var selectable = gridTransform.GetChild(i).GetComponent<Selectable>();
                var navigation = selectable.navigation;

                navigation.mode = Navigation.Mode.Explicit;

                if (i == 0) // first item
                {
                    FirstElement = gridTransform
                        .GetChild(i)
                        .GetComponent<Selectable>();

                    var rightItem = gridTransform.GetChild(i + 1);
                    navigation.selectOnRight = rightItem.GetComponent<Selectable>();

                    var leftItem = gridTransform.GetChild(childCount - 1);
                    navigation.selectOnLeft = leftItem.GetComponent<Selectable>();
                }
                else if (i == childCount - 1) // last item
                {
                    var rightItem = gridTransform.GetChild(0);
                    navigation.selectOnRight = rightItem.GetComponent<Selectable>();

                    var leftItem = gridTransform.GetChild(i - 1);
                    navigation.selectOnLeft = leftItem.GetComponent<Selectable>();
                }
                else
                {
                    var rightItem = gridTransform.GetChild(i + 1);
                    navigation.selectOnRight = rightItem.GetComponent<Selectable>();

                    var leftItem = gridTransform.GetChild(i - 1);
                    navigation.selectOnLeft = leftItem.GetComponent<Selectable>();
                }

                if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
                {
                    int rowCount = (childCount - 1) / constraintCount + 1;
                    int j, childIndex;
                    if (rowCount > 1)
                    {
                        if (i < constraintCount) // top row
                        {
                            j = i;
                            childIndex = -1;
                            while (childIndex < 0 || childIndex >= childCount)
                            {
                                childIndex = j + constraintCount * (rowCount - 1);
                                j--;
                            }

                            navigation.selectOnUp = overwriteTopRow
                                ? selectOnUp
                                : gridTransform.GetChild(childIndex).GetComponent<Selectable>();

                            j = i;
                            childIndex = -1;
                            while (childIndex < 0 || childIndex >= childCount)
                            {
                                childIndex = j + constraintCount;
                                j--;
                            }

                            navigation.selectOnDown = gridTransform.GetChild(childIndex).GetComponent<Selectable>();
                        }
                        else if (i >= constraintCount * (rowCount - 1)) // bottom row
                        {
                            Transform upItem = gridTransform.GetChild(i - constraintCount);
                            navigation.selectOnUp = upItem.GetComponent<Selectable>();

                            Transform downItem = gridTransform.GetChild(i - (constraintCount * (rowCount - 1)));

                            navigation.selectOnDown = overwriteBottomRow
                                ? selectOnDown
                                : downItem.GetComponent<Selectable>();
                        }
                        else
                        {
                            Transform upItem = gridTransform.GetChild(i - constraintCount);
                            navigation.selectOnUp = upItem.GetComponent<Selectable>();

                            j = i;
                            childIndex = -1;
                            while (childIndex < 0 || childIndex >= childCount)
                            {
                                childIndex = j + constraintCount;
                                j--;
                            }
                            navigation.selectOnDown = gridTransform.GetChild(childIndex).GetComponent<Selectable>();
                        }
                    }
                }

                selectable.navigation = navigation;
            }
        }
    }
}