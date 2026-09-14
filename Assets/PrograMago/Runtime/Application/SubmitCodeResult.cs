using System;
using System.Collections.Generic;
using PrograMago.Domain;
using PrograMago.Language;

namespace PrograMago.Application
{
    public sealed class SubmitCodeResult
    {
        private SubmitCodeResult(
            bool isSuccess,
            bool hasDeclaredClass,
            Diagnostic diagnostic,
            ValidationCriterion? satisfiedCriterion,
            ValidatedMagoProgram program,
            IReadOnlyList<EnemyState> enemies)
        {
            IsSuccess = isSuccess;
            HasDeclaredClass = hasDeclaredClass;
            Diagnostic = diagnostic;
            SatisfiedCriterion = satisfiedCriterion;
            Program = program;
            Enemies = enemies;
        }

        public bool IsSuccess { get; }

        public bool HasDeclaredClass { get; }

        public Diagnostic Diagnostic { get; }

        public ValidationCriterion? SatisfiedCriterion { get; }

        public ValidatedMagoProgram Program { get; }

        public IReadOnlyList<EnemyState> Enemies { get; }

        public static SubmitCodeResult Success(
            bool hasDeclaredClass,
            ValidationCriterion satisfiedCriterion)
        {
            return Success(hasDeclaredClass, satisfiedCriterion, null);
        }

        public static SubmitCodeResult Success(
            bool hasDeclaredClass,
            ValidationCriterion satisfiedCriterion,
            ValidatedMagoProgram program)
        {
            return Success(hasDeclaredClass, satisfiedCriterion, program, Array.Empty<EnemyState>());
        }

        public static SubmitCodeResult Success(
            bool hasDeclaredClass,
            ValidationCriterion satisfiedCriterion,
            ValidatedMagoProgram program,
            IReadOnlyList<EnemyState> enemies)
        {
            return new SubmitCodeResult(
                true,
                hasDeclaredClass,
                null,
                satisfiedCriterion,
                program,
                new List<EnemyState>(enemies ?? throw new ArgumentNullException(nameof(enemies))).AsReadOnly());
        }

        public static SubmitCodeResult Failure(bool hasDeclaredClass, Diagnostic diagnostic)
        {
            return new SubmitCodeResult(false, hasDeclaredClass, diagnostic, null, null, null);
        }
    }
}
