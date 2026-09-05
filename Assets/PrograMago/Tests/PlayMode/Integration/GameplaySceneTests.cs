using System.Collections;
using NUnit.Framework;
using PrograMago.UnityIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PrograMago.Tests.Integration
{
    public sealed class GameplaySceneTests
    {
        private TMP_InputField codeInput;
        private TMP_Text feedbackText;
        private GameObject wizardPlaceholder;
        private Button battleButton;

        [UnitySetUp]
        public IEnumerator LoadGameplayScene()
        {
            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            yield return null;

            Assert.That(Object.FindFirstObjectByType<GameplayBootstrapper>(), Is.Not.Null);
            codeInput = FindSceneComponent<TMP_InputField>("CodeInput");
            feedbackText = FindSceneComponent<TMP_Text>("RuntimeFeedbackText");
            wizardPlaceholder = FindSceneObject("WizardPlaceholder");
            battleButton = FindSceneComponent<Button>("BattleButton");

            Assert.That(wizardPlaceholder.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator Editor_IsConfiguredForMultipleLines()
        {
            Assert.That(codeInput.lineType, Is.EqualTo(TMP_InputField.LineType.MultiLineNewline));

            yield return null;
        }

        [UnityTest]
        public IEnumerator Edit_ValidDeclaration_ShowsPreviewWithoutFeedback()
        {
            codeInput.text = "public class Mago {}";

            yield return null;

            Assert.That(wizardPlaceholder.activeSelf, Is.True);
            Assert.That(feedbackText.text, Is.Empty);
        }

        [UnityTest]
        public IEnumerator Edit_InvalidDeclaration_HidesPreviewWithoutFeedback()
        {
            codeInput.text = "public class Mago {}";
            yield return null;

            codeInput.text = "public class Bruxo {}";
            yield return null;

            Assert.That(wizardPlaceholder.activeSelf, Is.False);
            Assert.That(feedbackText.text, Is.Empty);
        }

        [UnityTest]
        public IEnumerator BattleButton_ValidatesCodeAndUsesBattleLabel()
        {
            GameObject battleObject = FindSceneObjectOrNull("BattleButton");
            Assert.That(battleObject, Is.Not.Null);
            var button = battleObject.GetComponent<Button>();
            var label = battleObject.GetComponentInChildren<TMP_Text>();
            Assert.That(button, Is.Not.Null);
            Assert.That(label, Is.Not.Null);
            Assert.That(label.text, Is.EqualTo("Batalhar"));
            codeInput.text = "public class Mago {}";

            button.onClick.Invoke();
            yield return null;

            Assert.That(feedbackText.text, Does.Contain("sucesso"));
            Assert.That(codeInput.text, Is.EqualTo("public class Mago {}"));
        }

        [UnityTest]
        public IEnumerator EditorActions_ExposeOnlyBattleButton()
        {
            Assert.That(FindSceneObjectOrNull("BattleButton"), Is.Not.Null);
            Assert.That(FindSceneObjectOrNull("SubmitButton"), Is.Null);
            Assert.That(FindSceneObjectOrNull("RestartButton"), Is.Null);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Battle_ValidDeclaration_ShowsSuccessAndWizardSilhouette()
        {
            codeInput.text = "public class Mago {}";

            battleButton.onClick.Invoke();
            yield return null;

            Assert.That(feedbackText.text, Does.Contain("sucesso"));
            Assert.That(wizardPlaceholder.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator Battle_InvalidDeclarationAfterSuccess_ShowsErrorAndKeepsCurrentPreviewHidden()
        {
            codeInput.text = "public class Mago {}";
            battleButton.onClick.Invoke();
            yield return null;
            codeInput.text = "public class Bruxo {}";

            battleButton.onClick.Invoke();
            yield return null;

            Assert.That(feedbackText.text, Does.Contain("CLASS001"));
            Assert.That(wizardPlaceholder.activeSelf, Is.False);
            Assert.That(codeInput.text, Is.EqualTo("public class Bruxo {}"));
        }

        [UnityTest]
        public IEnumerator Edit_AfterBattleError_ClearsStaleFeedbackWithoutShowingAnotherError()
        {
            codeInput.text = "public class Bruxo {}";
            battleButton.onClick.Invoke();
            yield return null;
            Assert.That(feedbackText.text, Does.Contain("CLASS001"));

            codeInput.text = "public class Mago {";
            yield return null;

            Assert.That(feedbackText.text, Is.Empty);
            Assert.That(wizardPlaceholder.activeSelf, Is.False);
        }

        private static T FindSceneComponent<T>(string objectName) where T : Component
        {
            GameObject sceneObject = FindSceneObject(objectName);
            T component = sceneObject.GetComponent<T>();
            Assert.That(component, Is.Not.Null, $"{objectName} deve possuir {typeof(T).Name}.");
            return component;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            GameObject sceneObject = FindSceneObjectOrNull(objectName);
            if (sceneObject != null)
            {
                return sceneObject;
            }

            Assert.Fail($"Objeto {objectName} não encontrado na cena.");
            return null;
        }

        private static GameObject FindSceneObjectOrNull(string objectName)
        {
            Transform[] transforms = Object.FindObjectsByType<Transform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            foreach (Transform candidate in transforms)
            {
                if (candidate.name == objectName)
                {
                    return candidate.gameObject;
                }
            }

            return null;
        }
    }
}
