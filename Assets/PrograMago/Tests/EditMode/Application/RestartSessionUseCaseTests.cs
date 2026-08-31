using NUnit.Framework;
using PrograMago.Application;
using PrograMago.Domain;

namespace PrograMago.Tests.Application
{
    public sealed class RestartSessionUseCaseTests
    {
        [Test]
        public void Execute_AfterProgress_ClearsSessionAndReturnsInitialState()
        {
            var session = new LearningSession(new ExerciseDefinition(
                "declare-mago-class",
                "Mago",
                "Declare a classe Mago."));
            session.Apply(new ValidatedClassDeclaration("Mago"));
            var useCase = new RestartSessionUseCase(session);

            bool hasDeclaredClass = useCase.Execute();

            Assert.That(hasDeclaredClass, Is.False);
            Assert.That(session.HasDeclaredClass, Is.False);
        }
    }
}
