using System.Collections.Generic;
using NUnit.Framework;
using PrograMago.Domain;

namespace PrograMago.Tests.Domain
{
    public sealed class CombatEngineTests
    {
        [Test]
        public void Domain_ProvidesDeterministicCombatEngine()
        {
            Assert.That(typeof(MagoState).Assembly.GetType("PrograMago.Domain.CombatEngine"),
                Is.Not.Null);
        }

        [Test]
        public void Tick_WithSameSetup_ProducesTheSameSequenceAndResult()
        {
            CombatEngine first = CreateEngine(8, 3, 2, 6, 8, "gelo",
                CombatElement.Fire);
            CombatEngine second = CreateEngine(8, 3, 2, 6, 8, "gelo",
                CombatElement.Fire);
            var firstEvents = new List<string>();
            var secondEvents = new List<string>();

            for (int tick = 0; tick < 120; tick++)
            {
                firstEvents.Add(Describe(first.Tick()));
                secondEvents.Add(Describe(second.Tick()));
            }

            Assert.That(firstEvents, Is.EqualTo(secondEvents));
            Assert.That(first.Outcome, Is.EqualTo(second.Outcome));
            Assert.That(first.WizardLife, Is.EqualTo(second.WizardLife));
        }

        [Test]
        public void Tick_HigherInitiativeActsFirst()
        {
            CombatEngine fastWizard = CreateEngine(8, 2, 15, 6, 5, "gelo");
            CombatEngine slowWizard = CreateEngine(8, 2, 15, 1, 5, "gelo");

            Assert.That(fastWizard.Tick().Actor, Is.EqualTo("Mago"));
            Assert.That(slowWizard.Tick().Actor, Is.EqualTo("enemy"));
        }

        [Test]
        public void Tick_AttackSpeedControlsTheNextOpportunity()
        {
            CombatEngine engine = CreateEngine(15, 1, 4, 10, 15, "neutro");
            Assert.That(engine.Tick().Actor, Is.EqualTo("Mago"));
            Assert.That(engine.Tick().Actor, Is.EqualTo("Mago"));
            Assert.That(engine.Tick().Actor, Is.EqualTo("Mago"));
        }

        [Test]
        public void TrainingDummy_NeverMovesOrAttacksEvenWhenWizardCannotAttack()
        {
            CombatEngine engine = CreateEngine(1, 1, 1, 1, 1, "neutro");
            engine.Pause();
            engine.MoveAction(2, 0);
            engine.Resume();
            for (int tick = 0; tick < 500; tick++) engine.Tick();

            Assert.That(engine.WizardLife, Is.EqualTo(1));
            Assert.That(engine.Enemies[0].Position, Is.EqualTo(15));
            Assert.That(engine.Outcome, Is.EqualTo(CombatOutcome.InProgress));
        }

        [Test]
        public void CodeProgram_RepeatedAttacksExecuteAndReportTheirSourceBlock()
        {
            CombatEngine engine = CreateEngine(8, 2, 15, 10, 5, "neutro");
            var configure = typeof(CombatEngine).GetMethod("ConfigureActions");
            Assert.That(configure, Is.Not.Null);
            configure.Invoke(engine, new object[] {
                new[] { CombatAction.AnalyzeTarget, CombatAction.SelectSpell, CombatAction.Attack, CombatAction.Attack },
                new[] { 0, 1, 2, 3 } });
            CombatEvent result = engine.Tick();
            Assert.That(engine.Enemies[0].Life, Is.EqualTo(6));
            Assert.That(typeof(CombatEvent).GetProperty("BlockIndex").GetValue(result), Is.EqualTo(3));
        }

        [Test]
        public void CodeProgram_AttackBeforeSelectingMagicDoesNotDealDamage()
        {
            CombatEngine engine = CreateEngine(8, 2, 15, 10, 5, "neutro");
            engine.Pause();
            engine.MoveAction(2, 1);
            engine.Resume();
            CombatEvent result = engine.Tick();
            Assert.That(engine.Enemies[0].Life, Is.EqualTo(10));
            Assert.That(result.Kind.ToString(), Is.EqualTo("MissingSpell"));
        }

        [Test]
        public void Tick_OutOfRangeMovesWizardOneCellWithoutDamaging()
        {
            CombatEngine engine = CreateEngine(8, 5, 1, 10, 5, "neutro");

            CombatEvent action = engine.Tick();

            Assert.That(action.Kind, Is.EqualTo(CombatEventKind.Move));
            Assert.That(engine.WizardPosition, Is.EqualTo(1));
            Assert.That(engine.Enemies[0].Life, Is.EqualTo(10));
        }

        [Test]
        public void Tick_WithinRangeDealsBaseDamageToNeutralEnemy()
        {
            CombatEngine engine = CreateEngine(8, 3, 15, 10, 5, "neutro");

            CombatEvent action = engine.Tick();

            Assert.That(action.Kind, Is.EqualTo(CombatEventKind.Hit));
            Assert.That(action.Amount, Is.EqualTo(3));
            Assert.That(engine.Enemies[0].Life, Is.EqualTo(7));
        }

