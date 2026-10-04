using System;
using System.Collections.Generic;
using PrograMago.Domain;

namespace PrograMago.Language
{
    // A finite educational grammar; player code is validated, never executed.
    public sealed class PolymorphicSpellValidator
    {
        private readonly MagoValidationRules rules;
        public PolymorphicSpellValidator(MagoValidationRules rules) { this.rules = rules; }

        public ExerciseValidationResult Validate(IReadOnlyList<Token> tokens,
            ExerciseDefinition exercise, out CombatStrategy strategy)
        {
            strategy = null;
            var cursor = new Cursor(tokens);
            try
            {
                int split = -1;
                int depth = 0;
                for (int index = 0; index + 4 < tokens.Count; index++)
                {
                    if (tokens[index].Lexeme == "{") depth++;
                    if (tokens[index].Lexeme == "}") depth--;
                    if (depth == 0 && tokens[index].Lexeme == "Mago" &&
                        tokens[index + 2].Lexeme == "=" && tokens[index + 3].Kind == TokenKind.Identifier &&
                        tokens[index + 4].Lexeme == ";") { split = index; break; }
                }
                if (split < 0) throw cursor.Fail("Declare na Estratégia uma referência Mago ativo = mago; usando sua instância base.");
                var basisTokens = new List<Token>();
                for (int index = 0; index < split; index++) basisTokens.Add(tokens[index]);
                ExerciseValidationResult basis = new ElementalSpellValidator(rules).Validate(
                    basisTokens, exercise, ValidationCriterion.UsePolymorphicMagoReference);
                if (!basis.IsSuccess) return basis;
                var names = new HashSet<string>(StringComparer.Ordinal);
                var types = new HashSet<string>(StringComparer.Ordinal);
                foreach (EnemyState enemy in basis.Enemies) { names.Add(enemy.VariableName); types.Add(enemy.Name); }
                if (basis.Enemies.Count != 4 || types.Count != 4)
                    throw cursor.Fail("Instancie uma vez cada inimigo: Boneco, Golem de Gelo, Elemental de Fogo e Slime Aquático.");

                cursor.Index = split;
                cursor.Expect("Mago");
                string reference = cursor.Identifier();
                if (reference == basis.Program.InstanceName || reference == "alvo" || names.Contains(reference))
                    throw cursor.Fail("Dê à referência ativa um nome diferente das instâncias e de alvo.");
                cursor.Expect("="); cursor.Expect(basis.Program.InstanceName); cursor.Expect(";");
                var choices = new Dictionary<string, string>(StringComparer.Ordinal);
                cursor.Expect("if");
                while (true)
                {
                    cursor.Expect("("); cursor.Expect("alvo"); cursor.Expect(".");
                    cursor.Expect("getElemento"); cursor.Expect("("); cursor.Expect(")"); cursor.Expect(".");
                    cursor.Expect("equals"); cursor.Expect("(");
                    string element = cursor.String();
                    cursor.Expect(")"); cursor.Expect(")"); cursor.Expect("{");
                    if (element != "gelo" && element != "fogo" && element != "água")
                        throw cursor.Fail("Compare getElemento().equals com gelo, fogo ou água; o else trata o neutro.");
                    if (choices.ContainsKey(element)) throw cursor.Fail("Não repita o mesmo elemento em duas condições.");
                    cursor.Expect(reference); cursor.Expect("="); cursor.Expect("new");
                    string form = cursor.Identifier();
                    if (form != "Piromante" && form != "Hidromante" && form != "Eletromante")
                        throw cursor.Fail("A referência Mago recebe Piromante, Hidromante ou Eletromante.");
                    cursor.Expect("(");
                    string[] attributes = { "vida", "dano", "alcance", "iniciativa", "velocidadeAtaque" };
                    for (int index = 0; index < attributes.Length; index++)
                    {
                        if (index > 0) cursor.Expect(",");
                        int value = cursor.Integer();
                        if (value != basis.Program.InitialValues[attributes[index]])
                            throw cursor.Fail("Use na especialização os mesmos cinco valores do construtor da instância Mago; a troca preserva a build.");
                    }
                    cursor.Expect(")"); cursor.Expect(";"); cursor.Expect("}");
                    choices.Add(element, form.ToLowerInvariant());
                    cursor.Expect("else");
                    if (!cursor.Check("if")) break;
                    cursor.Expect("if");
                }
                if (choices.Count != 3) throw cursor.Fail("Trate gelo, fogo e água, em qualquer ordem, antes do else.");
                cursor.Expect("{"); cursor.Expect(reference); cursor.Expect("=");
                cursor.Expect(basis.Program.InstanceName); cursor.Expect(";"); cursor.Expect("}");
                cursor.Expect(reference); cursor.Expect("."); cursor.Expect("lancarMagia");
                cursor.Expect("("); cursor.Expect("alvo"); cursor.Expect(")"); cursor.Expect(";");
                if (!cursor.End) throw cursor.Fail("Após o if/else, use somente uma chamada pela referência ativa: lancarMagia(alvo).");
                strategy = new CombatStrategy(choices, "neutro");
                return ExerciseValidationResult.Success(ValidationCriterion.UsePolymorphicMagoReference,
                    basis.Program, basis.Enemies);
            }
            catch (InvalidStrategy failure) { return ExerciseValidationResult.Failure(failure.Diagnostic); }
        }

        private sealed class Cursor
        {
            private readonly IReadOnlyList<Token> tokens;
            public Cursor(IReadOnlyList<Token> tokens) { this.tokens = tokens; }
            public int Index { get; set; }
            public bool End => Index >= tokens.Count;
            public bool Check(string text) => !End && tokens[Index].Lexeme == text;
            public void Expect(string text)
            {
                if (!Check(text)) throw Fail("Esperado '" + text + "' na estratégia polimórfica. Use o modelo da dica, sem comandos adicionais.");
                Index++;
            }
            private Token Read(TokenKind kind)
            {
                if (End || tokens[Index].Kind != kind) throw Fail("Complete o valor ou nome esperado na Estratégia.");
                return tokens[Index++];
            }
            public string Identifier() => Read(TokenKind.Identifier).Lexeme;
            public string String() => Read(TokenKind.StringLiteral).Lexeme.Trim('"');
            public int Integer()
            {
                Token token = Read(TokenKind.IntegerLiteral);
                if (!int.TryParse(token.Lexeme, out int value)) throw Fail("Use um valor inteiro válido para o atributo.");
                return value;
            }
            public InvalidStrategy Fail(string detail)
            {
                SourcePosition position = !End ? tokens[Index].Position : tokens.Count > 0
                    ? tokens[tokens.Count - 1].Position : new SourcePosition(0, 1, 1);
                return new InvalidStrategy(new Diagnostic("POLY001", position, detail));
            }
        }
        private sealed class InvalidStrategy : Exception
        {
            public InvalidStrategy(Diagnostic diagnostic) { Diagnostic = diagnostic; }
            public Diagnostic Diagnostic { get; }
        }
    }
}
