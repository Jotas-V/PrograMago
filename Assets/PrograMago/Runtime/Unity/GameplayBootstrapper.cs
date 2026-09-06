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
        [SerializeField] private Transform wizardSpawnPoint;
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
            if (codeInput == null || feedbackText == null || wizardSpawnPoint == null ||
                wizardPrefab == null || battleButton == null)
            {
                Debug.LogError(
                    "GameplayBootstrapper precisa das referências de CodeInput, FeedbackText, " +
                    "WizardSpawnPoint, WizardPrefab e BattleButton configuradas na cena.");
                enabled = false;
                return;
            }

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

    }
}
