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
            ValidatedMagoProgram program)
        {
            IsSuccess = isSuccess;
            HasDeclaredClass = hasDeclaredClass;
            Diagnostic = diagnostic;
            SatisfiedCriterion = satisfiedCriterion;
            Program = program;
        }

        public bool IsSuccess { get; }

        public bool HasDeclaredClass { get; }

        public Diagnostic Diagnostic { get; }

        public ValidationCriterion? SatisfiedCriterion { get; }

        public ValidatedMagoProgram Program { get; }

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
            return new SubmitCodeResult(
                true,
                hasDeclaredClass,
                null,
                satisfiedCriterion,
                program);
        }

        public static SubmitCodeResult Failure(bool hasDeclaredClass, Diagnostic diagnostic)
        {
            return new SubmitCodeResult(false, hasDeclaredClass, diagnostic, null, null);
        }
    }
}
