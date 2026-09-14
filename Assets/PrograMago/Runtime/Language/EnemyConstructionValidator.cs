using System;
using System.Collections.Generic;
using PrograMago.Domain;

namespace PrograMago.Language
{
    internal sealed class EnemyConstructionValidator
    {
        private static readonly Dictionary<string, EnemyProfile> Profiles =
            new Dictionary<string, EnemyProfile>(StringComparer.Ordinal)
            {
                { "Boneco de Treinamento", new EnemyProfile(10, "neutro") },
                { "Golem de Gelo", new EnemyProfile(12, "gelo") },
                { "Elemental de Fogo", new EnemyProfile(12, "fogo") },
                { "Slime Aquático", new EnemyProfile(12, "água") }
            };

        private readonly MagoValidationRules rules;

        public EnemyConstructionValidator(MagoValidationRules rules)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public ExerciseValidationResult Validate(
            IReadOnlyList<Token> tokens,
            ExerciseDefinition exercise)
        {
            try
            {
                ProgramParts parts = SplitProgram(tokens);
                if (parts.MagoClass == null || parts.MagoInstance == null)
                {
                    throw Fail(tokens, 0, "ENEMY001", "Mantenha a classe e a instância do Mago aprovadas.");
                }

                if (parts.EnemyClass == null || parts.EnemyInstances.Count == 0)
                {
                    throw Fail(tokens, tokens.Count, "ENEMY001",
                        "Declare Inimigo e crie ao menos um objeto com new Inimigo(...).");
                }

                var magoTokens = new List<Token>(parts.MagoClass);
                magoTokens.AddRange(parts.MagoInstance);
                ExerciseValidationResult mago = new MagoConstructionValidator(rules).Validate(
                    magoTokens, exercise);
                if (!mago.IsSuccess)
                {
                    return mago;
                }

                IReadOnlyList<EnemyState> enemies = ParseEnemy(parts);
                return ExerciseValidationResult.Success(
                    ValidationCriterion.ConstructAndInstantiateEnemy,
                    mago.Program,
                    enemies);
            }
            catch (InvalidProgramException failure)
            {
                return ExerciseValidationResult.Failure(failure.Diagnostic);
            }
        }

        private static ProgramParts SplitProgram(IReadOnlyList<Token> tokens)
        {
            var parts = new ProgramParts();
            int index = 0;
            while (index < tokens.Count)
            {
                int start = index;
                if (tokens[index].Kind == TokenKind.PublicKeyword &&
                    index + 1 < tokens.Count && tokens[index + 1].Kind == TokenKind.ClassKeyword)
                {
                    if (index + 3 >= tokens.Count ||
                        tokens[index + 2].Kind != TokenKind.Identifier ||
                        tokens[index + 3].Kind != TokenKind.LeftBrace)
                    {
                        throw Fail(tokens, index, "SYN001", "Declare a classe com public class Nome {.");
                    }

                    string className = tokens[index + 2].Lexeme;
                    index += 4;
                    int depth = 1;
                    while (index < tokens.Count && depth > 0)
                    {
                        if (tokens[index].Kind == TokenKind.LeftBrace) depth++;
                        if (tokens[index].Kind == TokenKind.RightBrace) depth--;
                        index++;
                    }

                    if (depth != 0)
                    {
                        throw Fail(tokens, tokens.Count, "SYN004", "Feche a classe com }.");
                    }

                    List<Token> declaration = Slice(tokens, start, index);
                    if (className == "Mago" && parts.MagoClass == null)
                    {
                        parts.MagoClass = declaration;
                    }
                    else if (className == "Inimigo" && parts.EnemyClass == null)
                    {
                        parts.EnemyClass = declaration;
                    }
                    else
                    {
                        throw Fail(tokens, start, "ENEMY002",
                            "Declare Mago e Inimigo uma vez cada, sem classes adicionais.");
                    }

                    continue;
                }

                while (index < tokens.Count && tokens[index].Kind != TokenKind.Semicolon)
                {
                    if (tokens[index].Kind == TokenKind.LeftBrace ||
                        tokens[index].Kind == TokenKind.RightBrace)
                    {
                        throw Fail(tokens, index, "SYN005", "Código fora de uma classe ou instanciação.");
                    }

                    index++;
                }

                if (index >= tokens.Count)
                {
                    throw Fail(tokens, index, "SYN003", "Finalize a instanciação com ;.");
                }

                index++;
                List<Token> statement = Slice(tokens, start, index);
                if (statement[0].Kind != TokenKind.Identifier)
                {
                    throw Fail(tokens, start, "ENEMY002", "Use uma declaração de classe ou instância válida.");
                }

                if (statement[0].Lexeme == "Mago" && parts.MagoInstance == null)
                {
                    parts.MagoInstance = statement;
                }
                else if (statement[0].Lexeme == "Inimigo")
                {
                    parts.EnemyInstances.Add(statement);
                }
                else
                {
                    throw Fail(tokens, start, "ENEMY002", "Instância desconhecida ou duplicada.");
                }
            }

            return parts;
        }

