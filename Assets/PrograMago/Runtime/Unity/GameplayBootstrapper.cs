using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PrograMago.Application;
using PrograMago.Domain;
using PrograMago.Language;
using PrograMago.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PrograMago.UnityIntegration
{
    public sealed partial class GameplayBootstrapper : MonoBehaviour, ICodeEditorView, IFeedbackView, IArenaView,
        ILearningFlowView
    {
        [SerializeField] private TMP_InputField codeInput;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private LearningPathAsset learningPath;
        [SerializeField] private TMP_Text battleProgressText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text lessonText;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private GameObject victoryOverlay;
        [SerializeField] private TMP_Text victoryTitleText;
        [SerializeField] private TMP_Text victoryAchievementText;
        [SerializeField] private TMP_Text victoryReviewText;
        [SerializeField] private Button nextBattleButton;
        [SerializeField] private GameObject restartProgressPanel;
        [SerializeField] private TMP_Text restartProgressText;
        [SerializeField] private Camera arenaCamera;
        [SerializeField] private RectTransform arenaFrame;
        [SerializeField] private Transform wizardSpawnPoint;
        [SerializeField] private Vector2 wizardViewportPosition = new Vector2(0.1f, 0.85f);
        [SerializeField] private GameObject wizardPrefab;
        [SerializeField] private Button battleButton;

        private GameplayPresenter presenter;
        private LearningFlowPresenter learningFlowPresenter;
        private GameObject wizardInstance;
        [SerializeField] private TMP_Text magoStatsText;
        [SerializeField] private TMP_Text enemyStatsText;
        private readonly List<GameObject> enemyMarkers = new List<GameObject>();
        private static Sprite enemyPlaceholderSprite;
        private readonly CodeBlockDocument codeBlocks = new CodeBlockDocument();
        [SerializeField] private Button[] codeBlockButtons = new Button[CodeBlockDocument.BlockCount];
        private PhaseOneSaveStore progressStore;
        private string approvedCode = string.Empty;
        private bool pendingCodeSave;
        private float nextCodeSaveTime;
        private bool approvedClass;
        private bool classPreview;
        private CombatEngine combat;
        private float combatAccumulator;
        [SerializeField] private RectTransform editorPanel;
        [SerializeField] private GameObject combatActionPanel;
        [SerializeField] private GameObject combatDefeatOverlay;
        [SerializeField] private Button pauseCombatButton;
        [SerializeField] private TMP_Text combatStatusText;
        [SerializeField] private List<Button> combatActionButtons = new List<Button>();
        private float editorAnchorMaxX;
        private CombatElement[] activeExtraSpells = Array.Empty<CombatElement>();
        [SerializeField] private Button methodsWorkspaceButton;
        [SerializeField] private Button preparationWorkspaceButton;
        [SerializeField] private Button approveMethodButton;
        private MagoMethodBook magoMethodBook = new MagoMethodBook();
        private string methodDraft = string.Empty;
        private string preparationCode = string.Empty;
        private MagoState declaredMago;
        private MagoState preparedMago;
        private bool interactionEnabled = true;
        private WorkspaceArea workspaceArea = WorkspaceArea.Classes;

        private enum WorkspaceArea
        {
            Classes,
            Methods,
            Preparation,
            Strategy
        }

        public MagoState CurrentMago { get; private set; }
        public bool CanReorderCombatActions => false;

        public IReadOnlyList<EnemyState> CurrentEnemies { get; private set; } = Array.Empty<EnemyState>();

        public string SourceCode
        {
            get => OrderedSourceCode;
            set
            {
                workspaceArea = WorkspaceArea.Classes;
                selectedCombatBlock = -1;
                codeBlocks.SetActiveText(value);
                codeInput.SetTextWithoutNotify(codeBlocks.ActiveText);
            }
        }

        private void Awake()
        {
            if (codeInput == null || feedbackText == null || learningPath == null ||
                battleProgressText == null || titleText == null || lessonText == null ||
                objectiveText == null || hintText == null || victoryOverlay == null ||
                victoryTitleText == null || victoryAchievementText == null ||
                victoryReviewText == null || nextBattleButton == null ||
                restartProgressPanel == null || restartProgressText == null ||
                arenaCamera == null || arenaFrame == null || wizardSpawnPoint == null || wizardPrefab == null ||
                battleButton == null)
            {
                Debug.LogError(
                    "GameplayBootstrapper precisa de todas as referências do editor, conteúdo, " +
                    "progresso, vitória, reinício e arena configuradas na cena.");
                enabled = false;
                return;
            }

            LoadTCC40Visuals();
            if (!BindScenePresentation()) { enabled = false; return; }
            ApplyTCC40PresentationArt();
            EnsureArenaAtmosphere();
            UpdateArenaPresentation();
            PositionWizardSpawnPoint();

            LearningPath path;
            try
            {
                path = learningPath.ToDomain();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Conteúdo pedagógico inválido: {exception.Message}");
                enabled = false;
                return;
            }

            var session = new LearningSession(new ExerciseDefinition(
                "declare-mago-class",
                "Mago",
                "Declare a classe Mago."));
            var submitCode = new SubmitCodeUseCase(
                new CodeTokenizer(), new ExerciseCodeValidator(), session);
            var progress = new LearningProgress(path);
            progressStore = new PhaseOneSaveStore(Path.Combine(
                UnityEngine.Application.persistentDataPath, "programago-phase1.json"));
            PhaseOneSaveData saved = null;
            SubmitCodeResult restoredProgram = null;
            try
            {
                saved = progressStore.Load();
                if (saved != null)
                {
                    progress.RestorePhaseOne(saved);
                    if (!string.IsNullOrEmpty(saved.approvedCode))
                    {
                        for (int index = progress.CurrentBattleIndex; index >= 0; index--)
                        {
                            SubmitCodeResult candidate = submitCode.Execute(
                                saved.approvedCode, path.Battles[index].Criterion);
                            if (candidate.IsSuccess)
                            {
                                restoredProgram = candidate;
                                break;
                            }
                            if (saved.completed[index])
                            {
                                break;
                            }
                        }

                        if (restoredProgram == null)
                        {
                            throw new InvalidDataException("Código aprovado não confere com o progresso.");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Registro da fase 1 ignorado: {exception.Message}");
                saved = null;
                restoredProgram = null;
                progress = new LearningProgress(path);
            }

            learningFlowPresenter = new LearningFlowPresenter(
                progress,
                new HoldToRestartController(5f),
                this,
                this,
                this,
                this);
            presenter = new GameplayPresenter(
                submitCode,
                this,
                this,
                this,
                learningFlowPresenter);

            if (saved != null)
            {
                magoMethodBook = new MagoMethodBook(saved.approvedMethods);
                methodDraft = saved.methodDraft ?? string.Empty;
                preparationCode = saved.preparationCode ?? string.Empty;
                if (saved.combatBlocks != null && saved.combatBlocks.Length > 0 && saved.combatBlocks.Length <= 16)
                {
                    combatCodeBlocks.Clear();
                    combatCodeBlocks.AddRange(saved.combatBlocks);
                }
                if (saved.version >= 2)
                {
                    codeBlocks.Restore(saved.sourceBlocks, saved.activeBlock);
                    codeInput.SetTextWithoutNotify(codeBlocks.ActiveText);
                    RefreshCodeBlockButtons();
                }
                else
                {
                    SourceCode = saved.sourceCode ?? string.Empty;
                }
                if (restoredProgram != null)
                {
                    if (string.IsNullOrWhiteSpace(restoredProgram.Program.InstanceName))
                    {
                        CommitSilhouette();
                    }
                    else
                    {
                        ShowMago(MagoState.FromValidatedProgram(restoredProgram.Program));
                    }
                    if (restoredProgram.Enemies != null && restoredProgram.Enemies.Count > 0)
                    {
                        ShowEnemies(restoredProgram.Enemies);
                    }
                }

                approvedCode = saved.approvedCode ?? string.Empty;
            }

            RestoreTimelineOrder(saved?.timelineOrder);
            battleButton.onClick.AddListener(HandleBattle);
            nextBattleButton.onClick.AddListener(HandleNextBattle);
            codeInput.onValueChanged.AddListener(HandleCodeChanged);
            learningFlowPresenter.Initialize();
        }

        private void OnDestroy()
        {
            ClearCombatPresentation();
            if (presenter == null)
            {
                return;
            }

            SaveProgress();
            battleButton.onClick.RemoveListener(HandleBattle);
            nextBattleButton.onClick.RemoveListener(HandleNextBattle);
            codeInput.onValueChanged.RemoveListener(HandleCodeChanged);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                SaveProgress();
            }
        }

        private void OnApplicationQuit()
        {
            SaveProgress();
        }

        private void Update()
        {
            if (learningFlowPresenter == null)
            {
                return;
            }

            bool isRestartPressed = Keyboard.current?.rKey.isPressed == true;
            if (learningFlowPresenter.UpdateRestartHold(isRestartPressed, Time.unscaledDeltaTime))
            {
                SaveProgress();
            }

            if (pendingCodeSave && Time.unscaledTime >= nextCodeSaveTime)
            {
                SaveProgress();
            }
            UpdateCombatPresentation();

            if (combat != null && !combat.IsPaused &&
                combat.Outcome == CombatOutcome.InProgress)
            {
                combatAccumulator += Time.unscaledDeltaTime;
                while (combatAccumulator >= 0.2f && combat != null &&
                       combat.Outcome == CombatOutcome.InProgress)
                {
                    combatAccumulator -= 0.2f;
                    AdvanceCombatTick();
                }
            }
        }

        private void LateUpdate()
        {
            UpdateArenaPresentation();
            PositionWizardSpawnPoint();
            PositionCombatActors();
            UpdateSpectralWizardIdle();
        }

        public void ShowError(Diagnostic diagnostic)
        {
            feedbackText.color = new Color32(156, 39, 49, 255);
            if (diagnostic == null)
            {
                feedbackText.text = "Não foi possível validar o código.";
                return;
            }

            CodeBlockLocation location = LocateOrderedSource(diagnostic.Position.Offset);
            FocusTutorialError(diagnostic.Code);
            feedbackText.text = $"{diagnostic.Code} — bloco {location.BlockNumber}, " +
                $"linha {location.Line}, coluna {location.Column}: {diagnostic.Detail}";
        }

        public void Clear()
        {
            feedbackText.text = string.Empty;
        }

        public void SetClassDeclared(bool hasDeclaredClass)
        {
            classPreview = hasDeclaredClass;
            RenderWizard();
        }

        public void CommitSilhouette()
        {
            approvedClass = true;
            declaredMago = null;
            preparedMago = null;
            CurrentMago = null;
            approvedCode = SourceCode;
            RenderWizard();
        }

        public void ShowMago(MagoState mago)
        {
            declaredMago = mago ?? throw new ArgumentNullException(nameof(mago));
            preparedMago = null;
            CurrentMago = declaredMago;
            approvedClass = true;
            approvedCode = SourceCode;
            RenderWizard();
        }
        public void ShowEnemies(IReadOnlyList<EnemyState> enemies)
        {
            CurrentEnemies = new List<EnemyState>(enemies ??
                throw new ArgumentNullException(nameof(enemies))).AsReadOnly();
            RenderEnemies();
        }

        public void Reset()
        {
            ClearCombatPresentation();
            HideCombatControls();
            combat = null;
            ResetWorkspace();
            SetTimelineAvailable(timelineAvailable);
            approvedClass = false;
            classPreview = false;
            CurrentMago = null;
            CurrentEnemies = Array.Empty<EnemyState>();
            ClearEnemyMarkers();
            if (enemyStatsText != null) enemyStatsText.text = string.Empty;

            approvedCode = string.Empty;
            RenderWizard();
        }

        private void RenderWizard()
        {
            bool visible = approvedClass || classPreview;
            if (visible && wizardInstance == null)
            {
                wizardInstance = Instantiate(wizardPrefab, wizardSpawnPoint);
                wizardInstance.name = wizardPrefab.name;
                wizardInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                Animator prefabAnimator = wizardPrefab.GetComponent<Animator>();
                wizardBaseController = prefabAnimator == null ? null : prefabAnimator.runtimeAnimatorController;
            }

            if (wizardInstance != null)
            {
                wizardInstance.SetActive(visible);
                SpriteRenderer sprite = wizardInstance.GetComponent<SpriteRenderer>();
                if (sprite != null)
                    ApplyWizardFormTint(combat == null ? "neutro" : combat.WizardForm);
            }

            if (magoStatsText != null)
            {
                magoStatsText.text = CurrentMago == null
                    ? string.Empty
                    : $"Mago {CurrentMago.InstanceName} · Vida: {CurrentMago.Vida} · Pontos: {25 - CurrentMago.RemainingPoints}/25\n" +
                      $"Dano: {CurrentMago.Dano} · Alcance: {CurrentMago.Alcance} casas\n" +
                      $"Iniciativa: {CurrentMago.Iniciativa} · Velocidade: {CurrentMago.VelocidadeAtaque}";
            }
        }

        private void ApplyWizardFormTint(string form)
        {
            ApplyWizardAppearance(form);
        }

        private void CreateMagoStatsText()
        {
            var statsObject = new GameObject(
                "MagoStatsText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            statsObject.transform.SetParent(arenaFrame, false);
            var rect = statsObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.43f, 0.08f);
            rect.anchorMax = new Vector2(0.98f, 0.77f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            magoStatsText = statsObject.GetComponent<TextMeshProUGUI>();
            magoStatsText.font = battleProgressText.font;
            magoStatsText.color = new Color32(43, 39, 62, 255);
            magoStatsText.alignment = TextAlignmentOptions.TopLeft;
            magoStatsText.enableAutoSizing = true;
            magoStatsText.fontSizeMin = 11;
            magoStatsText.fontSizeMax = 20;
            magoStatsText.raycastTarget = false;
            magoStatsText.text = string.Empty;
        }

        private void CreateEnemyStatsText()
        {
            var statsObject = new GameObject(
                "EnemyStatsText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            statsObject.transform.SetParent(arenaFrame, false);
            var rect = statsObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.66f, 0.08f);
            rect.anchorMax = new Vector2(0.98f, 0.70f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            enemyStatsText = statsObject.GetComponent<TextMeshProUGUI>();
            enemyStatsText.font = battleProgressText.font;
            enemyStatsText.color = new Color32(43, 39, 62, 255);
            enemyStatsText.alignment = TextAlignmentOptions.TopLeft;
            enemyStatsText.enableAutoSizing = true;
            enemyStatsText.fontSizeMin = 10;
            enemyStatsText.fontSizeMax = 18;
            enemyStatsText.raycastTarget = false;
            enemyStatsText.text = string.Empty;
        }

        private void RenderEnemies()
        {
            ClearEnemyMarkers();
            var summary = new StringBuilder();
            for (int index = 0; index < CurrentEnemies.Count; index++)
            {
                EnemyState enemy = CurrentEnemies[index];
                var marker = new GameObject($"EnemyMarker-{enemy.VariableName}", typeof(SpriteRenderer));
                SpriteRenderer renderer = marker.GetComponent<SpriteRenderer>();
                bool isTrainingDummy = enemy.Name == "Boneco de Treinamento";
                renderer.sprite = isTrainingDummy && visualCatalog != null &&
                    visualCatalog.trainingDummyIdleSprite != null
                    ? visualCatalog.trainingDummyIdleSprite
                    : GetEnemyPlaceholderSprite();
                renderer.color = isTrainingDummy ? Color.white : EnemyColor(enemy.Elemento);
                renderer.sortingOrder = 10;
                marker.transform.localScale = new Vector3(0.65f, 0.85f, 1f);
                if (isTrainingDummy && visualCatalog != null &&
                    visualCatalog.trainingDummyController != null)
                {
                    Animator animator = marker.AddComponent<Animator>();
                    animator.runtimeAnimatorController = visualCatalog.trainingDummyController;
                    animator.speed = CharacterAnimationSpeed;
                    animator.Play("Idle", 0, 0f);
                }
                float distance = Vector3.Dot(
                    wizardSpawnPoint.position - arenaCamera.transform.position,
                    arenaCamera.transform.forward);
                marker.transform.position = arenaCamera.ViewportToWorldPoint(new Vector3(
                    0.65f + index * 0.09f, 0.80f, distance));
                StandInCell(marker, CombatEngine.CellCount - 1 - index);
                enemyMarkers.Add(marker);

                if (summary.Length > 0) summary.Append('\n');
                summary.Append(enemy.Name).Append(" — Vida: ").Append(enemy.Vida)
                    .Append(" — ").Append(enemy.Elemento);
            }

            enemyStatsText.text = summary.ToString();

        }

        private void ClearEnemyMarkers()
        {
            foreach (GameObject marker in enemyMarkers)
            {
                if (marker != null) Destroy(marker);
            }

            enemyMarkers.Clear();
        }

        private static Sprite GetEnemyPlaceholderSprite()
        {
            if (enemyPlaceholderSprite == null)
            {
                enemyPlaceholderSprite = Sprite.Create(Texture2D.whiteTexture,
                    new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }

            return enemyPlaceholderSprite;
        }

        private static Color EnemyColor(string elemento)
        {
            switch (elemento)
            {
                case "gelo": return new Color32(126, 211, 242, 255);
                case "fogo": return new Color32(242, 132, 79, 255);
                case "água": return new Color32(82, 149, 235, 255);
                default: return new Color32(191, 177, 145, 255);
            }
        }

        public void ShowBattle(BattleDefinition battle, int battleNumber, int battleCount)
        {
            battleProgressText.text = $"Batalha {battleNumber}/{battleCount}  •  Arco {battle.Chapter}";
            TMP_Text battleLabel = battleButton.GetComponentInChildren<TMP_Text>();
            if (battleLabel != null)
                battleLabel.text = battle.CompletionMode == BattleCompletionMode.OnCombatVictory
                    ? "Batalhar" : "Validar código";
            titleText.text = battle.Lesson.Title;
            lessonText.text =
                $"O QUE É\n{battle.Lesson.WhatItIs}\n\n" +
                $"PARA QUE SERVE\n{battle.Lesson.Purpose}\n\n" +
                $"COMO USAR\n{battle.Lesson.UsageExample}\n\n" +
                $"NO JOGO\n{battle.Lesson.GameEffect}";
            objectiveText.text = $"TAREFA\n{battle.Lesson.Task}";
            hintText.text = string.Empty;
            if (battle.Criterion == ValidationCriterion.AddMagoSetters)
            {
                FocusEditableDefinitionBlock(0);
                if (string.IsNullOrEmpty(approvedCode)) approvedCode = codeBlocks.Snapshot()[0];
            }
            else if (battle.Criterion == ValidationCriterion.ConstructAndInstantiateEnemy)
            {
                FocusEditableDefinitionBlock(1);
            }
            SetTimelineAvailable(battle.CompletionMode == BattleCompletionMode.OnCombatVictory);
            UpdateWorkspaceUi();
            ShowEnemyGuide(battle.Criterion == ValidationCriterion.ConstructAndInstantiateEnemy);
        }

        public void ShowHint(string hint)
        {
            hintText.text = string.IsNullOrWhiteSpace(hint)
                ? string.Empty
                : $"DICA\n{hint}";
        }

        public void ShowVictory(BattleVictoryContent content, bool isFinalBattle, bool isPhaseComplete)
        {
            victoryTitleText.text = content.Title;
            victoryAchievementText.text = content.Achievement;
            victoryReviewText.text = content.Review;
            TMP_Text buttonLabel = nextBattleButton.GetComponentInChildren<TMP_Text>();
            if (buttonLabel != null)
            {
                buttonLabel.text = learningFlowPresenter != null &&
                                   !learningFlowPresenter.Progress.CanContinueAfterVictory
                    ? "Continuação em breve"
                    : isFinalBattle ? "Concluir jornada" : "Próxima batalha";
            }

            nextBattleButton.interactable = learningFlowPresenter == null ||
                learningFlowPresenter.Progress.CanContinueAfterVictory;
            victoryOverlay.SetActive(true);
        }

        public void HideVictory()
        {
            victoryOverlay.SetActive(false);
            nextBattleButton.interactable = true;
        }

        public void SetInteractionEnabled(bool isEnabled)
        {
            interactionEnabled = isEnabled;
            battleButton.interactable = isEnabled;
            UpdateWorkspaceUi();
        }
        public void ShowRestartProgress(float progress)
        {
            restartProgressPanel.SetActive(true);
            if (progress >= 1f)
            {
                restartProgressText.text = "Fase reiniciada";
                return;
            }

            int secondsRemaining = Mathf.CeilToInt((1f - progress) * 5f);
            restartProgressText.text = $"Segure R para reiniciar • {secondsRemaining}s";
        }

        public void HideRestartProgress()
        {
            restartProgressPanel.SetActive(false);
        }

        public bool ReportBattleVictory()
        {
            bool accepted = learningFlowPresenter != null && learningFlowPresenter.ReportBattleVictory();
            if (accepted)
            {
                SaveProgress();
            }

            return accepted;
        }

        public bool ReportBattleDefeat()
        {
            bool accepted = learningFlowPresenter != null && learningFlowPresenter.ReportBattleDefeat();
            if (accepted)
            {
                SaveProgress();
            }

            return accepted;
        }

        private void HandleBattle()
        {
            if (learningFlowPresenter.Progress.CurrentBattle.CompletionMode == BattleCompletionMode.OnCombatVictory)
            {
                if (!TryApplyPreparation() || !CompileTimeline()) return;
            }
            presenter.Battle();
            if (learningFlowPresenter.Progress.Stage == LearningStage.BattleInProgress &&
                CurrentMago != null && CurrentEnemies.Count > 0)
            {
                StartCombat();
            }
            SaveProgress();
        }
        private void HandleCodeChanged(string _)
        {
            StoreWorkspaceText();
            bool setterLesson = learningFlowPresenter != null &&
                learningFlowPresenter.Progress.CurrentBattle.Criterion == ValidationCriterion.AddMagoSetters;
            if (workspaceArea == WorkspaceArea.Classes && selectedCombatBlock < 0 && !setterLesson)
                presenter.Preview();
            RefreshCodeBlockButtons();
            RefreshCombatActionButtons();
            pendingCodeSave = true;
            nextCodeSaveTime = Time.unscaledTime + 0.5f;
        }

        private void CreateCodeBlockButtons()
        {
            CreateTimelineStrip();
            for (int index = 0; index < codeBlockButtons.Length; index++)
            {
                string[] labels = { "1 · Mago", "2 · Inimigo", "3 · Estratégia" };
                Button button = CreateArrow($"CodeBlockButton{index + 1}", definitionStrip,
                    labels[index], index * 128f, 120f);
                button.gameObject.name = $"CodeBlockButton{index + 1}";
                button.onClick.RemoveAllListeners();
                RectTransform rect = button.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                rect.pivot = Vector2.zero;
                rect.anchoredPosition = new Vector2(index * 128f, 6f);
                rect.sizeDelta = new Vector2(120f, 40f);
                TMP_Text label = button.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = labels[index];
                    label.enableAutoSizing = true;
                    label.fontSizeMin = 10f;
                    label.fontSizeMax = 15f;
                    label.enableWordWrapping = false;
                }

                int selected = index;
                button.onClick.AddListener(() => SelectCodeBlock(selected));
                codeBlockButtons[index] = button;
            }

            RefreshCodeBlockButtons();
        }

        private void SelectCodeBlock(int index)
        {
            if (index < 0 || index >= codeBlockButtons.Length || !IsCodeBlockEditable(index)) return;
            if ((workspaceArea == WorkspaceArea.Classes || workspaceArea == WorkspaceArea.Strategy) &&
                selectedCombatBlock < 0 && codeBlocks.ActiveIndex == index) return;
            StoreWorkspaceText();
            workspaceArea = index == 2 ? WorkspaceArea.Strategy : WorkspaceArea.Classes;
            selectedCombatBlock = -1;
            codeBlocks.Select(index);
            codeInput.SetTextWithoutNotify(codeBlocks.ActiveText);
            RefreshCodeBlockButtons();
            UpdateWorkspaceUi();
            if (combat == null) presenter?.Preview();
            SaveProgress();
        }

        private void RefreshCodeBlockButtons()
        {
            string[] titles = { "Mago", "Inimigo", "Estratégia" };
            for (int index = 0; index < codeBlockButtons.Length; index++)
            {
                if (codeBlockButtons[index] == null) continue;
                var background = codeBlockButtons[index].targetGraphic;
                background.color = (workspaceArea == WorkspaceArea.Classes || workspaceArea == WorkspaceArea.Strategy) &&
                    selectedCombatBlock < 0 && index == codeBlocks.ActiveIndex
                    ? new Color32(91, 74, 190, 255)
                    : new Color32(69, 70, 90, 255);
                codeBlockButtons[index].GetComponentInChildren<TMP_Text>().text =
                    $"{index + 1} · {titles[index]}";
            }
        }

        private void HandleNextBattle()
        {
            if (learningFlowPresenter.NextBattle())
            {
                SaveProgress();
            }
        }

        public void StartCombat(params CombatElement[] extraSpells)
        {
            if (CurrentMago == null || CurrentEnemies.Count == 0)
                throw new InvalidOperationException("Mago e inimigos aprovados são necessários.");
            if (!TryApplyPreparation()) return;
            activeExtraSpells = extraSpells == null
                ? Array.Empty<CombatElement>()
                : (CombatElement[])extraSpells.Clone();
            MagoState combatMago = preparedMago ?? declaredMago ?? CurrentMago;
            combat = new CombatEngine(CombatWizard.FromMago(combatMago,
                activeExtraSpells), CurrentEnemies, compiledStrategy);
            if (!CompileTimeline()) { combat = null; return; }
            ClearCombatPresentation();
            combatAccumulator = 0f;
            SetTimelineAvailable(true);
            combatActionPanel.SetActive(true);
            combatDefeatOverlay.SetActive(false);
            pauseCombatButton.gameObject.SetActive(true);
            pauseCombatButton.GetComponentInChildren<TMP_Text>().text = "Pausar";
            combatStatusText.gameObject.SetActive(true);
            combatStatusText.text = "Combate iniciado";
            RefreshCombatActionButtons();
            SetInteractionEnabled(false);
            RenderCombatState();
            PositionCombatActors();
        }
        public CombatEvent AdvanceCombatTick()
        {
            if (combat == null) return null;
            CombatEvent action = combat.Tick();
            if (combat.EventsThisTick.Count > 0 && combat.EventsThisTick[0].BlockIndex >= 0) trace.Clear();
            foreach (CombatEvent step in combat.EventsThisTick) PresentCombatStep(step);
            if (action != null) RenderCombatEvent(action);
            RenderCombatState();
            PositionCombatActors();
            if (combat.Outcome == CombatOutcome.Victory)
            {
                CompleteCombatWhenVisualsFinish();
            }
            else if (combat.Outcome == CombatOutcome.Defeat)
            {
                combatDefeatOverlay.SetActive(true);
                pauseCombatButton.gameObject.SetActive(false);
                combatStatusText.text = "Derrota — tente novamente ou edite o código";
            }
            return action;
        }

        public void ToggleCombatPause()
        {
            if (combat == null || combat.Outcome != CombatOutcome.InProgress) return;
            if (combat.IsPaused)
            {
                combat.Resume();
                combatStatusText.text = "Combate retomado";
            }
            else
            {
                combat.Pause();
                combatStatusText.text = "Pausado — continue para retomar a batalha";
            }
            pauseCombatButton.GetComponentInChildren<TMP_Text>().text =
                combat.IsPaused ? "Continuar" : "Pausar";
        }

        public void ReorderCombatAction(int fromIndex, int toIndex)
        {
            // Kept as a compatibility entry point for older scene/test references.
        }
        private void RetryCombat()
        {
            if (combat == null || combat.Outcome != CombatOutcome.Defeat) return;
            combat.Restart();
            ClearCombatPresentation();
            combatAccumulator = 0f;
            combatDefeatOverlay.SetActive(false);
            pauseCombatButton.gameObject.SetActive(true);
            pauseCombatButton.GetComponentInChildren<TMP_Text>().text = "Pausar";
            combatStatusText.text = "Nova tentativa";
            RefreshCombatActionButtons();
            RenderCombatState();
            PositionCombatActors();
        }

        private void EditAfterDefeat()
        {
            ClearCombatPresentation();
            if (!ReportBattleDefeat())
            {
                HideCombatControls();
                combat = null;
                SetTimelineAvailable(timelineAvailable);
                SetInteractionEnabled(true);
            }
        }

        private void CreateCombatControls()
        {
            editorPanel = GameObject.Find("CodeEditorPanel").GetComponent<RectTransform>();
            editorAnchorMaxX = editorPanel.anchorMax.x;
            LayoutDefinitionStrip();
            CreateActionStrip();
            combatActionPanel.SetActive(false);

            pauseCombatButton = CreateButton("PauseCombatButton", arenaFrame,
                "Pausar", new Vector2(0.80f, 0.82f), new Vector2(0.98f, 0.98f));
            pauseCombatButton.onClick.AddListener(ToggleCombatPause);
            pauseCombatButton.gameObject.SetActive(false);

            combatStatusText = CreateLabel("CombatStatusText", arenaFrame,
                string.Empty, new Vector2(0.23f, 0.01f), new Vector2(0.78f, 0.20f));
            combatStatusText.color = new Color32(67, 47, 130, 255);
            combatStatusText.gameObject.SetActive(false);

            combatDefeatOverlay = CreatePanel("CombatDefeatOverlay", arenaFrame,
                new Vector2(0.28f, 0.12f), new Vector2(0.72f, 0.88f),
                new Color32(40, 29, 45, 245)).gameObject;
            CreateLabel("CombatDefeatTitle", combatDefeatOverlay.transform,
                "Derrota", new Vector2(0.08f, 0.64f), new Vector2(0.92f, 0.95f));
            retryCombatButton = CreateButton("RetryCombatButton", combatDefeatOverlay.transform,
                "Tentar novamente", new Vector2(0.08f, 0.36f),
                new Vector2(0.92f, 0.60f));
            editAfterDefeatButton = CreateButton("EditAfterDefeatButton", combatDefeatOverlay.transform,
                "Editar código", new Vector2(0.08f, 0.08f),
                new Vector2(0.92f, 0.32f));
            combatDefeatOverlay.SetActive(false);
        }

        private RectTransform CreatePanel(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var item = new GameObject(name, typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image));
            item.transform.SetParent(parent, false);
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = item.GetComponent<Image>();
            image.color = color;
            ApplyCreatedPanelArt(name, image);
            return rect;
        }

        private Button CreateButton(string name, Transform parent, string label,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform rect = CreatePanel(name, parent, anchorMin, anchorMax,
                new Color32(83, 67, 152, 255));
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            ApplyButtonArt(button, name == "BattleButton");
            CreateLabel(name + "Label", rect, label, Vector2.zero, Vector2.one);
            return button;
        }

        private TMP_Text CreateLabel(string name, Transform parent, string value,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var item = new GameObject(name, typeof(RectTransform),
                typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            item.transform.SetParent(parent, false);
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            TextMeshProUGUI text = item.GetComponent<TextMeshProUGUI>();
            text.font = battleProgressText.font;
            text.text = value;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = 10;
            text.fontSizeMax = 24;
            text.raycastTarget = false;
            return text;
        }

        private void RefreshCombatActionButtons()
        {
            int visibleActions = Mathf.Min(combatActionButtons.Count, combatCodeBlocks.Count);
            for (int index = 0; index < visibleActions; index++)
            {
                string label = $"{index + 1} · {CombatBlockLabel(combatCodeBlocks[index])}";
                combatActionButtons[index].GetComponentInChildren<TMP_Text>().text = label;
                combatActionButtons[index].targetGraphic.color = selectedCombatBlock == index
                    ? new Color32(97, 79, 192, 255) : new Color32(39, 77, 92, 255);
            }
        }

        private void RenderCombatEvent(CombatEvent action)
        {
            string actor = action.Actor == "Mago" ? "Mago" : action.Actor;
            switch (action.Kind)
            {
                case CombatEventKind.OutOfRange:
                    combatStatusText.text = $"Alvo fora do alcance ({TargetDistance(action.Target)} casas). O movimento é automático.";
                    combatStatusText.color = new Color32(255, 214, 137, 255);
                    break;
                case CombatEventKind.Blocked:
                    combatStatusText.text = $"{actor} aguarda: a próxima casa está ocupada.";
                    combatStatusText.color = new Color32(219, 228, 224, 255);
                    break;
                case CombatEventKind.Move:
                    if (action.Actor == "Mago") AnimateWizardMove(action.Position);
                    combatStatusText.text = $"{actor} avança para a casa {action.Position + 1} para entrar no alcance.";
                    combatStatusText.color = new Color32(255, 214, 137, 255);
                    break;
                case CombatEventKind.NoTarget:
                    combatStatusText.text = $"Comando {action.BlockIndex + 1}: nenhum alvo vivo está na arena.";
                    combatStatusText.color = new Color32(255, 164, 156, 255);
                    break;
                case CombatEventKind.MissingSpell:
                    combatStatusText.text = $"Comando {action.BlockIndex + 1}: nenhuma magia disponível para este alvo.";
                    combatStatusText.color = new Color32(255, 164, 156, 255);
                    break;
                case CombatEventKind.Ineffective:
                    combatStatusText.text = $"Magia {ElementLabel(action.Element)} ineficaz contra {action.Target}";
                    combatStatusText.color = new Color32(255, 164, 156, 255);
                    break;
                default:
                    combatStatusText.text = $"{actor} causa {action.Amount} de " +
                        $"{ElementLabel(action.Element)} em {action.Target}";
                    combatStatusText.color = ElementColor(action.Element);
                    break;
            }
        }

        private void RenderCombatState()
        {
            if (combat == null) return;
            ApplyWizardFormTint(combat.WizardForm);
            magoStatsText.text = $"Mago {CurrentMago.InstanceName} — Vida: {combat.WizardLife}/{CurrentMago.Vida}\n" +
                $"Casa: {combat.WizardPosition + 1}  Dano: {CurrentMago.Dano}  Alcance: {CurrentMago.Alcance}\n" +
                $"Iniciativa: {CurrentMago.Iniciativa}  Velocidade: {CurrentMago.VelocidadeAtaque}  Pontos: {25 - CurrentMago.RemainingPoints}/25";
            var summary = new StringBuilder();
            foreach (CombatEnemy enemy in combat.Enemies)
            {
                if (summary.Length > 0) summary.Append('\n');
                summary.Append(enemy.Source.Name).Append(" — Vida: ")
                    .Append(enemy.Life).Append('/').Append(enemy.Source.Vida)
                    .Append(" — casa ").Append(enemy.Position + 1).Append(" — ").Append(enemy.Source.Elemento);
            }
            enemyStatsText.text = summary.ToString();
        }

        private void PositionCombatActors()
        {
            if (arenaCamera == null) return;
            if (combat == null)
            {
                for (int index = 0; index < enemyMarkers.Count; index++)
                    StandInCell(enemyMarkers[index], CombatEngine.CellCount - 1 - index);
                return;
            }
            StandInCell(wizardInstance, combat.WizardPosition);
            for (int index = 0; index < enemyMarkers.Count && index < combat.Enemies.Count; index++)
            {
                if (enemyMarkers[index] == null) continue;
                enemyMarkers[index].SetActive(combat.Enemies[index].Life > 0 || HasCombatVisuals);
                StandInCell(enemyMarkers[index], combat.Enemies[index].Position);
            }
            UpdateReachIndicators();
        }
        private void HideCombatControls()
        {
            if (combatActionPanel != null) combatActionPanel.SetActive(false);
            if (combatDefeatOverlay != null) combatDefeatOverlay.SetActive(false);
            if (pauseCombatButton != null) pauseCombatButton.gameObject.SetActive(false);
            if (combatStatusText != null) combatStatusText.gameObject.SetActive(false);
            if (editorPanel != null)
                editorPanel.anchorMax = new Vector2(editorAnchorMaxX, editorPanel.anchorMax.y);
            if (wizardInstance != null) wizardInstance.transform.localPosition = Vector3.zero;
        }

        private static string ElementLabel(CombatElement element)
        {
            switch (element)
            {
                case CombatElement.Fire: return "fogo";
                case CombatElement.Water: return "água";
                case CombatElement.Electric: return "eletricidade";
                default: return "neutro";
            }
        }

        private static Color ElementColor(CombatElement element)
        {
            switch (element)
            {
                case CombatElement.Fire: return new Color32(255, 161, 103, 255);
                case CombatElement.Water: return new Color32(126, 204, 255, 255);
                case CombatElement.Electric: return new Color32(255, 230, 123, 255);
                default: return new Color32(218, 193, 255, 255);
            }
        }

        private void SaveProgress()
        {
            if (progressStore == null || learningFlowPresenter == null)
            {
                return;
            }

            try
            {
                StoreWorkspaceText();
                PhaseOneSaveData data = learningFlowPresenter.Progress.CapturePhaseOne(
                    SourceCode, approvedCode, codeBlocks.Snapshot(), codeBlocks.ActiveIndex);
                data.combatBlocks = combatCodeBlocks.ToArray();
                data.timelineOrder = timelineOrder.ToArray();
                data.approvedMethods = new List<string>(magoMethodBook.ApprovedSources).ToArray();
                data.methodDraft = methodDraft ?? string.Empty;
                data.preparationCode = preparationCode ?? string.Empty;
                progressStore.Save(data);
                pendingCodeSave = false;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Não foi possível salvar a fase 1: {exception.Message}");
            }
        }

        private void PositionWizardSpawnPoint()
        {
            if (arenaCamera == null || wizardSpawnPoint == null) return;
            wizardSpawnPoint.position = CellGroundPosition(0);
            StandInCell(wizardInstance, 0);
        }
    }
}
