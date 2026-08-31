using System;
using System.Collections.Generic;

namespace PrograMago.Language
{
    public sealed class TokenizationResult
    {
        private TokenizationResult(bool isSuccess, IReadOnlyList<Token> tokens, Diagnostic diagnostic)
        {
            IsSuccess = isSuccess;
            Tokens = tokens;
            Diagnostic = diagnostic;
        }

        public bool IsSuccess { get; }

        public IReadOnlyList<Token> Tokens { get; }

        public Diagnostic Diagnostic { get; }

        public static TokenizationResult Success(IReadOnlyList<Token> tokens)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            return new TokenizationResult(true, tokens, null);
        }

        public static TokenizationResult Failure(Diagnostic diagnostic)
        {
            if (diagnostic == null)
            {
                throw new ArgumentNullException(nameof(diagnostic));
            }

            return new TokenizationResult(false, Array.Empty<Token>(), diagnostic);
        }
    }
}
