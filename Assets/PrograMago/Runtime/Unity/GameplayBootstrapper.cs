using System;
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
        [SerializeField] private Transform wizardSpawnPoint;
        [SerializeField] private Vector2 wizardViewportPosition = new Vector2(0.1f, 0.85f);
        [SerializeField] private GameObject wizardPrefab;
        [SerializeField] private Button battleButton;

        private GameplayPresenter presenter;
        private LearningFlowPresenter learningFlowPresenter;
        private GameObject wizardInstance;

        public string SourceCode
        {
            get => codeInput.text;
            set => codeInput.text = value;
        }

        private void Awake()
        {
            if (codeInput == null || feedbackText == null || learningPath == null ||
                battleProgressText == null || titleText == null || lessonText == null ||
                objectiveText == null || hintText == null || victoryOverlay == null ||
                victoryTitleText == null || victoryAchievementText == null ||
                victoryReviewText == null || nextBattleButton == null ||
                restartProgressPanel == null || restartProgressText == null ||
                arenaCamera == null || wizardSpawnPoint == null || wizardPrefab == null ||
                battleButton == null)
            {
                Debug.LogError(
                    "GameplayBootstrapper precisa de todas as referências do editor, conteúdo, " +
                    "progresso, vitória, reinício e arena configuradas na cena.");
                enabled = false;
                return;
            }

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
            learningFlowPresenter = new LearningFlowPresenter(
                new LearningProgress(path),
                new HoldToRestartController(5f),
                this,
                this,
                this,
                this);
            presenter = new GameplayPresenter(
                new SubmitCodeUseCase(new CodeTokenizer(), new ClassDeclarationValidator(), session),
                this,
                this,
                this,
                learningFlowPresenter);

            battleButton.onClick.AddListener(presenter.Battle);
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

            battleButton.onClick.RemoveListener(presenter.Battle);
            nextBattleButton.onClick.RemoveListener(HandleNextBattle);
            codeInput.onValueChanged.RemoveListener(HandleCodeChanged);
        }

        private void Update()
        {
            if (learningFlowPresenter == null)
            {
                return;
            }

            bool isRestartPressed = Keyboard.current?.rKey.isPressed == true;
            learningFlowPresenter.UpdateRestartHold(isRestartPressed, Time.unscaledDeltaTime);
        }

        private void LateUpdate()
        {
            PositionWizardSpawnPoint();
        }

        public void ShowError(Diagnostic diagnostic)
        {
            feedbackText.color = new Color32(255, 120, 120, 255);
            feedbackText.text = diagnostic == null
                ? "Não foi possível validar o código."
                : $"{diagnostic.Code} — linha {diagnostic.Position.Line}, coluna {diagnostic.Position.Column}: {diagnostic.Detail}";
        }

        public void Clear()
        {
            feedbackText.text = string.Empty;
        }

        public void SetClassDeclared(bool hasDeclaredClass)
        {
            if (hasDeclaredClass && wizardInstance == null)
            {
                wizardInstance = Instantiate(wizardPrefab, wizardSpawnPoint);
                wizardInstance.name = wizardPrefab.name;
                wizardInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }

            if (wizardInstance != null)
            {
                wizardInstance.SetActive(hasDeclaredClass);
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

        public void ShowVictory(BattleVictoryContent content, bool isFinalBattle)
        {
            victoryTitleText.text = content.Title;
            victoryAchievementText.text = content.Achievement;
            victoryReviewText.text = content.Review;
            TMP_Text buttonLabel = nextBattleButton.GetComponentInChildren<TMP_Text>();
            if (buttonLabel != null)
            {
                buttonLabel.text = isFinalBattle ? "Concluir jornada" : "Próxima batalha";
            }

            victoryOverlay.SetActive(true);
        }

        public void HideVictory()
        {
            victoryOverlay.SetActive(false);
        }

        public void SetInteractionEnabled(bool isEnabled)
        {
            codeInput.interactable = isEnabled;
            battleButton.interactable = isEnabled;
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
            return learningFlowPresenter != null && learningFlowPresenter.ReportBattleVictory();
        }

        public bool ReportBattleDefeat()
        {
            return learningFlowPresenter != null && learningFlowPresenter.ReportBattleDefeat();
        }

        private void HandleCodeChanged(string _)
        {
            presenter.Preview();
        }

        private void HandleNextBattle()
        {
            learningFlowPresenter.NextBattle();
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
