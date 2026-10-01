using NUnit.Framework;
using PrograMago.Domain;
using PrograMago.Language;

namespace PrograMago.Tests.Language
{
    public sealed class ElementalSpellValidationTests
    {
        internal const string BaseProgram =
            "public class Mago { " +
            "private int vida; private int dano; private int alcance; private int iniciativa; private int velocidadeAtaque; " +
            "public Mago(int vida, int dano, int alcance, int iniciativa, int velocidadeAtaque) { " +
            "this.vida = vida; this.dano = dano; this.alcance = alcance; this.iniciativa = iniciativa; " +
            "this.velocidadeAtaque = velocidadeAtaque; } " +
            "public void lancarMagia(Inimigo alvo) {} } Mago mago = new Mago(5, 5, 5, 5, 5); " +
            "public class Inimigo { private String nome; private int vida; private String elemento; " +
            "public Inimigo(String nome, int vida, String elemento) { this.nome = nome; this.vida = vida; " +
            "this.elemento = elemento; } public String getElemento() { return elemento; } } " +
            "Inimigo boneco = new Inimigo(\"Boneco de Treinamento\", 10, \"neutro\"); ";

        internal const string OverrideBody =
            "@Override public void lancarMagia(Inimigo inimigo) { super.lancarMagia(inimigo); }";

        [Test]
        public void BaseMethodAndCall_PreserveValidatedBuildAndEnemy()
        {
            var result = Validate(BaseProgram + "mago.lancarMagia(boneco);", ValidationCriterion.DefineAndCallSpellMethod);
            Assert.That(result.IsSuccess, Is.True, result.Diagnostic?.Detail);
            Assert.That(result.SatisfiedCriterion, Is.EqualTo(ValidationCriterion.DefineAndCallSpellMethod));
            Assert.That(result.Program.InitialValues["vida"], Is.EqualTo(5));
            Assert.That(result.Enemies, Has.Count.EqualTo(1));
        }

        [TestCase("Piromante")]
        [TestCase("Hidromante")]
        [TestCase("Eletromante")]
        public void SupportedSubclass_AcceptsInheritance(string name)
        {
            var result = Validate(BaseProgram + "public class " + name + " extends Mago {}", ValidationCriterion.ExtendMago);
            Assert.That(result.IsSuccess, Is.True, result.Diagnostic?.Detail);
        }

        [TestCase("Piromante")]
        [TestCase("Hidromante")]
        [TestCase("Eletromante")]
        public void SupportedSubclass_AcceptsOverrideAndSuperWithRenamedParameter(string name)
        {
            var result = Validate(WithSubclass(name, OverrideBody), ValidationCriterion.OverrideSpellWithSuper);
            Assert.That(result.IsSuccess, Is.True, result.Diagnostic?.Detail);
        }

        [Test]
        public void AllThreeSubclasses_AcceptAnyDeclarationOrderAndWhitespace()
        {
            string source = "public class Eletromante extends Mago { " + OverrideBody + " } " + BaseProgram +
                "public class Hidromante extends Mago { " + OverrideBody + " } " +
                "public class Piromante extends Mago { " + OverrideBody + " }";
            var result = Validate(source.Replace(" {", "\r\n\t{").Replace("; ", ";\r\n\t"), ValidationCriterion.OverrideSpellWithSuper);
            Assert.That(result.IsSuccess, Is.True, result.Diagnostic?.Detail);
        }

        [TestCase("public class Piromante {}", "INHERIT001")]
        [TestCase("public class Piromante extends Inimigo {}", "INHERIT001")]
        [TestCase("public class Piromante extends Piromante {}", "INHERIT001")]
        [TestCase("public class Necromante extends Mago {}", "INHERIT002")]
        [TestCase("public class Piromante extends Mago {} public class Piromante extends Mago {}", "INHERIT003")]
        [TestCase("public class Piromante extends Mago { private int vida; }", "SPELL004")]
        public void Inheritance_RejectsUnsupportedOrDuplicateDeclarations(string declaration, string code)
        {
            AssertFailure(BaseProgram + declaration, ValidationCriterion.ExtendMago, code);
        }

