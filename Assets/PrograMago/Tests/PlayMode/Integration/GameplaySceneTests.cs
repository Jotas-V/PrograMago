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
        private Button submitButton;
        private Button restartButton;

        [UnitySetUp]
        public IEnumerator LoadGameplayScene()
        {
            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            yield return null;

            Assert.That(Object.FindFirstObjectByType<GameplayBootstrapper>(), Is.Not.Null);
            codeInput = FindSceneComponent<TMP_InputField>("CodeInput");
            feedbackText = FindSceneComponent<TMP_Text>("FeedbackText");
            wizardPlaceholder = FindSceneObject("WizardPlaceholder");
            submitButton = FindSceneComponent<Button>("SubmitButton");
            restartButton = FindSceneComponent<Button>("RestartButton");

            Assert.That(wizardPlaceholder.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator Submit_ValidDeclaration_ShowsSuccessAndWizardSilhouette()
        {
            codeInput.text = "public class Mago {}";

            submitButton.onClick.Invoke();
            yield return null;

            Assert.That(feedbackText.text, Does.Contain("sucesso"));
            Assert.That(wizardPlaceholder.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator Submit_InvalidDeclarationAfterSuccess_ShowsErrorAndPreservesWizard()
        {
            codeInput.text = "public class Mago {}";
            submitButton.onClick.Invoke();
            yield return null;
            codeInput.text = "public class Bruxo {}";

            submitButton.onClick.Invoke();
            yield return null;

            Assert.That(feedbackText.text, Does.Contain("CLASS001"));
            Assert.That(wizardPlaceholder.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator Restart_AfterSuccess_RestoresInitialSceneState()
        {
            codeInput.text = "public class Mago {}";
            submitButton.onClick.Invoke();
            yield return null;

            restartButton.onClick.Invoke();
            yield return null;

            Assert.That(codeInput.text, Is.EqualTo("public class Mago {\n\n}"));
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

            Assert.Fail($"Objeto {objectName} não encontrado na cena.");
            return null;
        }
    }
}
