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
            var session = CreateSession();
            SubmitCodeUseCase useCase = CreateUseCase(session);

            SubmitCodeResult result = useCase.Execute("public class Mago {}");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.HasDeclaredClass, Is.True);
            Assert.That(result.Diagnostic, Is.Null);
            Assert.That(session.HasDeclaredClass, Is.True);
        }

        [Test]
        public void Execute_InvalidDeclaration_DoesNotAdvanceInitialSession()
        {
            var session = CreateSession();
            SubmitCodeUseCase useCase = CreateUseCase(session);

            SubmitCodeResult result = useCase.Execute("public class Bruxo {}");

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.HasDeclaredClass, Is.False);
            Assert.That(result.Diagnostic.Code, Is.EqualTo("CLASS001"));
            Assert.That(session.HasDeclaredClass, Is.False);
        }

        [Test]
        public void Execute_InvalidDeclarationAfterSuccess_PreservesValidatedState()
        {
            var session = CreateSession();
            SubmitCodeUseCase useCase = CreateUseCase(session);
            useCase.Execute("public class Mago {}");

            SubmitCodeResult result = useCase.Execute("class public Mago {}");

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.HasDeclaredClass, Is.True);
            Assert.That(session.HasDeclaredClass, Is.True);
        }

        [Test]
        public void Execute_SameValidDeclarationTwice_RemainsSuccessfulAndDeclared()
        {
            var session = CreateSession();
            SubmitCodeUseCase useCase = CreateUseCase(session);

            SubmitCodeResult first = useCase.Execute("public class Mago {}");
            SubmitCodeResult second = useCase.Execute("public class Mago {}");

            Assert.That(first.IsSuccess, Is.True);
            Assert.That(second.IsSuccess, Is.True);
            Assert.That(second.HasDeclaredClass, Is.True);
            Assert.That(session.HasDeclaredClass, Is.True);
        }

        [Test]
        public void CanPreview_ValidDeclaration_DoesNotAdvanceSession()
        {
            var session = CreateSession();
            SubmitCodeUseCase useCase = CreateUseCase(session);

            bool canPreview = useCase.CanPreview("public class Mago {}");

            Assert.That(canPreview, Is.True);
            Assert.That(session.HasDeclaredClass, Is.False);
        }

        [Test]
        public void CanPreview_InvalidDeclaration_DoesNotAdvanceSession()
        {
            var session = CreateSession();
            SubmitCodeUseCase useCase = CreateUseCase(session);

            bool canPreview = useCase.CanPreview("public class Bruxo {}");

            Assert.That(canPreview, Is.False);
            Assert.That(session.HasDeclaredClass, Is.False);
        }

        private static LearningSession CreateSession()
        {
            return new LearningSession(new ExerciseDefinition(
                "declare-mago-class",
                "Mago",
                "Declare a classe Mago."));
        }

        private static SubmitCodeUseCase CreateUseCase(LearningSession session)
        {
            return new SubmitCodeUseCase(
                new CodeTokenizer(),
                new ClassDeclarationValidator(),
                session);
        }
    }
}
