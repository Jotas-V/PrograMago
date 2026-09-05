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

        [TestCase("public\tclass Mago { }")]
        [TestCase("public\r\nclass\r\nMago\r\n{\r\n}")]
        [TestCase("  public   class   Mago   {   }  ")]
        public void Tokenize_EquivalentWhitespace_ReturnsExpectedTokenSequence(string source)
        {
            var tokenizer = new CodeTokenizer();

            TokenizationResult result = tokenizer.Tokenize(source);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Tokens, Has.Count.EqualTo(5));
            Assert.That(result.Tokens[0].Kind, Is.EqualTo(TokenKind.PublicKeyword));
            Assert.That(result.Tokens[1].Kind, Is.EqualTo(TokenKind.ClassKeyword));
            Assert.That(result.Tokens[2].Kind, Is.EqualTo(TokenKind.Identifier));
            Assert.That(result.Tokens[3].Kind, Is.EqualTo(TokenKind.LeftBrace));
            Assert.That(result.Tokens[4].Kind, Is.EqualTo(TokenKind.RightBrace));
        }

        [Test]
        public void Tokenize_NullSource_ReturnsEmptySuccessfulResult()
        {
            var tokenizer = new CodeTokenizer();

            TokenizationResult result = tokenizer.Tokenize(null);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Tokens, Is.Empty);
            Assert.That(result.Diagnostic, Is.Null);
        }

        [Test]
        public void Tokenize_UnknownCharacter_ReturnsPositionedLexicalDiagnostic()
        {
            var tokenizer = new CodeTokenizer();

            TokenizationResult result = tokenizer.Tokenize("public @ class Mago {}");

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Tokens, Is.Empty);
            Assert.That(result.Diagnostic.Code, Is.EqualTo("LEX001"));
            Assert.That(result.Diagnostic.Detail, Is.EqualTo("@"));
            Assert.That(result.Diagnostic.Position.Offset, Is.EqualTo(7));
            Assert.That(result.Diagnostic.Position.Line, Is.EqualTo(1));
            Assert.That(result.Diagnostic.Position.Column, Is.EqualTo(8));
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
