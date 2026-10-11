using UnityEngine;

namespace PrograMago.UnityIntegration
{
    public static class ThinScrollbar
    {
        public static UnityEngine.UI.Scrollbar Create(Transform parent, string name)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Scrollbar));
            var rect = item.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(1, 0); rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(-12, 8); rect.offsetMax = new Vector2(-4, -8);
            item.GetComponent<UnityEngine.UI.Image>().color = new Color(0.24f, .18f, .1f, .12f);
            var handle = new GameObject("Handle", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.SetParent(rect, false); handleRect.anchorMin = Vector2.zero; handleRect.anchorMax = Vector2.one;
            handleRect.offsetMin = handleRect.offsetMax = Vector2.zero;
            var graphic = handle.GetComponent<UnityEngine.UI.Image>(); graphic.color = new Color(.32f, .23f, .12f, .55f);
            var bar = item.GetComponent<UnityEngine.UI.Scrollbar>();
            bar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
            bar.handleRect = handleRect; bar.targetGraphic = graphic;
            return bar;
        }
    }
}
