using System.Collections.Generic;
using PrograMago.Application;
using PrograMago.Domain;
using TMPro;
using UnityEngine;

namespace PrograMago.UnityIntegration
{
    public sealed partial class GameplayBootstrapper
    {
        private readonly List<string> combatCodeBlocks = new List<string>(CombatCodeCompiler.BattleBlocks());
        private int selectedCombatBlock = -1;
        [SerializeField] private RectTransform timelineContent;
        [SerializeField] private RectTransform definitionStrip;
        [SerializeField] private UnityEngine.UI.ScrollRect timelineScroll;
        private bool timelineAvailable;

        private void CreateTimelineStrip()
        {
            Transform parent = battleButton.transform.parent;
            RectTransform strip = CreatePanel("CodeTimeline", parent, Vector2.zero, new Vector2(1, 0), new Color32(28, 30, 45, 255));
            strip.offsetMin = new Vector2(12, 10);
            strip.offsetMax = new Vector2(-208, 78);
            timelineScroll = strip.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
            timelineScroll.horizontal = true;
            timelineScroll.vertical = false;
            timelineScroll.scrollSensitivity = 35;
            timelineScroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            RectTransform viewport = CreatePanel("Viewport", strip, Vector2.zero, Vector2.one, Color.white);
            viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;
            timelineContent = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            timelineContent.SetParent(viewport, false);
            timelineContent.anchorMin = timelineContent.anchorMax = new Vector2(0, 0);
            timelineContent.pivot = Vector2.zero;
            timelineContent.sizeDelta = new Vector2(306, 68);
            timelineScroll.viewport = viewport;
            timelineScroll.content = timelineContent;
            RectTransform track = CreatePanel("TimelineScrollbar", strip, Vector2.zero, new Vector2(1, 0), new Color32(44, 47, 66, 255));
            track.pivot = Vector2.zero;
            track.sizeDelta = new Vector2(0, 6);
            RectTransform handle = CreatePanel("Handle", track, Vector2.zero, Vector2.one, new Color32(145, 139, 180, 255));
            var scrollbar = track.gameObject.AddComponent<UnityEngine.UI.Scrollbar>();
            scrollbar.handleRect = handle;
            scrollbar.targetGraphic = handle.GetComponent<UnityEngine.UI.Image>();
            scrollbar.direction = UnityEngine.UI.Scrollbar.Direction.LeftToRight;
            timelineScroll.horizontalScrollbar = scrollbar;
            timelineScroll.horizontalScrollbarVisibility = UnityEngine.UI.ScrollRect.ScrollbarVisibility.AutoHide;
            definitionStrip = CreatePanel("DefinitionBlocks", timelineContent, Vector2.zero, Vector2.zero, Color.clear);
            definitionStrip.pivot = Vector2.zero;
            definitionStrip.sizeDelta = new Vector2(306, 68);
            TMP_Text label = CreateLabel("DefinitionBlocksTitle", definitionStrip, "SEQUÊNCIA DE CÓDIGO · clique para editar · arraste para ordenar", new Vector2(0, 0.75f), Vector2.one);
            label.fontSizeMax = 14;
            label.alignment = TextAlignmentOptions.Left;
        }

