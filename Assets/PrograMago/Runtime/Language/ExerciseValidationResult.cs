using System;
using System.Collections.Generic;
using PrograMago.Domain;

namespace PrograMago.Language
{
    public sealed class ExerciseValidationResult
    {
        private ExerciseValidationResult(
            bool isSuccess,
            ValidationCriterion? satisfiedCriterion,
            ValidatedMagoProgram program,
            IReadOnlyList<EnemyState> enemies,
            Diagnostic diagnostic)
        {
            IsSuccess = isSuccess;
            SatisfiedCriterion = satisfiedCriterion;
            Program = program;
            Enemies = enemies;
            Diagnostic = diagnostic;
        }

        public bool IsSuccess { get; }

        public ValidationCriterion? SatisfiedCriterion { get; }

        public ValidatedMagoProgram Program { get; }

        public IReadOnlyList<EnemyState> Enemies { get; }

        public Diagnostic Diagnostic { get; }

        public static ExerciseValidationResult Success(
            ValidationCriterion criterion,
            ValidatedMagoProgram program)
        {
            return Success(criterion, program, Array.Empty<EnemyState>());
        }

        public static ExerciseValidationResult Success(
            ValidationCriterion criterion,
            ValidatedMagoProgram program,
            IReadOnlyList<EnemyState> enemies)
        {
            if (program == null)
            {
                throw new ArgumentNullException(nameof(program));
            }

            return new ExerciseValidationResult(
                true,
                criterion,
                program,
                new List<EnemyState>(enemies ?? throw new ArgumentNullException(nameof(enemies))).AsReadOnly(),
                null);
        }

        public static ExerciseValidationResult Failure(Diagnostic diagnostic)
        {
            if (diagnostic == null)
            {
                throw new ArgumentNullException(nameof(diagnostic));
            }

            return new ExerciseValidationResult(false, null, null, null, diagnostic);
        }
    }
}