        private static IReadOnlyList<EnemyState> ParseEnemy(ProgramParts parts)
        {
            var allTokens = new List<Token>(parts.EnemyClass);
            foreach (List<Token> instance in parts.EnemyInstances)
            {
                allTokens.AddRange(instance);
            }

            var cursor = new Cursor(allTokens);
            cursor.Expect(TokenKind.PublicKeyword, "ENEMY003", "Declare public class Inimigo.");
            cursor.Expect(TokenKind.ClassKeyword, "ENEMY003", "Declare public class Inimigo.");
            cursor.ExpectName("Inimigo", "ENEMY003", "A classe deve se chamar Inimigo.");
            cursor.Expect(TokenKind.LeftBrace, "SYN004", "Abra a classe Inimigo com {.");

            var requiredTypes = new Dictionary<string, TokenKind>(StringComparer.Ordinal)
            {
                { "nome", TokenKind.StringType },
                { "vida", TokenKind.IntKeyword },
                { "elemento", TokenKind.StringType }
            };
            var fields = new HashSet<string>(StringComparer.Ordinal);
            while (cursor.Check(TokenKind.PrivateKeyword))
            {
                cursor.Take();
                Token type = cursor.Take();
                Token field = cursor.Expect(TokenKind.Identifier, "ENEMY004",
                    "Informe o nome do atributo de Inimigo.");
                if (!requiredTypes.TryGetValue(field.Lexeme, out TokenKind expectedType) ||
                    type.Kind != expectedType || !fields.Add(field.Lexeme))
                {
                    throw cursor.Failure("ENEMY004",
                        "Declare nome e elemento como String e vida como int, uma vez cada.", field);
                }

                cursor.Expect(TokenKind.Semicolon, "SYN003", "Finalize o atributo com ;.");
            }

            if (fields.Count != 3)
            {
                throw cursor.Failure("ENEMY004", "Os três atributos de Inimigo devem ser private.");
            }

            cursor.Expect(TokenKind.PublicKeyword, "ENEMY005", "Declare um construtor público Inimigo.");
            cursor.ExpectName("Inimigo", "ENEMY005", "O construtor deve se chamar Inimigo.");
            cursor.Expect(TokenKind.LeftParenthesis, "SYN006", "Abra os parâmetros do construtor.");
            var parameters = new List<string>();
            while (!cursor.Check(TokenKind.RightParenthesis))
            {
                Token type = cursor.Take();
                Token parameter = cursor.Expect(TokenKind.Identifier, "ENEMY005",
                    "Informe o nome do parâmetro.");
                if (!requiredTypes.TryGetValue(parameter.Lexeme, out TokenKind expectedType) ||
                    type.Kind != expectedType || parameters.Contains(parameter.Lexeme))
                {
                    throw cursor.Failure("ENEMY005", "O construtor deve receber nome, vida e elemento.",
                        parameter);
                }

                parameters.Add(parameter.Lexeme);
                if (!cursor.Match(TokenKind.Comma)) break;
            }

            cursor.Expect(TokenKind.RightParenthesis, "SYN006", "Feche os parâmetros com ).");
            if (parameters.Count != 3)
            {
                throw cursor.Failure("ENEMY005", "O construtor deve receber os três atributos.");
            }

            cursor.Expect(TokenKind.LeftBrace, "SYN006", "Abra o corpo do construtor.");
            var assigned = new HashSet<string>(StringComparer.Ordinal);
            while (cursor.Check(TokenKind.ThisKeyword))
            {
                cursor.Take();
                cursor.Expect(TokenKind.Dot, "ENEMY006", "Use this.atributo = parametro;.");
                Token field = cursor.Expect(TokenKind.Identifier, "ENEMY006",
                    "Informe o atributo depois de this.");
                cursor.Expect(TokenKind.Equals, "ENEMY006", "Atribua o parâmetro ao atributo.");
                Token parameter = cursor.Expect(TokenKind.Identifier, "ENEMY006",
                    "Informe o parâmetro da atribuição.");
                if (field.Lexeme != parameter.Lexeme ||
                    !parameters.Contains(field.Lexeme) || !assigned.Add(field.Lexeme))
                {
                    throw cursor.Failure("ENEMY006",
                        "Cada atributo deve receber seu próprio parâmetro uma única vez.", field);
                }

                cursor.Expect(TokenKind.Semicolon, "SYN003", "Finalize a atribuição com ;.");
            }

            if (assigned.Count != 3)
            {
                throw cursor.Failure("ENEMY006", "Inicialize nome, vida e elemento com this.");
            }

            cursor.Expect(TokenKind.RightBrace, "SYN006", "Feche o construtor com }.");
            cursor.Expect(TokenKind.PublicKeyword, "ENEMY007", "Declare public String getElemento().");
            cursor.Expect(TokenKind.StringType, "ENEMY007", "getElemento deve retornar String.");
            cursor.ExpectName("getElemento", "ENEMY007", "Declare getElemento().");
            cursor.Expect(TokenKind.LeftParenthesis, "ENEMY007", "Abra getElemento com (.");
            cursor.Expect(TokenKind.RightParenthesis, "ENEMY007", "getElemento não recebe parâmetros.");
            cursor.Expect(TokenKind.LeftBrace, "ENEMY007", "Abra o corpo de getElemento.");
            cursor.Expect(TokenKind.ReturnKeyword, "ENEMY007", "Retorne o elemento protegido.");
            if (cursor.Match(TokenKind.ThisKeyword))
            {
                cursor.Expect(TokenKind.Dot, "ENEMY007", "Use this.elemento.");
            }

            cursor.ExpectName("elemento", "ENEMY007", "Retorne elemento em getElemento.");
            cursor.Expect(TokenKind.Semicolon, "ENEMY007", "Finalize return elemento com ;.");
            cursor.Expect(TokenKind.RightBrace, "ENEMY007", "Feche getElemento com }.");
            cursor.Expect(TokenKind.RightBrace, "SYN004", "Feche a classe Inimigo com }.");

            var enemies = new List<EnemyState>();
            var variables = new HashSet<string>(StringComparer.Ordinal);
            while (!cursor.AtEnd)
            {
                cursor.ExpectName("Inimigo", "ENEMY008", "Declare uma variável do tipo Inimigo.");
                Token variable = cursor.Expect(TokenKind.Identifier, "ENEMY008",
                    "Informe o nome da variável do Inimigo.");
                if (!variables.Add(variable.Lexeme))
                {
                    throw cursor.Failure("ENEMY008", "Não repita o nome da variável.", variable);
                }

                cursor.Expect(TokenKind.Equals, "ENEMY008", "Atribua a nova instância com =.");
                cursor.Expect(TokenKind.NewKeyword, "ENEMY008", "Use new para criar Inimigo.");
                cursor.ExpectName("Inimigo", "ENEMY008", "Instancie Inimigo.");
                cursor.Expect(TokenKind.LeftParenthesis, "ENEMY008", "Abra os argumentos com (.");
                var values = new Dictionary<string, object>(StringComparer.Ordinal);
                foreach (string parameter in parameters)
                {
                    Token argument = cursor.Take();
                    if (requiredTypes[parameter] == TokenKind.IntKeyword)
                    {
                        if (argument == null || argument.Kind != TokenKind.IntegerLiteral ||
                            !int.TryParse(argument.Lexeme, out int number))
                        {
                            throw cursor.Failure("ENEMY009", "Vida deve ser um inteiro.", argument);
                        }

                        values.Add(parameter, number);
                    }
                    else
                    {
                        if (argument == null || argument.Kind != TokenKind.StringLiteral)
                        {
                            throw cursor.Failure("ENEMY009", "Nome e elemento devem ser Strings.",
                                argument);
                        }

                        values.Add(parameter, argument.Lexeme.Substring(1, argument.Lexeme.Length - 2));
                    }

                    if (values.Count < parameters.Count)
                    {
                        cursor.Expect(TokenKind.Comma, "ENEMY009", "Separe argumentos com vírgula.");
                    }
                }

                cursor.Expect(TokenKind.RightParenthesis, "ENEMY009", "Feche os argumentos com ).");
                cursor.Expect(TokenKind.Semicolon, "SYN003", "Finalize a instância com ;.");

                string name = (string)values["nome"];
                int vida = (int)values["vida"];
                string elemento = (string)values["elemento"];
                if (!Profiles.TryGetValue(name, out EnemyProfile profile) ||
                    profile.Vida != vida || profile.Elemento != elemento)
                {
                    throw cursor.Failure("ENEMY010", "Use nome, vida e elemento de uma ficha prevista.",
                        variable);
                }

                enemies.Add(new EnemyState(variable.Lexeme, name, vida, elemento));
            }

            if (enemies.Count == 0 || !ContainsTrainingDummy(enemies))
            {
                throw cursor.Failure("ENEMY010", "Instancie o Boneco de Treinamento nesta batalha.");
            }

            return enemies.AsReadOnly();
        }

