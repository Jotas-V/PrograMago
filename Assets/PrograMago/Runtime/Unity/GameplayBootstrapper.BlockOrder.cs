using System.Collections.Generic;
using System.Text;
using PrograMago.Application;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace PrograMago.UnityIntegration
{
    public sealed partial class GameplayBootstrapper
    {
        // IDs 0–2 identify definition slots; the remaining IDs identify action slots.
        private readonly List<int> timelineOrder = new List<int> { 0, 1, 2, 3 };

        private string OrderedSourceCode
        {
            get
            {
                var source = new StringBuilder();
                string[] definitions = codeBlocks.Snapshot();
                foreach (int id in timelineOrder)
                {
                    if (id >= 2 || string.IsNullOrEmpty(definitions[id])) continue;
                    if (source.Length > 0) source.Append('\n');
                    source.Append(definitions[id]);
                }
                return source.ToString();
            }
        }

        private CodeBlockLocation LocateOrderedSource(int offset)
        {
            string[] definitions = codeBlocks.Snapshot();
            var document = new CodeBlockDocument();
            document.Restore(definitions, 0);
            return document.Locate(offset);
        }

        private void RestoreTimelineOrder(int[] saved)
        {
            string attackCode = "lancarMagia();";
            string instanceName = CurrentMago == null ? "mago" : CurrentMago.InstanceName;
            for (int index = combatCodeBlocks.Count - 1; index >= 0; index--)
            {
                if (CombatCodeCompiler.TryCompileBattle(new[] { combatCodeBlocks[index] },
                    instanceName, out _, out _, out _, out _))
                {
                    attackCode = combatCodeBlocks[index];
                    break;
                }
            }
            combatCodeBlocks.Clear();
            combatCodeBlocks.Add(attackCode);
            timelineOrder.Clear();
            timelineOrder.AddRange(new[] { 0, 1, 2, 3 });
            RebuildActionArrows();
            for (int index = 0; index < codeBlockButtons.Length; index++)
            {
                CombatActionDragHandle dragHandle = codeBlockButtons[index] == null
                    ? null : codeBlockButtons[index].GetComponent<CombatActionDragHandle>();
                if (dragHandle != null) dragHandle.enabled = false;
            }
            if (addCombatBlockButton != null) addCombatBlockButton.gameObject.SetActive(false);
            if (removeCombatBlockButton != null) removeCombatBlockButton.gameObject.SetActive(false);
            RefreshCodeBlockButtons();
        }

        public void MoveTimelineBlock(int sourceId, int targetId)
        {
            // The gameplay timeline now contains one fixed Atacar block.
        }

        public void RefreshTimelineLayout()
        {
            if (timelineContent == null) return;
            LayoutDefinitionStrip();
            int actionCount = Mathf.Min(1, combatActionButtons.Count);
            for (int index = 0; index < actionCount; index++)
            {
                var actionRect = (RectTransform)combatActionButtons[index].transform;
                actionRect.anchoredPosition = new Vector2(index * 126, 9);
                actionRect.sizeDelta = new Vector2(116, 41);
            }
            timelineContent.sizeDelta = new Vector2(TimelineContentWidth, 68);
            if (combatActionPanel != null)
            {
                var panel = (RectTransform)combatActionPanel.transform;
                panel.anchorMin = panel.anchorMax = panel.pivot = Vector2.zero;
                panel.anchoredPosition = new Vector2(CombatActionStripStartX, 0);
                panel.sizeDelta = new Vector2(122, 68);
                Image image = panel.GetComponent<Image>();
                if (image != null) image.raycastTarget = false;
                Transform title = panel.Find("CombatActionTitle");
                if (title != null)
                {
                    title.gameObject.SetActive(true);
                    TMP_Text titleText = title.GetComponent<TMP_Text>();
                    if (titleText != null) titleText.text = "AÇÃO DE BATALHA";
                    RectTransform titleRect = title.GetComponent<RectTransform>();
                    titleRect.anchorMin = new Vector2(0, 0.75f);
                    titleRect.anchorMax = Vector2.one;
                    titleRect.offsetMin = titleRect.offsetMax = Vector2.zero;
                }
            }
        }

        private void LayoutDefinitionStrip()
        {
            if (definitionStrip == null || timelineContent == null) return;
            if (definitionStrip.parent != timelineContent)
                definitionStrip.SetParent(timelineContent, false);
            definitionStrip.anchorMin = definitionStrip.anchorMax = definitionStrip.pivot = Vector2.zero;
            definitionStrip.anchoredPosition = Vector2.zero;
            definitionStrip.sizeDelta = new Vector2(DefinitionStripWidth, 68);
            definitionStrip.SetAsFirstSibling();
            Image image = definitionStrip.GetComponent<Image>();
            if (image != null) image.raycastTarget = false;
        }

        private UnityEngine.UI.Button TimelineButton(int id) => id < 3 ? codeBlockButtons[id] :
            id - 3 < combatActionButtons.Count ? combatActionButtons[id - 3] : null;

        private UnityEngine.UI.Button TimelineButtonAt(int position) =>
            position >= 0 && position < timelineOrder.Count ? TimelineButton(timelineOrder[position]) : null;

    }
}
