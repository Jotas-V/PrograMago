using NUnit.Framework;
using PrograMago.Domain;

namespace PrograMago.Tests.Domain
{
    public sealed class GrimoireProgressTests
    {
        private static LearningProgress Create()
        {
            var battles = new BattleDefinition[9];
            for (int i = 0; i < 9; i++) battles[i] = new BattleDefinition("page-" + i, 1, i + 1,
                new BattleLessonContent("Título", "Conceito", "Explicação", "Uso", "Exemplo", "Efeito", "Tarefa"),
                new[] { "conceito", "local", "raciocínio", "conferência" }, ValidationCriterion.DeclareMagoClass,
                new BattleVictoryContent("Vitória", "Aprendeu", "Revisão"));
            return new LearningProgress(new LearningPath(battles));
        }
        private static object Call(LearningProgress progress, string method, params object[] args)
        {
            var member = typeof(LearningProgress).GetMethod(method);
            Assert.That(member, Is.Not.Null, "O grimório precisa de " + method);
            return member.Invoke(progress, args);
        }
        [Test]
        public void Pages_UnlockAfterPreviousVictoryAndRemainReadable()
        {
            var p = Create();
            Assert.That(Call(p, "CanReadGrimoirePage", 0), Is.True);
            Assert.That(Call(p, "CanReadGrimoirePage", 1), Is.False);
            p.RegisterSubmission(); p.TryStartBattle(p.CurrentBattle.Criterion); p.ReportVictory();
            Assert.That(Call(p, "CanReadGrimoirePage", 1), Is.True);
            Assert.That(Call(p, "CanReadGrimoirePage", 8), Is.False);
            p.ContinueAfterVictory();
            Assert.That(Call(p, "CanReadGrimoirePage", 0), Is.True);
            Assert.That(Call(p, "CanReadGrimoirePage", -1), Is.False);
            Assert.That(Call(p, "CanReadGrimoirePage", 9), Is.False);
        }
        [Test]
        public void ManualHints_PersistWithoutRecordingAttemptsOrFailures()
        {
            var p = Create();
            for (int i = 0; i < 8; i++) Call(p, "RevealGrimoireHint", 0);
            Assert.That(Call(p, "GetRevealedHintCount", 0), Is.EqualTo(4));
            Assert.That(p.FailedAttempts, Is.Zero);
            Assert.That(p.TotalPhaseOneAttempts, Is.Zero);
            Assert.That(Call(p, "RevealGrimoireHint", 8), Is.Null);
            var restored = Create(); restored.RestorePhaseOne(p.CapturePhaseOne("rascunho", ""));
            Assert.That(Call(restored, "GetRevealedHintCount", 0), Is.EqualTo(4));
            Assert.That(restored.FailedAttempts, Is.Zero);
        }
        [Test]
        public void OldSave_RecoversHintHistoryFromFailures()
        {
            var p = Create(); p.RegisterSubmission(); p.RegisterFailedAttempt();
            var data = p.CapturePhaseOne("", "");
            var field = typeof(PhaseOneSaveData).GetField("revealedHintCounts");
            Assert.That(field, Is.Not.Null); field.SetValue(data, null);
            var restored = Create(); restored.RestorePhaseOne(data);
            Assert.That(Call(restored, "GetRevealedHintCount", 0), Is.EqualTo(1));
        }
    }
}
