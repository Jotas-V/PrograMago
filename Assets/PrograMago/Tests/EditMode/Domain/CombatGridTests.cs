using NUnit.Framework;
using PrograMago.Domain;

namespace PrograMago.Tests.Domain
{
    public sealed class CombatGridTests
    {
        private static CombatEngine Create(int range, params EnemyState[] enemies) =>
            new CombatEngine(new CombatWizard(20, 3, range, 10, 5, CombatElement.Neutral), enemies);

        private static EnemyState Dummy() => new EnemyState("boneco", "Boneco de Treinamento", 10, "neutro");

        [Test]
        public void Arena_StartsAtOppositeEndsOfSixteenCells()
        {
            var engine = Create(1, Dummy());
            Assert.That(engine.WizardPosition, Is.Zero);
            Assert.That(engine.Enemies[0].Position, Is.EqualTo(15));
        }

        [Test]
        public void OutOfRange_WizardStaysPutAndDoesNoDamage()
        {
            var engine = Create(14, Dummy());
            var attack = engine.Tick();
            Assert.That(attack.Kind.ToString(), Is.EqualTo("OutOfRange"));
            for (int i = 0; i < 200; i++) engine.Tick();
            Assert.That(engine.WizardPosition, Is.Zero);
            Assert.That(engine.Enemies[0].Life, Is.EqualTo(10));
        }

        [Test]
        public void RangeFifteen_ReachesTheLastCellAndUsesWizardDamage()
        {
            var engine = Create(15, Dummy());
            var attack = engine.Tick();
            Assert.That(engine.Enemies[0].Position, Is.EqualTo(15));
            Assert.That(attack.Amount, Is.EqualTo(3));
            Assert.That(engine.Enemies[0].Life, Is.EqualTo(7));
        }

        [Test]
        public void Enemies_AdvanceOneCellWithoutOverlappingOrEnteringWizardCell()
        {
            var engine = Create(1,
                new EnemyState("back", "Golem de Gelo", 12, "gelo"),
                new EnemyState("front", "Golem de Gelo", 12, "gelo"));
            Assert.That(engine.Enemies[0].Position, Is.EqualTo(15));
            Assert.That(engine.Enemies[1].Position, Is.EqualTo(14));
            for (int i = 0; i < 350 && engine.Outcome == CombatOutcome.InProgress; i++)
            {
                int a = engine.Enemies[0].Position, b = engine.Enemies[1].Position;
                engine.Tick();
                Assert.That(a - engine.Enemies[0].Position, Is.InRange(0, 1));
                Assert.That(b - engine.Enemies[1].Position, Is.InRange(0, 1));
                Assert.That(engine.Enemies[0].Position, Is.Not.EqualTo(engine.Enemies[1].Position));
                Assert.That(engine.Enemies[1].Position, Is.GreaterThan(0));
            }
            Assert.That(engine.Enemies[1].Position, Is.EqualTo(1));
            Assert.That(engine.WizardLife, Is.LessThan(20));
        }
    }
}
