using System.Collections;
using System.IO;
using NUnit.Framework;
using PrograMago.Domain;
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
        private string progressSavePath;
        private byte[] previousProgressSave;

        [UnitySetUp]
        public IEnumerator LoadGameplayScene()
        {
            progressSavePath = Path.Combine(
                UnityEngine.Application.persistentDataPath, "programago-phase1.json");
            previousProgressSave = File.Exists(progressSavePath)
                ? File.ReadAllBytes(progressSavePath)
                : null;
            if (File.Exists(progressSavePath))
            {
                File.Delete(progressSavePath);
            }

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

        [UnityTearDown]
        public IEnumerator RestoreProgressSave()
        {
            GameplayBootstrapper liveBootstrapper =
                Object.FindFirstObjectByType<GameplayBootstrapper>();
            if (liveBootstrapper != null)
            {
                Object.Destroy(liveBootstrapper.gameObject);
                yield return null;
            }

            if (previousProgressSave == null)
            {
                if (File.Exists(progressSavePath))
                {
                    File.Delete(progressSavePath);
                }
            }
            else
            {
                File.WriteAllBytes(progressSavePath, previousProgressSave);
            }

            yield return null;
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
        public IEnumerator CodeBlocks_SwitchingPreservesEachTextInTheVisibleEditor()
        {
            Button first = FindSceneComponent<Button>("CodeBlockButton1");
            Button second = FindSceneComponent<Button>("CodeBlockButton2");
            Button third = FindSceneComponent<Button>("CodeBlockButton3");

            codeInput.text = "public class Mago {}";
            second.onClick.Invoke();
            yield return null;
            Assert.That(codeInput.text, Is.Empty);

            codeInput.text = "public class Inimigo {}";
            third.onClick.Invoke();
            yield return null;
            Assert.That(codeInput.text, Is.Empty);

            first.onClick.Invoke();
            yield return null;
            Assert.That(codeInput.text, Is.EqualTo("public class Mago {}"));
            second.onClick.Invoke();
            yield return null;
            Assert.That(codeInput.text, Is.EqualTo("public class Inimigo {}"));
        }

        [UnityTest]
        public IEnumerator CodeBlocks_ReloadRestoresThreeTextsAndSelectedBlock()
        {
            Button second = FindSceneComponent<Button>("CodeBlockButton2");
            Button third = FindSceneComponent<Button>("CodeBlockButton3");
            codeInput.text = "public class Mago {}";
            second.onClick.Invoke();
            codeInput.text = "public class Inimigo {}";
            third.onClick.Invoke();
            codeInput.text = "Mago mago = new Mago();";

            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            yield return null;

            Assert.That(FindSceneComponent<TMP_InputField>("CodeInput").text,
                Is.EqualTo("Mago mago = new Mago();"));
            FindSceneComponent<Button>("CodeBlockButton1").onClick.Invoke();
            Assert.That(FindSceneComponent<TMP_InputField>("CodeInput").text,
                Is.EqualTo("public class Mago {}"));
            FindSceneComponent<Button>("CodeBlockButton2").onClick.Invoke();
            Assert.That(FindSceneComponent<TMP_InputField>("CodeInput").text,
                Is.EqualTo("public class Inimigo {}"));
        }

        [UnityTest]
        public IEnumerator CodeBlocks_BattleReadsCodeFromHiddenBlock()
        {
            FindSceneComponent<Button>("CodeBlockButton2").onClick.Invoke();
            codeInput.text = "public class Mago {}";
            FindSceneComponent<Button>("CodeBlockButton3").onClick.Invoke();
            Assert.That(codeInput.text, Is.Empty);

            battleButton.onClick.Invoke();
            yield return null;

            Assert.That(victoryOverlay.activeSelf, Is.True);
            Assert.That(FindSceneComponent<TMP_Text>("VictoryTitleText").text,
                Does.Contain("Primeira batalha concluída"));
        }

        [UnityTest]
        public IEnumerator CodeBlocks_BattleErrorPointsToHiddenBlockAndLocalLine()
        {
            codeInput.text = "public class Mago {}";
            FindSceneComponent<Button>("CodeBlockButton2").onClick.Invoke();
            codeInput.text = "\n#";
            FindSceneComponent<Button>("CodeBlockButton1").onClick.Invoke();

            battleButton.onClick.Invoke();
            yield return null;

            Assert.That(feedbackText.text, Does.Contain("LEX001"));
            Assert.That(feedbackText.text, Does.Contain("bloco 2, linha 2, coluna 1"));
            Assert.That(codeInput.text, Is.EqualTo("public class Mago {}"));
            Assert.That(victoryOverlay.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator CodeBlocks_LegacySingleTextSaveOpensInFirstBlock()
        {
            codeInput.text = "public class Mago {}";
            Object.DestroyImmediate(bootstrapper);
            PhaseOneSaveData legacy = JsonUtility.FromJson<PhaseOneSaveData>(
                File.ReadAllText(progressSavePath));
            legacy.version = 1;
            legacy.sourceBlocks = null;
            legacy.activeBlock = 0;
            File.WriteAllText(progressSavePath, JsonUtility.ToJson(legacy));

            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            yield return null;

            Assert.That(FindSceneComponent<TMP_InputField>("CodeInput").text,
                Is.EqualTo("public class Mago {}"));
            FindSceneComponent<Button>("CodeBlockButton2").onClick.Invoke();
            Assert.That(FindSceneComponent<TMP_InputField>("CodeInput").text, Is.Empty);
        }

        [UnityTest]
        public IEnumerator Arena_ApprovedEnemiesAppearOnRightAndResetRemovesThem()
        {
            bootstrapper.ShowEnemies(new[]
            {
                new EnemyState("boneco", "Boneco de Treinamento", 10, "neutro"),
                new EnemyState("golem", "Golem de Gelo", 12, "gelo")
            });
            yield return null;

            GameObject marker = GameObject.Find("EnemyMarker-boneco");
            Assert.That(marker, Is.Not.Null);
            Assert.That(marker.GetComponent<SpriteRenderer>(), Is.Not.Null);
            Assert.That(Camera.main.WorldToViewportPoint(marker.transform.position).x,
                Is.GreaterThan(0.5f));
            Assert.That(FindSceneComponent<TMP_Text>("EnemyStatsText").text,
                Does.Contain("Boneco de Treinamento"));

            bootstrapper.Reset();
            yield return null;
            Assert.That(GameObject.Find("EnemyMarker-boneco"), Is.Null);
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
        public IEnumerator NextBattle_AfterVictory_AdvancesAndPreservesEditorCode()
        {
            codeInput.text = "public class Mago {}";
            battleButton.onClick.Invoke();
            yield return null;

            nextBattleButton.onClick.Invoke();
            yield return null;

            Assert.That(battleProgressText.text, Does.Contain("Batalha 2/8"));
            Assert.That(titleText.text, Is.EqualTo("Estado protegido"));
            Assert.That(codeInput.text, Is.EqualTo("public class Mago {}"));
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
        public IEnumerator Battle_ValidatedInstance_ShowsMappedAttributesAndRemainingPoints()
        {
            codeInput.text = "public class Mago {}";
            battleButton.onClick.Invoke();
            yield return null;
            nextBattleButton.onClick.Invoke();
            yield return null;

            const string attributeCode =
                "public class Mago { private int vida; private int dano; " +
                "private int alcance; private int iniciativa; private int velocidadeAtaque; }";
            codeInput.text = attributeCode;
            battleButton.onClick.Invoke();
            yield return null;
            nextBattleButton.onClick.Invoke();
            yield return null;
            Assert.That(codeInput.text, Is.EqualTo(attributeCode));

            codeInput.text =
                "public class Mago { private int vida; private int dano; " +
                "private int alcance; private int iniciativa; private int velocidadeAtaque; " +
                "public Mago(int dano, int vida, int alcance, int iniciativa, int velocidadeAtaque) { " +
                "this.vida = vida; this.dano = dano; this.alcance = alcance; " +
                "this.iniciativa = iniciativa; this.velocidadeAtaque = velocidadeAtaque; } } " +
                "Mago heroi = new Mago(7, 5, 3, 4, 2);";
            battleButton.onClick.Invoke();
            yield return null;

            TMP_Text stats = FindSceneComponent<TMP_Text>("MagoStatsText");
            Assert.That(stats.text, Does.Contain("Vida: 5"));
            Assert.That(stats.text, Does.Contain("Dano: 7"));
            Assert.That(stats.text, Does.Contain("Alcance: 3"));
            Assert.That(stats.text, Does.Contain("Iniciativa: 4"));
            Assert.That(stats.text, Does.Contain("Velocidade de ataque: 2"));
            Assert.That(stats.text, Does.Contain("Pontos restantes: 4"));
            Assert.That(wizardSpawnPoint.GetChild(0).GetComponent<SpriteRenderer>().color.a,
                Is.EqualTo(1f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator Battle_ExactPointBudget_ShowsZeroRemainingPoints()
        {
            codeInput.text = "public class Mago {}";
            battleButton.onClick.Invoke();
            yield return null;
            nextBattleButton.onClick.Invoke();
            yield return null;

            codeInput.text =
                "public class Mago { private int vida; private int dano; " +
                "private int alcance; private int iniciativa; private int velocidadeAtaque; }";
            battleButton.onClick.Invoke();
            yield return null;
            nextBattleButton.onClick.Invoke();
            yield return null;

            codeInput.text =
                "public class Mago { private int vida; private int dano; " +
                "private int alcance; private int iniciativa; private int velocidadeAtaque; " +
                "public Mago(int dano, int vida, int alcance, int iniciativa, int velocidadeAtaque) { " +
                "this.vida = vida; this.dano = dano; this.alcance = alcance; " +
                "this.iniciativa = iniciativa; this.velocidadeAtaque = velocidadeAtaque; } } " +
                "Mago heroi = new Mago(5, 5, 5, 5, 5);";
            battleButton.onClick.Invoke();
            yield return null;

            TMP_Text stats = FindSceneComponent<TMP_Text>("MagoStatsText");
            Assert.That(stats.text, Does.Contain("Pontos restantes: 0"));
        }

        [UnityTest]
        public IEnumerator PhaseOne_ThreeValidatedTasks_EndWithSavedCompletionAndMappedMago()
        {
            codeInput.text = "public class Bruxo {}";
            battleButton.onClick.Invoke();
            yield return null;
            Assert.That(hintText.text, Does.StartWith("DICA"));

            codeInput.text = "public class Mago {}";
            battleButton.onClick.Invoke();
            yield return null;
            nextBattleButton.onClick.Invoke();
            yield return null;

            codeInput.text =
                "public class Mago { private int vida; private int dano; " +
                "private int alcance; private int iniciativa; private int velocidadeAtaque; }";
            battleButton.onClick.Invoke();
            yield return null;
            nextBattleButton.onClick.Invoke();
            yield return null;

            const string approvedCode =
                "public class Mago { private int vida; private int dano; " +
                "private int alcance; private int iniciativa; private int velocidadeAtaque; " +
                "public Mago(int vida, int dano, int alcance, int iniciativa, int velocidadeAtaque) { " +
                "this.vida = vida; this.dano = dano; this.alcance = alcance; " +
                "this.iniciativa = iniciativa; this.velocidadeAtaque = velocidadeAtaque; } } " +
                "Mago heroi = new Mago(5, 7, 3, 4, 2);";
            codeInput.text = approvedCode;
            battleButton.onClick.Invoke();
            yield return null;

            Assert.That(FindSceneComponent<TMP_Text>("VictoryTitleText").text,
                Does.Contain("Fase 1 concluída"));
            Assert.That(nextBattleButton.interactable, Is.False);
            Assert.That(FindSceneComponent<TMP_Text>("MagoStatsText").text,
                Does.Contain("Vida: 5"));
            Assert.That(File.Exists(progressSavePath), Is.True);

            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            yield return null;

            Assert.That(FindSceneComponent<TMP_InputField>("CodeInput").text,
                Is.EqualTo(approvedCode));
            Assert.That(FindSceneObject("VictoryOverlay").activeSelf, Is.True);
            Assert.That(FindSceneComponent<TMP_Text>("VictoryTitleText").text,
                Does.Contain("Fase 1 concluída"));
            Assert.That(FindSceneComponent<TMP_Text>("MagoStatsText").text,
                Does.Contain("Pontos restantes: 4"));
        }

        [UnityTest]
        public IEnumerator PhaseOne_ReloadAfterFirstVictory_ResumesAttributesWithCode()
        {
            codeInput.text = "public class Mago {}";
            battleButton.onClick.Invoke();
            yield return null;

            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            yield return null;

            Assert.That(FindSceneComponent<TMP_Text>("Title").text,
                Is.EqualTo("Estado protegido"));
            Assert.That(FindSceneComponent<TMP_InputField>("CodeInput").text,
                Is.EqualTo("public class Mago {}"));
            Assert.That(FindSceneObject("VictoryOverlay").activeSelf, Is.False);
            Assert.That(FindSceneObject("WizardSpawnPoint").transform.childCount,
                Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator PhaseOne_ReloadAfterRestart_KeepsArenaClearedAndEditorCode()
        {
            codeInput.text = "public class Mago {}";
            battleButton.onClick.Invoke();
            yield return null;
            nextBattleButton.onClick.Invoke();
            yield return null;

            var field = typeof(GameplayBootstrapper).GetField(
                "learningFlowPresenter",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            object flow = field.GetValue(bootstrapper);
            bool restarted = (bool)flow.GetType().GetMethod("RestartCurrentBattle")
                .Invoke(flow, null);
            Assert.That(restarted, Is.True);
            Assert.That(wizardSpawnPoint.GetChild(0).gameObject.activeSelf, Is.False);

            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            yield return null;

            Assert.That(FindSceneComponent<TMP_InputField>("CodeInput").text,
                Is.EqualTo("public class Mago {}"));
            Transform restoredSpawn = FindSceneObject("WizardSpawnPoint").transform;
            Assert.That(restoredSpawn.childCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator Edit_InvalidCodeAfterApprovedClass_KeepsSilhouetteVisible()
        {
            codeInput.text = "public class Mago {}";
            battleButton.onClick.Invoke();
            yield return null;
            nextBattleButton.onClick.Invoke();
            yield return null;

            codeInput.text = "public class Bruxo {}";
            yield return null;

            AssertWizardAtSpawn(active: true);
        }

        [UnityTest]
        public IEnumerator Battle_InvalidDeclarationAfterSuccess_ShowsErrorAndPreservesApprovedSilhouette()
        {
            codeInput.text = "public class Mago {}";
            battleButton.onClick.Invoke();
            yield return null;
            codeInput.text = "public class Bruxo {}";

            battleButton.onClick.Invoke();
            yield return null;

            Assert.That(feedbackText.text, Does.Contain("CLASS001"));
            AssertWizardAtSpawn(active: true);
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
