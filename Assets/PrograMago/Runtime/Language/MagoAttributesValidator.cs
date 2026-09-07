using System;
using System.Collections.Generic;
using PrograMago.Domain;

namespace PrograMago.Language
{
    internal sealed class MagoAttributesValidator
    {
        private readonly MagoValidationRules rules;

        internal static readonly string[] RequiredAttributeNames =
        {
            "vida",
            "dano",
            "alcance",
            "iniciativa",
            "velocidadeAtaque"
        };

        public MagoAttributesValidator(MagoValidationRules rules)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public ExerciseValidationResult Validate(
            IReadOnlyList<Token> tokens,
            ExerciseDefinition exercise)
        {
            int index = 0;
            ExerciseValidationResult headerFailure = ValidateHeader(tokens, exercise, ref index);
            if (headerFailure != null)
            {
                return headerFailure;
            }

            var attributes = new List<string>();
            while (index < tokens.Count && tokens[index].Kind != TokenKind.RightBrace)
            {
                Token visibility = tokens[index];
                if (visibility.Kind == TokenKind.PublicKeyword)
                {
                    return Failure("CONCEPT001", visibility, "Os atributos do Mago devem ser private.");
                }

                if (visibility.Kind == TokenKind.IntKeyword)
                {
                    return Failure("CONCEPT001", visibility, "Use private antes do tipo do atributo.");
                }

                if (visibility.Kind != TokenKind.PrivateKeyword)
                {
                    return Failure("STRUCT001", visibility, "Era esperada uma declaração de atributo.");
                }

                index++;
                if (index >= tokens.Count || tokens[index].Kind != TokenKind.IntKeyword)
                {
                    return FailureAt(
                        tokens,
                        index,
                        "CONCEPT002",
                        "Todos os cinco atributos devem usar o tipo int.");
                }

                index++;
                if (index >= tokens.Count || tokens[index].Kind != TokenKind.Identifier)
                {
                    return FailureAt(tokens, index, "SYN002", "Informe o nome do atributo.");
                }

                Token name = tokens[index++];
                if (Array.IndexOf(RequiredAttributeNames, name.Lexeme) < 0)
                {
                    return Failure("STRUCT002", name, $"Atributo não esperado: {name.Lexeme}.");
                }

                if (attributes.Contains(name.Lexeme))
                {
                    return Failure("STRUCT003", name, $"O atributo {name.Lexeme} foi declarado mais de uma vez.");
                }

                attributes.Add(name.Lexeme);
                if (index >= tokens.Count || tokens[index].Kind != TokenKind.Semicolon)
                {
                    return FailureAt(
                        tokens,
                        index,
                        "SYN003",
                        $"Finalize o atributo {name.Lexeme} com ponto e vírgula.");
                }

                index++;
            }

            if (attributes.Count != RequiredAttributeNames.Length)
            {
                return FailureAt(
                    tokens,
                    index,
                    "STRUCT004",
                    "Declare uma vez cada atributo: vida, dano, alcance, iniciativa e velocidadeAtaque.");
            }

            if (index >= tokens.Count || tokens[index].Kind != TokenKind.RightBrace)
            {
                return FailureAt(tokens, index, "SYN004", "Feche a classe Mago com }.");
            }

            index++;
            if (index != tokens.Count)
            {
                return Failure("SYN005", tokens[index], "Não adicione conteúdo depois da classe nesta batalha.");
            }

            return ExerciseValidationResult.Success(
                ValidationCriterion.AddPrivateAttributes,
                new ValidatedMagoProgram(
                    exercise.ExpectedClassName,
                    attributes,
                    null,
                    new Dictionary<string, int>(),
                    rules.PointBudget));
        }

        internal static ExerciseValidationResult ValidateHeader(
            IReadOnlyList<Token> tokens,
            ExerciseDefinition exercise,
            ref int index)
        {
            TokenKind[] expected =
            {
                TokenKind.PublicKeyword,
                TokenKind.ClassKeyword,
                TokenKind.Identifier,
                TokenKind.LeftBrace
            };

            for (int expectedIndex = 0; expectedIndex < expected.Length; expectedIndex++)
            {
                if (index >= tokens.Count || tokens[index].Kind != expected[expectedIndex])
                {
                    return FailureAt(
                        tokens,
                        index,
                        "SYN001",
                        "Comece com public class Mago {.");
                }

                index++;
            }

            Token className = tokens[2];
            if (!string.Equals(className.Lexeme, exercise.ExpectedClassName, StringComparison.Ordinal))
            {
                return Failure("CLASS001", className, exercise.ExpectedClassName);
            }

            return null;
        }

        internal static ExerciseValidationResult Failure(
            string code,
            Token token,
            string detail)
        {
            return ExerciseValidationResult.Failure(new Diagnostic(code, token.Position, detail));
        }

        internal static ExerciseValidationResult FailureAt(
            IReadOnlyList<Token> tokens,
            int index,
            string code,
            string detail)
        {
            SourcePosition position = index < tokens.Count
                ? tokens[index].Position
                : tokens.Count == 0
                    ? new SourcePosition(0, 1, 1)
                    : tokens[tokens.Count - 1].Position;
            return ExerciseValidationResult.Failure(new Diagnostic(code, position, detail));
        }
    }
}