        private static bool ContainsTrainingDummy(IEnumerable<EnemyState> enemies)
        {
            foreach (EnemyState enemy in enemies)
            {
                if (enemy.Name == "Boneco de Treinamento") return true;
            }

            return false;
        }

        private static List<Token> Slice(IReadOnlyList<Token> tokens, int start, int end)
        {
            var result = new List<Token>(end - start);
            for (int index = start; index < end; index++) result.Add(tokens[index]);
            return result;
        }

        private static InvalidProgramException Fail(
            IReadOnlyList<Token> tokens, int index, string code, string detail)
        {
            SourcePosition position = tokens.Count == 0
                ? new SourcePosition(0, 1, 1)
                : tokens[Math.Min(index, tokens.Count - 1)].Position;
            return new InvalidProgramException(new Diagnostic(code, position, detail));
        }

        private sealed class ProgramParts
        {
            public List<Token> MagoClass;
            public List<Token> MagoInstance;
            public List<Token> EnemyClass;
            public readonly List<List<Token>> EnemyInstances = new List<List<Token>>();
        }

        private readonly struct EnemyProfile
        {
            public EnemyProfile(int vida, string elemento)
            {
                Vida = vida;
                Elemento = elemento;
            }

            public int Vida { get; }
            public string Elemento { get; }
        }

