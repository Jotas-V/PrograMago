using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrograMago.UnityIntegration
{
    public sealed partial class GameplayBootstrapper
    {
        [SerializeField] private GameObject editorPreviewRoot;
        [SerializeField] private Button addCombatBlockButton;
        [SerializeField] private Button removeCombatBlockButton;
        [SerializeField] private Button retryCombatButton;
        [SerializeField] private Button editAfterDefeatButton;
        [SerializeField] private Button tutorialPreviousButton;
        [SerializeField] private Button tutorialNextButton;
        [SerializeField] private Button copyEnemyExampleButton;

        private bool BindScenePresentation()
        {
            if (editorPreviewRoot != null) editorPreviewRoot.SetActive(false);
            if (timelineContent == null || enemyGuide == null || magoStatsText == null ||
                enemyStatsText == null || combatDefeatOverlay == null || editorPanel == null ||
                pauseCombatButton == null || combatStatusText == null ||
                addCombatBlockButton == null || removeCombatBlockButton == null ||
                retryCombatButton == null || editAfterDefeatButton == null ||
                tutorialPreviousButton == null || tutorialNextButton == null || copyEnemyExampleButton == null)
            {
                Debug.LogError("A apresentação da MainScene precisa ser preparada e salva no Editor.");
                return false;
            }

            editorAnchorMaxX = editorPanel.anchorMax.x;
            magoStatsText.text = enemyStatsText.text = combatStatusText.text = string.Empty;
            combatStatusText.gameObject.SetActive(false);
            pauseCombatButton.gameObject.SetActive(false);
            combatDefeatOverlay.SetActive(false);
            for (int i = 0; i < codeBlockButtons.Length; i++)
            {
                int index = i;
                codeBlockButtons[i].onClick.AddListener(() => SelectCodeBlock(index));
                codeBlockButtons[i].GetComponent<CombatActionDragHandle>().ConfigureBlock(this, i);
            }
            RebuildActionArrows();
            addCombatBlockButton.onClick.AddListener(AddCombatBlock);
            removeCombatBlockButton.onClick.AddListener(RemoveCombatBlock);
            pauseCombatButton.onClick.AddListener(ToggleCombatPause);
            retryCombatButton.onClick.AddListener(RetryCombat);
            editAfterDefeatButton.onClick.AddListener(EditAfterDefeat);
            tutorialPreviousButton.onClick.AddListener(() => { tutorialStep = Mathf.Max(0, tutorialStep - 1); RenderTutorialStep(); });
            tutorialNextButton.onClick.AddListener(() => { tutorialStep = Mathf.Min(5, tutorialStep + 1); RenderTutorialStep(); });
            copyEnemyExampleButton.onClick.AddListener(() => {
                GUIUtility.systemCopyBuffer = EnemyTutorialExample;
                feedbackText.text = "Exemplo copiado. Cole em um bloco vazio; preserve o Mago.";
            });
            return true;
        }

#if UNITY_EDITOR
        // Explicit authoring operation: never loads a save or starts the game presenters.
        [ContextMenu("Preparar apresentação da cena")]
        public void PrepareScenePresentation()
        {
            if (UnityEngine.Application.isPlaying) return;
            if (timelineContent == null)
            {
                CreateMagoStatsText();
                CreateEnemyStatsText();
                CreateCodeBlockButtons();
                CreateCombatControls();
                CreateArenaPresentation();
                CreateEnemyGuide();
            }
            RestoreTimelineOrder(null);
            SetTimelineAvailable(true);
            ShowEnemyGuide(true);
            var path = learningPath.ToDomain();
            for (int i = 0; i < path.Battles.Count; i++)
                if (path.Battles[i].Criterion == Domain.ValidationCriterion.ConstructAndInstantiateEnemy)
                { ShowBattle(path.Battles[i], i + 1, path.Battles.Count); break; }
            magoStatsText.text = "Prévia do Mago · atributos definidos pelo código";
            enemyStatsText.text = "Prévia do Boneco · casa 16";
            combatStatusText.text = "Prévia do Editor · o progresso do jogador é carregado somente no Play";
            combatStatusText.gameObject.SetActive(true);
            victoryOverlay.SetActive(false);
            restartProgressPanel.SetActive(false);
            UnityEngine.Canvas.ForceUpdateCanvases();
            UpdateArenaPresentation();
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

        public void SetEditorPreview(GameObject root) { editorPreviewRoot = root; }

        public void RefreshEditorArena(GameObject wizard, GameObject enemy)
        {
            if (UnityEngine.Application.isPlaying) return;
            UpdateArenaPresentation();
            StandInCell(wizard, 0);
            StandInCell(enemy, Domain.CombatEngine.CellCount - 1);
        }
#endif
    }
}
