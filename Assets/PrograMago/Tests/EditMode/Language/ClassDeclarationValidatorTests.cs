using NUnit.Framework;
using PrograMago.Domain;
using PrograMago.Language;

namespace PrograMago.Tests.Language
{
    public sealed class ClassDeclarationValidatorTests
    {
        [Test]
        public void Validate_ExpectedEmptyClass_ReturnsValidatedDeclaration()
        {
            ClassDeclarationValidationResult result = Validate("public class Mago {}");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Declaration.Name, Is.EqualTo("Mago"));
            Assert.That(result.Diagnostic, Is.Null);
        }

        [TestCase("")]
        [TestCase("public class Mago {")]
        [TestCase("public class Mago {} extra")]
        public void Validate_IncompleteOrExtraStructure_ReturnsSyntaxDiagnostic(string source)
        {
            ClassDeclarationValidationResult result = Validate(source);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Declaration, Is.Null);
            Assert.That(result.Diagnostic.Code, Is.EqualTo("SYN001"));
        }

        [Test]
        public void Validate_IncorrectKeywordOrder_ReturnsDiagnosticAtFirstUnexpectedToken()
        {
            ClassDeclarationValidationResult result = Validate("class public Mago {}");

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Diagnostic.Code, Is.EqualTo("SYN001"));
            Assert.That(result.Diagnostic.Position.Line, Is.EqualTo(1));
            Assert.That(result.Diagnostic.Position.Column, Is.EqualTo(1));
        }

        [Test]
        public void Validate_UnexpectedClassName_ReturnsExpectedNameDiagnostic()
        {
            ClassDeclarationValidationResult result = Validate("public class Bruxo {}");

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Diagnostic.Code, Is.EqualTo("CLASS001"));
            Assert.That(result.Diagnostic.Detail, Is.EqualTo("Mago"));
            Assert.That(result.Diagnostic.Position.Line, Is.EqualTo(1));
            Assert.That(result.Diagnostic.Position.Column, Is.EqualTo(14));
        }

        private static ClassDeclarationValidationResult Validate(string source)
        {
            var exercise = new ExerciseDefinition(
                "declare-mago-class",
                "Mago",
                "Declare a classe Mago.");
            TokenizationResult tokenization = new CodeTokenizer().Tokenize(source);
            Assert.That(tokenization.IsSuccess, Is.True);

            return new ClassDeclarationValidator().Validate(tokenization.Tokens, exercise);
        }
    }
}
