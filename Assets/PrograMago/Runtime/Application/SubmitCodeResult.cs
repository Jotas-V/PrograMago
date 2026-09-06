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
            ValidationCriterion? satisfiedCriterion)
        {
            IsSuccess = isSuccess;
            HasDeclaredClass = hasDeclaredClass;
            Diagnostic = diagnostic;
            SatisfiedCriterion = satisfiedCriterion;
        }

        public bool IsSuccess { get; }

        public bool HasDeclaredClass { get; }

        public Diagnostic Diagnostic { get; }

        public ValidationCriterion? SatisfiedCriterion { get; }

        public static SubmitCodeResult Success(
            bool hasDeclaredClass,
            ValidationCriterion satisfiedCriterion)
        {
            return new SubmitCodeResult(true, hasDeclaredClass, null, satisfiedCriterion);
        }

        public static SubmitCodeResult Failure(bool hasDeclaredClass, Diagnostic diagnostic)
        {
            return new SubmitCodeResult(false, hasDeclaredClass, diagnostic, null);
        }
    }
}
