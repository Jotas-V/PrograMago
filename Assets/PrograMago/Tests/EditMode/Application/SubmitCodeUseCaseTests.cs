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
            Assert.That(
                result.SatisfiedCriterion,
                Is.EqualTo(ValidationCriterion.DeclareMagoClass));
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
            Assert.That(result.SatisfiedCriterion, Is.Null);
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

        [Test]
        public void SubmitCodeResult_ExposesSatisfiedValidationCriterion()
        {
            System.Reflection.PropertyInfo property = typeof(SubmitCodeResult).GetProperty(
                "SatisfiedCriterion");

            Assert.That(property, Is.Not.Null);
            Assert.That(property.PropertyType, Is.EqualTo(typeof(ValidationCriterion?)));
        }

        [Test]
        public void SubmitCodeUseCase_ExposesCriterionBasedSubmissionContract()
        {
            Assert.That(
                typeof(SubmitCodeUseCase).GetMethod(
                    "Execute",
                    new[] { typeof(string), typeof(ValidationCriterion) })?.ReturnType,
                Is.EqualTo(typeof(SubmitCodeResult)));
            Assert.That(
                typeof(SubmitCodeUseCase).GetMethod(
                    "CanPreview",
                    new[] { typeof(string), typeof(ValidationCriterion) })?.ReturnType,
                Is.EqualTo(typeof(bool)));
            Assert.That(
                typeof(SubmitCodeResult).GetProperty("Program")?.PropertyType,
                Is.EqualTo(typeof(ValidatedMagoProgram)));
        }

        [Test]
        public void Execute_PrivateAttributeCriterion_ReturnsMatchingSuccess()
        {
            var session = CreateSession();
            SubmitCodeUseCase useCase = CreateUseCase(session);
            const string source =
                "public class Mago { private int vida; private int dano; " +
                "private int alcance; private int iniciativa; private int velocidadeAtaque; }";

            SubmitCodeResult result = useCase.Execute(
                source,
                ValidationCriterion.AddPrivateAttributes);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.SatisfiedCriterion, Is.EqualTo(ValidationCriterion.AddPrivateAttributes));
            Assert.That(result.Program.AttributeNames, Has.Count.EqualTo(5));
        }

        [Test]
        public void Execute_ConstructionCriterion_ReturnsValidatedPointDistribution()
        {
            var session = CreateSession();
            SubmitCodeUseCase useCase = CreateUseCase(session);
            const string source =
                "public class Mago { " +
                "private int vida; private int dano; private int alcance; " +
                "private int iniciativa; private int velocidadeAtaque; " +
                "public Mago(int vida, int dano, int alcance, int iniciativa, int velocidadeAtaque) { " +
                "this.vida = vida; this.dano = dano; this.alcance = alcance; " +
                "this.iniciativa = iniciativa; this.velocidadeAtaque = velocidadeAtaque; } } " +
                "Mago mago = new Mago(5, 5, 5, 5, 5);";

            SubmitCodeResult result = useCase.Execute(
                source,
                ValidationCriterion.ConstructAndInstantiateMago);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.SatisfiedCriterion,
                Is.EqualTo(ValidationCriterion.ConstructAndInstantiateMago));
            Assert.That(result.Program.TotalPoints, Is.EqualTo(25));
            Assert.That(result.Program.RemainingPoints, Is.Zero);
        }

        [Test]
        public void Execute_CriterionOutsideTcc8_ReturnsControlledFailure()
        {
            var session = CreateSession();
            SubmitCodeUseCase useCase = CreateUseCase(session);
            SubmitCodeResult result = null;

            Assert.DoesNotThrow(() => result = useCase.Execute(
                "public class Inimigo {}",
                ValidationCriterion.ConstructAndInstantiateEnemy));
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Diagnostic.Code, Is.EqualTo("VALID001"));
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
                new ExerciseCodeValidator(),
                session);
        }
    }
}
