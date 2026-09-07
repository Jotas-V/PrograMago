using System;
using System.Collections.Generic;
using PrograMago.Domain;

namespace PrograMago.Language
{
    public sealed class ExerciseCodeValidator
    {
        private readonly MagoValidationRules rules;

        public ExerciseCodeValidator()
            : this(new MagoValidationRules(1, 15, 25))
        {
        }

        public ExerciseCodeValidator(MagoValidationRules rules)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public ExerciseValidationResult Validate(
            IReadOnlyList<Token> tokens,
            ExerciseDefinition exercise,
            ValidationCriterion criterion)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            if (exercise == null)
            {
                throw new ArgumentNullException(nameof(exercise));
            }

            if (criterion == ValidationCriterion.AddPrivateAttributes)
            {
                return new MagoAttributesValidator(rules).Validate(tokens, exercise);
            }

            if (criterion == ValidationCriterion.ConstructAndInstantiateMago)
            {
                return new MagoConstructionValidator(rules).Validate(tokens, exercise);
            }

            if (criterion == ValidationCriterion.DeclareMagoClass)
            {
                ClassDeclarationValidationResult result = new ClassDeclarationValidator().Validate(
                    tokens,
                    exercise);
                if (!result.IsSuccess)
                {
                    return ExerciseValidationResult.Failure(result.Diagnostic);
                }

                var program = new ValidatedMagoProgram(
                    result.Declaration.Name,
                    Array.Empty<string>(),
                    null,
                    new Dictionary<string, int>(),
                    rules.PointBudget);
                return ExerciseValidationResult.Success(criterion, program);
            }

            SourcePosition position = tokens.Count == 0
                ? new SourcePosition(0, 1, 1)
                : tokens[0].Position;
            return ExerciseValidationResult.Failure(new Diagnostic(
                "VALID001",
                position,
                "A validação desta batalha ainda não está disponível."));
        }
    }
}
