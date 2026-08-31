using System;
using PrograMago.Domain;
using PrograMago.Language;

namespace PrograMago.Application
{
    public sealed class SubmitCodeUseCase
    {
        private readonly CodeTokenizer tokenizer;
        private readonly ClassDeclarationValidator validator;
        private readonly LearningSession session;

        public SubmitCodeUseCase(
            CodeTokenizer tokenizer,
            ClassDeclarationValidator validator,
            LearningSession session)
        {
            this.tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public SubmitCodeResult Execute(string source)
        {
            TokenizationResult tokenization = tokenizer.Tokenize(source);
            if (!tokenization.IsSuccess)
            {
                return SubmitCodeResult.Failure(session.HasDeclaredClass, tokenization.Diagnostic);
            }

            ClassDeclarationValidationResult validation = validator.Validate(
                tokenization.Tokens,
                session.Exercise);
            if (!validation.IsSuccess)
            {
                return SubmitCodeResult.Failure(session.HasDeclaredClass, validation.Diagnostic);
            }

            session.Apply(validation.Declaration);
            return SubmitCodeResult.Success(session.HasDeclaredClass);
        }
    }
}
