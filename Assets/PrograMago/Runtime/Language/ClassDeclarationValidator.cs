using System;
using System.Collections.Generic;
using PrograMago.Domain;

namespace PrograMago.Language
{
    public sealed class ClassDeclarationValidator
    {
        private static readonly TokenKind[] ExpectedKinds =
        {
            TokenKind.PublicKeyword,
            TokenKind.ClassKeyword,
            TokenKind.Identifier,
            TokenKind.LeftBrace,
            TokenKind.RightBrace
        };

        public ClassDeclarationValidationResult Validate(
            IReadOnlyList<Token> tokens,
            ExerciseDefinition exercise)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            if (exercise == null)
            {
                throw new ArgumentNullException(nameof(exercise));
            }

            if (tokens.Count != ExpectedKinds.Length)
            {
                return FailureAt(tokens, "SYN001", "A declaração deve conter somente a classe vazia.");
            }

            for (int index = 0; index < ExpectedKinds.Length; index++)
            {
                if (tokens[index].Kind != ExpectedKinds[index])
                {
                    return ClassDeclarationValidationResult.Failure(new Diagnostic(
                        "SYN001",
                        tokens[index].Position,
                        "Estrutura esperada: public class Nome {}."));
                }
            }

            Token className = tokens[2];
            if (!string.Equals(className.Lexeme, exercise.ExpectedClassName, StringComparison.Ordinal))
            {
                return ClassDeclarationValidationResult.Failure(new Diagnostic(
                    "CLASS001",
                    className.Position,
                    exercise.ExpectedClassName));
            }

            return ClassDeclarationValidationResult.Success(
                new ValidatedClassDeclaration(className.Lexeme));
        }

        private static ClassDeclarationValidationResult FailureAt(
            IReadOnlyList<Token> tokens,
            string code,
            string detail)
        {
            SourcePosition position = tokens.Count == 0
                ? new SourcePosition(0, 1, 1)
                : tokens[tokens.Count - 1].Position;
            return ClassDeclarationValidationResult.Failure(new Diagnostic(code, position, detail));
        }
    }
}