        private UnityEngine.UI.Button CreateArrow(string name, Transform parent, string label, float x, float width)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TimelineArrowGraphic), typeof(UnityEngine.UI.Button));
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(x, 9);
            rect.sizeDelta = new Vector2(width, 41);
            var graphic = item.GetComponent<TimelineArrowGraphic>();
            graphic.color = new Color32(39, 77, 92, 255);
            var button = item.GetComponent<UnityEngine.UI.Button>();
            button.targetGraphic = graphic;
            TMP_Text text = CreateLabel(name + "Label", rect, label, new Vector2(0.1f, 0.08f), new Vector2(0.88f, 0.92f));
            text.fontSizeMin = 11;
            text.fontSizeMax = 16;
            return button;
        }

        private void CreateActionStrip()
        {
            RectTransform panel = CreatePanel("CombatActionPanel", timelineContent, Vector2.zero, Vector2.zero, Color.clear);
            panel.pivot = Vector2.zero;
            panel.anchoredPosition = new Vector2(320, 0);
            combatActionPanel = panel.gameObject;
            CreateInitialActionArrows();
        }

        private void CreateInitialActionArrows()
        {
            if (combatActionPanel == null) return;
            combatActionButtons.Clear();
            float width = combatCodeBlocks.Count * 148 + 85;
            ((RectTransform)combatActionPanel.transform).sizeDelta = new Vector2(width, 68);
            TMP_Text title = CreateLabel("CombatActionTitle", combatActionPanel.transform,
                "AÇÕES EM CICLO · arraste para ordenar · role para ver mais", new Vector2(0, 0.75f), Vector2.one);
            title.alignment = TextAlignmentOptions.Left;
            title.fontSizeMax = 12;
            title.gameObject.SetActive(false);
            for (int i = 0; i < combatCodeBlocks.Count; i++)
            {
                int index = i;
                var button = CreateArrow("CombatAction" + (i + 1), combatActionPanel.transform, CombatBlockLabel(combatCodeBlocks[i]), i * 148, 144);
                button.onClick.AddListener(() => SelectCombatBlock(index));
                button.gameObject.AddComponent<CombatActionDragHandle>().Configure(this, index);
                combatActionButtons.Add(button);
            }
            addCombatBlockButton = CreateArrow("AddCombatBlockButton", combatActionPanel.transform, "+", combatCodeBlocks.Count * 148, 40);

            removeCombatBlockButton = CreateArrow("RemoveCombatBlockButton", combatActionPanel.transform, "−", combatCodeBlocks.Count * 148 + 42, 40);

            ResizeTimeline();
            RefreshCombatActionButtons();
        }

        private void RebuildActionArrows()
        {
            if (combatActionPanel == null) return;
            if (combatActionButtons.Count == 0)
            {
                foreach (Transform child in combatActionPanel.transform)
                    if (child.name.StartsWith("CombatAction"))
                    {
                        var discovered = child.GetComponent<UnityEngine.UI.Button>();
                        if (discovered != null) combatActionButtons.Add(discovered);
                    }
                if (combatActionButtons.Count == 0)
                {
                    CreateInitialActionArrows();
                    return;
                }
            }
            while (combatActionButtons.Count > combatCodeBlocks.Count)
            {
                int last = combatActionButtons.Count - 1;
                var removed = combatActionButtons[last];
                combatActionButtons.RemoveAt(last);
                removed.gameObject.SetActive(false);
                removed.name = "RetiredArrow";
#if UNITY_EDITOR
                if (!UnityEngine.Application.isPlaying) DestroyImmediate(removed.gameObject);
                else Destroy(removed.gameObject);
#else
                Destroy(removed.gameObject);
#endif
            }
            while (combatActionButtons.Count < combatCodeBlocks.Count)
            {
                var added = Instantiate(combatActionButtons[0], combatActionPanel.transform);
                added.name = "CombatAction" + (combatActionButtons.Count + 1);
                added.GetComponentInChildren<TMP_Text>().name = added.name + "Label";
                combatActionButtons.Add(added);
            }
            for (int i = 0; i < combatActionButtons.Count; i++)
            {
                int index = i;
                var button = combatActionButtons[i];
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SelectCombatBlock(index));
                button.GetComponent<CombatActionDragHandle>().Configure(this, i);
            }
            ResizeTimeline();
            RefreshCombatActionButtons();
        }
        private void SetTimelineAvailable(bool available)
        {
            timelineAvailable = available;
            if (combatActionPanel != null) combatActionPanel.SetActive(available);
            ResizeTimeline();
        }

        private void ResizeTimeline()
        {
            if (timelineContent == null) return;
            RefreshTimelineLayout();
        }

        private void StoreSelectedBlock()
        {
            StoreWorkspaceText();
        }

        private void SelectCombatBlock(int index)
        {
            StoreSelectedBlock();
            workspaceArea = WorkspaceArea.Classes;
            selectedCombatBlock = index;
            codeInput.SetTextWithoutNotify(combatCodeBlocks[index]);
            RefreshCodeBlockButtons();
            RefreshCombatActionButtons();
            feedbackText.color = new Color32(40, 82, 100, 255);
            feedbackText.text = $"Comando {BlockNumber(index + 3)} · lance a magia com alcance e atributos atuais.\nArraste os comandos para definir a ordem de cada ciclo.";
        }

        private void AddCombatBlock()
        {
            if (combat != null || combatCodeBlocks.Count >= 16) return;
            StoreSelectedBlock();
            combatCodeBlocks.Add((CurrentMago == null ? "mago" : CurrentMago.InstanceName) + ".lancarMagia();");
            timelineOrder.Add(combatCodeBlocks.Count + 2);
            RebuildActionArrows();
            SelectCombatBlock(combatCodeBlocks.Count - 1);
            timelineScroll.horizontalNormalizedPosition = 1;
            SaveProgress();
        }

        private void RemoveCombatBlock()
        {
            if (combat != null || selectedCombatBlock < 0 || combatCodeBlocks.Count <= 1) return;
            int removed = selectedCombatBlock + 3;
            timelineOrder.Remove(removed);
            for (int i = 0; i < timelineOrder.Count; i++) if (timelineOrder[i] > removed) timelineOrder[i]--;
            combatCodeBlocks.RemoveAt(selectedCombatBlock);
            selectedCombatBlock = -1;
            codeInput.SetTextWithoutNotify(codeBlocks.ActiveText);
            RebuildActionArrows();
            SaveProgress();
        }

        private bool CompileTimeline()
        {
            StoreSelectedBlock();
            var orderedIds = timelineOrder.FindAll(id => id >= 3);
            var orderedCode = orderedIds.ConvertAll(id => combatCodeBlocks[id - 3]);
            if (!CombatCodeCompiler.TryCompileBattle(orderedCode, CurrentMago == null ? "mago" : CurrentMago.InstanceName, out CombatAction[] actions, out int[] origins, out int block, out string error))
            {
                feedbackText.color = new Color32(156, 39, 49, 255);
                feedbackText.text = $"Comando {(block >= 0 ? BlockNumber(orderedIds[block]) : 1)}: {error}";
                if (block >= 0 && block < combatActionButtons.Count)
                    combatActionButtons[orderedIds[block] - 3].targetGraphic.color = new Color32(155, 58, 66, 255);
                return false;
            }
            for (int i = 0; i < origins.Length; i++) origins[i] = timelineOrder.IndexOf(orderedIds[origins[i]]);
            if (combat != null) combat.ConfigureActions(actions, origins);
            return true;
        }

        private static string CombatBlockLabel(string code)
        {
            CombatAction[] commands;
            int[] origins;
            int errorBlock;
            string error;
            if (!CombatCodeCompiler.TryCompileBattle(new[] { code }, "mago", out commands, out origins, out errorBlock, out error))
                return "Editar comando";
            return commands.Length == 1 && commands[0] == CombatAction.Cast ? "Atacar" : "Editar comando";
        }
    }
}
