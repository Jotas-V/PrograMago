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
    public sealed class GameplayBootstrapper : MonoBehaviour, ICodeEditorView, IFeedbackView, IArenaView,
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
        private TMP_Text magoStatsText;
        private TMP_Text enemyStatsText;
        private readonly List<GameObject> enemyMarkers = new List<GameObject>();
        private static Sprite enemyPlaceholderSprite;
        private readonly CodeBlockDocument codeBlocks = new CodeBlockDocument();
        private readonly Button[] codeBlockButtons = new Button[CodeBlockDocument.BlockCount];
        private PhaseOneSaveStore progressStore;
        private string approvedCode = string.Empty;
        private bool pendingCodeSave;
        private float nextCodeSaveTime;
        private bool approvedClass;
        private bool classPreview;
        private CombatEngine combat;
        private float combatAccumulator;
        private RectTransform editorPanel;
        private GameObject combatActionPanel;
        private GameObject combatDefeatOverlay;
        private Button pauseCombatButton;
        private TMP_Text combatStatusText;
        private readonly Button[] combatActionButtons = new Button[3];
        private float editorAnchorMaxX;
        private CombatElement[] activeExtraSpells = Array.Empty<CombatElement>();

        public MagoState CurrentMago { get; private set; }
        public bool CanReorderCombatActions => combat != null && combat.IsPaused;

        public IReadOnlyList<EnemyState> CurrentEnemies { get; private set; } = Array.Empty<EnemyState>();

        public string SourceCode
        {
            get => codeBlocks.SourceCode;
            set
            {
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

            PositionWizardSpawnPoint();
            CreateMagoStatsText();
            CreateEnemyStatsText();
            CreateCodeBlockButtons();
            CreateCombatControls();

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

            battleButton.onClick.AddListener(HandleBattle);
            nextBattleButton.onClick.AddListener(HandleNextBattle);
            codeInput.onValueChanged.AddListener(HandleCodeChanged);
            learningFlowPresenter.Initialize();
        }

        private void OnDestroy()
        {
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
            PositionWizardSpawnPoint();
            PositionCombatActors();
        }

        public void ShowError(Diagnostic diagnostic)
        {
            feedbackText.color = new Color32(255, 120, 120, 255);
            if (diagnostic == null)
            {
                feedbackText.text = "Não foi possível validar o código.";
                return;
            }

            CodeBlockLocation location = codeBlocks.Locate(diagnostic.Position.Offset);
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
            CurrentMago = null;
            approvedCode = SourceCode;
            RenderWizard();
        }

        public void ShowMago(MagoState mago)
        {
            CurrentMago = mago ?? throw new ArgumentNullException(nameof(mago));
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
            HideCombatControls();
            combat = null;
            approvedClass = false;
            classPreview = false;
            CurrentMago = null;
            CurrentEnemies = Array.Empty<EnemyState>();
            ClearEnemyMarkers();
            if (enemyStatsText != null) enemyStatsText.text = string.Empty;
            if (magoStatsText != null)
            {
                ((RectTransform)magoStatsText.transform).anchorMax = new Vector2(0.98f, 0.77f);
            }
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
            }

            if (wizardInstance != null)
            {
                wizardInstance.SetActive(visible);
                SpriteRenderer sprite = wizardInstance.GetComponent<SpriteRenderer>();
                if (sprite != null)
                {
                    Color color = wizardPrefab.GetComponent<SpriteRenderer>().color;
                    color.a = CurrentMago == null ? 0.35f : 1f;
                    sprite.color = color;
                }
            }

            if (magoStatsText != null)
            {
                magoStatsText.text = CurrentMago == null
                    ? string.Empty
                    : $"Mago {CurrentMago.InstanceName}\n" +
                      $"Vida: {CurrentMago.Vida}   Dano: {CurrentMago.Dano}\n" +
                      $"Alcance: {CurrentMago.Alcance}   Iniciativa: {CurrentMago.Iniciativa}\n" +
                      $"Velocidade de ataque: {CurrentMago.VelocidadeAtaque}\n" +
                      $"Pontos restantes: {CurrentMago.RemainingPoints}";
            }
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
            magoStatsText.color = Color.white;
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
            enemyStatsText.color = Color.white;
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
                renderer.sprite = GetEnemyPlaceholderSprite();
                renderer.color = EnemyColor(enemy.Elemento);
                renderer.sortingOrder = 10;
                marker.transform.localScale = new Vector3(0.65f, 0.85f, 1f);
                float distance = Vector3.Dot(
                    wizardSpawnPoint.position - arenaCamera.transform.position,
                    arenaCamera.transform.forward);
                marker.transform.position = arenaCamera.ViewportToWorldPoint(new Vector3(
                    0.65f + index * 0.09f, 0.80f, distance));
                enemyMarkers.Add(marker);

                if (summary.Length > 0) summary.Append('\n');
                summary.Append(enemy.Name).Append(" — Vida: ").Append(enemy.Vida)
                    .Append(" — ").Append(enemy.Elemento);
            }

            enemyStatsText.text = summary.ToString();
            ((RectTransform)magoStatsText.transform).anchorMax =
                CurrentEnemies.Count == 0 ? new Vector2(0.98f, 0.77f) : new Vector2(0.64f, 0.77f);
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
            titleText.text = battle.Lesson.Title;
            lessonText.text =
                $"O QUE É\n{battle.Lesson.WhatItIs}\n\n" +
                $"PARA QUE SERVE\n{battle.Lesson.Purpose}\n\n" +
                $"COMO USAR\n{battle.Lesson.UsageExample}\n\n" +
                $"NO JOGO\n{battle.Lesson.GameEffect}";
            objectiveText.text = $"TAREFA\n{battle.Lesson.Task}";
            hintText.text = string.Empty;
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
            codeInput.interactable = isEnabled;
            battleButton.interactable = isEnabled;
            foreach (Button button in codeBlockButtons)
                if (button != null) button.interactable = isEnabled;
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
            codeBlocks.SetActiveText(codeInput.text);
            presenter.Preview();
            pendingCodeSave = true;
            nextCodeSaveTime = Time.unscaledTime + 0.5f;
        }

        private void CreateCodeBlockButtons()
        {
            for (int index = 0; index < codeBlockButtons.Length; index++)
            {
                Button button = Instantiate(battleButton, battleButton.transform.parent);
                button.gameObject.name = $"CodeBlockButton{index + 1}";
                button.onClick.RemoveAllListeners();
                RectTransform rect = button.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                rect.pivot = Vector2.zero;
                rect.anchoredPosition = new Vector2(12 + index * 74, 14);
                rect.sizeDelta = new Vector2(62, 46);
                TMP_Text label = button.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = (index + 1).ToString();
                    label.fontSize = 25;
                }

                int selected = index;
                button.onClick.AddListener(() => SelectCodeBlock(selected));
                codeBlockButtons[index] = button;
            }

            RefreshCodeBlockButtons();
        }

        private void SelectCodeBlock(int index)
        {
            if (codeBlocks.ActiveIndex == index) return;

            codeBlocks.SetActiveText(codeInput.text);
            codeBlocks.Select(index);
            codeInput.SetTextWithoutNotify(codeBlocks.ActiveText);
            RefreshCodeBlockButtons();
            presenter?.Preview();
            SaveProgress();
        }

        private void RefreshCodeBlockButtons()
        {
            for (int index = 0; index < codeBlockButtons.Length; index++)
            {
                Image background = codeBlockButtons[index].GetComponent<Image>();
                background.color = index == codeBlocks.ActiveIndex
                    ? new Color32(91, 74, 190, 255)
                    : new Color32(69, 70, 90, 255);
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
            activeExtraSpells = extraSpells == null
                ? Array.Empty<CombatElement>()
                : (CombatElement[])extraSpells.Clone();
            combat = new CombatEngine(CombatWizard.FromMago(CurrentMago,
                activeExtraSpells), CurrentEnemies);
            combatAccumulator = 0f;
            combatActionPanel.SetActive(true);
            combatDefeatOverlay.SetActive(false);
            pauseCombatButton.gameObject.SetActive(true);
            pauseCombatButton.GetComponentInChildren<TMP_Text>().text = "Pausar";
            editorPanel.anchorMax = new Vector2(0.56f, editorPanel.anchorMax.y);
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
            if (action != null) RenderCombatEvent(action);
            RenderCombatState();
            PositionCombatActors();
            if (combat.Outcome == CombatOutcome.Victory)
            {
                HideCombatControls();
                if (!ReportBattleVictory()) SetInteractionEnabled(true);
                combat = null;
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
                combatStatusText.text = "Pausado — arraste os blocos para mudar a ordem";
            }
            pauseCombatButton.GetComponentInChildren<TMP_Text>().text =
                combat.IsPaused ? "Continuar" : "Pausar";
        }

        public void ReorderCombatAction(int fromIndex, int toIndex)
        {
            if (combat == null) return;
            combat.MoveAction(fromIndex, toIndex);
            RefreshCombatActionButtons();
        }

        private void RetryCombat()
        {
            if (combat == null || combat.Outcome != CombatOutcome.Defeat) return;
            combat.Restart();
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
            if (!ReportBattleDefeat())
            {
                HideCombatControls();
                combat = null;
                SetInteractionEnabled(true);
            }
        }

        private void CreateCombatControls()
        {
            editorPanel = GameObject.Find("CodeEditorPanel").GetComponent<RectTransform>();
            editorAnchorMaxX = editorPanel.anchorMax.x;
            RectTransform bottomArea = (RectTransform)editorPanel.parent;
            combatActionPanel = CreatePanel("CombatActionPanel", bottomArea,
                new Vector2(0.56f, 0.02f), new Vector2(0.75f, 0.98f),
                new Color32(31, 28, 48, 245)).gameObject;
            CreateLabel("CombatActionTitle", combatActionPanel.transform,
                "FILA DE AÇÕES", new Vector2(0.05f, 0.82f),
                new Vector2(0.95f, 0.98f));
            for (int index = 0; index < combatActionButtons.Length; index++)
            {
                float top = 0.78f - index * 0.22f;
                combatActionButtons[index] = CreateButton($"CombatAction{index + 1}",
                    combatActionPanel.transform, string.Empty,
                    new Vector2(0.06f, top - 0.17f), new Vector2(0.94f, top));
                combatActionButtons[index].gameObject
                    .AddComponent<CombatActionDragHandle>().Configure(this, index);
            }
            CreateLabel("CombatActionHint", combatActionPanel.transform,
                "Pause para reordenar", new Vector2(0.05f, 0.03f),
                new Vector2(0.95f, 0.16f));
            combatActionPanel.SetActive(false);

            pauseCombatButton = CreateButton("PauseCombatButton", arenaFrame,
                "Pausar", new Vector2(0.80f, 0.82f), new Vector2(0.98f, 0.98f));
            pauseCombatButton.onClick.AddListener(ToggleCombatPause);
            pauseCombatButton.gameObject.SetActive(false);

            combatStatusText = CreateLabel("CombatStatusText", arenaFrame,
                string.Empty, new Vector2(0.23f, 0.01f), new Vector2(0.78f, 0.20f));
            combatStatusText.gameObject.SetActive(false);

            combatDefeatOverlay = CreatePanel("CombatDefeatOverlay", arenaFrame,
                new Vector2(0.28f, 0.12f), new Vector2(0.72f, 0.88f),
                new Color32(40, 29, 45, 245)).gameObject;
            CreateLabel("CombatDefeatTitle", combatDefeatOverlay.transform,
                "Derrota", new Vector2(0.08f, 0.64f), new Vector2(0.92f, 0.95f));
            CreateButton("RetryCombatButton", combatDefeatOverlay.transform,
                "Tentar novamente", new Vector2(0.08f, 0.36f),
                new Vector2(0.92f, 0.60f)).onClick.AddListener(RetryCombat);
            CreateButton("EditAfterDefeatButton", combatDefeatOverlay.transform,
                "Editar código", new Vector2(0.08f, 0.08f),
                new Vector2(0.92f, 0.32f)).onClick.AddListener(EditAfterDefeat);
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
            item.GetComponent<Image>().color = color;
            return rect;
        }

        private Button CreateButton(string name, Transform parent, string label,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform rect = CreatePanel(name, parent, anchorMin, anchorMax,
                new Color32(83, 67, 152, 255));
            Button button = rect.gameObject.AddComponent<Button>();
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
            if (combat == null) return;
            for (int index = 0; index < combatActionButtons.Length; index++)
            {
                string label;
                switch (combat.ActionOrder[index])
                {
                    case CombatAction.AnalyzeTarget: label = "Analisar alvo"; break;
                    case CombatAction.SelectSpell: label = "Selecionar magia"; break;
                    default: label = "Atacar"; break;
                }
                combatActionButtons[index].GetComponentInChildren<TMP_Text>().text = label;
            }
        }

        private void RenderCombatEvent(CombatEvent action)
        {
            string actor = action.Actor == "Mago" ? "Mago" : action.Actor;
            switch (action.Kind)
            {
                case CombatEventKind.Move:
                    combatStatusText.text = $"{actor} avança: alvo fora do alcance";
                    combatStatusText.color = new Color32(245, 221, 151, 255);
                    break;
                case CombatEventKind.NoTarget:
                    combatStatusText.text = "Mago atacou sem analisar um alvo";
                    combatStatusText.color = new Color32(255, 169, 139, 255);
                    break;
                case CombatEventKind.Ineffective:
                    combatStatusText.text = $"Magia {ElementLabel(action.Element)} ineficaz contra {action.Target}";
                    combatStatusText.color = new Color32(255, 169, 139, 255);
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
            magoStatsText.text = $"Mago {CurrentMago.InstanceName} — Vida: {combat.WizardLife}/{CurrentMago.Vida}\n" +
                $"Dano: {CurrentMago.Dano}  Alcance: {CurrentMago.Alcance}\n" +
                $"Iniciativa: {CurrentMago.Iniciativa}  Velocidade: {CurrentMago.VelocidadeAtaque}";
            var summary = new StringBuilder();
            foreach (CombatEnemy enemy in combat.Enemies)
            {
                if (summary.Length > 0) summary.Append('\n');
                summary.Append(enemy.Source.Name).Append(" — Vida: ")
                    .Append(enemy.Life).Append('/').Append(enemy.Source.Vida)
                    .Append(" — ").Append(enemy.Source.Elemento);
            }
            enemyStatsText.text = summary.ToString();
        }

        private void PositionCombatActors()
        {
            if (combat == null || arenaCamera == null) return;
            float depth = Vector3.Dot(wizardSpawnPoint.position - arenaCamera.transform.position,
                arenaCamera.transform.forward);
            if (wizardInstance != null)
            {
                wizardInstance.transform.position = arenaCamera.ViewportToWorldPoint(
                    new Vector3(0.1f + combat.WizardPosition * 0.075f, 0.85f, depth));
            }
            for (int index = 0; index < enemyMarkers.Count && index < combat.Enemies.Count; index++)
            {
                GameObject marker = enemyMarkers[index];
                CombatEnemy enemy = combat.Enemies[index];
                if (marker == null) continue;
                marker.SetActive(enemy.Life > 0);
                marker.transform.position = arenaCamera.ViewportToWorldPoint(
                    new Vector3(0.1f + enemy.Position * 0.075f, 0.80f, depth));
            }
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
                case CombatElement.Fire: return new Color32(255, 142, 88, 255);
                case CombatElement.Water: return new Color32(112, 190, 255, 255);
                case CombatElement.Electric: return new Color32(255, 233, 120, 255);
                default: return Color.white;
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
                progressStore.Save(learningFlowPresenter.Progress.CapturePhaseOne(
                    SourceCode, approvedCode, codeBlocks.Snapshot(), codeBlocks.ActiveIndex));
                pendingCodeSave = false;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Não foi possível salvar a fase 1: {exception.Message}");
            }
        }

        private void PositionWizardSpawnPoint()
        {
            if (arenaCamera == null || wizardSpawnPoint == null)
            {
                return;
            }

            float distanceFromCamera = Vector3.Dot(
                wizardSpawnPoint.position - arenaCamera.transform.position,
                arenaCamera.transform.forward);
            wizardSpawnPoint.position = arenaCamera.ViewportToWorldPoint(new Vector3(
                wizardViewportPosition.x,
                wizardViewportPosition.y,
                distanceFromCamera));
        }

    }
}
