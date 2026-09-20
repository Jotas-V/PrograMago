$ErrorActionPreference='Stop'
function Replace-Code([string]$path,[string]$old,[string]$new) {
    $content=[IO.File]::ReadAllText((Join-Path $PWD $path)).Replace("`r`n","`n")
    if(-not $content.Contains($old)){throw "Trecho não encontrado em $path : $($old.Substring(0,[Math]::Min(70,$old.Length)))"}
    [IO.File]::WriteAllText((Join-Path $PWD $path),$content.Replace($old,$new))
}
$main='Assets/PrograMago/Runtime/Unity/GameplayBootstrapper.cs'
$timeline='Assets/PrograMago/Runtime/Unity/GameplayBootstrapper.Timeline.cs'
$drag='Assets/PrograMago/Runtime/Unity/CombatActionDragHandle.cs'
Replace-Code $main 'get => codeBlocks.SourceCode;' 'get => OrderedSourceCode;'
Replace-Code $main 'CodeBlockLocation location = codeBlocks.Locate(diagnostic.Position.Offset);' 'CodeBlockLocation location = LocateOrderedSource(diagnostic.Position.Offset);'
Replace-Code $main '            battleButton.onClick.AddListener(HandleBattle);' "            RestoreTimelineOrder(saved?.timelineOrder);`n            battleButton.onClick.AddListener(HandleBattle);"
Replace-Code $main '                codeBlockButtons[index] = button;' "                codeBlockButtons[index] = button;`n                button.gameObject.AddComponent<CombatActionDragHandle>().ConfigureBlock(this, index);"
Replace-Code $main '$"{index + 1} · {title}"' '$"{BlockNumber(index)} · {title}"'
Replace-Code $main 'string label = CombatBlockLabel(combatCodeBlocks[index]);' 'string label = $"{BlockNumber(index + 3)} · {CombatBlockLabel(combatCodeBlocks[index])}";'
Replace-Code $main '                data.combatBlocks = combatCodeBlocks.ToArray();' "                data.combatBlocks = combatCodeBlocks.ToArray();`n                data.timelineOrder = timelineOrder.ToArray();"
$mainText=[IO.File]::ReadAllText((Join-Path $PWD $main))
$mainText=[regex]::Replace($mainText,'(?s)        public void ReorderCombatAction\(int fromIndex, int toIndex\).*?(?=        private void RetryCombat\(\))',@'
        public void ReorderCombatAction(int fromIndex, int toIndex)
        {
            var actions = timelineOrder.FindAll(id => id >= 3);
            if (fromIndex < 0 || toIndex < 0 || fromIndex >= actions.Count || toIndex >= actions.Count) return;
            MoveTimelineBlock(actions[fromIndex], actions[toIndex]);
        }

'@)
[IO.File]::WriteAllText((Join-Path $PWD $main),$mainText)
Replace-Code $timeline 'PREPARAÇÃO · clique para editar' 'SEQUÊNCIA DE CÓDIGO · clique para editar · arraste para ordenar'
Replace-Code $timeline '            title.fontSizeMax = 12;' "            title.fontSizeMax = 12;`n            title.gameObject.SetActive(false);"
Replace-Code $timeline '            label.fontSizeMax = 12;' '            label.fontSizeMax = 14;'
Replace-Code $timeline '            timelineContent.sizeDelta = new Vector2(timelineAvailable ? 320 + combatCodeBlocks.Count * 148 + 85 : 306, 68);' '            RefreshTimelineLayout();'
Replace-Code $timeline '            combatCodeBlocks.Add("lancarMagia();");' "            combatCodeBlocks.Add(`"lancarMagia();`");`n            timelineOrder.Add(combatCodeBlocks.Count + 2);"
Replace-Code $timeline '            combatCodeBlocks.RemoveAt(selectedCombatBlock);' @'
            int removed = selectedCombatBlock + 3;
            timelineOrder.Remove(removed);
            for (int i = 0; i < timelineOrder.Count; i++) if (timelineOrder[i] > removed) timelineOrder[i]--;
            combatCodeBlocks.RemoveAt(selectedCombatBlock);
'@
Replace-Code $timeline '$"Ação {index + 1} · comandos disponíveis:' '$"Bloco {BlockNumber(index + 3)} · comandos disponíveis:'
Replace-Code $timeline '            if (!CombatCodeCompiler.TryCompile(combatCodeBlocks, out CombatAction[] actions, out int[] origins, out int block, out string error))' @'
            var orderedIds = timelineOrder.FindAll(id => id >= 3);
            var orderedCode = orderedIds.ConvertAll(id => combatCodeBlocks[id - 3]);
            if (!CombatCodeCompiler.TryCompile(orderedCode, out CombatAction[] actions, out int[] origins, out int block, out string error))
'@
Replace-Code $timeline '                feedbackText.text = $"Ação {block + 1}: {error}";' '                feedbackText.text = $"Bloco {(block >= 0 ? BlockNumber(orderedIds[block]) : 1)}: {error}";'
Replace-Code $timeline '                    combatActionButtons[block].targetGraphic.color' '                    combatActionButtons[orderedIds[block] - 3].targetGraphic.color'
Replace-Code $timeline '            if (combat != null) combat.ConfigureActions(actions, origins);' "            for (int i = 0; i < origins.Length; i++) origins[i] = timelineOrder.IndexOf(orderedIds[origins[i]]);`n            if (combat != null) combat.ConfigureActions(actions, origins);"
Replace-Code $drag '            this.index = index;' '            this.index = index + 3;'
Replace-Code $drag '        public void OnBeginDrag(PointerEventData eventData)' @'
        public void ConfigureBlock(GameplayBootstrapper owner, int blockId)
        {
            this.owner = owner;
            index = blockId;
        }

        public void OnBeginDrag(PointerEventData eventData)
'@
Replace-Code $drag '            dragging = false;' "            dragging = false;`n            owner.RefreshTimelineLayout();"
Replace-Code $drag 'owner.ReorderCombatAction(source.index, index);' 'owner.MoveTimelineBlock(source.index, index);'
Replace-Code 'Assets/PrograMago/Runtime/Domain/PhaseOneSaveData.cs' '        public string[] combatBlocks;' "        public string[] combatBlocks;`n        public int[] timelineOrder;"
Replace-Code 'Assets/PrograMago/Runtime/Unity/GameplayBootstrapper.Visuals.cs' @'
                if (step.BlockIndex >= 0 && step.BlockIndex < combatActionButtons.Count)
                    combatActionButtons[step.BlockIndex].targetGraphic.color =
'@ @'
                var activeButton = TimelineButtonAt(step.BlockIndex);
                if (activeButton != null)
                    activeButton.targetGraphic.color =
'@
