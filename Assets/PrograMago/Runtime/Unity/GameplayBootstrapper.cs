using PrograMago.Application;
using PrograMago.Domain;
using PrograMago.Language;
using PrograMago.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrograMago.UnityIntegration
{
    public sealed class GameplayBootstrapper : MonoBehaviour, ICodeEditorView, IFeedbackView, IArenaView
    {
        [SerializeField] private TMP_InputField codeInput;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private Camera arenaCamera;
        [SerializeField] private Transform wizardSpawnPoint;
        [SerializeField] private Vector2 wizardViewportPosition = new Vector2(0.1f, 0.85f);
        [SerializeField] private GameObject wizardPrefab;
        [SerializeField] private Button battleButton;

        private GameplayPresenter presenter;
        private GameObject wizardInstance;

        public string SourceCode
        {
            get => codeInput.text;
            set => codeInput.text = value;
        }

        private void Awake()
        {
            if (codeInput == null || feedbackText == null || arenaCamera == null || wizardSpawnPoint == null ||
                wizardPrefab == null || battleButton == null)
            {
                Debug.LogError(
                    "GameplayBootstrapper precisa das referências de CodeInput, FeedbackText, " +
                    "ArenaCamera, WizardSpawnPoint, WizardPrefab e BattleButton configuradas na cena.");
                enabled = false;
                return;
            }

            PositionWizardSpawnPoint();

            var session = new LearningSession(new ExerciseDefinition(
                "declare-mago-class",
                "Mago",
                "Declare a classe Mago."));
            presenter = new GameplayPresenter(
                new SubmitCodeUseCase(new CodeTokenizer(), new ClassDeclarationValidator(), session),
                this,
                this,
                this);

            battleButton.onClick.AddListener(presenter.Battle);
            codeInput.onValueChanged.AddListener(HandleCodeChanged);
        }

        private void OnDestroy()
        {
            if (presenter == null)
            {
                return;
            }

            battleButton.onClick.RemoveListener(presenter.Battle);
            codeInput.onValueChanged.RemoveListener(HandleCodeChanged);
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

        private void HandleCodeChanged(string _)
        {
            presenter.Preview();
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
