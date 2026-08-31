using PrograMago.Language;

namespace PrograMago.Application
{
    public sealed class SubmitCodeResult
    {
        private SubmitCodeResult(bool isSuccess, bool hasDeclaredClass, Diagnostic diagnostic)
        {
            IsSuccess = isSuccess;
            HasDeclaredClass = hasDeclaredClass;
            Diagnostic = diagnostic;
        }

        public bool IsSuccess { get; }

        public bool HasDeclaredClass { get; }

        public Diagnostic Diagnostic { get; }

        public static SubmitCodeResult Success(bool hasDeclaredClass)
        {
            return new SubmitCodeResult(true, hasDeclaredClass, null);
        }

        public static SubmitCodeResult Failure(bool hasDeclaredClass, Diagnostic diagnostic)
        {
            return new SubmitCodeResult(false, hasDeclaredClass, diagnostic);
        }
    }
}
