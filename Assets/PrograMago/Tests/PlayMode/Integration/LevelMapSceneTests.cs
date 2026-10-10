using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public sealed class LevelMapSceneTests
    {
        private GameplayBootstrapper game;
        private string savePath;
        private byte[] previousSave;
        private object Map => Property(game, "LevelMap");
        private LearningProgress Progress => (LearningProgress)Property(typeof(GameplayBootstrapper)
            .GetField("learningFlowPresenter", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game), "Progress");

        private static object Property(object item, string name)
        {
            var property = item.GetType().GetProperty(name);
            Assert.That(property, Is.Not.Null, "Mapa precisa expor " + name);
            return property.GetValue(item);
        }
        private static object Call(object item, string name, params object[] args)
        {
            var method = item.GetType().GetMethod(name);
            Assert.That(method, Is.Not.Null, "Mapa precisa implementar " + name);
            return method.Invoke(item, args);
        }

        private IEnumerator OpenMap()
        {
            Assert.That((bool)Call(game, "ToggleLevelMap"), Is.True);
            for (int i = 0; i < 60 && (Map == null || !(bool)Property(Map, "IsVisible")); i++) yield return null;
            Assert.That(Map, Is.Not.Null);
            Assert.That(SceneManager.GetSceneByName("MapScene").isLoaded, Is.True);
            Assert.That(SceneManager.GetSceneByName("MainScene").isLoaded, Is.True);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MapScene"));
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            savePath = Path.Combine(UnityEngine.Application.persistentDataPath, "programago-phase1.json");
            previousSave = File.Exists(savePath) ? File.ReadAllBytes(savePath) : null;
            if (File.Exists(savePath)) File.Delete(savePath);
            SceneManager.LoadScene("MainScene");
            yield return null;
            game = UnityEngine.Object.FindFirstObjectByType<GameplayBootstrapper>();
            Assert.That(game, Is.Not.Null);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            game = UnityEngine.Object.FindFirstObjectByType<GameplayBootstrapper>();
            if (game != null) UnityEngine.Object.Destroy(game.gameObject);
            yield return null;
            if (SceneManager.GetSceneByName("MapScene").isLoaded)
                yield return SceneManager.UnloadSceneAsync("MapScene");
            if (previousSave == null) { if (File.Exists(savePath)) File.Delete(savePath); }
            else File.WriteAllBytes(savePath, previousSave);
        }

        [UnityTest]
        public IEnumerator LevelMap_OpeningAndClosingPreservesDraftAndCurrentPhase()
        {
            var input = GameObject.Find("CodeInput").GetComponent<TMP_InputField>();
            input.text = "public class Mago { // meu rascunho";
            yield return OpenMap();
            Assert.That((bool)Property(Map, "IsVisible"), Is.True);
            Assert.That((bool)Call(game, "ToggleLevelMap"), Is.True);
            Assert.That(input.text, Does.Contain("meu rascunho"));
            Assert.That(Progress.CurrentBattleIndex, Is.Zero);
            yield return null;
        }

        [UnityTest]
        public IEnumerator LevelMap_ArrivalAutomaticallyEntersPhaseAndIgnoresRepeatedClicks()
        {
            Progress.RegisterSubmission();
            Progress.TryStartBattle(Progress.CurrentBattle.Criterion);
            Progress.ReportVictory();
            yield return OpenMap();
            Assert.That((bool)Call(Map, "SelectPhase", 1), Is.True);
            Assert.That((bool)Call(Map, "SelectPhase", 1), Is.False);
            Assert.That(Progress.CurrentBattleIndex, Is.Zero, "Não entrar antes de terminar a caminhada.");
            Call(Map, "AdvanceTravel", 0.05f);
            Assert.That((bool)Property(Map, "IsTravelling"), Is.True);
            Call(Map, "AdvanceTravel", 100f);
            Assert.That(Progress.CurrentBattleIndex, Is.EqualTo(1));
            Assert.That((bool)Property(Map, "IsVisible"), Is.False);
            Assert.That(Progress.IsCompleted(Progress.Path.Battles[0].Id), Is.True);
            Assert.That((new PhaseOneSaveStore(savePath)).Load().currentBattleIndex, Is.EqualTo(1));
            yield return null;
        }

        [UnityTest]
        public IEnumerator LevelMap_BlockedPhaseDoesNotMoveWizard()
        {
            yield return OpenMap();
            var before = (Vector2)Property(Map, "WizardPosition");
            Assert.That((bool)Call(Map, "SelectPhase", 8), Is.False);
            Call(Map, "AdvanceTravel", 100f);
            Assert.That((Vector2)Property(Map, "WizardPosition"), Is.EqualTo(before));
            Assert.That(Progress.CurrentBattleIndex, Is.Zero);
            yield return null;
        }

        [UnityTest]
        public IEnumerator LevelMap_ShortcutDoesNotInterruptCodeTyping()
        {
            var input = GameObject.Find("CodeInput").GetComponent<TMP_InputField>();
            input.ActivateInputField();
            yield return null;
            Assert.That(input.isFocused, Is.True);
            Assert.That((bool)Call(game, "HandleMapShortcut", true), Is.False);
            Assert.That((bool)Property(game, "IsMapActive"), Is.False);
            input.DeactivateInputField();
            yield return null;
            Assert.That((bool)Call(game, "HandleMapShortcut", true), Is.True);
            for (int i = 0; i < 60 && (Map == null || !(bool)Property(Map, "IsVisible")); i++) yield return null;
            Assert.That((bool)Property(Map, "IsVisible"), Is.True);
        }

        [UnityTest]
        public IEnumerator LevelMap_ActiveCombatOnlyAllowsViewingAndClosingMap()
        {
            Progress.RegisterSubmission();
            Progress.TryStartBattle(Progress.CurrentBattle.Criterion);
            yield return OpenMap();
            Assert.That((bool)Call(Map, "SelectPhase", 0), Is.False);
            Assert.That((bool)Call(game, "ToggleLevelMap"), Is.True);
            Assert.That(Progress.Stage, Is.EqualTo(LearningStage.BattleInProgress));
            yield return null;
        }

        [UnityTest]
        public IEnumerator LevelMap_ReplayCameraStaysInsideMapAndKeepsProgress()
        {
            for (int i = 0; i < Progress.Path.Battles.Count; i++) {
                Progress.RegisterSubmission(); Progress.TryStartBattle(Progress.CurrentBattle.Criterion);
                Progress.ReportVictory(); if (i < Progress.Path.Battles.Count - 1) Progress.ContinueAfterVictory();
            }
            yield return OpenMap();
            Assert.That((bool)Call(Map, "SelectPhase", 0), Is.True);
            Call(Map, "AdvanceTravel", 100f);
            Assert.That(Progress.CurrentBattleIndex, Is.Zero);
            Assert.That(Progress.HasCompletedJourney, Is.True);
            for (int i = 0; i < 60 && (bool)Property(game, "IsMapActive"); i++) yield return null;
            yield return OpenMap();
            Assert.That((bool)Property(Map, "CameraWithinBounds"), Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator LevelMap_StandaloneEntryUpgradesOlderProgressWithoutLosingCode()
        {
            var legacy = Progress.CapturePhaseOne("legacy draft", "", new[] { "legacy draft", "enemy draft", "" }, 1);
            legacy.battleIds = legacy.battleIds.Take(6).ToArray();
            legacy.completed = Enumerable.Repeat(true, 6).ToArray();
            legacy.attemptCounts = Enumerable.Repeat(1, 6).ToArray();
            legacy.failedCounts = new int[6];
            legacy.currentBattleIndex = 5; legacy.stage = LearningStage.VictoryReview;
            UnityEngine.Object.Destroy(game.gameObject);
            yield return null;
            new PhaseOneSaveStore(savePath).Save(legacy);
            SceneManager.LoadScene("MapScene");
            yield return null;
            var map = UnityEngine.Object.FindFirstObjectByType<LevelMapView>();
            for (int i = 0; i < 60 && !map.IsVisible; i++) yield return null;
            Assert.That(map.SelectPhase(6), Is.True);
            map.AdvanceTravel(100f);
            yield return null;
            game = UnityEngine.Object.FindFirstObjectByType<GameplayBootstrapper>();
            Assert.That(game, Is.Not.Null);
            Assert.That(Progress.CurrentBattleIndex, Is.EqualTo(6));
            var saved = new PhaseOneSaveStore(savePath).Load();
            Assert.That(saved.battleIds, Has.Length.EqualTo(9));
            Assert.That(saved.sourceBlocks[0], Is.EqualTo("legacy draft"));
            Assert.That(saved.sourceBlocks[1], Is.EqualTo("enemy draft"));
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MainScene"));
        }

        private void PrepareCombat(int life, string enemyName, string element)
        {
            var values = new Dictionary<string, int> { ["vida"] = life, ["dano"] = 1, ["alcance"] = 1,
                ["iniciativa"] = 1, ["velocidadeAtaque"] = 1 };
            game.ShowMago(MagoState.FromValidatedProgram(new ValidatedMagoProgram("Mago", values.Keys, "heroi", values, 25)));
            game.ShowEnemies(new[] { new EnemyState("alvo", enemyName, 12, element) });
            Progress.RegisterSubmission(); Progress.TryStartBattle(Progress.CurrentBattle.Criterion);
            game.StartCombat();
        }

        [UnityTest]
        public IEnumerator LevelMap_OpeningPausesRealCombatAndClosingResumesSameEngine()
        {
            PrepareCombat(5, "Boneco de Treinamento", "neutro");
            var engine = (CombatEngine)typeof(GameplayBootstrapper).GetField("combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game);
            yield return OpenMap();
            Assert.That(engine.IsPaused, Is.True, "A apresentação também deve pausar, além dos ticks.");
            int position = engine.WizardPosition;
            yield return new WaitForSecondsRealtime(0.3f);
            Assert.That(engine.WizardPosition, Is.EqualTo(position));
            Assert.That((bool)Call(game, "ToggleLevelMap"), Is.True);
            for (int i = 0; i < 60 && game.IsMapActive; i++) yield return null;
            Assert.That(engine.IsPaused, Is.False);
            Assert.That(typeof(GameplayBootstrapper).GetField("combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game), Is.SameAs(engine));
        }

        [UnityTest]
        public IEnumerator LevelMap_DefeatAllowsRetryOfCurrentPhaseWithoutUnlockingNext()
        {
            PrepareCombat(1, "Golem de Gelo", "gelo");
            var engine = (CombatEngine)typeof(GameplayBootstrapper).GetField("combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game);
            for (int i = 0; i < 300 && engine.Outcome == CombatOutcome.InProgress; i++) game.AdvanceCombatTick();
            Assert.That(engine.Outcome, Is.EqualTo(CombatOutcome.Defeat));
            yield return OpenMap();
            Assert.That((bool)Call(Map, "SelectPhase", 1), Is.False);
            Assert.That((bool)Call(Map, "SelectPhase", 0), Is.True);
            Call(Map, "AdvanceTravel", 100f);
            Assert.That(Progress.Stage, Is.EqualTo(LearningStage.Editing));
            Assert.That(Progress.CurrentBattleIndex, Is.Zero);
            yield return null;
        }
    }
}
