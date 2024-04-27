using UnityEngine;
using UnityEngine.UI;

namespace Evbishop.Runtime.UIUtils
{
    public class GridLayoutCellScaler : MonoBehaviour
    {
        [SerializeField] Canvas canvas;
        [SerializeField] GridLayoutGroup gridLayoutGroup;
        [SerializeField] float targetWidthFor1920x1080;
        [SerializeField] float targetHeightFor1920x1080;

        void Start()
        {
            float cellWidth = canvas.pixelRect.width * (targetWidthFor1920x1080 / 1920);
            float cellHeight = canvas.pixelRect.height * (targetHeightFor1920x1080 / 1080);
            gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
        }
    }
}
