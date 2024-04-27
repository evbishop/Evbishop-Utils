using UnityEngine;
using UnityEngine.UI;

public static class ScrollRectExtensions
{
    public static void FitScrollAreaToChildVertically(this ScrollRect sr, RectTransform selected, float scrollAreaUpperBoundMargin = -10, float scrollAreaLowerBoundMargin = -10)
    {
        var selectedRectTransform = selected;
        Canvas.ForceUpdateCanvases();

        var objPosition = (Vector2)sr.transform.InverseTransformPoint(selectedRectTransform.position);
        var scrollHeight = sr.GetComponent<RectTransform>().rect.height;
        var objHeight = selectedRectTransform.rect.height;

        float ubound = scrollHeight / 2 - scrollAreaUpperBoundMargin;
        float dbound = -scrollHeight / 2 + scrollAreaLowerBoundMargin;

        float itemdbound = objPosition.y - objHeight / 2;
        float itemubound = objPosition.y + objHeight / 2;

        if (itemdbound < dbound)
            sr.content.anchoredPosition += new Vector2(0, dbound - itemdbound);
        else if (itemubound > ubound)
            sr.content.anchoredPosition += new Vector2(0, -(itemubound - ubound));
    }

    public static void FitScrollAreaToChildHorizontally(this ScrollRect sr, RectTransform selected, float scrollAreaUpperBoundMargin = -10, float scrollAreaLowerBoundMargin = -10)
    {
        var selectedRectTransform = selected;
        Canvas.ForceUpdateCanvases();

        var objPosition = (Vector2)sr.transform.InverseTransformPoint(selectedRectTransform.position);
        var scrollWidth = sr.GetComponent<RectTransform>().rect.width;
        var objWidth = selectedRectTransform.rect.width;

        float ubound = scrollWidth / 2 - scrollAreaUpperBoundMargin;
        float dbound = -scrollWidth / 2 + scrollAreaLowerBoundMargin;

        float itemdbound = objPosition.y - objWidth / 2;
        float itemubound = objPosition.y + objWidth / 2;

        if (itemdbound < dbound)
            sr.content.anchoredPosition += new Vector2(dbound - itemdbound, 0);
        else if (itemubound > ubound)
            sr.content.anchoredPosition += new Vector2(-(itemubound - ubound), 0);
    }
}
