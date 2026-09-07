using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PrograMago.Domain;
using PrograMago.Language;

namespace PrograMago.Tests.Language
{
    public sealed class PhaseOneValidationContractTests
    {
        [TestCase("PrograMago.Domain.ValidatedMagoProgram")]
        [TestCase("PrograMago.Domain.MagoValidationRules")]
        [TestCase("PrograMago.Language.ExerciseValidationResult")]
        [TestCase("PrograMago.Language.ExerciseCodeValidator")]
        public void PhaseOneValidation_ExposesApprovedContracts(string fullTypeName)
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(fullTypeName))
                .FirstOrDefault(candidate => candidate != null);

            Assert.That(type, Is.Not.Null);
        }

        [Test]
        public void ExerciseValidationResult_ExposesExclusiveOutcome()
        {
            Type type = typeof(ExerciseValidationResult);

            Assert.That(type.GetProperty("IsSuccess")?.PropertyType, Is.EqualTo(typeof(bool)));
            Assert.That(
                type.GetProperty("SatisfiedCriterion")?.PropertyType,
                Is.EqualTo(typeof(ValidationCriterion?)));
            Assert.That(
                type.GetProperty("Program")?.PropertyType,
                Is.EqualTo(typeof(ValidatedMagoProgram)));
            Assert.That(
                type.GetProperty("Diagnostic")?.PropertyType,
                Is.EqualTo(typeof(Diagnostic)));
        }

        [Test]
        public void ValidatedMagoProgram_ExposesDataForMapping()
        {
            Type type = typeof(ValidatedMagoProgram);

            Assert.That(type.GetProperty("ClassName")?.PropertyType, Is.EqualTo(typeof(string)));
            Assert.That(
                type.GetProperty("AttributeNames")?.PropertyType,
                Is.EqualTo(typeof(IReadOnlyList<string>)));
            Assert.That(type.GetProperty("InstanceName")?.PropertyType, Is.EqualTo(typeof(string)));
            Assert.That(
                type.GetProperty("InitialValues")?.PropertyType,
                Is.EqualTo(typeof(IReadOnlyDictionary<string, int>)));
            Assert.That(type.GetProperty("TotalPoints")?.PropertyType, Is.EqualTo(typeof(int)));
            Assert.That(type.GetProperty("RemainingPoints")?.PropertyType, Is.EqualTo(typeof(int)));
        }

        [Test]
        public void ExerciseCodeValidator_ExposesCriterionBasedValidation()
        {
            var parameterTypes = new[]
            {
                typeof(IReadOnlyList<Token>),
                typeof(ExerciseDefinition),
                typeof(ValidationCriterion)
            };

            Assert.That(
                typeof(ExerciseCodeValidator).GetMethod("Validate", parameterTypes)?.ReturnType,
                Is.EqualTo(typeof(ExerciseValidationResult)));
        }

        [Test]
        public void MagoValidationRules_CanBeInjectedIntoExerciseValidator()
        {
            Type rulesType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("PrograMago.Domain.MagoValidationRules"))
                .FirstOrDefault(candidate => candidate != null);

            Assert.That(rulesType, Is.Not.Null);
            Assert.That(
                rulesType.GetConstructor(new[] { typeof(int), typeof(int), typeof(int) }),
                Is.Not.Null);
            Assert.That(
                typeof(ExerciseCodeValidator).GetConstructor(new[] { rulesType }),
                Is.Not.Null);
        }
    }
}
