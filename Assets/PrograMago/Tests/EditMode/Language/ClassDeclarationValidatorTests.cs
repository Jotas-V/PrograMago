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
            var exercise = new ExerciseDefinition(
                "declare-mago-class",
                "Mago",
                "Declare a classe Mago.");
            TokenizationResult tokenization = new CodeTokenizer().Tokenize("public class Mago {}");
            var validator = new ClassDeclarationValidator();

            ClassDeclarationValidationResult result = validator.Validate(tokenization.Tokens, exercise);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Declaration.Name, Is.EqualTo("Mago"));
            Assert.That(result.Diagnostic, Is.Null);
        }
    }
}
