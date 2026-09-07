using System;
using PrograMago.Domain;
using PrograMago.Language;

namespace PrograMago.Application
{
    public sealed class SubmitCodeUseCase
    {
        private readonly CodeTokenizer tokenizer;
        private readonly ExerciseCodeValidator validator;
        private readonly LearningSession session;

        public SubmitCodeUseCase(
            CodeTokenizer tokenizer,
            ExerciseCodeValidator validator,
            LearningSession session)
        {
            this.tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public SubmitCodeResult Execute(string source)
        {
            return Execute(source, ValidationCriterion.DeclareMagoClass);
        }

        public SubmitCodeResult Execute(string source, ValidationCriterion criterion)
        {
            ExerciseValidationResult validation = Validate(source, criterion);
            if (!validation.IsSuccess)
            {
                return SubmitCodeResult.Failure(session.HasDeclaredClass, validation.Diagnostic);
            }

            session.Apply(new ValidatedClassDeclaration(validation.Program.ClassName));
            return SubmitCodeResult.Success(
                session.HasDeclaredClass,
                validation.SatisfiedCriterion.Value,
                validation.Program);
        }

        public bool CanPreview(string source)
        {
            return CanPreview(source, ValidationCriterion.DeclareMagoClass);
        }

        public bool CanPreview(string source, ValidationCriterion criterion)
        {
            return Validate(source, criterion).IsSuccess;
        }

        private ExerciseValidationResult Validate(string source, ValidationCriterion criterion)
        {
            TokenizationResult tokenization = tokenizer.Tokenize(source);
            if (!tokenization.IsSuccess)
            {
                return ExerciseValidationResult.Failure(tokenization.Diagnostic);
            }

            return validator.Validate(
                tokenization.Tokens,
                session.Exercise,
                criterion);
        }
    }
}
