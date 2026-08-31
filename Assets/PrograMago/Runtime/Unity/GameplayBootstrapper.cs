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
        private const string StarterCode = "public class Mago {\n\n}";

        [SerializeField] private TMP_InputField codeInput;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private GameObject wizardPlaceholder;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button restartButton;

        private GameplayPresenter presenter;

        public string SourceCode
        {
            get => codeInput.text;
            set => codeInput.text = value;
        }

        private void Awake()
        {
            ResolveSceneReferences();
            if (codeInput == null || wizardPlaceholder == null)
            {
                Debug.LogError("GameplayBootstrapper não encontrou CodeInput ou WizardPlaceholder.");
                enabled = false;
                return;
            }

            feedbackText ??= CreateFeedbackText();
            CreateButtonsWhenMissing();
            wizardPlaceholder.SetActive(false);

            var session = new LearningSession(new ExerciseDefinition(
                "declare-mago-class",
                "Mago",
                "Declare a classe Mago."));
            presenter = new GameplayPresenter(
                new SubmitCodeUseCase(new CodeTokenizer(), new ClassDeclarationValidator(), session),
                new RestartSessionUseCase(session),
                this,
                this,
                this,
                StarterCode);

            submitButton.onClick.AddListener(presenter.Submit);
            restartButton.onClick.AddListener(presenter.Restart);
        }

        private void OnDestroy()
        {
            if (presenter == null)
            {
                return;
            }

            submitButton.onClick.RemoveListener(presenter.Submit);
            restartButton.onClick.RemoveListener(presenter.Restart);
        }

        public void ShowSuccess(string message)
        {
            feedbackText.color = new Color32(93, 214, 140, 255);
            feedbackText.text = message;
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
            wizardPlaceholder.SetActive(hasDeclaredClass);
        }

        private void ResolveSceneReferences()
        {
            codeInput ??= GetComponentInChildren<TMP_InputField>(true);

            wizardPlaceholder ??= FindSceneObject("WizardPlaceholder");
        }

        private TMP_Text CreateFeedbackText()
        {
            var feedbackObject = new GameObject(
                "RuntimeFeedbackText",
                typeof(RectTransform),
                typeof(TextMeshProUGUI));
            feedbackObject.layer = gameObject.layer;
            feedbackObject.transform.SetParent(transform, false);

            var rect = (RectTransform)feedbackObject.transform;
            rect.anchorMin = new Vector2(0.08f, 0.12f);
            rect.anchorMax = new Vector2(0.65f, 0.18f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = feedbackObject.GetComponent<TextMeshProUGUI>();
            text.text = string.Empty;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.fontSize = 22;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            Transform[] sceneTransforms = Object.FindObjectsByType<Transform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            foreach (Transform candidate in sceneTransforms)
            {
                if (candidate.name == objectName)
                {
                    return candidate.gameObject;
                }
            }

            return null;
        }

        private void CreateButtonsWhenMissing()
        {
            submitButton ??= CreateButton(
                "SubmitButton",
                "Executar",
                new Vector2(0.08f, 0.04f),
                new Vector2(0.20f, 0.11f),
                new Color32(91, 74, 190, 255));
            restartButton ??= CreateButton(
                "RestartButton",
                "Reiniciar",
                new Vector2(0.22f, 0.04f),
                new Vector2(0.34f, 0.11f),
                new Color32(76, 81, 105, 255));
        }

        private Button CreateButton(
            string objectName,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color backgroundColor)
        {
            var buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.layer = gameObject.layer;
            buttonObject.transform.SetParent(transform, false);

            var rect = (RectTransform)buttonObject.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            buttonObject.GetComponent<Image>().color = backgroundColor;
            Button button = buttonObject.GetComponent<Button>();

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.layer = gameObject.layer;
            labelObject.transform.SetParent(buttonObject.transform, false);
            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var labelText = labelObject.GetComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;
            labelText.fontSize = 24;

            return button;
        }
    }
}