        private sealed class InvalidProgramException : Exception
        {
            public InvalidProgramException(Diagnostic diagnostic)
            {
                Diagnostic = diagnostic;
            }

            public Diagnostic Diagnostic { get; }
        }

        private sealed class Cursor
        {
            private readonly IReadOnlyList<Token> tokens;
            private int index;

            public Cursor(IReadOnlyList<Token> tokens)
            {
                this.tokens = tokens;
            }

            public bool AtEnd => index >= tokens.Count;

            public bool Check(TokenKind kind) => !AtEnd && tokens[index].Kind == kind;

            public bool Match(TokenKind kind)
            {
                if (!Check(kind)) return false;
                index++;
                return true;
            }

            public Token Take() => AtEnd ? null : tokens[index++];

            public Token Expect(TokenKind kind, string code, string detail)
            {
                if (!Check(kind)) throw Failure(code, detail);
                return Take();
            }

            public Token ExpectName(string name, string code, string detail)
            {
                Token token = Expect(TokenKind.Identifier, code, detail);
                if (token.Lexeme != name) throw Failure(code, detail, token);
                return token;
            }

            public InvalidProgramException Failure(string code, string detail, Token token = null)
            {
                SourcePosition position = token?.Position ??
                    (tokens.Count == 0 ? new SourcePosition(0, 1, 1) :
                        tokens[Math.Min(index, tokens.Count - 1)].Position);
                return new InvalidProgramException(new Diagnostic(code, position, detail));
            }
        }
    }
}
