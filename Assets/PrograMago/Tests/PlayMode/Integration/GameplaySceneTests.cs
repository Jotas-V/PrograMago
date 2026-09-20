using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using PrograMago.Domain;
using PrograMago.UnityIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
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

            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
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
        public IEnumerator CombatControls_ArePreparedWithoutChangingTheFirstPhase()
        {
            GameObject actionPanel = FindSceneObject("CombatActionPanel");
            Button pauseButton = FindSceneComponent<Button>("PauseCombatButton");
            GameObject defeatOverlay = FindSceneObject("CombatDefeatOverlay");

            Assert.That(actionPanel.activeSelf, Is.False);
            Assert.That(pauseButton.gameObject.activeSelf, Is.False);
            Assert.That(defeatOverlay.activeSelf, Is.False);
            Assert.That(codeInput.interactable, Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CombatControls_AttackUpdatesVisibleLifeAndBlocksCodeEditing()
        {
            PrepareCombatMago(1, 3, 15, 5, 1);
            bootstrapper.ShowEnemies(new[]
            {
                new EnemyState("boneco", "Boneco de Treinamento", 10, "neutro")
            });
            bootstrapper.StartCombat();
            bootstrapper.AdvanceCombatTick();

            Assert.That(codeInput.interactable, Is.False);
            Assert.That(FindSceneObject("CombatActionPanel").activeSelf, Is.True);
            Assert.That(FindSceneComponent<TMP_Text>("EnemyStatsText").text,
                Does.Contain("7/10"));
            Assert.That(FindSceneComponent<TMP_Text>("CombatStatusText").text,
                Does.Contain("3 de neutro"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator CombatControls_ElementalWeaknessChangesDamageAndFeedbackColor()
        {
            PrepareCombatMago(1, 3, 15, 5, 1);
            bootstrapper.ShowEnemies(new[]
            {
                new EnemyState("golem", "Golem de Gelo", 12, "gelo")
            });
            bootstrapper.StartCombat(CombatElement.Fire);
            bootstrapper.AdvanceCombatTick();

            Assert.That(FindSceneComponent<TMP_Text>("EnemyStatsText").text,
                Does.Contain("6/12"));
            TMP_Text status = FindSceneComponent<TMP_Text>("CombatStatusText");
            Assert.That(status.text, Does.Contain("6 de fogo"));
            Assert.That(status.color.r, Is.GreaterThan(status.color.b));
            yield return null;
        }

        [UnityTest]
        public IEnumerator CombatControls_PauseAndDragReordersTheNextCycle()
        {
            PrepareCombatMago(1, 3, 15, 5, 1);
            bootstrapper.ShowEnemies(new[]
            {
                new EnemyState("golem", "Golem de Gelo", 12, "gelo")
            });
            bootstrapper.StartCombat();
            FindSceneComponent<Button>("PauseCombatButton").onClick.Invoke();
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute<IBeginDragHandler>(FindSceneObject("CombatAction3"),
                pointer, ExecuteEvents.beginDragHandler);
            pointer.position += new Vector2(80, 0);
            ExecuteEvents.Execute<IDragHandler>(FindSceneObject("CombatAction3"), pointer, ExecuteEvents.dragHandler);
            Assert.That(FindSceneComponent<CanvasGroup>("CombatAction3").blocksRaycasts, Is.False);
            ExecuteEvents.Execute<IDropHandler>(FindSceneObject("CombatAction1"),
                pointer, ExecuteEvents.dropHandler);
            ExecuteEvents.Execute<IEndDragHandler>(FindSceneObject("CombatAction3"), pointer, ExecuteEvents.endDragHandler);
            Assert.That(FindSceneComponent<CanvasGroup>("CombatAction3").blocksRaycasts, Is.True);

            Assert.That(FindSceneComponent<TMP_Text>("CombatAction3Label").text,
                Does.Contain("Atacar"));
            FindSceneComponent<Button>("PauseCombatButton").onClick.Invoke();
            bootstrapper.AdvanceCombatTick();
            Assert.That(FindSceneComponent<TMP_Text>("CombatStatusText").text,
                Does.Contain("sem analisar"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator CombatControls_DefeatOffersARestartWithFullLife()
        {
            PrepareCombatMago(1, 1, 1, 1, 1);
            bootstrapper.ShowEnemies(new[]
            {
                new EnemyState("golem", "Golem de Gelo", 12, "gelo")
            });
            bootstrapper.StartCombat();
            FindSceneComponent<Button>("PauseCombatButton").onClick.Invoke();
            bootstrapper.ReorderCombatAction(2, 0);
            FindSceneComponent<Button>("PauseCombatButton").onClick.Invoke();
            for (int tick = 0; tick < 300 &&
                !FindSceneObject("CombatDefeatOverlay").activeSelf; tick++)
                bootstrapper.AdvanceCombatTick();

            Assert.That(FindSceneObject("CombatDefeatOverlay").activeSelf, Is.True);
            FindSceneComponent<Button>("RetryCombatButton").onClick.Invoke();
            Assert.That(FindSceneObject("CombatDefeatOverlay").activeSelf, Is.False);
            Assert.That(FindSceneComponent<TMP_Text>("MagoStatsText").text,
                Does.Contain("Vida: 1/1"));
            Assert.That(FindSceneComponent<TMP_Text>("CombatAction3Label").text,
                Does.Contain("Atacar"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator Timeline_DefinitionsCanBeDraggedAcrossActionBlocksAndPersist()
        {
            codeInput.text = "public class Mago {}";
            PrepareCombatMago(4, 3, 15, 1, 2);
            bootstrapper.ShowEnemies(new[] { new EnemyState("boneco", "Boneco de Treinamento", 10, "neutro") });
            bootstrapper.StartCombat();
            bootstrapper.ToggleCombatPause();
            GameObject source = FindSceneObject("CodeBlockButton1");
            GameObject target = FindSceneObject("CombatAction3");
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute<IBeginDragHandler>(source, pointer, ExecuteEvents.beginDragHandler);
            Assert.That(pointer.pointerDrag, Is.EqualTo(source), "Todos os blocos precisam ser arrastáveis.");
            ExecuteEvents.Execute<IDropHandler>(target, pointer, ExecuteEvents.dropHandler);
            ExecuteEvents.Execute<IEndDragHandler>(source, pointer, ExecuteEvents.endDragHandler);
            Assert.That(source.transform.position.x, Is.GreaterThan(target.transform.position.x));
            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
            yield return null;
            Assert.That(FindSceneComponent<TMP_Text>("CodeBlockButton1Label").text, Does.StartWith("6"));
            FindSceneComponent<Button>("CodeBlockButton1").onClick.Invoke();
            Assert.That(FindSceneComponent<TMP_InputField>("CodeInput").text, Is.EqualTo("public class Mago {}"));
        }

        [UnityTest]
        public IEnumerator Arena_ShowsSixteenCellsAndCharactersStandOnTheSameGround()
        {
            PrepareCombatMago(4, 3, 15, 1, 2);
            bootstrapper.ShowEnemies(new[] { new EnemyState("boneco", "Boneco de Treinamento", 10, "neutro") });
            bootstrapper.StartCombat();
            bootstrapper.ToggleCombatPause();
            yield return null;
            Assert.That(FindSceneObject("ArenaCells").transform.childCount, Is.EqualTo(16));
            SpriteRenderer wizard = wizardSpawnPoint.GetChild(0).GetComponentInChildren<SpriteRenderer>();
            SpriteRenderer dummy = FindSceneComponent<SpriteRenderer>("EnemyMarker-boneco");
            Assert.That(wizard.bounds.min.y, Is.EqualTo(dummy.bounds.min.y).Within(0.02f));
            Assert.That(arenaCamera.WorldToViewportPoint(wizard.bounds.center).x, Is.LessThan(0.08f));
            Assert.That(arenaCamera.WorldToViewportPoint(dummy.bounds.center).x, Is.GreaterThan(0.92f));
        }
        private void PrepareCombatMago(int life, int damage, int range,
            int initiative, int attackSpeed)
        {
            var values = new Dictionary<string, int>
            {
                ["vida"] = life,
                ["dano"] = damage,
                ["alcance"] = range,
                ["iniciativa"] = initiative,
                ["velocidadeAtaque"] = attackSpeed
            };
            bootstrapper.ShowMago(MagoState.FromValidatedProgram(new ValidatedMagoProgram(
                "Mago", values.Keys, "heroi", values, 25)));
        }

        [UnityTest]
        public IEnumerator Timeline_EditAfterDefeatKeepsActionsAvailableAndRemovesProjectiles()
        {
            PrepareCombatMago(1, 1, 1, 1, 1);
            bootstrapper.ShowEnemies(new[] { new EnemyState("golem", "Golem de Gelo", 12, "gelo") });
            bootstrapper.StartCombat();
            for (int tick = 0; tick < 300 && !FindSceneObject("CombatDefeatOverlay").activeSelf; tick++)
                bootstrapper.AdvanceCombatTick();
            FindSceneComponent<Button>("EditAfterDefeatButton").onClick.Invoke();
            yield return null;
            Assert.That(FindSceneObject("CombatActionPanel").activeSelf, Is.True);
            Assert.That(codeInput.interactable, Is.True);
            Assert.That(Object.FindObjectsByType<CombatProjectileView>(FindObjectsSortMode.None), Is.Empty);
        }

        [UnityTest]
        public IEnumerator Timeline_VariableBlocksPersistWithoutLeakingIntoDefinitions()
        {
            codeInput.text = "public class Mago {}";
            PrepareCombatMago(1, 3, 15, 5, 1);
            bootstrapper.ShowEnemies(new[] { new EnemyState("boneco", "Boneco de Treinamento", 10, "neutro") });
            bootstrapper.StartCombat();
            bootstrapper.Reset();
            FindSceneComponent<Button>("AddCombatBlockButton").onClick.Invoke();
            codeInput.text = "analisarAlvo(); selecionarMagia(); lancarMagia();";
            bootstrapper.ReorderCombatAction(3, 0);
            Assert.That(bootstrapper.SourceCode, Is.EqualTo("public class Mago {}"));
            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
            yield return null;
            PhaseOneSaveData saved = JsonUtility.FromJson<PhaseOneSaveData>(File.ReadAllText(progressSavePath));
            Assert.That(saved.combatBlocks, Has.Length.EqualTo(4));
            Assert.That(saved.combatBlocks[3], Does.StartWith("analisarAlvo(); selecionarMagia();"));
            Assert.That(FindSceneComponent<TMP_Text>("CombatAction4Label").text, Does.Contain("3 comandos"));
            Assert.That(FindSceneComponent<TMP_InputField>("CodeInput").text, Is.EqualTo("public class Mago {}"));
        }

        [UnityTest]
        public IEnumerator Timeline_StaysInFooterAndSelectingArrowShowsItsCode()
        {
            PrepareCombatMago(1, 3, 15, 5, 1);
            bootstrapper.ShowEnemies(new[] { new EnemyState("boneco", "Boneco de Treinamento", 10, "neutro") });
            RectTransform editor = FindSceneComponent<RectTransform>("CodeEditorPanel");
            float width = editor.anchorMax.x;
            bootstrapper.StartCombat();
            Assert.That(editor.anchorMax.x, Is.EqualTo(width));
            Assert.That(FindSceneObject("CombatActionPanel").transform.IsChildOf(editor), Is.True);
            FindSceneComponent<Button>("CombatAction3").onClick.Invoke();
            Assert.That(codeInput.text, Does.Contain("lancarMagia();"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator CombatProjectile_HitProducesVisibleTravelAndPauseFreezesIt()
        {
            PrepareCombatMago(1, 3, 15, 5, 1);
            bootstrapper.ShowEnemies(new[] { new EnemyState("boneco", "Boneco de Treinamento", 10, "neutro") });
            bootstrapper.StartCombat();
            bootstrapper.AdvanceCombatTick();
            yield return new WaitForSecondsRealtime(0.1f);
            GameObject projectile = GameObject.Find("NeutralProjectile(Clone)");
            Assert.That(projectile, Is.Not.Null);
            bootstrapper.ToggleCombatPause();
            Vector3 before = projectile.transform.position;
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.That(projectile.transform.position, Is.EqualTo(before));
            bootstrapper.ToggleCombatPause();
            yield return new WaitForSecondsRealtime(0.15f);
            Assert.That(projectile.transform.position, Is.Not.EqualTo(before));
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

            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
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
            System.Array.Resize(ref legacy.battleIds, 3);
            System.Array.Resize(ref legacy.attemptCounts, 3);
            System.Array.Resize(ref legacy.failedCounts, 3);
            System.Array.Resize(ref legacy.completed, 3);
            legacy.sourceBlocks = null;
            legacy.activeBlock = 0;
            File.WriteAllText(progressSavePath, JsonUtility.ToJson(legacy));

            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
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
            AssertWizardViewportPosition(new Vector2(0.03125f, 0.79f));

            arenaCamera.aspect = 0.75f;
            yield return null;

            AssertWizardViewportPosition(new Vector2(0.03125f, 0.79f));
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
            Assert.That(nextBattleButton.interactable, Is.True);
            Assert.That(FindSceneComponent<TMP_Text>("MagoStatsText").text,
                Does.Contain("Vida: 5"));
            Assert.That(File.Exists(progressSavePath), Is.True);

            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
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
        public IEnumerator FirstEnemyBattle_BonecoCanBeDefeatedAndReviewSurvivesReload()
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

            const string magoCode =
                "public class Mago { private int vida; private int dano; " +
                "private int alcance; private int iniciativa; private int velocidadeAtaque; " +
                "public Mago(int vida, int dano, int alcance, int iniciativa, int velocidadeAtaque) { " +
                "this.vida = vida; this.dano = dano; this.alcance = alcance; " +
                "this.iniciativa = iniciativa; this.velocidadeAtaque = velocidadeAtaque; } } " +
                "Mago heroi = new Mago(4, 3, 15, 1, 2);";
            codeInput.text = magoCode;
            battleButton.onClick.Invoke();
            yield return null;
            Assert.That(nextBattleButton.interactable, Is.True);
            nextBattleButton.onClick.Invoke();
            yield return null;

            Assert.That(titleText.text, Is.EqualTo("Surge um inimigo"));
            Assert.That(FindSceneComponent<TMP_Text>("TutorialStepText").text, Does.Contain("1/6"));
            FindSceneComponent<Button>("TutorialNextButton").onClick.Invoke();
            Assert.That(FindSceneComponent<TMP_Text>("TutorialBodyText").text, Does.Contain("public class Inimigo"));
            Assert.That(battleProgressText.text, Does.Contain("Batalha 4/8"));
            Assert.That(victoryOverlay.activeSelf, Is.False);
            const string enemyCode =
                "public class Inimigo { private String nome; private int vida; " +
                "private String elemento; public Inimigo(String nome, int vida, String elemento) { " +
                "this.nome = nome; this.vida = vida; this.elemento = elemento; } " +
                "public String getElemento() { return elemento; } } " +
                "Inimigo boneco = new Inimigo(\"Boneco de Treinamento\", 10, \"neutro\");";
            codeInput.text = magoCode + " " + enemyCode;
            battleButton.onClick.Invoke();
            yield return null;

            Assert.That(GameObject.Find("EnemyMarker-boneco"), Is.Not.Null);
            Assert.That(FindSceneObject("CombatActionPanel").activeSelf, Is.True);
            for (int tick = 0; tick < 200 && !victoryOverlay.activeSelf; tick++)
                bootstrapper.AdvanceCombatTick();
            yield return new WaitForSecondsRealtime(0.85f);
            Assert.That(victoryOverlay.activeSelf, Is.True);
            Assert.That(FindSceneComponent<TMP_Text>("VictoryTitleText").text,
                Is.EqualTo("Adversário derrotado!"));
            Assert.That(nextBattleButton.interactable, Is.False);

            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
            yield return null;
            Assert.That(FindSceneComponent<TMP_Text>("Title").text,
                Is.EqualTo("Surge um inimigo"));
            Assert.That(FindSceneObject("VictoryOverlay").activeSelf, Is.True);
            Assert.That(GameObject.Find("EnemyMarker-boneco"), Is.Not.Null);
            Assert.That(FindSceneComponent<TMP_InputField>("CodeInput").text,
                Is.EqualTo(magoCode + " " + enemyCode));
        }

        [UnityTest]
        public IEnumerator PhaseOne_ReloadAfterFirstVictory_PreservesReviewAndCanAdvance()
        {
            codeInput.text = "public class Mago {}";
            battleButton.onClick.Invoke();
            yield return null;

            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
            yield return null;

            Assert.That(FindSceneComponent<TMP_Text>("Title").text,
                Is.EqualTo("O nascimento do Mago"));
            Assert.That(FindSceneComponent<TMP_InputField>("CodeInput").text,
                Is.EqualTo("public class Mago {}"));
            Assert.That(FindSceneObject("VictoryOverlay").activeSelf, Is.True);
            Assert.That(FindSceneObject("WizardSpawnPoint").transform.childCount,
                Is.EqualTo(1));
            FindSceneComponent<Button>("NextBattleButton").onClick.Invoke();
            Assert.That(FindSceneComponent<TMP_Text>("Title").text,
                Is.EqualTo("Estado protegido"));
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

            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
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
            Assert.That(arenaCamera.WorldToViewportPoint(wizard.position).x, Is.LessThan(0.08f));
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
