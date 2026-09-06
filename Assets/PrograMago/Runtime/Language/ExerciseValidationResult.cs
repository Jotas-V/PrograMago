using System;
using PrograMago.Domain;

namespace PrograMago.Language
{
    public sealed class ExerciseValidationResult
    {
        private ExerciseValidationResult(
            bool isSuccess,
            ValidationCriterion? satisfiedCriterion,
            ValidatedMagoProgram program,
            Diagnostic diagnostic)
        {
            IsSuccess = isSuccess;
            SatisfiedCriterion = satisfiedCriterion;
            Program = program;
            Diagnostic = diagnostic;
        }

        public bool IsSuccess { get; }

        public ValidationCriterion? SatisfiedCriterion { get; }

        public ValidatedMagoProgram Program { get; }

        public Diagnostic Diagnostic { get; }

        public static ExerciseValidationResult Success(
            ValidationCriterion criterion,
            ValidatedMagoProgram program)
        {
            if (program == null)
            {
                throw new ArgumentNullException(nameof(program));
            }

            return new ExerciseValidationResult(true, criterion, program, null);
        }

        public static ExerciseValidationResult Failure(Diagnostic diagnostic)
        {
            if (diagnostic == null)
            {
                throw new ArgumentNullException(nameof(diagnostic));
            }

            return new ExerciseValidationResult(false, null, null, diagnostic);
        }
    }
}
