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
        private Transform wizardSpawnPoint;
        private Camera arenaCamera;
        private Button battleButton;
        private GameplayBootstrapper bootstrapper;
        private TMP_Text battleProgressText;
        private TMP_Text titleText;
        private TMP_Text lessonText;
        private TMP_Text objectiveText;
        private TMP_Text hintText;
        private GameObject victoryOverlay;
        private Button nextBattleButton;
        private GameObject restartProgressPanel;

        [UnitySetUp]
        public IEnumerator LoadGameplayScene()
        {
            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            yield return null;

            bootstrapper = Object.FindFirstObjectByType<GameplayBootstrapper>();
            Assert.That(bootstrapper, Is.Not.Null);
            codeInput = FindSceneComponent<TMP_InputField>("CodeInput");
            feedbackText = FindSceneComponent<TMP_Text>("FeedbackText");
            wizardSpawnPoint = FindSceneObject("WizardSpawnPoint").transform;
            arenaCamera = FindSceneComponent<Camera>("Main Camera");
            battleButton = FindSceneComponent<Button>("BattleButton");
            battleProgressText = FindSceneComponent<TMP_Text>("BattleProgressText");
            titleText = FindSceneComponent<TMP_Text>("Title");
            lessonText = FindSceneComponent<TMP_Text>("LessonText");
            objectiveText = FindSceneComponent<TMP_Text>("ObjectiveText");
            hintText = FindSceneComponent<TMP_Text>("HintText");
            victoryOverlay = FindSceneObject("VictoryOverlay");
            nextBattleButton = FindSceneComponent<Button>("NextBattleButton");
            restartProgressPanel = FindSceneObject("RestartProgressPanel");

            Assert.That(wizardSpawnPoint.childCount, Is.Zero);
            Assert.That(FindSceneObjectOrNull("RuntimeFeedbackText"), Is.Null);
        }

        [UnityTest]
        public IEnumerator LearningFlow_LoadsFirstBattleContentAndKeepsOverlaysHidden()
        {
            Assert.That(battleProgressText.text, Does.Contain("Batalha 1/8"));
            Assert.That(titleText.text, Is.EqualTo("O nascimento do Mago"));
            Assert.That(lessonText.text, Does.Contain("O QUE É"));
            Assert.That(lessonText.text, Does.Contain("public class Mago"));
            Assert.That(objectiveText.text, Does.Contain("TAREFA"));
            Assert.That(victoryOverlay.activeSelf, Is.False);
            Assert.That(restartProgressPanel.activeSelf, Is.False);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Battle_InvalidCode_RevealsProgressiveHint()
        {
            codeInput.text = "public class Bruxo {}";

            battleButton.onClick.Invoke();
            yield return null;

            Assert.That(feedbackText.text, Does.Contain("CLASS001"));
            Assert.That(hintText.text, Does.StartWith("DICA"));
            Assert.That(hintText.text, Does.Contain("classe será pública"));
            Assert.That(codeInput.interactable, Is.True);
            Assert.That(battleButton.interactable, Is.True);
        }

        [UnityTest]
        public IEnumerator Battle_ValidCodeWithoutEnemy_ShowsVictoryReviewImmediately()
        {
            codeInput.text = "public class Mago {}";

            battleButton.onClick.Invoke();
            yield return null;

            Assert.That(codeInput.interactable, Is.False);
            Assert.That(battleButton.interactable, Is.False);
            Assert.That(victoryOverlay.activeSelf, Is.True);
            Assert.That(
                FindSceneComponent<TMP_Text>("VictoryTitleText").text,
                Is.EqualTo("Primeira batalha concluída!"));
            Assert.That(FindSceneComponent<TMP_Text>("VictoryAchievementText").text, Is.Not.Empty);
            Assert.That(
                FindSceneComponent<TMP_Text>("VictoryReviewText").text,
                Does.Contain("classe").IgnoreCase);
            Assert.That(nextBattleButton.GetComponentInChildren<TMP_Text>().text, Is.EqualTo("Próxima batalha"));
        }

        [UnityTest]
        public IEnumerator NextBattle_AfterVictory_AdvancesAndClearsEditor()
        {
            codeInput.text = "public class Mago {}";
            battleButton.onClick.Invoke();
            yield return null;

            nextBattleButton.onClick.Invoke();
            yield return null;

            Assert.That(battleProgressText.text, Does.Contain("Batalha 2/8"));
            Assert.That(titleText.text, Is.EqualTo("Estado protegido"));
            Assert.That(codeInput.text, Is.Empty);
            Assert.That(codeInput.interactable, Is.True);
            Assert.That(battleButton.interactable, Is.True);
            Assert.That(victoryOverlay.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator AutomaticVictory_CannotBeOverriddenByDefeat()
        {
            const string submittedCode = "public class Mago {}";
            codeInput.text = submittedCode;
            battleButton.onClick.Invoke();
            yield return null;

            Assert.That(bootstrapper.ReportBattleDefeat(), Is.False);
            yield return null;

            Assert.That(codeInput.text, Is.EqualTo(submittedCode));
            Assert.That(codeInput.interactable, Is.False);
            Assert.That(battleButton.interactable, Is.False);
            Assert.That(victoryOverlay.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator WizardSpawnPoint_TracksLeftSideOfArenaWhenAspectChanges()
        {
            AssertWizardViewportPosition(new Vector2(0.1f, 0.85f));

            arenaCamera.aspect = 0.75f;
            yield return null;

            AssertWizardViewportPosition(new Vector2(0.1f, 0.85f));
        }

        [UnityTest]
        public IEnumerator Editor_IsConfiguredForMultipleLines()
        {
            Assert.That(codeInput.lineType, Is.EqualTo(TMP_InputField.LineType.MultiLineNewline));
            Assert.That(codeInput.interactable, Is.True);
            Assert.That(codeInput.readOnly, Is.False);
            Assert.That(codeInput.text, Is.Empty);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Editor_ReturnKey_InsertsNewLineAndKeepsEditing()
        {
            codeInput.ActivateInputField();
            codeInput.text = "public";
            codeInput.caretPosition = codeInput.text.Length;
            codeInput.ProcessEvent(Event.KeyboardEvent("return"));

            yield return null;

            Assert.That(codeInput.text, Is.EqualTo("public\n"));
            Assert.That(codeInput.isFocused, Is.True);
        }

        [UnityTest]
        public IEnumerator Edit_ValidDeclaration_ShowsPreviewWithoutFeedback()
        {
            codeInput.text = "public class Mago {}";

            yield return null;

            AssertWizardAtSpawn(active: true);
            Assert.That(feedbackText.text, Is.Empty);
        }

        [UnityTest]
        public IEnumerator Edit_InvalidDeclaration_HidesPreviewWithoutFeedback()
        {
            codeInput.text = "public class Mago {}";
            yield return null;

            codeInput.text = "public class Bruxo {}";
            yield return null;

            AssertWizardAtSpawn(active: false);
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

            Assert.That(feedbackText.text, Is.Empty);
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
        public IEnumerator Battle_ValidDeclaration_KeepsFeedbackEmptyAndMagoPrefabVisible()
        {
            codeInput.text = "public class Mago {}";

            battleButton.onClick.Invoke();
            yield return null;

            Assert.That(feedbackText.text, Is.Empty);
            AssertWizardAtSpawn(active: true);
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
            AssertWizardAtSpawn(active: false);
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
            AssertWizardAtSpawn(active: false);
        }

        private void AssertWizardAtSpawn(bool active)
        {
            if (!active && wizardSpawnPoint.childCount == 0)
            {
                return;
            }

            Assert.That(wizardSpawnPoint.childCount, Is.EqualTo(1));
            Transform wizard = wizardSpawnPoint.GetChild(0);
            Assert.That(wizard.name, Does.StartWith("Mago"));
            Assert.That(wizard.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(wizard.gameObject.activeSelf, Is.EqualTo(active));
            Assert.That(wizard.GetComponentsInChildren<SpriteRenderer>(true), Has.Length.EqualTo(1));
        }

        private void AssertWizardViewportPosition(Vector2 expected)
        {
            Vector3 viewportPosition = arenaCamera.WorldToViewportPoint(wizardSpawnPoint.position);
            Assert.That(viewportPosition.x, Is.EqualTo(expected.x).Within(0.001f));
            Assert.That(viewportPosition.y, Is.EqualTo(expected.y).Within(0.001f));
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
