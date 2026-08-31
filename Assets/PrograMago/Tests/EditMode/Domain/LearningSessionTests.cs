using NUnit.Framework;
using PrograMago.Domain;

namespace PrograMago.Tests.Domain
{
    public sealed class LearningSessionTests
    {
        [Test]
        public void Apply_ExpectedValidatedDeclaration_MarksClassAsDeclared()
        {
            var exercise = new ExerciseDefinition(
                "declare-mago-class",
                "Mago",
                "Declare a classe Mago.");
            var session = new LearningSession(exercise);

            session.Apply(new ValidatedClassDeclaration("Mago"));

            Assert.That(session.HasDeclaredClass, Is.True);
        }

        [Test]
        public void Restart_DeclaredClass_ReturnsToInitialState()
        {
            var exercise = new ExerciseDefinition(
                "declare-mago-class",
                "Mago",
                "Declare a classe Mago.");
            var session = new LearningSession(exercise);
            session.Apply(new ValidatedClassDeclaration("Mago"));

            session.Restart();

            Assert.That(session.HasDeclaredClass, Is.False);
        }
    }
}
