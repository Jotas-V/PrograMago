using System.Collections.Generic;
using System.Text;
using PrograMago.Application;
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
                    if (id >= 3 || string.IsNullOrEmpty(definitions[id])) continue;
                    if (source.Length > 0) source.Append('\n');
                    source.Append(definitions[id]);
                }
                return source.ToString();
            }
        }

        private CodeBlockLocation LocateOrderedSource(int offset)
        {
            var ordered = new List<string>();
            var positions = new List<int>();
            string[] definitions = codeBlocks.Snapshot();
            for (int i = 0; i < timelineOrder.Count; i++)
                if (timelineOrder[i] < 3)
                {
                    ordered.Add(definitions[timelineOrder[i]]);
                    positions.Add(i + 1);
                }
            var document = new CodeBlockDocument();
            document.Restore(ordered.ToArray(), 0);
            CodeBlockLocation location = document.Locate(offset);
            return new CodeBlockLocation(positions[location.BlockNumber - 1], location.Line, location.Column);
        }

        private void RestoreTimelineOrder(int[] saved)
        {
            for (int index = 0; index < combatCodeBlocks.Count; index++)
            {
                string command = combatCodeBlocks[index] ?? string.Empty;
                if (command.Contains("analisarAlvo") || command.Contains("selecionarMagia"))
                    combatCodeBlocks[index] = "lancarMagia();";
            }
            if (combatCodeBlocks.Count == 0)
                combatCodeBlocks.Add("lancarMagia();");
            int count = 3 + combatCodeBlocks.Count;
            var seen = new HashSet<int>();
            bool valid = saved != null && saved.Length == count;
            if (valid)
                foreach (int id in saved)
                    if (id < 0 || id >= count || !seen.Add(id)) { valid = false; break; }
            timelineOrder.Clear();
            if (valid) timelineOrder.AddRange(saved);
            else for (int i = 0; i < count; i++) timelineOrder.Add(i);
            RefreshCodeBlockButtons();
            RefreshCombatActionButtons();
            RefreshTimelineLayout();
        }

        public void MoveTimelineBlock(int sourceId, int targetId)
        {
            if (!CanReorderCombatActions || sourceId == targetId || sourceId < 3 || targetId < 3) return;
            int from = timelineOrder.IndexOf(sourceId), to = timelineOrder.IndexOf(targetId);
            if (from < 0 || to < 0) return;
            StoreSelectedBlock();
            timelineOrder.RemoveAt(from);
            timelineOrder.Insert(to, sourceId);
            if (combat != null) { CompileTimeline(); trace.Clear(); }
            RefreshCodeBlockButtons();
            RefreshCombatActionButtons();
            RefreshTimelineLayout();
            SaveProgress();
        }

        public void RefreshTimelineLayout()
        {
            if (timelineContent == null) return;
            int actionCount = combatActionButtons.Count;
            for (int index = 0; index < actionCount; index++)
            {
                var actionRect = (RectTransform)combatActionButtons[index].transform;
                actionRect.anchoredPosition = new Vector2(index * 130, 10);
                actionRect.sizeDelta = new Vector2(122, 42);
            }
            timelineContent.sizeDelta = new Vector2(actionCount * 130 + 90, 68);
            if (combatActionPanel != null)
            {
                var panel = (RectTransform)combatActionPanel.transform;
                panel.anchoredPosition = Vector2.zero;
                panel.sizeDelta = timelineContent.sizeDelta;
                panel.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
                PositionTimelineControl("AddCombatBlockButton", actionCount * 130 + 12);
                PositionTimelineControl("RemoveCombatBlockButton", actionCount * 130 + 48);
            }
            LayoutDefinitionStrip();
        }

        private void LayoutDefinitionStrip()
        {
            if (definitionStrip == null) return;
            if (editorPanel != null && definitionStrip.parent != editorPanel)
                definitionStrip.SetParent(editorPanel, false);
            if (definitionStrip.parent == editorPanel)
            {
                definitionStrip.anchorMin = new Vector2(0, 1);
                definitionStrip.anchorMax = new Vector2(1, 1);
                definitionStrip.pivot = new Vector2(0, 1);
                definitionStrip.offsetMin = new Vector2(12, -72);
                definitionStrip.offsetMax = new Vector2(-12, -4);
                definitionStrip.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            }
        }
        private void PositionTimelineControl(string name, float x)
        {
            Transform item = combatActionPanel.transform.Find(name);
            if (item == null) return;
            var rect = (RectTransform)item;
            rect.anchoredPosition = new Vector2(x, 10);
            rect.sizeDelta = new Vector2(34, 42);
        }

        private UnityEngine.UI.Button TimelineButton(int id) => id < 3 ? codeBlockButtons[id] :
            id - 3 < combatActionButtons.Count ? combatActionButtons[id - 3] : null;

        private UnityEngine.UI.Button TimelineButtonAt(int position) =>
            position >= 0 && position < timelineOrder.Count ? TimelineButton(timelineOrder[position]) : null;

        private int BlockNumber(int id) => timelineOrder.IndexOf(id) + 1;
    }
}
