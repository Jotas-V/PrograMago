using System;
using System.Collections.Generic;
using NUnit.Framework;
using PrograMago.Domain;
using PrograMago.Language;

namespace PrograMago.Tests.Language
{
    public sealed class ExerciseCodeValidatorTests
    {
        private const string CompleteProgram =
            "public class Mago {\n" +
            "private int vida; private int dano; private int alcance;\n" +
            "private int iniciativa; private int velocidadeAtaque;\n" +
            "public Mago(int vida, int dano, int alcance, int iniciativa, int velocidadeAtaque) {\n" +
            "this.vida = vida; this.dano = dano; this.alcance = alcance;\n" +
            "this.iniciativa = iniciativa; this.velocidadeAtaque = velocidadeAtaque;\n" +
            "}\n}\n" +
            "Mago mago = new Mago(5, 4, 6, 3, 2);";

        private const string EnemyProgram = CompleteProgram + "\n" +
            "public class Inimigo {\n" +
            "private String nome; private int vida; private String elemento;\n" +
            "public Inimigo(String nome, int vida, String elemento) {\n" +
            "this.nome = nome; this.vida = vida; this.elemento = elemento;\n" +
            "}\n" +
            "public String getElemento() { return elemento; }\n" +
            "}\n" +
            "Inimigo boneco = new Inimigo(\"Boneco de Treinamento\", 10, \"neutro\");";

        [Test]
        public void Validate_EnemyAfterApprovedMago_AcceptsConstructionAndInstantiation()
        {
            ExerciseValidationResult result = Validate(
                EnemyProgram,
                ValidationCriterion.ConstructAndInstantiateEnemy);

            Assert.That(result.IsSuccess, Is.True, result.Diagnostic?.Detail);
            Assert.That(result.SatisfiedCriterion,
                Is.EqualTo(ValidationCriterion.ConstructAndInstantiateEnemy));
            Assert.That(result.Program.InstanceName, Is.EqualTo("mago"));
            Assert.That(result.Enemies, Has.Count.EqualTo(1));
            Assert.That(result.Enemies[0].VariableName, Is.EqualTo("boneco"));
            Assert.That(result.Enemies[0].Name, Is.EqualTo("Boneco de Treinamento"));
            Assert.That(result.Enemies[0].Vida, Is.EqualTo(10));
            Assert.That(result.Enemies[0].Elemento, Is.EqualTo("neutro"));
        }

        [Test]
        public void Validate_EnemyClassBeforeMago_AcceptsEitherClassOrder()
        {
            int split = EnemyProgram.IndexOf("public class Inimigo", StringComparison.Ordinal);
            string source = EnemyProgram.Substring(split) + "\n" + EnemyProgram.Substring(0, split);

            ExerciseValidationResult result = Validate(
                source,
                ValidationCriterion.ConstructAndInstantiateEnemy);

            Assert.That(result.IsSuccess, Is.True, result.Diagnostic?.Detail);
        }

        [Test]
        public void ExerciseValidationResult_ExposesValidatedEnemyInstances()
        {
            Assert.That(typeof(ExerciseValidationResult).GetProperty("Enemies"), Is.Not.Null);
        }

        [Test]
        public void Validate_EnemyFieldInsideMago_RejectsWrongClassScope()
        {
            string source = EnemyProgram.Replace(
                "private int vida; private int dano;",
                "private String elemento; private int vida; private int dano;");

            ExerciseValidationResult result = Validate(
                source,
                ValidationCriterion.ConstructAndInstantiateEnemy);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Diagnostic.Code, Is.Not.EqualTo("VALID001"));
        }

        [Test]
        public void Validate_FourEnemyProfiles_MapsDistinctObjectsFromOneClass()
        {
            string source = EnemyProgram + "\n" +
                "Inimigo golem = new Inimigo(\"Golem de Gelo\", 12, \"gelo\");\n" +
                "Inimigo elemental = new Inimigo(\"Elemental de Fogo\", 12, \"fogo\");\n" +
                "Inimigo slime = new Inimigo(\"Slime Aquático\", 12, \"água\");";

            ExerciseValidationResult result = Validate(
                source,
                ValidationCriterion.ConstructAndInstantiateEnemy);

            Assert.That(result.IsSuccess, Is.True, result.Diagnostic?.Detail);
            Assert.That(result.Enemies, Has.Count.EqualTo(4));
            Assert.That(result.Enemies[1].Name, Is.EqualTo("Golem de Gelo"));
            Assert.That(result.Enemies[2].Elemento, Is.EqualTo("fogo"));
            Assert.That(result.Enemies[3].Elemento, Is.EqualTo("água"));
        }

