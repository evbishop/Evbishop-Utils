using Cysharp.Threading.Tasks;
using Evbishop.Runtime.Pooling;
using Evbishop.Runtime.Singletons;
using UnityEngine;

namespace Evbishop.Runtime.UI.Tooltips
{
    [RequireComponent(
        typeof(Canvas),
        typeof(RectTransform))]
    public class UICanvasTooltips : MonoSingleton<UICanvasTooltips>
    {
        public Canvas Canvas { get; private set; }
        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            if (TryInitializeSingleton())
            {
                Canvas = GetComponent<Canvas>();
                RectTransform = GetComponent<RectTransform>();
            }
        }

        public async UniTask<UITooltip> GetTooltipAsync(UITooltipInfo info, Transform parent)
        {
            var item = await Pools.InstantiateAsync(info.PrefabReference, Vector3.zero, Quaternion.identity, parent);
            if (item == null || !item.TryGetComponent(out UITooltip tooltip))
                return null;
            return tooltip;
        }
    }
}