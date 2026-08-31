using NUnit.Framework;
using PrograMago.Application;
using PrograMago.Domain;
using PrograMago.Language;

namespace PrograMago.Tests.Application
{
    public sealed class SubmitCodeUseCaseTests
    {
        [Test]
        public void Execute_ValidDeclaration_UpdatesSessionAndReturnsSuccess()
        {
            var exercise = new ExerciseDefinition(
                "declare-mago-class",
                "Mago",
                "Declare a classe Mago.");
            var session = new LearningSession(exercise);
            var useCase = new SubmitCodeUseCase(
                new CodeTokenizer(),
                new ClassDeclarationValidator(),
                session);

            SubmitCodeResult result = useCase.Execute("public class Mago {}");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.HasDeclaredClass, Is.True);
            Assert.That(result.Diagnostic, Is.Null);
            Assert.That(session.HasDeclaredClass, Is.True);
        }
    }
}
