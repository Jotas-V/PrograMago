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

        [TestCase("public", TokenKind.PublicKeyword)]
        [TestCase("private", TokenKind.PrivateKeyword)]
        [TestCase("class", TokenKind.ClassKeyword)]
        [TestCase("new", TokenKind.NewKeyword)]
        [TestCase("this", TokenKind.ThisKeyword)]
        [TestCase("extends", TokenKind.ExtendsKeyword)]
        [TestCase("super", TokenKind.SuperKeyword)]
        [TestCase("return", TokenKind.ReturnKeyword)]
        [TestCase("void", TokenKind.VoidKeyword)]
        [TestCase("int", TokenKind.IntKeyword)]
        [TestCase("float", TokenKind.FloatKeyword)]
        [TestCase("String", TokenKind.StringType)]
        public void Tokenize_ReservedWord_ReturnsSpecificTokenKind(
            string source,
            TokenKind expectedKind)
        {
            var tokenizer = new CodeTokenizer();

            TokenizationResult result = tokenizer.Tokenize(source);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Tokens, Has.Count.EqualTo(1));
            Assert.That(result.Tokens[0].Kind, Is.EqualTo(expectedKind));
            Assert.That(result.Tokens[0].Lexeme, Is.EqualTo(source));
        }

        [Test]
        public void Tokenize_OrdinaryName_ReturnsIdentifier()
        {
            var tokenizer = new CodeTokenizer();

            TokenizationResult result = tokenizer.Tokenize("Override");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Tokens, Has.Count.EqualTo(1));
            Assert.That(result.Tokens[0].Kind, Is.EqualTo(TokenKind.Identifier));
        }

        [TestCase("{", TokenKind.LeftBrace)]
        [TestCase("}", TokenKind.RightBrace)]
        [TestCase("(", TokenKind.LeftParenthesis)]
        [TestCase(")", TokenKind.RightParenthesis)]
        [TestCase(";", TokenKind.Semicolon)]
        [TestCase(",", TokenKind.Comma)]
        [TestCase(".", TokenKind.Dot)]
        [TestCase("=", TokenKind.Equals)]
        [TestCase("@", TokenKind.At)]
        public void Tokenize_SupportedSymbol_ReturnsSpecificTokenKind(
            string source,
            TokenKind expectedKind)
        {
            var tokenizer = new CodeTokenizer();

            TokenizationResult result = tokenizer.Tokenize(source);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Tokens, Has.Count.EqualTo(1));
            Assert.That(result.Tokens[0].Kind, Is.EqualTo(expectedKind));
            Assert.That(result.Tokens[0].Lexeme, Is.EqualTo(source));
            Assert.That(result.Tokens[0].Position.Offset, Is.Zero);
            Assert.That(result.Tokens[0].Position.Line, Is.EqualTo(1));
            Assert.That(result.Tokens[0].Position.Column, Is.EqualTo(1));
        }

        [Test]
        public void Tokenize_IntegerAndDecimalLiterals_ReturnsExactTokensWithPositions()
        {
            var tokenizer = new CodeTokenizer();

            TokenizationResult result = tokenizer.Tokenize("100 12.5");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Tokens, Has.Count.EqualTo(2));
            Assert.That(result.Tokens[0].Kind, Is.EqualTo(TokenKind.IntegerLiteral));
            Assert.That(result.Tokens[0].Lexeme, Is.EqualTo("100"));
            Assert.That(result.Tokens[0].Position.Offset, Is.Zero);
            Assert.That(result.Tokens[0].Position.Column, Is.EqualTo(1));
            Assert.That(result.Tokens[1].Kind, Is.EqualTo(TokenKind.FloatLiteral));
            Assert.That(result.Tokens[1].Lexeme, Is.EqualTo("12.5"));
            Assert.That(result.Tokens[1].Position.Offset, Is.EqualTo(4));
            Assert.That(result.Tokens[1].Position.Column, Is.EqualTo(5));
        }

        [TestCase("12.")]
        [TestCase("12abc")]
        [TestCase(".5")]
        public void Tokenize_MalformedNumber_ReturnsPositionedDiagnostic(string source)
        {
            var tokenizer = new CodeTokenizer();

            TokenizationResult result = tokenizer.Tokenize(source);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Tokens, Is.Empty);
            Assert.That(result.Diagnostic.Code, Is.EqualTo("LEX003"));
            Assert.That(
                result.Diagnostic.Detail,
                Is.EqualTo($"Número malformado: '{source}'."));
            Assert.That(result.Diagnostic.Position.Offset, Is.Zero);
            Assert.That(result.Diagnostic.Position.Line, Is.EqualTo(1));
            Assert.That(result.Diagnostic.Position.Column, Is.EqualTo(1));
        }

        [Test]
        public void Tokenize_StringLiterals_PreservesQuotesSpacesAccentsAndPositions()
        {
            var tokenizer = new CodeTokenizer();

            TokenizationResult result = tokenizer.Tokenize(
                "\"Boneco de Treinamento\" \"Água\"");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Tokens, Has.Count.EqualTo(2));
            Assert.That(result.Tokens[0].Kind, Is.EqualTo(TokenKind.StringLiteral));
            Assert.That(result.Tokens[0].Lexeme, Is.EqualTo("\"Boneco de Treinamento\""));
            Assert.That(result.Tokens[0].Position.Offset, Is.Zero);
            Assert.That(result.Tokens[0].Position.Column, Is.EqualTo(1));
            Assert.That(result.Tokens[1].Kind, Is.EqualTo(TokenKind.StringLiteral));
            Assert.That(result.Tokens[1].Lexeme, Is.EqualTo("\"Água\""));
            Assert.That(result.Tokens[1].Position.Offset, Is.EqualTo(24));
            Assert.That(result.Tokens[1].Position.Column, Is.EqualTo(25));
        }

        [TestCase("\"Mago")]
        [TestCase("\"Mago\nclasse\"")]
        public void Tokenize_UnterminatedString_ReturnsSpecificDiagnostic(string source)
        {
            var tokenizer = new CodeTokenizer();

            TokenizationResult result = tokenizer.Tokenize(source);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Tokens, Is.Empty);
            Assert.That(result.Diagnostic.Code, Is.EqualTo("LEX002"));
            Assert.That(result.Diagnostic.Detail, Is.EqualTo("String não terminada."));
            Assert.That(result.Diagnostic.Position.Offset, Is.Zero);
            Assert.That(result.Diagnostic.Position.Line, Is.EqualTo(1));
            Assert.That(result.Diagnostic.Position.Column, Is.EqualTo(1));
        }

        [Test]
        public void Tokenize_StringEscape_ReturnsDiagnosticAtBackslash()
        {
            var tokenizer = new CodeTokenizer();

            TokenizationResult result = tokenizer.Tokenize("\"Mago\\n\"");

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Tokens, Is.Empty);
            Assert.That(result.Diagnostic.Code, Is.EqualTo("LEX004"));
            Assert.That(
                result.Diagnostic.Detail,
                Is.EqualTo("Sequência de escape não permitida: '\\n'."));
            Assert.That(result.Diagnostic.Position.Offset, Is.EqualTo(5));
            Assert.That(result.Diagnostic.Position.Line, Is.EqualTo(1));
            Assert.That(result.Diagnostic.Position.Column, Is.EqualTo(6));
        }

        [Test]
        public void Tokenize_CompletePedagogicalVocabulary_ConsumesAllEightBattleConstructs()
        {
            const string source =
                "public class Mago {\n" +
                "private int vida; private float alcance; private String elemento;\n" +
                "public Mago(int vida, float alcance, String elemento) {\n" +
                "this.vida = vida; this.alcance = alcance; this.elemento = elemento; }\n" +
                "public String getElemento() { return elemento; }\n" +
                "public void lancarMagia(Inimigo inimigo) {} }\n" +
                "Mago mago = new Mago(100, 1.5, \"Água\");\n" +
                "public class Piromante extends Mago {\n" +
                "@Override public void lancarMagia(Inimigo inimigo) {\n" +
                "super.lancarMagia(inimigo); } }";
            var tokenizer = new CodeTokenizer();

            TokenizationResult result = tokenizer.Tokenize(source);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Diagnostic, Is.Null);
            Assert.That(result.Tokens, Is.Not.Empty);
        }

        [Test]
        public void Tokenize_ConcatenatedKeywords_ReturnsSingleIdentifier()
        {
            var tokenizer = new CodeTokenizer();

            TokenizationResult result = tokenizer.Tokenize("publicclass");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Tokens, Has.Count.EqualTo(1));
            Assert.That(result.Tokens[0].Kind, Is.EqualTo(TokenKind.Identifier));
            Assert.That(result.Tokens[0].Lexeme, Is.EqualTo("publicclass"));
        }

        [Test]
        public void Tokenize_TabsAndCrLf_TracksOriginalTokenPositions()
        {
            var tokenizer = new CodeTokenizer();

            TokenizationResult result = tokenizer.Tokenize(
                "\tprivate\r\nint vida = 100;");

            Assert.That(result.IsSuccess, Is.True);
            AssertToken(result.Tokens[0], TokenKind.PrivateKeyword, "private", 1, 1, 2);
            AssertToken(result.Tokens[1], TokenKind.IntKeyword, "int", 10, 2, 1);
            AssertToken(result.Tokens[2], TokenKind.Identifier, "vida", 14, 2, 5);
            AssertToken(result.Tokens[3], TokenKind.Equals, "=", 19, 2, 10);
            AssertToken(result.Tokens[4], TokenKind.IntegerLiteral, "100", 21, 2, 12);
            AssertToken(result.Tokens[5], TokenKind.Semicolon, ";", 24, 2, 15);
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

            TokenizationResult result = tokenizer.Tokenize("public # class Mago {}");

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Tokens, Is.Empty);
            Assert.That(result.Diagnostic.Code, Is.EqualTo("LEX001"));
            Assert.That(
                result.Diagnostic.Detail,
                Is.EqualTo("Caractere não reconhecido: '#'."));
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
