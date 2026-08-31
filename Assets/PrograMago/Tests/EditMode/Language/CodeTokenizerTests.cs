using NUnit.Framework;
using PrograMago.Language;

namespace PrograMago.Tests.Language
{
    public sealed class CodeTokenizerTests
    {
        [Test]
        public void Tokenize_ValidEmptyMagoClass_ReturnsAllTokensWithPositions()
        {
            const string source = "public class Mago {\n}";
            var tokenizer = new CodeTokenizer();

            TokenizationResult result = tokenizer.Tokenize(source);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Tokens, Has.Count.EqualTo(5));
            AssertToken(result.Tokens[0], TokenKind.PublicKeyword, "public", 0, 1, 1);
            AssertToken(result.Tokens[1], TokenKind.ClassKeyword, "class", 7, 1, 8);
            AssertToken(result.Tokens[2], TokenKind.Identifier, "Mago", 13, 1, 14);
            AssertToken(result.Tokens[3], TokenKind.LeftBrace, "{", 18, 1, 19);
            AssertToken(result.Tokens[4], TokenKind.RightBrace, "}", 20, 2, 1);
        }

        private static void AssertToken(
            Token token,
            TokenKind kind,
            string lexeme,
            int offset,
            int line,
            int column)
        {
            Assert.That(token.Kind, Is.EqualTo(kind));
            Assert.That(token.Lexeme, Is.EqualTo(lexeme));
            Assert.That(token.Position.Offset, Is.EqualTo(offset));
            Assert.That(token.Position.Line, Is.EqualTo(line));
            Assert.That(token.Position.Column, Is.EqualTo(column));
        }
    }
}