        [TestCase("public void lancarMagia(Inimigo alvo) { super.lancarMagia(alvo); }", "OVERRIDE001")]
        [TestCase("@override public void lancarMagia(Inimigo alvo) { super.lancarMagia(alvo); }", "OVERRIDE001")]
        [TestCase("@Override private void lancarMagia(Inimigo alvo) { super.lancarMagia(alvo); }", "SPELL002")]
        [TestCase("@Override public int lancarMagia(Inimigo alvo) { super.lancarMagia(alvo); }", "SPELL002")]
        [TestCase("@Override public void lancarMagia(Mago alvo) { super.lancarMagia(alvo); }", "SPELL002")]
        [TestCase("@Override public void lancarMagia() { super.lancarMagia(alvo); }", "SPELL002")]
        [TestCase("@Override public void lancarMagia(Inimigo alvo, int dano) { super.lancarMagia(alvo); }", "SPELL002")]
        [TestCase("@Override public void atacar(Inimigo alvo) { super.lancarMagia(alvo); }", "SPELL002")]
        [TestCase("@Override public void lancarMagia(Inimigo alvo) {}", "SUPER001")]
        [TestCase("@Override public void lancarMagia(Inimigo alvo) { super.lancarMagia(outro); }", "SUPER001")]
        [TestCase("@Override public void lancarMagia(Inimigo alvo) { super(); }", "SUPER001")]
        [TestCase("@Override public void lancarMagia(Inimigo alvo) { this.lancarMagia(alvo); }", "SUPER001")]
        [TestCase("@Override public void lancarMagia(Inimigo alvo) { super.lancarMagia(alvo); lancarAgua(); }", "SPELL004")]
        [TestCase("@Override public void lancarMagia(Inimigo alvo) { super.lancarMagia(alvo); super.lancarMagia(alvo); }", "SPELL004")]
        public void Override_DifferentiatesAnnotationSignatureSuperAndUnsupportedCommands(string body, string code)
        {
            AssertFailure(WithSubclass("Piromante", body), ValidationCriterion.OverrideSpellWithSuper, code);
        }

        [Test]
        public void DuplicateMethod_IsRejected()
        {
            AssertFailure(WithSubclass("Piromante", OverrideBody + OverrideBody),
                ValidationCriterion.OverrideSpellWithSuper, "SPELL003");
        }

        [TestCase("", "SPELL005")]
        [TestCase("outro.lancarMagia(boneco);", "SPELL005")]
        [TestCase("mago.lancarMagia(outro);", "SPELL005")]
        [TestCase("mago.lancarMagia();", "SPELL005")]
        [TestCase("mago.lancarAgua(boneco);", "SPELL005")]
        public void SpellCall_RequiresValidatedWizardAndEnemy(string call, string code)
        {
            AssertFailure(BaseProgram + call, ValidationCriterion.DefineAndCallSpellMethod, code);
        }

        [Test]
        public void BaseMethod_CannotUseSuperOrArbitraryCommands()
        {
            AssertFailure(BaseProgram.Replace("lancarMagia(Inimigo alvo) {}",
                "lancarMagia(Inimigo alvo) { super.lancarMagia(alvo); }") + "mago.lancarMagia(boneco);",
                ValidationCriterion.DefineAndCallSpellMethod, "SPELL004");
        }

        [Test]
        public void MissingBaseMethod_IsRejectedBeforeOverride()
        {
            AssertFailure(WithSubclass("Piromante", OverrideBody).Replace("public void lancarMagia(Inimigo alvo) {}", ""),
                ValidationCriterion.OverrideSpellWithSuper, "SPELL001");
        }

        [Test]
        public void Diagnostic_PointsToOriginalSubclassToken()
        {
            string source = BaseProgram + "\npublic class Piromante extends Inimigo {}";
            var result = Validate(source, ValidationCriterion.ExtendMago);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Diagnostic.Code, Is.EqualTo("INHERIT001"));
            Assert.That(result.Diagnostic.Position.Offset, Is.EqualTo(source.LastIndexOf("Inimigo", System.StringComparison.Ordinal)));
            Assert.That(result.Diagnostic.Position.Line, Is.EqualTo(2));
        }

        [TestCase("}")]
        [TestCase("public class Extra {}")]
        [TestCase("mago.vida = 15;")]
        [TestCase("public class Piromante extends Mago {")]
        public void TrailingOrIncompleteCode_IsNeverDiscarded(string extra)
        {
            var result = Validate(WithSubclass("Piromante", OverrideBody) + extra, ValidationCriterion.OverrideSpellWithSuper);
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void ExistingBuildValidation_StillRejectsOverBudget()
        {
            var result = Validate(WithSubclass("Piromante", OverrideBody).Replace("new Mago(5, 5, 5, 5, 5)",
                "new Mago(15, 15, 15, 15, 15)"), ValidationCriterion.OverrideSpellWithSuper);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Diagnostic.Code, Is.EqualTo("GAME002"));
        }

        private static string WithSubclass(string name, string body) =>
            BaseProgram + "public class " + name + " extends Mago { " + body + " }";

        private static void AssertFailure(string source, ValidationCriterion criterion, string code)
        {
            var result = Validate(source, criterion);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Diagnostic.Code, Is.EqualTo(code), result.Diagnostic.Detail);
        }

        private static ExerciseValidationResult Validate(string source, ValidationCriterion criterion)
        {
            var tokens = new CodeTokenizer().Tokenize(source);
            Assert.That(tokens.IsSuccess, Is.True, tokens.Diagnostic?.Detail);
            return new ExerciseCodeValidator().Validate(tokens.Tokens,
                new ExerciseDefinition("tcc-15", "Mago", "Especialize as magias."), criterion);
        }
    }
}
