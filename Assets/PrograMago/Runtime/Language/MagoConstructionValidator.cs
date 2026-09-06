using System;
using System.Collections.Generic;
using PrograMago.Domain;

namespace PrograMago.Language
{
    internal sealed class MagoConstructionValidator
    {
        private readonly MagoValidationRules rules;

        public MagoConstructionValidator(MagoValidationRules rules)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public ExerciseValidationResult Validate(
            IReadOnlyList<Token> tokens,
            ExerciseDefinition exercise)
        {
            int index = 0;
            ExerciseValidationResult failure = MagoAttributesValidator.ValidateHeader(
                tokens,
                exercise,
                ref index);
            if (failure != null)
            {
                return failure;
            }

            var attributes = new List<string>();
            failure = ParseAttributes(tokens, ref index, attributes);
            if (failure != null)
            {
                return failure;
            }

            var parameters = new List<string>();
            failure = ParseConstructor(tokens, exercise, ref index, parameters);
            if (failure != null)
            {
                return failure;
            }

            if (index >= tokens.Count || tokens[index].Kind != TokenKind.RightBrace)
            {
                return At(tokens, index, "SYN007", "Feche a classe Mago depois do construtor.");
            }

            index++;
            string instanceName;
            Dictionary<string, int> values;
            failure = ParseInstantiation(
                tokens,
                exercise,
                parameters,
                ref index,
                out instanceName,
                out values);
            if (failure != null)
            {
                return failure;
            }

            return ExerciseValidationResult.Success(
                ValidationCriterion.ConstructAndInstantiateMago,
                new ValidatedMagoProgram(
                    exercise.ExpectedClassName,
                    attributes,
                    instanceName,
                    values,
                    rules.PointBudget));
        }

        private static ExerciseValidationResult ParseAttributes(
            IReadOnlyList<Token> tokens,
            ref int index,
            List<string> attributes)
        {
            while (index < tokens.Count)
            {
                Token visibility = tokens[index];
                bool startsConstructor = visibility.Kind == TokenKind.PublicKeyword &&
                    index + 1 < tokens.Count &&
                    tokens[index + 1].Kind == TokenKind.Identifier;
                if (startsConstructor)
                {
                    break;
                }

                bool constructorIsPrivate = visibility.Kind == TokenKind.PrivateKeyword &&
                    index + 2 < tokens.Count &&
                    tokens[index + 1].Kind == TokenKind.Identifier &&
                    tokens[index + 2].Kind == TokenKind.LeftParenthesis;
                if (constructorIsPrivate)
                {
                    return From(
                        "STRUCT005",
                        visibility,
                        "O construtor do exercício deve ser public.");
                }

                bool constructorHasReturnType = visibility.Kind == TokenKind.PublicKeyword &&
                    index + 3 < tokens.Count &&
                    IsTypeOrVoid(tokens[index + 1].Kind) &&
                    tokens[index + 2].Kind == TokenKind.Identifier &&
                    tokens[index + 3].Kind == TokenKind.LeftParenthesis;
                if (constructorHasReturnType)
                {
                    return From(
                        "STRUCT005",
                        tokens[index + 1],
                        "Construtores não declaram tipo de retorno.");
                }

                if (visibility.Kind == TokenKind.PublicKeyword)
                {
                    return From("CONCEPT001", visibility, "Os atributos do Mago devem ser private.");
                }

                if (visibility.Kind == TokenKind.IntKeyword)
                {
                    return From("CONCEPT001", visibility, "Use private antes do tipo do atributo.");
                }

                if (visibility.Kind != TokenKind.PrivateKeyword)
                {
                    break;
                }

                index++;
                if (index >= tokens.Count || tokens[index].Kind != TokenKind.IntKeyword)
                {
                    return At(tokens, index, "CONCEPT002", "Todos os atributos devem usar int.");
                }

                index++;
                if (index >= tokens.Count || tokens[index].Kind != TokenKind.Identifier)
                {
                    return At(tokens, index, "SYN002", "Informe o nome do atributo.");
                }

                Token name = tokens[index++];
                if (Array.IndexOf(MagoAttributesValidator.RequiredAttributeNames, name.Lexeme) < 0)
                {
                    return From("STRUCT002", name, $"Atributo não esperado: {name.Lexeme}.");
                }

                if (attributes.Contains(name.Lexeme))
                {
                    return From("STRUCT003", name, $"O atributo {name.Lexeme} foi duplicado.");
                }

                attributes.Add(name.Lexeme);
                if (index >= tokens.Count || tokens[index].Kind != TokenKind.Semicolon)
                {
                    return At(tokens, index, "SYN003", $"Finalize o atributo {name.Lexeme} com ;.");
                }

                index++;
            }

            if (attributes.Count != MagoAttributesValidator.RequiredAttributeNames.Length)
            {
                return At(
                    tokens,
                    index,
                    "STRUCT004",
                    "Declare os cinco atributos antes do construtor.");
            }

            return null;
        }