        [TestCase("gelo", CombatElement.Fire, 6)]
        [TestCase("fogo", CombatElement.Water, 6)]
        [TestCase("água", CombatElement.Electric, 6)]
        [TestCase("gelo", CombatElement.Water, 0)]
        public void Tick_ElementalWeaknessDeterminesDamage(
            string enemyElement, CombatElement spell, int expectedDamage)
        {
            CombatEngine engine = CreateEngine(8, 3, 15, 10, 5, enemyElement,
                spell);

            CombatEvent action = engine.Tick();

            Assert.That(action.Amount, Is.EqualTo(expectedDamage));
            Assert.That(engine.Enemies[0].Life, Is.EqualTo(12 - expectedDamage));
        }

        [Test]
        public void Tick_SelectsNearestLivingEnemyThenCreationOrder()
        {
            var wizard = new CombatWizard(8, 3, 15, 10, 5, CombatElement.Neutral);
            var enemies = new[]
            {
                new EnemyState("first", "Boneco de Treinamento", 10, "neutro"),
                new EnemyState("second", "Boneco de Treinamento", 10, "neutro")
            };
            var engine = new CombatEngine(wizard, enemies);

            Assert.That(engine.Tick().Target, Is.EqualTo("second"));
        }

        [Test]
        public void Pause_FreezesStateAndAllowsReorderingOnlyWhilePaused()
        {
            CombatEngine engine = CreateEngine(8, 3, 15, 10, 5, "gelo",
                CombatElement.Fire);
            Assert.Throws<System.InvalidOperationException>(() => engine.MoveAction(2, 0));
            engine.Pause();
            int tick = engine.CurrentTick;
            int life = engine.WizardLife;
            engine.MoveAction(2, 0);

            Assert.That(engine.Tick(), Is.Null);
            Assert.That(engine.CurrentTick, Is.EqualTo(tick));
            Assert.That(engine.WizardLife, Is.EqualTo(life));
            Assert.That(engine.ActionOrder[0], Is.EqualTo(CombatAction.Attack));

            engine.Resume();
            Assert.That(engine.Tick().Kind, Is.EqualTo(CombatEventKind.NoTarget));
        }

        [Test]
        public void Tick_DefeatOccursWhenEnemyAttackReducesWizardLifeToZero()
        {
            CombatEngine engine = CreateEngine(1, 1, 1, 1, 1, "gelo");
            for (int tick = 0; tick < 300 && engine.Outcome == CombatOutcome.InProgress;
                 tick++) engine.Tick();

            Assert.That(engine.Outcome, Is.EqualTo(CombatOutcome.Defeat));
            Assert.That(engine.WizardLife, Is.Zero);
        }

        [Test]
        public void Tick_VictoryOccursWhenAllEnemyLifeReachesZero()
        {
            CombatEngine engine = CreateEngine(15, 15, 15, 10, 15, "neutro");

            engine.Tick();

            Assert.That(engine.Outcome, Is.EqualTo(CombatOutcome.Victory));
            Assert.That(engine.Enemies[0].Life, Is.Zero);
            Assert.That(engine.Tick(), Is.Null);
        }

        [Test]
        public void Restart_RestoresCombatStateAndKeepsTheChosenActionOrder()
        {
            CombatEngine engine = CreateEngine(1, 1, 1, 1, 1, "gelo");
            engine.Pause();
            engine.MoveAction(2, 0);
            engine.Resume();
            for (int tick = 0; tick < 300 && engine.Outcome == CombatOutcome.InProgress;
                 tick++) engine.Tick();
            Assert.That(engine.Outcome, Is.EqualTo(CombatOutcome.Defeat));

            engine.Restart();

            Assert.That(engine.Outcome, Is.EqualTo(CombatOutcome.InProgress));
            Assert.That(engine.CurrentTick, Is.Zero);
            Assert.That(engine.WizardLife, Is.EqualTo(1));
            Assert.That(engine.WizardPosition, Is.Zero);
            Assert.That(engine.Enemies[0].Life, Is.EqualTo(12));
            Assert.That(engine.Enemies[0].Position, Is.EqualTo(15));
            Assert.That(engine.ActionOrder[0], Is.EqualTo(CombatAction.Attack));
        }

        private static CombatEngine CreateEngine(int life, int damage, int range,
            int initiative, int speed, string enemyElement,
            params CombatElement[] extraSpells)
        {
            var spells = new List<CombatElement> { CombatElement.Neutral };
            spells.AddRange(extraSpells);
            var wizard = new CombatWizard(life, damage, range, initiative, speed,
                spells.ToArray());
            string name = enemyElement == "neutro" ? "Boneco de Treinamento" :
                enemyElement == "gelo" ? "Golem de Gelo" :
                enemyElement == "fogo" ? "Elemental de Fogo" : "Slime Aquático";
            int enemyLife = enemyElement == "neutro" ? 10 : 12;
            return new CombatEngine(wizard, new[]
            {
                new EnemyState("enemy", name, enemyLife, enemyElement)
            });
        }

        private static string Describe(CombatEvent action)
        {
            return action == null ? "idle" :
                $"{action.Tick}:{action.Actor}:{action.Kind}:{action.Target}:" +
                $"{action.Element}:{action.Amount}";
        }
    }
}
