using UnityEngine;
using UnityEngine.Events;

namespace Evbishop.Runtime.UI
{
    public static class UIPopupExtensions
    {
        public static UIPopup SetTexts(this UIPopup popup, params string[] texts)
        {
            if (popup.Texts == null || popup.Texts.Count == 0)
                return popup;

            for (int i = 0; i < texts.Length; i++)
            {
                if (i >= popup.Texts.Count)
                    return popup;
                popup.Texts[i].text = texts[i];
            }

            return popup;
        }

        public static UIPopup SetSprites(this UIPopup popup, params Sprite[] sprites)
        {
            if (popup.Images == null || popup.Images.Count == 0)
                return popup;

            for (int i = 0; i < sprites.Length; i++)
            {
                if (i >= popup.Images.Count)
                    return popup;
                popup.Images[i].sprite = sprites[i];
            }

            return popup;
        }

        public static UIPopup SetEvents(this UIPopup popup, params UnityEvent[] events)
        {
            if (popup.Buttons == null || popup.Buttons.Count == 0)
                return popup;

            for (int i = 0; i < events.Length; i++)
            {
                if (i >= popup.Buttons.Count)
                    return popup;
                var e = events[i];
                if (e == null)
                    continue;
                popup.AddListenerToButtonUntilHidden(i, e.Invoke);
            }

            return popup;
        }
    }
}