        private static ExerciseValidationResult ParseConstructor(
            IReadOnlyList<Token> tokens,
            ExerciseDefinition exercise,
            ref int index,
            List<string> parameters)
        {
            if (index >= tokens.Count || tokens[index].Kind != TokenKind.PublicKeyword)
            {
                return At(tokens, index, "STRUCT005", "Declare um construtor público para Mago.");
            }

            index++;
            if (index >= tokens.Count || tokens[index].Kind != TokenKind.Identifier)
            {
                return At(tokens, index, "STRUCT005", "O construtor não declara tipo de retorno.");
            }

            Token constructorName = tokens[index++];
            if (!string.Equals(
                    constructorName.Lexeme,
                    exercise.ExpectedClassName,
                    StringComparison.Ordinal))
            {
                return From(
                    "STRUCT005",
                    constructorName,
                    $"O construtor deve se chamar {exercise.ExpectedClassName}.");
            }

            if (!Consume(tokens, ref index, TokenKind.LeftParenthesis))
            {
                return At(tokens, index, "SYN006", "Abra os parâmetros do construtor com (.");
            }

            while (index < tokens.Count && tokens[index].Kind != TokenKind.RightParenthesis)
            {
                if (tokens[index].Kind == TokenKind.Comma)
                {
                    return At(tokens, index, "SYN006", "Não use uma vírgula sem parâmetro.");
                }

                if (tokens[index].Kind != TokenKind.IntKeyword)
                {
                    return At(tokens, index, "CONCEPT002", "Todos os parâmetros devem usar int.");
                }

                index++;
                if (index >= tokens.Count || tokens[index].Kind != TokenKind.Identifier)
                {
                    return At(tokens, index, "SYN006", "Informe o nome do parâmetro.");
                }

                Token parameter = tokens[index++];
                if (Array.IndexOf(MagoAttributesValidator.RequiredAttributeNames, parameter.Lexeme) < 0 ||
                    parameters.Contains(parameter.Lexeme))
                {
                    return From(
                        "STRUCT006",
                        parameter,
                        "Os parâmetros devem corresponder uma vez a cada atributo.");
                }

                parameters.Add(parameter.Lexeme);
                if (index < tokens.Count && tokens[index].Kind == TokenKind.Comma)
                {
                    index++;
                    if (index >= tokens.Count || tokens[index].Kind == TokenKind.RightParenthesis)
                    {
                        return At(
                            tokens,
                            index,
                            "SYN006",
                            "Informe outro parâmetro depois da vírgula.");
                    }

                    continue;
                }

                break;
            }

            if (!Consume(tokens, ref index, TokenKind.RightParenthesis))
            {
                return At(tokens, index, "SYN006", "Feche os parâmetros com ).");
            }

            if (parameters.Count != MagoAttributesValidator.RequiredAttributeNames.Length)
            {
                return At(
                    tokens,
                    index,
                    "STRUCT006",
                    "O construtor deve receber um parâmetro para cada atributo.");
            }

            if (!Consume(tokens, ref index, TokenKind.LeftBrace))
            {
                return At(tokens, index, "SYN006", "Abra o corpo do construtor com {.");
            }

            var assigned = new HashSet<string>(StringComparer.Ordinal);
            while (index < tokens.Count && tokens[index].Kind != TokenKind.RightBrace)
            {
                if (tokens[index].Kind != TokenKind.ThisKeyword)
                {
                    return At(
                        tokens,
                        index,
                        "CONCEPT003",
                        "Use this.atributo = atributo para inicializar o objeto.");
                }

                index++;
                if (!Consume(tokens, ref index, TokenKind.Dot) ||
                    index >= tokens.Count ||
                    tokens[index].Kind != TokenKind.Identifier)
                {
                    return At(tokens, index, "SYN006", "Depois de this, informe .atributo.");
                }

                Token field = tokens[index++];
                if (!Consume(tokens, ref index, TokenKind.Equals) ||
                    index >= tokens.Count ||
                    tokens[index].Kind != TokenKind.Identifier)
                {
                    return At(tokens, index, "SYN006", "Atribua o parâmetro ao atributo com =.");
                }

                Token parameter = tokens[index++];
                if (!string.Equals(field.Lexeme, parameter.Lexeme, StringComparison.Ordinal) ||
                    !parameters.Contains(field.Lexeme) ||
                    !assigned.Add(field.Lexeme))
                {
                    return From(
                        "STRUCT007",
                        field,
                        "Cada atributo deve receber seu parâmetro correspondente uma única vez.");
                }

                if (!Consume(tokens, ref index, TokenKind.Semicolon))
                {
                    return At(tokens, index, "SYN003", "Finalize a atribuição com ;.");
                }
            }

            if (assigned.Count != MagoAttributesValidator.RequiredAttributeNames.Length)
            {
                return At(
                    tokens,
                    index,
                    "STRUCT008",
                    "Inicialize os cinco atributos com this.");
            }

            if (!Consume(tokens, ref index, TokenKind.RightBrace))
            {
                return At(tokens, index, "SYN006", "Feche o construtor com }.");
            }

            return null;
        }

