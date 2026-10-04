using System.Collections.Generic;
using NUnit.Framework;
using PrograMago.Domain;

namespace PrograMago.Tests.Domain
{
    public sealed class MultiEnemyCombatTests
    {
        [Test]
        public void FourEnemyTypes_UseNearestLivingTargetsAndKeepIndependentStates()
        {
            var wizard = new CombatWizard(100, 12, 15, 15, 15,
                CombatElement.Neutral, CombatElement.Fire, CombatElement.Water, CombatElement.Electric);
            var enemies = new[]
            {
                new EnemyState("boneco", "Boneco de Treinamento", 10, "neutro"),
                new EnemyState("golem", "Golem de Gelo", 12, "gelo"),
                new EnemyState("elemental", "Elemental de Fogo", 12, "fogo"),
                new EnemyState("slime", "Slime Aquático", 12, "água")
            };
            var strategy = new CombatStrategy(new Dictionary<string, string>
            {
                ["gelo"] = "piromante", ["fogo"] = "hidromante", ["água"] = "eletromante"
            }, "neutro");
            var engine = new CombatEngine(wizard, enemies, strategy);
            var states = new List<CombatEnemy>(engine.Enemies);
            var targets = new List<string>();
            var forms = new List<string>();
            for (int tick = 0; tick < 100 && engine.Outcome == CombatOutcome.InProgress; tick++)
            {
                CombatEvent step = engine.Tick();
                if (step == null || step.Actor != "Mago" || step.Kind != CombatEventKind.Hit) continue;
                targets.Add(step.Target);
                forms.Add(engine.WizardForm);
                for (int index = 0; index < enemies.Length; index++)
                    Assert.That(engine.Enemies[index], Is.SameAs(states[index]));
            }
            Assert.That(targets, Is.EqualTo(new[] { "slime", "elemental", "golem", "boneco" }));
            Assert.That(forms, Is.EqualTo(new[] { "eletromante", "hidromante", "piromante", "neutro" }));
            Assert.That(engine.Outcome, Is.EqualTo(CombatOutcome.Victory));
            Assert.That(wizard.Life, Is.EqualTo(100));
        }
    }
}