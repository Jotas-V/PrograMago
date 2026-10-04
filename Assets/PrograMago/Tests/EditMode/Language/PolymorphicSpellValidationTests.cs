using NUnit.Framework;
using PrograMago.Domain;
using PrograMago.Language;

namespace PrograMago.Tests.Language
{
    public sealed class PolymorphicSpellValidationTests
    {
        internal static string Program
        {
            get
            {
                string source = ElementalSpellValidationTests.BaseProgram +
                    "Inimigo golem = new Inimigo(\"Golem de Gelo\", 12, \"gelo\");" +
                    "Inimigo elemental = new Inimigo(\"Elemental de Fogo\", 12, \"fogo\");" +
                    "Inimigo slime = new Inimigo(\"Slime Aquático\", 12, \"água\");";
                foreach (string name in new[] { "Piromante", "Hidromante", "Eletromante" })
                    source += "public class " + name + " extends Mago { public " + name +
                        "(int vida, int dano, int alcance, int iniciativa, int velocidadeAtaque) { " +
                        "super(vida, dano, alcance, iniciativa, velocidadeAtaque); } " +
                        ElementalSpellValidationTests.OverrideBody + " }";
                return source;
            }
        }
        internal const string Strategy =
            "Mago ativo = mago; " +
            "if (alvo.getElemento().equals(\"gelo\")) { ativo = new Piromante(5,5,5,5,5); } " +
            "else if (alvo.getElemento().equals(\"fogo\")) { ativo = new Hidromante(5,5,5,5,5); } " +
            "else if (alvo.getElemento().equals(\"água\")) { ativo = new Eletromante(5,5,5,5,5); } " +
            "else { ativo = mago; } ativo.lancarMagia(alvo);";

        [Test]
        public void FourEnemiesAndOneBaseReference_AcceptCompletePolymorphicProgram()
        {
            var result = Validate(Program + Strategy);
            Assert.That(result.IsSuccess, Is.True, result.Diagnostic?.Detail);
            Assert.That(result.Enemies.Count, Is.EqualTo(4));
            Assert.That(result.Program.InstanceName, Is.EqualTo("mago"));
            Assert.That(result.SatisfiedCriterion, Is.EqualTo(ValidationCriterion.UsePolymorphicMagoReference));
        }

        [Test]
        public void Branches_AcceptDifferentOrderNamesAndWhitespace()
        {
            string source = Strategy.Replace("ativo", "escolhido").Replace("gelo", "temporary")
                .Replace("fogo", "gelo").Replace("temporary", "fogo")
                .Replace(";", ";\r\n\t");
            Assert.That(Validate(Program + source).IsSuccess, Is.True);
        }

        [TestCase("Mago ativo = mago;", "Piromante ativo = mago;")]
        [TestCase("getElemento()", "getNome()")]
        [TestCase("ativo = new Piromante", "outro = new Piromante")]
        [TestCase("new Piromante", "new Necromante")]
        [TestCase("Piromante(5,5,5,5,5)", "Piromante(15,5,5,5,5)")]
        [TestCase("else { ativo = mago; }", "")]
        [TestCase("ativo.lancarMagia(alvo);", "mago.lancarMagia(alvo);")]
        [TestCase("\"água\"", "\"gelo\"")]
        public void Strategy_RejectsUnsupportedOrIncompleteCode(string before, string after)
        {
            Assert.That(Validate(Program + Strategy.Replace(before, after)).IsSuccess, Is.False);
        }

        [Test]
        public void WholeInput_RejectsTrailingCommandsAndMissingEnemy()
        {
            Assert.That(Validate(Program + Strategy + "ativo.lancarMagia(alvo);").IsSuccess, Is.False);
            Assert.That(Validate(Program.Replace("Inimigo slime = new Inimigo(\"Slime Aquático\", 12, \"água\");", "") + Strategy).IsSuccess, Is.False);
        }

        private static ExerciseValidationResult Validate(string source)
        {
            var tokens = new CodeTokenizer().Tokenize(source);
            Assert.That(tokens.IsSuccess, Is.True, tokens.Diagnostic?.Detail);
            return new ExerciseCodeValidator().Validate(tokens.Tokens,
                new ExerciseDefinition("final", "Mago", "Polimorfismo"), ValidationCriterion.UsePolymorphicMagoReference);
        }
    }
}