        private ExerciseValidationResult ParseInstantiation(
            IReadOnlyList<Token> tokens,
            ExerciseDefinition exercise,
            IReadOnlyList<string> parameters,
            ref int index,
            out string instanceName,
            out Dictionary<string, int> values)
        {
            instanceName = null;
            values = null;
            if (index >= tokens.Count ||
                tokens[index].Kind != TokenKind.Identifier ||
                !string.Equals(tokens[index].Lexeme, exercise.ExpectedClassName, StringComparison.Ordinal))
            {
                return At(
                    tokens,
                    index,
                    "STRUCT009",
                    $"Depois da classe, declare uma variável do tipo {exercise.ExpectedClassName}.");
            }

            index++;
            if (index >= tokens.Count || tokens[index].Kind != TokenKind.Identifier)
            {
                return At(tokens, index, "SYN008", "Informe o nome da variável do Mago.");
            }

            instanceName = tokens[index++].Lexeme;
            if (!Consume(tokens, ref index, TokenKind.Equals))
            {
                return At(tokens, index, "SYN008", "Atribua a nova instância com =.");
            }

            if (index >= tokens.Count || tokens[index].Kind != TokenKind.NewKeyword)
            {
                return At(tokens, index, "CONCEPT004", "Use new para criar o objeto Mago.");
            }

            index++;
            if (index >= tokens.Count ||
                tokens[index].Kind != TokenKind.Identifier ||
                !string.Equals(tokens[index].Lexeme, exercise.ExpectedClassName, StringComparison.Ordinal))
            {
                return At(tokens, index, "STRUCT009", "Instancie a classe Mago.");
            }

            index++;
            if (!Consume(tokens, ref index, TokenKind.LeftParenthesis))
            {
                return At(tokens, index, "SYN008", "Abra os argumentos de new Mago com (.");
            }

            var arguments = new List<KeyValuePair<Token, int>>();
            while (index < tokens.Count && tokens[index].Kind != TokenKind.RightParenthesis)
            {
                Token argument = tokens[index];
                if (argument.Kind == TokenKind.Comma)
                {
                    return At(tokens, index, "SYN008", "Não use uma vírgula sem argumento.");
                }

                if (argument.Kind != TokenKind.IntegerLiteral ||
                    !int.TryParse(argument.Lexeme, out int value))
                {
                    return At(tokens, index, "CONCEPT002", "Os pontos dos atributos devem ser inteiros.");
                }

                arguments.Add(new KeyValuePair<Token, int>(argument, value));
                index++;
                if (index < tokens.Count && tokens[index].Kind == TokenKind.Comma)
                {
                    index++;
                    if (index >= tokens.Count || tokens[index].Kind == TokenKind.RightParenthesis)
                    {
                        return At(
                            tokens,
                            index,
                            "SYN008",
                            "Informe outro argumento depois da vírgula.");
                    }

                    continue;
                }

                break;
            }

            if (!Consume(tokens, ref index, TokenKind.RightParenthesis))
            {
                return At(tokens, index, "SYN008", "Feche os argumentos com ).");
            }

            if (arguments.Count != parameters.Count)
            {
                return At(
                    tokens,
                    index,
                    "STRUCT009",
                    "Informe um valor para cada parâmetro do construtor.");
            }

            values = new Dictionary<string, int>(StringComparer.Ordinal);
            int total = 0;
            for (int argumentIndex = 0; argumentIndex < arguments.Count; argumentIndex++)
            {
                Token token = arguments[argumentIndex].Key;
                int value = arguments[argumentIndex].Value;
                if (value < rules.MinimumAttributeValue || value > rules.MaximumAttributeValue)
                {
                    return From(
                        "GAME001",
                        token,
                        $"Cada atributo deve receber entre {rules.MinimumAttributeValue} e " +
                        $"{rules.MaximumAttributeValue} pontos.");
                }

                values.Add(parameters[argumentIndex], value);
                total += value;
            }

            if (total > rules.PointBudget)
            {
                return From(
                    "GAME002",
                    arguments[arguments.Count - 1].Key,
                    $"Distribua no máximo {rules.PointBudget} pontos entre os atributos.");
            }

            if (!Consume(tokens, ref index, TokenKind.Semicolon))
            {
                return At(tokens, index, "SYN008", "Finalize a instanciação com ;.");
            }

            if (index != tokens.Count)
            {
                return At(tokens, index, "SYN005", "Não adicione conteúdo depois da instanciação.");
            }

            return null;
        }

        private static bool Consume(
            IReadOnlyList<Token> tokens,
            ref int index,
            TokenKind expected)
        {
            if (index >= tokens.Count || tokens[index].Kind != expected)
            {
                return false;
            }

            index++;
            return true;
        }

        private static bool IsTypeOrVoid(TokenKind kind)
        {
            return kind == TokenKind.IntKeyword ||
                kind == TokenKind.FloatKeyword ||
                kind == TokenKind.StringType ||
                kind == TokenKind.VoidKeyword;
        }

        private static ExerciseValidationResult From(string code, Token token, string detail)
        {
            return MagoAttributesValidator.Failure(code, token, detail);
        }

        private static ExerciseValidationResult At(
            IReadOnlyList<Token> tokens,
            int index,
            string code,
            string detail)
        {
            return MagoAttributesValidator.FailureAt(tokens, index, code, detail);
        }
    }
}
