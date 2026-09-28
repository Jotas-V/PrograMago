using NUnit.Framework;
using PrograMago.Application;
using PrograMago.Domain;
using PrograMago.Language;

namespace PrograMago.Tests.Application
{
    public sealed class CombatAttributesIntegrationTests
    {
        private static SubmitCodeResult Compile(int life, int damage, int range, int initiative, int speed)
        {
            string code = "public class Mago { private int vida; private int dano; private int alcance; " +
                "private int iniciativa; private int velocidadeAtaque; " +
                "public Mago(int alcance, int velocidadeAtaque, int vida, int iniciativa, int dano) { " +
                "this.alcance=alcance; this.velocidadeAtaque=velocidadeAtaque; this.vida=vida; " +
                "this.iniciativa=iniciativa; this.dano=dano; } } " +
                $"Mago heroi = new Mago({range}, {speed}, {life}, {initiative}, {damage});";
            var useCase = new SubmitCodeUseCase(new CodeTokenizer(), new ExerciseCodeValidator(),
                new LearningSession(new ExerciseDefinition("attributes", "Mago", "Crie o Mago.")));
            return useCase.Execute(code, ValidationCriterion.ConstructAndInstantiateMago);
        }

        [TestCase(4, 3, 15, 1, 2)]
        [TestCase(2, 2, 15, 1, 5)]
        public void CodeAttributes_DetermineLifeDamageRangeAndInterval(int life, int damage, int range, int initiative, int speed)
        {
            SubmitCodeResult result = Compile(life, damage, range, initiative, speed);
            Assert.That(result.IsSuccess, Is.True, result.Diagnostic?.Detail);
            MagoState state = MagoState.FromValidatedProgram(result.Program);
            Assert.That(state.RemainingPoints, Is.Zero);
            var wizard = CombatWizard.FromMago(state);
            Assert.That(wizard.Initiative, Is.EqualTo(initiative));
            var engine = new CombatEngine(wizard, new[] { new EnemyState("boneco", "Boneco de Treinamento", 10, "neutro") });
            Assert.That(engine.WizardLife, Is.EqualTo(life));
            Assert.That(engine.Enemies[0].Position, Is.EqualTo(range));
            Assert.That(engine.Tick().Amount, Is.EqualTo(damage));
            for (int i = 1; i < 16 - speed; i++) Assert.That(engine.Tick(), Is.Null);
            Assert.That(engine.Tick().Amount, Is.EqualTo(damage));
            Assert.That(engine.Enemies[0].Life, Is.EqualTo(10 - 2 * damage));
        }

        [Test]
        public void CodeAttributes_RejectsMoreThanTwentyFivePoints()
        {
            Assert.That(Compile(5, 3, 15, 1, 2).IsSuccess, Is.False);
        }
    }
}
