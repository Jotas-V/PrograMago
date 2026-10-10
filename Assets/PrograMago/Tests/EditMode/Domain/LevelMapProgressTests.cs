using System;
using System.Reflection;
using NUnit.Framework;
using PrograMago.Domain;

namespace PrograMago.Tests.Domain
{
    public sealed class LevelMapProgressTests
    {
        private static LearningProgress CreateProgress()
        {
            var battles = new BattleDefinition[9];
            for (int i = 0; i < battles.Length; i++)
                battles[i] = new BattleDefinition("phase-" + i, 1, i + 1,
                    new BattleLessonContent("Fase " + (i + 1), "Conceito", "O que", "Uso", "Como", "Efeito", "Tarefa"),
                    new[] { "Dica" }, ValidationCriterion.DeclareMagoClass,
                    new BattleVictoryContent("Vitória", "Conquista", "Revisão"));
            return new LearningProgress(new LearningPath(battles));
        }

        private static bool Call(LearningProgress progress, string method, int index)
        {
            MethodInfo member = typeof(LearningProgress).GetMethod(method);
            Assert.That(member, Is.Not.Null, "O progresso precisa oferecer " + method + " para o mapa.");
            return (bool)member.Invoke(progress, new object[] { index });
        }

        private static void Win(LearningProgress progress)
        {
            progress.RegisterSubmission();
            Assert.That(progress.TryStartBattle(progress.CurrentBattle.Criterion), Is.True);
            Assert.That(progress.ReportVictory(), Is.True);
        }

        [Test]
        public void LevelMap_NewJourneyOnlyAllowsFirstPhase()
        {
            var progress = CreateProgress();
            Assert.That(Call(progress, "CanSelectBattle", 0), Is.True);
            for (int i = 1; i < 9; i++) Assert.That(Call(progress, "TrySelectBattle", i), Is.False);
            Assert.That(progress.CurrentBattleIndex, Is.Zero);
        }

        [Test]
        public void LevelMap_VictoryOnlyAllowsNextPhaseUntilJourneyEnds()
        {
            var progress = CreateProgress();
            Win(progress);
            Assert.That(Call(progress, "CanSelectBattle", 0), Is.False);
            Assert.That(Call(progress, "CanSelectBattle", 1), Is.True);
            Assert.That(Call(progress, "TrySelectBattle", 2), Is.False);
            Assert.That(Call(progress, "TrySelectBattle", 1), Is.True);
            Assert.That(progress.Stage, Is.EqualTo(LearningStage.Editing));
            Assert.That(progress.IsCompleted("phase-0"), Is.True);
        }

        [Test]
        public void LevelMap_ActiveBattleCannotNavigate()
        {
            var progress = CreateProgress();
            progress.RegisterSubmission();
            progress.TryStartBattle(progress.CurrentBattle.Criterion);
            Assert.That(Call(progress, "TrySelectBattle", 0), Is.False);
            Assert.That(progress.Stage, Is.EqualTo(LearningStage.BattleInProgress));
        }

        [Test]
        public void LevelMap_ReplayAndReloadPreserveAllCompletions()
        {
            var progress = CreateProgress();
            for (int i = 0; i < 9; i++) { Win(progress); if (i < 8) progress.ContinueAfterVictory(); }
            Assert.That(Call(progress, "TrySelectBattle", 2), Is.True);
            var save = progress.CapturePhaseOne("draft", "approved", new[] { "draft", "enemy", "strategy" }, 2);
            var restored = CreateProgress();
            Assert.DoesNotThrow(() => restored.RestorePhaseOne(save));
            Assert.That(restored.CurrentBattleIndex, Is.EqualTo(2));
            for (int i = 0; i < 9; i++) {
                Assert.That(Call(restored, "CanSelectBattle", i), Is.True);
                Assert.That(restored.IsCompleted("phase-" + i), Is.True);
            }
            Win(restored);
            var review = CreateProgress();
            Assert.DoesNotThrow(() => review.RestorePhaseOne(restored.CapturePhaseOne("draft", "approved")));
            Assert.That(Call(review, "TrySelectBattle", 8), Is.True);
        }

        [Test]
        public void LevelMap_InvalidIndicesAndDefeatNeverUnlockFuturePhases()
        {
            var progress = CreateProgress();
            Assert.That(Call(progress, "TrySelectBattle", -1), Is.False);
            Assert.That(Call(progress, "TrySelectBattle", 9), Is.False);
            progress.RegisterSubmission();
            progress.TryStartBattle(progress.CurrentBattle.Criterion);
            progress.ReportDefeat();
            Assert.That(Call(progress, "CanSelectBattle", 0), Is.True);
            Assert.That(Call(progress, "CanSelectBattle", 1), Is.False);
        }
    }
}
