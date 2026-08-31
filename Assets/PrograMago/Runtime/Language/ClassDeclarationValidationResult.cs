using System;
using PrograMago.Domain;

namespace PrograMago.Language
{
    public sealed class ClassDeclarationValidationResult
    {
        private ClassDeclarationValidationResult(
            bool isSuccess,
            ValidatedClassDeclaration declaration,
            Diagnostic diagnostic)
        {
            IsSuccess = isSuccess;
            Declaration = declaration;
            Diagnostic = diagnostic;
        }

        public bool IsSuccess { get; }

        public ValidatedClassDeclaration Declaration { get; }

        public Diagnostic Diagnostic { get; }

        public static ClassDeclarationValidationResult Success(ValidatedClassDeclaration declaration)
        {
            if (declaration == null)
            {
                throw new ArgumentNullException(nameof(declaration));
            }

            return new ClassDeclarationValidationResult(true, declaration, null);
        }

        public static ClassDeclarationValidationResult Failure(Diagnostic diagnostic)
        {
            if (diagnostic == null)
            {
                throw new ArgumentNullException(nameof(diagnostic));
            }

            return new ClassDeclarationValidationResult(false, null, diagnostic);
        }
    }
}
