using NUnit.Framework;
using PrograMago.Application;
using PrograMago.Domain;
using System.Collections.Generic;

namespace PrograMago.Tests.Application
{
    public sealed class MagoPreparationTests
    {
        private static MagoState Initial() => MagoState.FromValidatedProgram(new ValidatedMagoProgram(
            "Mago", new[] { "vida", "dano", "alcance", "iniciativa", "velocidadeAtaque" }, "jorge",
            new Dictionary<string, int> { ["vida"] = 6, ["dano"] = 1, ["alcance"] = 1,
                ["iniciativa"] = 10, ["velocidadeAtaque"] = 2 }, 25));

        [Test]
        public void Methods_RequireCorrectAssignmentAndCannotReplaceApprovedImplementation()
        {
            var book = new MagoMethodBook();
            Assert.That(book.TryApprove("public void setAlcance(int valor) { this.dano = valor; }", out _), Is.False);
            Assert.That(book.TryApprove("public void setAlcance(int valor) { this.alcance = valor; }", out _), Is.True);
            Assert.That(book.ApprovedSources.Count, Is.EqualTo(1));
            Assert.That(book.TryApprove("public void setAlcance(int outro) { this.alcance = outro; }", out _), Is.False);
        }

        [Test]
        public void Preparation_ValidatesFinalDistributionAndNeverMutatesInitialState()
        {
            var book = AllMethods(); var initial = Initial();
            var invalid = MagoPreparation.Evaluate(initial, book, "jorge.setAlcance(15);");
            Assert.That(invalid.IsSuccess, Is.False);
            Assert.That(invalid.TotalPoints, Is.EqualTo(34));
            var valid = MagoPreparation.Evaluate(initial, book,
                "jorge.setAlcance(15); jorge.setIniciativa(1);");
            Assert.That(valid.IsSuccess, Is.True, valid.Error);
            Assert.That(valid.Mago.Alcance, Is.EqualTo(15));
            Assert.That(valid.Mago.RemainingPoints, Is.Zero);
            Assert.That(initial.Alcance, Is.EqualTo(1));
            Assert.That(initial.Iniciativa, Is.EqualTo(10));
        }

        [TestCase("jorge.setVida(0);")]
        [TestCase("jorge.setDano(16);")]
        [TestCase("outro.setVida(2);")]
        [TestCase("jorge.vida = 2;")]
        [TestCase("jorge.setVida(2); lancarMagia();")]
        public void Preparation_RejectsInvalidFinalValuesOrUnrecognizedCode(string source)
        {
            Assert.That(MagoPreparation.Evaluate(Initial(), AllMethods(), source).IsSuccess, Is.False);
        }

        [Test]
        public void Preparation_RequiresLearnedSetter()
        {
            Assert.That(MagoPreparation.Evaluate(Initial(), new MagoMethodBook(), "jorge.setAlcance(2);").IsSuccess, Is.False);
        }

        [Test]
        public void MethodRestore_RevalidatesSourcesAndPreservesLearnedOrder()
        {
            var restored = new MagoMethodBook(AllMethods().ApprovedSources);
            Assert.That(restored.IsComplete, Is.True);
            Assert.That(new MagoMethodBook(new[] { "invalid" }).ApprovedSources, Is.Empty);
        }

        private static MagoMethodBook AllMethods()
        {
            var book = new MagoMethodBook();
            foreach (string source in MagoMethodBook.Examples)
                Assert.That(book.TryApprove(source, out string error), Is.True, error);
            return book;
        }
    }
}
