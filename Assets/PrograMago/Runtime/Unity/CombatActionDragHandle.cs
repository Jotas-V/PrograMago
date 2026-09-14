using UnityEngine;
using UnityEngine.EventSystems;

namespace PrograMago.UnityIntegration
{
    public sealed class CombatActionDragHandle : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        private GameplayBootstrapper owner;
        private int index;

        public void Configure(GameplayBootstrapper owner, int index)
        {
            this.owner = owner;
            this.index = index;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (owner == null || !owner.CanReorderCombatActions) return;
            eventData.pointerDrag = gameObject;
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.pointerDrag == gameObject) eventData.pointerDrag = null;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (owner == null || !owner.CanReorderCombatActions ||
                eventData.pointerDrag == null) return;
            CombatActionDragHandle source =
                eventData.pointerDrag.GetComponent<CombatActionDragHandle>();
            if (source == null || source.owner != owner) return;
            owner.ReorderCombatAction(source.index, index);
        }
    }
}
