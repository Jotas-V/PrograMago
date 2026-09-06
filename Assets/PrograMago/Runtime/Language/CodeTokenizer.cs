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

                if (char.IsDigit(current))
                {
                    int start = offset;
                    while (offset < source.Length && char.IsDigit(source[offset]))
                    {
                        offset++;
                        column++;
                    }

                    bool isFloat = offset + 1 < source.Length &&
                        source[offset] == '.' &&
                        char.IsDigit(source[offset + 1]);
                    if (isFloat)
                    {
                        offset++;
                        column++;
                        while (offset < source.Length && char.IsDigit(source[offset]))
                        {
                            offset++;
                            column++;
                        }
                    }

                    if (offset < source.Length && IsNumberContinuation(source[offset]))
                    {
                        return MalformedNumber(source, start, offset, position);
                    }

                    string lexeme = source.Substring(start, offset - start);
                    tokens.Add(new Token(
                        isFloat ? TokenKind.FloatLiteral : TokenKind.IntegerLiteral,
                        lexeme,
                        position));
                    continue;
                }

                if (current == '.' &&
                    offset + 1 < source.Length &&
                    char.IsDigit(source[offset + 1]))
                {
                    return MalformedNumber(source, offset, offset, position);
                }

                if (current == '"')
                {
                    int start = offset;
                    offset++;
                    column++;

                    while (offset < source.Length &&
                        source[offset] != '"' &&
                        source[offset] != '\r' &&
                        source[offset] != '\n' &&
                        source[offset] != '\\')
                    {
                        offset++;
                        column++;
                    }

                    if (offset >= source.Length ||
                        source[offset] == '\r' ||
                        source[offset] == '\n')
                    {
                        return TokenizationResult.Failure(new Diagnostic(
                            "LEX002",
                            position,
                            "String não terminada."));
                    }

                    if (source[offset] == '\\')
                    {
                        int detailLength = offset + 1 < source.Length ? 2 : 1;
                        string escape = source.Substring(offset, detailLength);
                        return TokenizationResult.Failure(new Diagnostic(
                            "LEX004",
                            new SourcePosition(offset, line, column),
                            $"Sequência de escape não permitida: '{escape}'."));
                    }

                    offset++;
                    column++;
                    string lexeme = source.Substring(start, offset - start);
                    tokens.Add(new Token(TokenKind.StringLiteral, lexeme, position));
                    continue;
                }

                if (TryGetSymbolKind(current, out TokenKind symbolKind))
                {
                    tokens.Add(new Token(
                        symbolKind,
                        current.ToString(),
                        position));
                    offset++;
                    column++;
                    continue;
                }

                return TokenizationResult.Failure(new Diagnostic(
                    "LEX001",
                    position,
                    $"Caractere não reconhecido: '{current}'."));
            }

            return TokenizationResult.Success(tokens.AsReadOnly());
        }

        private static TokenizationResult MalformedNumber(
            string source,
            int start,
            int continuationOffset,
            SourcePosition position)
        {
            int end = continuationOffset;
            while (end < source.Length && IsNumberContinuation(source[end]))
            {
                end++;
            }

            string number = source.Substring(start, end - start);
            return TokenizationResult.Failure(new Diagnostic(
                "LEX003",
                position,
                $"Número malformado: '{number}'."));
        }

        private static bool TryGetSymbolKind(char value, out TokenKind kind)
        {
            switch (value)
            {
                case '{':
                    kind = TokenKind.LeftBrace;
                    return true;
                case '}':
                    kind = TokenKind.RightBrace;
                    return true;
                case '(':
                    kind = TokenKind.LeftParenthesis;
                    return true;
                case ')':
                    kind = TokenKind.RightParenthesis;
                    return true;
                case ';':
                    kind = TokenKind.Semicolon;
                    return true;
                case ',':
                    kind = TokenKind.Comma;
                    return true;
                case '.':
                    kind = TokenKind.Dot;
                    return true;
                case '=':
                    kind = TokenKind.Equals;
                    return true;
                case '@':
                    kind = TokenKind.At;
                    return true;
                default:
                    kind = default;
                    return false;
            }
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

        private static bool IsNumberContinuation(char value)
        {
            return IsIdentifierPart(value) || value == '.';
        }

        private static TokenKind KeywordOrIdentifier(string lexeme)
        {
            switch (lexeme)
            {
                case "public":
                    return TokenKind.PublicKeyword;
                case "private":
                    return TokenKind.PrivateKeyword;
                case "class":
                    return TokenKind.ClassKeyword;
                case "new":
                    return TokenKind.NewKeyword;
                case "this":
                    return TokenKind.ThisKeyword;
                case "extends":
                    return TokenKind.ExtendsKeyword;
                case "super":
                    return TokenKind.SuperKeyword;
                case "return":
                    return TokenKind.ReturnKeyword;
                case "void":
                    return TokenKind.VoidKeyword;
                case "int":
                    return TokenKind.IntKeyword;
                case "float":
                    return TokenKind.FloatKeyword;
                case "String":
                    return TokenKind.StringType;
                default:
                    return TokenKind.Identifier;
            }
        }
    }
}
