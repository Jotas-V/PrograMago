using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using PrograMago.Domain;
using PrograMago.UnityIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PrograMago.Tests.Integration
{
    public sealed class GrimoireSceneTests
    {
        private GameplayBootstrapper game;
        private string savePath;
        private byte[] previousSave;
        private object Call(object item, string method, params object[] args)
        {
            var member = item.GetType().GetMethod(method);
            Assert.That(member, Is.Not.Null, "Grimório precisa de " + method);
            return member.Invoke(item, args);
        }
        private LearningProgress Progress => (LearningProgress)typeof(GameplayBootstrapper)
            .GetField("learningFlowPresenter", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game)
            .GetType().GetProperty("Progress").GetValue(typeof(GameplayBootstrapper)
            .GetField("learningFlowPresenter", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game));
        private object View => game.GetType().GetProperty("Grimoire")?.GetValue(game);
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            savePath = Path.Combine(UnityEngine.Application.persistentDataPath, "programago-phase1.json");
            previousSave = File.Exists(savePath) ? File.ReadAllBytes(savePath) : null;
            if (File.Exists(savePath)) File.Delete(savePath);
            SceneManager.LoadScene("MainScene"); yield return null;
            game = UnityEngine.Object.FindFirstObjectByType<GameplayBootstrapper>();
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (game != null) UnityEngine.Object.Destroy(game.gameObject);
            yield return null;
            if (SceneManager.GetSceneByName("GrimoireScene").isLoaded)
                yield return SceneManager.UnloadSceneAsync("GrimoireScene");
            if (previousSave == null) { if (File.Exists(savePath)) File.Delete(savePath); }
            else File.WriteAllBytes(savePath, previousSave);
        }
        private IEnumerator Open()
        {
            Assert.That(Call(game, "OpenGrimoire"), Is.True);
            for (int i = 0; i < 120 && View == null; i++) yield return null;
            Assert.That(View, Is.Not.Null);
        }
        [UnityTest]
        public IEnumerator Polish_BookKeepsSpriteProportionsAndLockedPageHasDrawnRunes()
        {
            yield return Open(); yield return new WaitForSecondsRealtime(.4f);
            var image = GameObject.Find("OpenBook").GetComponent<UnityEngine.UI.Image>();
            Assert.That(image.rectTransform.rect.width / image.rectTransform.rect.height,
                Is.EqualTo(image.sprite.rect.width / image.sprite.rect.height).Within(.02f));
            Call(View, "SelectPage", 1);
            Assert.That(GameObject.Find("LeftLockedRunes"), Is.Not.Null);
            Assert.That(GameObject.Find("GrimoireLeftText").GetComponent<TMP_Text>().text, Does.Not.Contain("+ : /"));
        }

        [UnityTest]
        public IEnumerator Polish_SpectralWizardStaysAboveTheGround()
        {
            game.SetClassDeclared(true); yield return null;
            var actor = (GameObject)typeof(GameplayBootstrapper).GetField("wizardInstance", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game);
            var ground = (Vector3)typeof(GameplayBootstrapper).GetMethod("CellGroundPosition", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(game,new object[]{0});
            // The imported 1254 px artwork has 148 transparent pixels below the visible soles.
            var sprite = actor.GetComponent<SpriteRenderer>();
            for (int frame = 0; frame < 12; frame++) {
                yield return null;
                float visibleFeet = sprite.bounds.min.y + sprite.bounds.size.y * (148f / 1254f);
                Assert.That(visibleFeet, Is.EqualTo(ground.y).Within(.01f),
                    "Os pés visíveis devem encostar no chão, sem flutuar nem afundar.");
            }
        }

        [UnityTest]
        public IEnumerator Polish_ToolbarHasLeftPaddingAndPauseDoesNotOverlapMap()
        {
            yield return null; Canvas.ForceUpdateCanvases();
            var strip = GameObject.Find("DefinitionBlocks").GetComponent<RectTransform>();
            var button = GameObject.Find("CodeBlockButton1").GetComponent<RectTransform>();
            var a = new Vector3[4]; var b = new Vector3[4];
            strip.GetWorldCorners(a); button.GetWorldCorners(b);
            Assert.That(b[0].x - a[0].x, Is.GreaterThan(4));
            var pause = (UnityEngine.UI.Button)typeof(GameplayBootstrapper).GetField("pauseCombatButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game);
            pause.GetComponent<RectTransform>().GetWorldCorners(a);
            GameObject.Find("OpenLevelMapButton").GetComponent<RectTransform>().GetWorldCorners(b);
            Assert.That(a[2].x, Is.LessThanOrEqualTo(b[0].x));
        }

        [UnityTest]
        public IEnumerator Polish_NextPhaseOpensMapWithoutVictoryMapButton()
        {
            game.SourceCode = "public class Mago {}";
            GameObject.Find("BattleButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            yield return null;
            Assert.That(GameObject.Find("VictoryMapButton"), Is.Null);
            ((UnityEngine.UI.Button)typeof(GameplayBootstrapper).GetField("nextBattleButton",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(game)).onClick.Invoke();
            for(int i=0;i<120 && !SceneManager.GetSceneByName("MapScene").isLoaded;i++) yield return null;
            Assert.That(SceneManager.GetSceneByName("MapScene").isLoaded, Is.True);
            Assert.That(Progress.CurrentBattleIndex, Is.EqualTo(1));
            Assert.That(game.SourceCode, Does.Contain("class Mago"));
        }

        [UnityTest]
        public IEnumerator Book_ButtonDoesNotCoverCodeErrorFeedback()
        {
            game.SourceCode = "class errado";
            GameObject.Find("BattleButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            yield return null;
            Canvas.ForceUpdateCanvases();
            var feedback = GameObject.Find("FeedbackText").GetComponent<TMP_Text>();
            Assert.That(feedback.text, Is.Not.Empty);
            var feedbackCorners = new Vector3[4]; var buttonCorners = new Vector3[4];
            feedback.rectTransform.GetWorldCorners(feedbackCorners);
            GameObject.Find("OpenGrimoireButton").GetComponent<RectTransform>().GetWorldCorners(buttonCorners);
            Assert.That(feedbackCorners[1].y, Is.LessThanOrEqualTo(buttonCorners[0].y),
                "O botão do grimório não pode cobrir o diagnóstico do código.");
        }

        [UnityTest]
        public IEnumerator Book_PreservesDraftAndReturnsToSameGameplayScene()
        {
            var input = GameObject.Find("CodeInput").GetComponent<TMP_InputField>();
            input.text = "public class Mago { // rascunho";
            yield return Open();
            Assert.That(SceneManager.GetSceneByName("MainScene").isLoaded, Is.True);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("GrimoireScene"));
            Assert.That(Call(game, "ToggleLevelMap"), Is.False);
            Call(View, "RequestClose");
            for (int i = 0; i < 120 && SceneManager.GetSceneByName("GrimoireScene").isLoaded; i++) yield return null;
            Assert.That(input.text, Does.Contain("rascunho"));
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MainScene"));
            Assert.That(Progress.CurrentBattleIndex, Is.Zero);
        }
        [UnityTest]
        public IEnumerator Book_FuturePageUsesMagicWritingAndManualHintsDoNotFail()
        {
            yield return Open();
            Call(View, "SelectPage", 8);
            var left = GameObject.Find("GrimoireLeftText").GetComponent<TMP_Text>();
            Assert.That(left.text, Does.Contain("ainda não"));
            Assert.That(left.text, Does.Not.Contain("Polimorfismo"));
            Call(View, "SelectPage", 0); Call(View, "RevealHint");
            Assert.That(Progress.FailedAttempts, Is.Zero);
            Assert.That(Progress.GetRevealedHintCount(0), Is.EqualTo(1));
            Assert.That(new PhaseOneSaveStore(savePath).Load().revealedHintCounts[0], Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Book_PausesSameCombatAndPreservesAnExistingPause()
        {
            var values = new Dictionary<string, int> { ["vida"] = 5, ["dano"] = 1, ["alcance"] = 1,
                ["iniciativa"] = 1, ["velocidadeAtaque"] = 1 };
            game.ShowMago(MagoState.FromValidatedProgram(new ValidatedMagoProgram("Mago", values.Keys, "heroi", values, 25)));
            game.ShowEnemies(new[] { new EnemyState("boneco", "Boneco de Treinamento", 10, "neutro") });
            Progress.RegisterSubmission(); Progress.TryStartBattle(Progress.CurrentBattle.Criterion); game.StartCombat();
            var engine = (CombatEngine)typeof(GameplayBootstrapper).GetField("combat", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game);
            yield return Open(); Assert.That(engine.IsPaused, Is.True);
            Call(View, "RequestClose");
            for (int i=0; i<120 && (bool)game.GetType().GetProperty("IsGrimoireActive").GetValue(game); i++) yield return null;
            Assert.That(engine.IsPaused, Is.False);
            game.ToggleCombatPause();
            yield return Open(); Call(View, "RequestClose");
            for (int i=0; i<120 && (bool)game.GetType().GetProperty("IsGrimoireActive").GetValue(game); i++) yield return null;
            Assert.That(engine.IsPaused, Is.True);
            Assert.That(typeof(GameplayBootstrapper).GetField("combat", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game), Is.SameAs(engine));
            Assert.That(Progress.Stage, Is.EqualTo(LearningStage.BattleInProgress));
        }
    }
}
