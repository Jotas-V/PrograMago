using System.Collections.Generic;
using NUnit.Framework;
using PrograMago.Domain;

namespace PrograMago.Tests.Domain
{
    public sealed class FinalEncounterBalanceTests
    {
        private static CombatEngine Encounter(CombatWizard wizard, bool correct = true)
        {
            var enemies = new[] {
                new EnemyState("boneco", "Boneco de Treinamento", 10, "neutro"),
                new EnemyState("golem", "Golem de Gelo", 12, "gelo"),
                new EnemyState("elemental", "Elemental de Fogo", 12, "fogo"),
                new EnemyState("slime", "Slime Aquático", 12, "água") };
            var strategy = new CombatStrategy(correct ? new Dictionary<string, string> {
                ["gelo"] = "piromante", ["fogo"] = "hidromante", ["água"] = "eletromante"
            } : null, "neutro");
            return new CombatEngine(wizard, enemies, strategy, true);
        }

        private static void Finish(CombatEngine engine)
        {
            for (int tick = 0; tick < 1000 && engine.Outcome == CombatOutcome.InProgress; tick++) engine.Tick();
        }

        [Test]
        public void FinalEncounterBalance_ProfilesHaveDistinctRoles()
        {
            var engine = Encounter(new CombatWizard(10, 4, 9, 1, 1, CombatElement.Neutral, CombatElement.Fire, CombatElement.Water, CombatElement.Electric));
            Assert.That(engine.Enemies[2].Range, Is.GreaterThan(engine.Enemies[1].Range));
            Assert.That(engine.Enemies[3].AttackSpeed, Is.GreaterThan(engine.Enemies[1].AttackSpeed));
            Assert.That(engine.Enemies[1].Damage, Is.GreaterThan(engine.Enemies[3].Damage));
            Assert.That(engine.Enemies[0].Damage, Is.Zero);
        }

        [Test]
        public void FinalEncounterBalance_GlassCannonCanLoseWithCorrectStrategy()
        {
            var engine = Encounter(new CombatWizard(1, 6, 15, 2, 1, CombatElement.Neutral, CombatElement.Fire, CombatElement.Water, CombatElement.Electric));
            Finish(engine);
            Assert.That(engine.Outcome, Is.EqualTo(CombatOutcome.Defeat));
        }

        [Test]
        public void FinalEncounterBalance_BalancedBuildWinsButReceivesDamage()
        {
            var engine = Encounter(new CombatWizard(10, 4, 9, 1, 1, CombatElement.Neutral, CombatElement.Fire, CombatElement.Water, CombatElement.Electric));
            Finish(engine);
            Assert.That(engine.Outcome, Is.EqualTo(CombatOutcome.Victory));
            Assert.That(engine.WizardLife, Is.InRange(1, 9));
        }

        [Test]
        public void FinalEncounterBalance_WrongElementalStrategyLoses()
        {
            var engine = Encounter(new CombatWizard(10, 4, 9, 1, 1, CombatElement.Neutral, CombatElement.Fire, CombatElement.Water, CombatElement.Electric), false);
            Finish(engine);
            Assert.That(engine.Outcome, Is.EqualTo(CombatOutcome.Defeat));
        }

        [Test]
        public void FinalEncounterBalance_RestartRestoresHealthAndInitialFormation()
        {
            var engine = Encounter(new CombatWizard(1, 6, 15, 2, 1, CombatElement.Neutral,
                CombatElement.Fire, CombatElement.Water, CombatElement.Electric));
            var positions = new List<int>();
            foreach (var enemy in engine.Enemies) positions.Add(enemy.Position);
            Finish(engine);
            Assert.That(engine.Outcome, Is.EqualTo(CombatOutcome.Defeat));
            engine.Restart();
            Assert.That(engine.WizardLife, Is.EqualTo(1));
            Assert.That(engine.Outcome, Is.EqualTo(CombatOutcome.InProgress));
            for (int index = 0; index < engine.Enemies.Count; index++)
            {
                Assert.That(engine.Enemies[index].Position, Is.EqualTo(positions[index]));
                Assert.That(engine.Enemies[index].Life, Is.EqualTo(engine.Enemies[index].Source.Vida));
            }
        }
    }
}
