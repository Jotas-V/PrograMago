using UnityEngine;
using UnityEngine.EventSystems;

namespace PrograMago.UnityIntegration
{
    public sealed class CombatActionDragHandle : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        private GameplayBootstrapper owner;
        private int index;
        private Vector2 initialPosition;
        private Vector2 pointerOffset;
        private CanvasGroup group;
        private bool dragging;

        public void Configure(GameplayBootstrapper owner, int index)
        {
            this.owner = owner;
            this.index = index + 3;
        }

        public void ConfigureBlock(GameplayBootstrapper owner, int blockId)
        {
            this.owner = owner;
            index = blockId;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (owner == null || !owner.CanReorderCombatActions) return;
            eventData.pointerDrag = gameObject;
            var rect = (RectTransform)transform;
            initialPosition = rect.anchoredPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)rect.parent,
                eventData.position, eventData.pressEventCamera, out Vector2 point);
            pointerOffset = initialPosition - point;
            group = GetComponent<CanvasGroup>();
            if (group == null) group = gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.alpha = 0.65f;
            dragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging) return;
            var rect = (RectTransform)transform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)rect.parent,
                    eventData.position, eventData.pressEventCamera, out Vector2 point))
                rect.anchoredPosition = point + pointerOffset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.pointerDrag == gameObject) eventData.pointerDrag = null;
            if (!dragging) return;
            ((RectTransform)transform).anchoredPosition = initialPosition;
            group.blocksRaycasts = true;
            group.alpha = 1f;
            dragging = false;
            owner.RefreshTimelineLayout();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (owner == null || !owner.CanReorderCombatActions ||
                eventData.pointerDrag == null) return;
            CombatActionDragHandle source =
                eventData.pointerDrag.GetComponent<CombatActionDragHandle>();
            if (source == null || source.owner != owner) return;
            owner.MoveTimelineBlock(source.index, index);
        }
    }
}
