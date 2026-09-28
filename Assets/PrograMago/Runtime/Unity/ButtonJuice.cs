using UnityEngine;
using UnityEngine.EventSystems;

namespace PrograMago.UnityIntegration
{
    public sealed class ButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField, Range(1f, 1.12f)] private float hoverScale = 1.035f;
        [SerializeField, Range(0.85f, 1f)] private float pressedScale = 0.94f;
        [SerializeField, Min(1f)] private float response = 24f;

        private RectTransform rectTransform;
        private Vector3 baseScale;
        private bool pointerInside;
        private bool pointerDown;

        private void Awake() => CacheTransform();

        private void OnEnable()
        {
            CacheTransform();
            pointerInside = pointerDown = false;
        }

        private void OnDisable()
        {
            pointerInside = pointerDown = false;
            if (rectTransform != null) rectTransform.localScale = baseScale;
        }

        private void Update()
        {
            if (rectTransform == null) return;
            float multiplier = pointerDown ? pressedScale : pointerInside ? hoverScale : 1f;
            float blend = 1f - Mathf.Exp(-response * Time.unscaledDeltaTime);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, baseScale * multiplier, blend);
        }

        public void OnPointerEnter(PointerEventData eventData) => pointerInside = true;
        public void OnPointerExit(PointerEventData eventData) { pointerInside = false; pointerDown = false; }
        public void OnPointerDown(PointerEventData eventData) => pointerDown = true;
        public void OnPointerUp(PointerEventData eventData) => pointerDown = false;

        private void CacheTransform()
        {
            rectTransform = transform as RectTransform;
            if (rectTransform != null) baseScale = rectTransform.localScale;
        }
    }
}
