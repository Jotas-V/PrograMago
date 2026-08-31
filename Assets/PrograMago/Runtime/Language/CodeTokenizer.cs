using System;
using System.Collections.Generic;

namespace PrograMago.Language
{
    public sealed class CodeTokenizer
    {
        public TokenizationResult Tokenize(string source)
        {
            source = source ?? string.Empty;

            var tokens = new List<Token>();
            var offset = 0;
            var line = 1;
            var column = 1;

            while (offset < source.Length)
            {
                char current = source[offset];

                if (char.IsWhiteSpace(current))
                {
                    AdvanceWhitespace(source, ref offset, ref line, ref column);
                    continue;
                }

                var position = new SourcePosition(offset, line, column);

                if (IsIdentifierStart(current))
                {
                    int start = offset;
                    do
                    {
                        offset++;
                        column++;
                    }
                    while (offset < source.Length && IsIdentifierPart(source[offset]));

                    string lexeme = source.Substring(start, offset - start);
                    tokens.Add(new Token(KeywordOrIdentifier(lexeme), lexeme, position));
                    continue;
                }

                if (current == '{' || current == '}')
                {
                    tokens.Add(new Token(
                        current == '{' ? TokenKind.LeftBrace : TokenKind.RightBrace,
                        current.ToString(),
                        position));
                    offset++;
                    column++;
                    continue;
                }

                return TokenizationResult.Failure(new Diagnostic("LEX001", position, current.ToString()));
            }

            return TokenizationResult.Success(tokens.AsReadOnly());
        }

        private static void AdvanceWhitespace(string source, ref int offset, ref int line, ref int column)
        {
            char current = source[offset];
            offset++;

            if (current == '\r')
            {
                if (offset < source.Length && source[offset] == '\n')
                {
                    offset++;
                }

                line++;
                column = 1;
                return;
            }

            if (current == '\n')
            {
                line++;
                column = 1;
                return;
            }

            column++;
        }

        private static bool IsIdentifierStart(char value)
        {
            return char.IsLetter(value) || value == '_';
        }

        private static bool IsIdentifierPart(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_';
        }

        private static TokenKind KeywordOrIdentifier(string lexeme)
        {
            if (string.Equals(lexeme, "public", StringComparison.Ordinal))
            {
                return TokenKind.PublicKeyword;
            }

            if (string.Equals(lexeme, "class", StringComparison.Ordinal))
            {
                return TokenKind.ClassKeyword;
            }

            return TokenKind.Identifier;
        }
    }
}
