$ErrorActionPreference='Stop'
function Replace-Code([string]$path,[string]$old,[string]$new) {
    $content=[IO.File]::ReadAllText((Join-Path $PWD $path)).Replace("`r`n","`n")
    if(-not $content.Contains($old)){throw "Trecho não encontrado: $old"}
    [IO.File]::WriteAllText((Join-Path $PWD $path),$content.Replace($old,$new))
}
$main='Assets/PrograMago/Runtime/Unity/GameplayBootstrapper.cs'
Replace-Code $main '            CreateCombatControls();' "            CreateCombatControls();`n            CreateArenaPresentation();"
Replace-Code $main "        private void LateUpdate()`n        {" "        private void LateUpdate()`n        {`n            UpdateArenaPresentation();"
Replace-Code $main @'
            if (magoStatsText != null)
            {
                ((RectTransform)magoStatsText.transform).anchorMax = new Vector2(0.98f, 0.77f);
            }
'@ ''
Replace-Code $main @'
            ((RectTransform)magoStatsText.transform).anchorMax =
                CurrentEnemies.Count == 0 ? new Vector2(0.98f, 0.77f) : new Vector2(0.64f, 0.77f);
'@ ''
Replace-Code $main '                    0.65f + index * 0.09f, 0.80f, distance));' "                    0.65f + index * 0.09f, 0.80f, distance));`n                StandInCell(marker, CombatEngine.CellCount - 1 - index);"
$content=[IO.File]::ReadAllText((Join-Path $PWD $main))
$content=[regex]::Replace($content,'(?s)        private void PositionCombatActors\(\).*?(?=        private void HideCombatControls\(\))',@'
        private void PositionCombatActors()
        {
            if (combat == null || arenaCamera == null) return;
            StandInCell(wizardInstance, combat.WizardPosition);
            for (int index = 0; index < enemyMarkers.Count && index < combat.Enemies.Count; index++)
            {
                if (enemyMarkers[index] == null) continue;
                enemyMarkers[index].SetActive(combat.Enemies[index].Life > 0 || HasCombatVisuals);
                StandInCell(enemyMarkers[index], combat.Enemies[index].Position);
            }
        }

'@)
$content=[regex]::Replace($content,'(?s)        private void PositionWizardSpawnPoint\(\).*?(?=\r?\n    \}\r?\n\})',@'
        private void PositionWizardSpawnPoint()
        {
            if (arenaCamera == null || wizardSpawnPoint == null) return;
            wizardSpawnPoint.position = CellGroundPosition(0);
            StandInCell(wizardInstance, 0);
        }
'@)
[IO.File]::WriteAllText((Join-Path $PWD $main),$content)
Replace-Code $main '            switch (action.Kind)' @'
            switch (action.Kind)
'@
Replace-Code $main '                case CombatEventKind.Move:' @'
                case CombatEventKind.OutOfRange:
                    combatStatusText.text = $"Distância: {TargetDistance(action.Target)} casas · alcance: {CurrentMago.Alcance}. Fora do alcance! Segure R por 5s para editar o Mago.";
                    combatStatusText.color = new Color32(255, 214, 137, 255);
                    break;
                case CombatEventKind.Blocked:
                    combatStatusText.text = $"{actor} aguarda: a próxima casa está ocupada.";
                    combatStatusText.color = new Color32(219, 228, 224, 255);
                    break;
                case CombatEventKind.Move:
'@
Replace-Code $main 'new Color32(123, 80, 12, 255)' 'new Color32(255, 214, 137, 255)'
# Battle messages now sit on a dark background.
$content=[IO.File]::ReadAllText((Join-Path $PWD $main))
$start=$content.IndexOf('        private void RenderCombatEvent(')
$end=$content.IndexOf('        private void RenderCombatState()', $start)
$section=$content.Substring($start,$end-$start).Replace('new Color32(156, 39, 49, 255)','new Color32(255, 164, 156, 255)')
$content=$content.Substring(0,$start)+$section+$content.Substring($end)
[IO.File]::WriteAllText((Join-Path $PWD $main),$content)
Replace-Code $main 'case CombatElement.Fire: return new Color32(170, 58, 14, 255);' 'case CombatElement.Fire: return new Color32(255, 161, 103, 255);'
Replace-Code $main 'case CombatElement.Water: return new Color32(23, 83, 163, 255);' 'case CombatElement.Water: return new Color32(126, 204, 255, 255);'
Replace-Code $main 'case CombatElement.Electric: return new Color32(120, 88, 8, 255);' 'case CombatElement.Electric: return new Color32(255, 230, 123, 255);'
Replace-Code $main 'default: return new Color32(83, 55, 142, 255);' 'default: return new Color32(218, 193, 255, 255);'
Replace-Code $main '$"Iniciativa: {CurrentMago.Iniciativa}  Velocidade: {CurrentMago.VelocidadeAtaque}";' '$"Iniciativa: {CurrentMago.Iniciativa}  Velocidade: {CurrentMago.VelocidadeAtaque}  Pontos: {25 - CurrentMago.RemainingPoints}/25";'
Replace-Code $main '.Append(" — ").Append(enemy.Source.Elemento);' '.Append(" — casa ").Append(enemy.Position + 1).Append(" — ").Append(enemy.Source.Elemento);'
