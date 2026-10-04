using NUnit.Framework;
using PrograMago.Application;
using PrograMago.Domain;
using PrograMago.Tests.Language;

namespace PrograMago.Tests.Application
{
    public sealed class PolymorphicCombatCompilerTests
    {
        [Test]
        public void AuthoredReference_CompilesConcreteFormsAndWinsWithoutResettingState()
        {
            string source = (PolymorphicSpellValidationTests.Program + PolymorphicSpellValidationTests.Strategy)
                .Replace("5, 5, 5, 5, 5", "1, 6, 15, 2, 1").Replace("5,5,5,5,5", "1,6,15,2,1");
            Assert.That(CombatCodeCompiler.TryCompilePolymorphicLesson(source, out var strategy, out var diagnostic),
                Is.True, diagnostic?.Detail);
            Assert.That(strategy.FormFor("gelo"), Is.EqualTo("piromante"));
            Assert.That(strategy.FormFor("fogo"), Is.EqualTo("hidromante"));
            Assert.That(strategy.FormFor("água"), Is.EqualTo("eletromante"));
            Assert.That(strategy.FormFor("neutro"), Is.EqualTo("neutro"));
            var engine = new CombatEngine(new CombatWizard(1, 6, 15, 2, 1,
                CombatElement.Neutral, CombatElement.Fire, CombatElement.Water, CombatElement.Electric),
                new[] {
                    new EnemyState("boneco", "Boneco de Treinamento", 10, "neutro"),
                    new EnemyState("golem", "Golem de Gelo", 12, "gelo"),
                    new EnemyState("elemental", "Elemental de Fogo", 12, "fogo"),
                    new EnemyState("slime", "Slime Aquático", 12, "água") }, strategy);
            var enemies = new System.Collections.Generic.List<CombatEnemy>(engine.Enemies);
            for (int tick = 0; tick < 300 && engine.Outcome == CombatOutcome.InProgress; tick++) engine.Tick();
            Assert.That(engine.Outcome, Is.EqualTo(CombatOutcome.Victory));
            for (int index = 0; index < enemies.Count; index++)
                Assert.That(engine.Enemies[index], Is.SameAs(enemies[index]));
        }

        [Test]
        public void WrongElementalChoice_IsNotSilentlyCorrectedByCompiler()
        {
            string source = PolymorphicSpellValidationTests.Program +
                PolymorphicSpellValidationTests.Strategy.Replace("new Piromante", "new Hidromante");
            Assert.That(CombatCodeCompiler.TryCompilePolymorphicLesson(source, out var strategy, out _), Is.True);
            Assert.That(strategy.FormFor("gelo"), Is.EqualTo("hidromante"));
        }
    }
}