        [TestCase("private String elemento;", "public String elemento;", "ENEMY004")]
        [TestCase("this.elemento = elemento;", "this.elemento = nome;", "ENEMY006")]
        [TestCase("return elemento;", "return nome;", "ENEMY007")]
        [TestCase("getElemento()", "getElemento(String valor)", "ENEMY007")]
        [TestCase("\"neutro\"", "\"fogo\"", "ENEMY010")]
        [TestCase("\"Boneco de Treinamento\"", "\"Outro\"", "ENEMY010")]
        [TestCase("\"Boneco de Treinamento\", 10", "\"Boneco de Treinamento\", 11", "ENEMY010")]
        public void Validate_InvalidEnemyDefinitionOrProfile_ReportsSpecificFailure(
            string oldText, string newText, string code)
        {
            ExerciseValidationResult result = Validate(
                EnemyProgram.Replace(oldText, newText),
                ValidationCriterion.ConstructAndInstantiateEnemy);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Diagnostic.Code, Is.EqualTo(code));
        }

        [Test]
        public void Validate_DuplicateEnemyVariable_RejectsSecondInstance()
        {
            ExerciseValidationResult result = Validate(
                EnemyProgram + "\nInimigo boneco = new Inimigo(\"Golem de Gelo\", 12, \"gelo\");",
                ValidationCriterion.ConstructAndInstantiateEnemy);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Diagnostic.Code, Is.EqualTo("ENEMY008"));
        }

        [Test]
        public void Validate_DeclareMagoClassCriterion_ReturnsGeneralSuccess()
        {
            ExerciseValidationResult result = Validate(
                "public class Mago {}",
                ValidationCriterion.DeclareMagoClass);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.SatisfiedCriterion, Is.EqualTo(ValidationCriterion.DeclareMagoClass));
            Assert.That(result.Program.ClassName, Is.EqualTo("Mago"));
            Assert.That(result.Program.AttributeNames, Is.Empty);
            Assert.That(result.Program.InitialValues, Is.Empty);
            Assert.That(result.Program.InstanceName, Is.Null);
            Assert.That(result.Diagnostic, Is.Null);
        }

        [Test]
        public void Validate_PrivateAttributesInAnyOrderAndWhitespace_ReturnsAttributeSuccess()
        {
            const string source =
                "public\nclass Mago {\n" +
                "\tprivate int iniciativa;\n" +
                "private int vida; private int velocidadeAtaque;\n" +
                "private int dano;\nprivate int alcance;\n}";

            ExerciseValidationResult result = Validate(
                source,
                ValidationCriterion.AddPrivateAttributes);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.SatisfiedCriterion, Is.EqualTo(ValidationCriterion.AddPrivateAttributes));
            Assert.That(result.Program.ClassName, Is.EqualTo("Mago"));
            Assert.That(result.Program.AttributeNames, Is.EquivalentTo(new[]
            {
                "vida",
                "dano",
                "alcance",
                "iniciativa",
                "velocidadeAtaque"
            }));
            Assert.That(result.Program.InitialValues, Is.Empty);
            Assert.That(result.Diagnostic, Is.Null);
        }

        [Test]
        public void Validate_CompleteConstruction_MapsArgumentsByParameterOrder()
        {
            const string source =
                "public class Mago {\n" +
                "private int vida; private int dano; private int alcance;\n" +
                "private int iniciativa; private int velocidadeAtaque;\n" +
                "public Mago(int dano, int vida, int alcance, int iniciativa, int velocidadeAtaque) {\n" +
                "this.iniciativa = iniciativa; this.vida = vida; this.dano = dano;\n" +
                "this.velocidadeAtaque = velocidadeAtaque; this.alcance = alcance;\n" +
                "}\n}\n" +
                "Mago merlin = new Mago(4, 5, 6, 3, 2);";

            ExerciseValidationResult result = Validate(
                source,
                ValidationCriterion.ConstructAndInstantiateMago);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.SatisfiedCriterion,
                Is.EqualTo(ValidationCriterion.ConstructAndInstantiateMago));
            Assert.That(result.Program.InstanceName, Is.EqualTo("merlin"));
            Assert.That(result.Program.InitialValues["vida"], Is.EqualTo(5));
            Assert.That(result.Program.InitialValues["dano"], Is.EqualTo(4));
            Assert.That(result.Program.InitialValues["alcance"], Is.EqualTo(6));
            Assert.That(result.Program.InitialValues["iniciativa"], Is.EqualTo(3));
            Assert.That(result.Program.InitialValues["velocidadeAtaque"], Is.EqualTo(2));
            Assert.That(result.Program.TotalPoints, Is.EqualTo(20));
            Assert.That(result.Program.RemainingPoints, Is.EqualTo(5));
            Assert.That(result.Diagnostic, Is.Null);
        }

        [TestCase(
            "public class Mago { public int vida; private int dano; private int alcance; " +
            "private int iniciativa; private int velocidadeAtaque; }",
            "CONCEPT001")]
        [TestCase(
            "public class Mago { private float vida; private int dano; private int alcance; " +
            "private int iniciativa; private int velocidadeAtaque; }",
            "CONCEPT002")]
        [TestCase(
            "public class Mago { private int vida; private int vida; private int dano; " +
            "private int alcance; private int iniciativa; private int velocidadeAtaque; }",
            "STRUCT003")]
        [TestCase(
            "public class Mago { private int vida; private int dano; private int alcance; " +
            "private int iniciativa; }",
            "STRUCT004")]
        [TestCase(
            "public class Mago { private int vida private int dano; private int alcance; " +
            "private int iniciativa; private int velocidadeAtaque; }",
            "SYN003")]
        [TestCase(
            "public class Mago { private int vida; private int dano; private int alcance; " +
            "private int iniciativa; private int velocidadeAtaque;",
            "SYN004")]
        [TestCase(
            "public class Mago { int vida; private int dano; private int alcance; " +
            "private int iniciativa; private int velocidadeAtaque; }",
            "CONCEPT001")]
        public void Validate_InvalidAttributeDefinition_ReturnsCategorizedDiagnostic(
            string source,
            string expectedCode)
        {
            ExerciseValidationResult result = Validate(
                source,
                ValidationCriterion.AddPrivateAttributes);

            AssertFailure(result, expectedCode);
        }

        [TestCase("public Mago(", "public Bruxo(", "STRUCT005")]
        [TestCase("public Mago(", "public int Mago(", "STRUCT005")]
        [TestCase("public Mago(", "private Mago(", "STRUCT005")]
        [TestCase("(int vida,", "(, int vida,", "SYN006")]
        [TestCase("int vida, int dano", "int vida,, int dano", "SYN006")]
        [TestCase("int vida, int dano", "float vida, int dano", "CONCEPT002")]
        [TestCase("int vida, int dano", "int vida, int vida", "STRUCT006")]
        [TestCase("this.vida = vida;", "vida = vida;", "CONCEPT003")]
        [TestCase("this.vida = vida;", "this.vida = dano;", "STRUCT007")]
        [TestCase("this.vida = vida;", "", "STRUCT008")]
        [TestCase("= new Mago", "= Mago", "CONCEPT004")]
        [TestCase("new Mago", "new Bruxo", "STRUCT009")]
        [TestCase("(5, 4, 6, 3, 2);", "(5.0, 4, 6, 3, 2);", "CONCEPT002")]
        [TestCase("(5, 4, 6, 3, 2);", "(, 5, 4, 6, 3, 2);", "SYN008")]
        [TestCase("(5, 4, 6, 3, 2);", "(5,, 4, 6, 3, 2);", "SYN008")]
        [TestCase("(5, 4, 6, 3, 2);", "(0, 4, 6, 3, 2);", "GAME001")]
        [TestCase("(5, 4, 6, 3, 2);", "(16, 4, 6, 3, 2);", "GAME001")]
        [TestCase("(5, 4, 6, 3, 2);", "(10, 4, 6, 3, 3);", "GAME002")]
        [TestCase("(5, 4, 6, 3, 2);", "(5, 4, 6, 3, 2)", "SYN008")]
        public void Validate_InvalidConstruction_ReturnsCategorizedDiagnostic(
            string original,
            string replacement,
            string expectedCode)
        {
            string source = CompleteProgram.Replace(original, replacement);

            ExerciseValidationResult result = Validate(
                source,
                ValidationCriterion.ConstructAndInstantiateMago);

            AssertFailure(result, expectedCode);
        }

        [Test]
        public void Validate_ConstructorParameterWithTrailingComma_ReturnsSyntaxDiagnostic()
        {
            string source = CompleteProgram.Replace(
                "int velocidadeAtaque) {",
                "int velocidadeAtaque,) {");

            ExerciseValidationResult result = Validate(
                source,
                ValidationCriterion.ConstructAndInstantiateMago);

            AssertFailure(result, "SYN006");
        }

        [Test]
        public void Validate_InstantiationArgumentWithTrailingComma_ReturnsSyntaxDiagnostic()
        {
            string source = CompleteProgram.Replace(
                "(5, 4, 6, 3, 2);",
                "(5, 4, 6, 3, 2,);");

            ExerciseValidationResult result = Validate(
                source,
                ValidationCriterion.ConstructAndInstantiateMago);

            AssertFailure(result, "SYN008");
        }

        [TestCase("15, 1, 1, 1, 1", 19, 6)]
        [TestCase("5, 5, 5, 5, 5", 25, 0)]
        [TestCase("1, 1, 1, 1, 1", 5, 20)]
        public void Validate_ValidPointDistribution_PreservesTotalAndRemaining(
            string arguments,
            int total,
            int remaining)
        {
            string source = CompleteProgram.Replace("5, 4, 6, 3, 2", arguments);

            ExerciseValidationResult result = Validate(
                source,
                ValidationCriterion.ConstructAndInstantiateMago);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Program.TotalPoints, Is.EqualTo(total));
            Assert.That(result.Program.RemainingPoints, Is.EqualTo(remaining));
        }

        [TestCase("1, 4, 4, 4, 4", "GAME001")]
        [TestCase("11, 2, 2, 2, 2", "GAME001")]
        [TestCase("5, 4, 5, 4, 3", "GAME002")]
        public void Validate_CustomRulesControlIndividualAndTotalLimits(
            string arguments,
            string expectedCode)
        {
            string source = CompleteProgram.Replace("5, 4, 6, 3, 2", arguments);
            TokenizationResult tokenization = new CodeTokenizer().Tokenize(source);
            var exercise = new ExerciseDefinition("phase-one", "Mago", "Construa o Mago.");
            var validator = new ExerciseCodeValidator(new MagoValidationRules(2, 10, 20));

            ExerciseValidationResult result = validator.Validate(
                tokenization.Tokens,
                exercise,
                ValidationCriterion.ConstructAndInstantiateMago);

            AssertFailure(result, expectedCode);
        }

        [Test]
        public void Validate_NullInput_ThrowsArgumentNullException()
        {
            var validator = new ExerciseCodeValidator();
            var exercise = new ExerciseDefinition("phase-one", "Mago", "Construa o Mago.");
            IReadOnlyList<Token> tokens = new CodeTokenizer().Tokenize(
                "public class Mago {}").Tokens;

            Assert.Throws<ArgumentNullException>(() => validator.Validate(
                null,
                exercise,
                ValidationCriterion.AddPrivateAttributes));
            Assert.Throws<ArgumentNullException>(() => validator.Validate(
                tokens,
                null,
                ValidationCriterion.AddPrivateAttributes));
        }

        private static ExerciseValidationResult Validate(
            string source,
            ValidationCriterion criterion)
        {
            TokenizationResult tokenization = new CodeTokenizer().Tokenize(source);
            Assert.That(tokenization.IsSuccess, Is.True);

            var exercise = new ExerciseDefinition(
                "phase-one",
                "Mago",
                "Construa o Mago.");
            return new ExerciseCodeValidator().Validate(
                tokenization.Tokens,
                exercise,
                criterion);
        }

        private static void AssertFailure(ExerciseValidationResult result, string expectedCode)
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.SatisfiedCriterion, Is.Null);
            Assert.That(result.Program, Is.Null);
            Assert.That(result.Diagnostic, Is.Not.Null);
            Assert.That(result.Diagnostic.Code, Is.EqualTo(expectedCode));
            Assert.That(result.Diagnostic.Position.Line, Is.GreaterThanOrEqualTo(1));
            Assert.That(result.Diagnostic.Position.Column, Is.GreaterThanOrEqualTo(1));
            Assert.That(result.Diagnostic.Detail, Is.Not.Empty);
        }
    }
}
