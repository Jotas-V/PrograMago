using System;
using System.Collections.Generic;
using PrograMago.Domain;

namespace PrograMago.Language
{
    // Recognizes only the educational spell contract; never executes player code.
    internal sealed class ElementalSpellValidator
    {
        private readonly MagoValidationRules rules;
        private static readonly HashSet<string> Forms = new HashSet<string>(StringComparer.Ordinal)
            { "Piromante", "Hidromante", "Eletromante" };

        public ElementalSpellValidator(MagoValidationRules rules)
        {
            this.rules = rules;
        }

        public ExerciseValidationResult Validate(IReadOnlyList<Token> tokens,
            ExerciseDefinition exercise, ValidationCriterion criterion)
        {
            try
            {
                var originalProgram = new List<Token>();
                var calls = new List<List<Token>>();
                var forms = new HashSet<string>(StringComparer.Ordinal);
                bool baseSpell = false;
                bool hasOverride = false;
                var cursor = new Cursor(tokens);
                while (!cursor.End)
                {
                    int start = cursor.Index;
                    if (cursor.Check("public") && cursor.Check("class", 1))
                    {
                        cursor.Read();
                        cursor.Read();
                        Token name = cursor.ExpectKind(TokenKind.Identifier, "INHERIT002", "Informe o nome da classe.");
                        if (name.Lexeme == "Mago" || name.Lexeme == "Inimigo")
                        {
                            cursor.Expect("{", "SPELL004", "Mago e Inimigo mantêm a estrutura base aprovada.");
                            ReadClassEnd(cursor);
                            List<Token> declaration = Slice(tokens, start, cursor.Index);
                            if (name.Lexeme == "Mago")
                            {
                                declaration = ReadBaseSpell(declaration, out bool found);
                                baseSpell |= found;
                            }
                            originalProgram.AddRange(declaration);
                        }
                        else
                        {
                            if (!Forms.Contains(name.Lexeme))
                                throw cursor.Fail("INHERIT002", "Use Piromante, Hidromante ou Eletromante como especialização.", name);
                            if (!forms.Add(name.Lexeme))
                                throw cursor.Fail("INHERIT003", "Declare cada especialização uma única vez.", name);
                            cursor.Expect("extends", "INHERIT001", "A especialização deve declarar extends Mago.");
                            cursor.Expect("Mago", "INHERIT001", "A classe base da especialização deve ser Mago.");
                            cursor.Expect("{", "INHERIT001", "Abra o corpo da especialização com {.");
                            bool declaredSpell = false;
                            bool declaredConstructor = false;
                            while (!cursor.Check("}"))
                            {
                                if (criterion == ValidationCriterion.UsePolymorphicMagoReference &&
                                    cursor.Check("public") && cursor.Check(name.Lexeme, 1))
                                {
                                    if (declaredConstructor) throw cursor.Fail("POLY002", "Declare um único construtor na especialização.");
                                    ReadSpecializationConstructor(cursor, name.Lexeme);
                                    declaredConstructor = true;
                                    continue;
                                }
                                if (declaredSpell)
                                    throw cursor.Fail("SPELL003", "Declare lancarMagia uma única vez em cada especialização.");
                                if (criterion != ValidationCriterion.OverrideSpellWithSuper &&
                                    criterion != ValidationCriterion.UsePolymorphicMagoReference)
                                    throw cursor.Fail("SPELL004", "Nesta etapa, declare a especialização com corpo vazio; a sobrescrita vem depois.");
                                if (!cursor.Check("@") && !cursor.Check("public") && !cursor.Check("private"))
                                    throw cursor.Fail("SPELL004", "Na especialização, use apenas a sobrescrita de lancarMagia prevista nesta atividade.");
                                ReadSpell(cursor, true);
                                declaredSpell = true;
                                hasOverride = true;
                            }
                            if (criterion == ValidationCriterion.UsePolymorphicMagoReference &&
                                (!declaredSpell || !declaredConstructor))
                                throw cursor.Fail("POLY002", "Cada especialização precisa do construtor com super e da sobrescrita de lancarMagia.");
                            cursor.Expect("}", "SPELL004", "Feche a especialização com }.");
                        }
                    }
                    else
                    {
                        while (!cursor.End && !cursor.Check(";"))
                        {
                            if (cursor.Check("{") || cursor.Check("}"))
                                throw cursor.Fail("SPELL004", "Declare métodos dentro de suas classes e finalize as chamadas com ;.");
                            cursor.Read();
                        }
                        cursor.Expect(";", "SPELL005", "Finalize a chamada ou a instanciação com ;.");
                        List<Token> statement = Slice(tokens, start, cursor.Index);
                        if (statement.Count > 1 && statement[1].Lexeme == ".") calls.Add(statement);
                        else originalProgram.AddRange(statement);
                    }
                }

                if (!baseSpell)
                    throw cursor.Fail("SPELL001", "Declare public void lancarMagia(Inimigo alvo) dentro de Mago.");
                if (criterion != ValidationCriterion.DefineAndCallSpellMethod && forms.Count == 0)
                    throw cursor.Fail("INHERIT001", "Declare ao menos uma especialização pública com extends Mago.");
                if (criterion == ValidationCriterion.OverrideSpellWithSuper && !hasOverride)
                    throw cursor.Fail("OVERRIDE001", "Sobrescreva lancarMagia em uma especialização usando @Override.");
                if (criterion == ValidationCriterion.UsePolymorphicMagoReference && forms.Count != 3)
                    throw cursor.Fail("POLY002", "Declare Piromante, Hidromante e Eletromante com suas sobrescritas.");
                if (criterion == ValidationCriterion.DefineAndCallSpellMethod && forms.Count != 0)
                    throw cursor.Fail("INHERIT002", "As especializações serão declaradas na etapa de herança.");

                // Only verified additions are removed. All remaining tokens still go through
                // the existing class, constructor, setter, instance and attribute validation.
                ExerciseValidationResult basis = new EnemyConstructionValidator(rules,
                    criterion != ValidationCriterion.OverrideSpellWithSuper &&
                    criterion != ValidationCriterion.UsePolymorphicMagoReference).Validate(originalProgram, exercise);
                if (!basis.IsSuccess) return basis;
                foreach (List<Token> call in calls)
                {
                    var invocation = new Cursor(call);
                    invocation.Expect(basis.Program.InstanceName, "SPELL005", "Chame a magia pela instância aprovada do Mago.");
                    invocation.Expect(".", "SPELL005", "Use a instância do Mago para chamar lancarMagia.");
                    invocation.Expect("lancarMagia", "SPELL005", "A chamada permitida é lancarMagia(Inimigo).");
                    invocation.Expect("(", "SPELL005", "Abra o argumento da magia com (.");
                    Token target = invocation.ExpectKind(TokenKind.Identifier, "SPELL005", "Passe uma instância aprovada de Inimigo.");
                    bool knownTarget = false;
                    foreach (EnemyState enemy in basis.Enemies)
                        knownTarget |= enemy.VariableName == target.Lexeme;
                    if (!knownTarget)
                        throw invocation.Fail("SPELL005", "Passe uma instância aprovada de Inimigo, como boneco.", target);
                    invocation.Expect(")", "SPELL005", "lancarMagia recebe somente um Inimigo.");
                    invocation.Expect(";", "SPELL005", "Finalize a chamada com ;.");
                    if (!invocation.End)
                        throw invocation.Fail("SPELL005", "Não acrescente comandos à chamada de magia.");
                }
                if (criterion == ValidationCriterion.DefineAndCallSpellMethod && calls.Count == 0)
                    throw cursor.Fail("SPELL005", "Após declarar o método, chame " + basis.Program.InstanceName + ".lancarMagia(boneco); usando seu Inimigo.");
                return ExerciseValidationResult.Success(criterion, basis.Program, basis.Enemies);
            }
            catch (SpellValidationException failure)
            {
                return ExerciseValidationResult.Failure(failure.Diagnostic);
            }
        }

        private static void ReadSpecializationConstructor(Cursor cursor, string name)
        {
            const string detail = "O construtor deve encaminhar os cinco parâmetros int para super, na mesma ordem.";
            cursor.Expect("public", "POLY002", detail);
            cursor.Expect(name, "POLY002", detail);
            cursor.Expect("(", "POLY002", detail);
            var parameters = new HashSet<string>(StringComparer.Ordinal);
            var ordered = new List<string>();
            for (int index = 0; index < 5; index++)
            {
                if (index > 0) cursor.Expect(",", "POLY002", detail);
                cursor.Expect("int", "POLY002", detail);
                Token parameter = cursor.ExpectKind(TokenKind.Identifier, "POLY002", detail);
                if (!parameters.Add(parameter.Lexeme)) throw cursor.Fail("POLY002", detail);
                ordered.Add(parameter.Lexeme);
            }
            cursor.Expect(")", "POLY002", detail);
            cursor.Expect("{", "POLY002", detail);
            cursor.Expect("super", "POLY002", detail);
            cursor.Expect("(", "POLY002", detail);
            for (int index = 0; index < 5; index++)
            {
                if (index > 0) cursor.Expect(",", "POLY002", detail);
                cursor.Expect(ordered[index], "POLY002", detail);
            }
            cursor.Expect(")", "POLY002", detail);
            cursor.Expect(";", "POLY002", detail);
            cursor.Expect("}", "POLY002", detail);
        }

        private static List<Token> ReadBaseSpell(List<Token> declaration, out bool found)
        {
            found = false;
            var kept = new List<Token>();
            var cursor = new Cursor(declaration);
            int depth = 0;
            while (!cursor.End)
            {
                bool method = cursor.Check("@") ||
                    ((cursor.Check("public") || cursor.Check("private")) &&
                     cursor.Check("(", 3));
                bool constructor = cursor.Check("public") && cursor.Check("Mago", 1);
                bool setter = cursor.Index + 2 < declaration.Count &&
                    declaration[cursor.Index + 2].Lexeme.StartsWith("set", StringComparison.Ordinal);
                if (depth == 1 && method && !constructor && !setter)
                {
                    if (found)
                        throw cursor.Fail("SPELL003", "Declare lancarMagia uma única vez na classe Mago.");
                    ReadSpell(cursor, false);
                    found = true;
                    continue;
                }
                Token token = cursor.Read();
                if (token.Lexeme == "{") depth++;
                if (token.Lexeme == "}") depth--;
                kept.Add(token);
            }
            return kept;
        }

        private static void ReadSpell(Cursor cursor, bool overriding)
        {
            if (overriding)
            {
                cursor.Expect("@", "OVERRIDE001", "Marque a sobrescrita com @Override.");
                cursor.Expect("Override", "OVERRIDE001", "Escreva @Override respeitando a letra maiúscula.");
            }
            else if (cursor.Check("@"))
                throw cursor.Fail("OVERRIDE002", "Mago declara o método base; @Override pertence à especialização.");
            cursor.Expect("public", "SPELL002", "lancarMagia deve ser public, como o método base.");
            cursor.Expect("void", "SPELL002", "lancarMagia não retorna um valor; use void.");
            cursor.Expect("lancarMagia", "SPELL002", "Mantenha o nome lancarMagia para sobrescrever o método base.");
            cursor.Expect("(", "SPELL002", "Abra o parâmetro de lancarMagia com (.");
            cursor.Expect("Inimigo", "SPELL002", "lancarMagia recebe um parâmetro do tipo Inimigo.");
            Token parameter = cursor.ExpectKind(TokenKind.Identifier, "SPELL002", "Dê um nome ao parâmetro Inimigo.");
            cursor.Expect(")", "SPELL002", "lancarMagia recebe exatamente um parâmetro Inimigo.");
            cursor.Expect("{", "SPELL002", "Abra o corpo de lancarMagia com {.");
            if (overriding)
            {
                cursor.Expect("super", "SUPER001", "Reutilize a magia base com super.lancarMagia(parametro).");
                cursor.Expect(".", "SUPER001", "Use super.lancarMagia, sem chamar um construtor.");
                cursor.Expect("lancarMagia", "SUPER001", "Chame apenas a magia herdada com super.lancarMagia.");
                cursor.Expect("(", "SUPER001", "Passe o parâmetro Inimigo à magia base.");
                cursor.Expect(parameter.Lexeme, "SUPER001", "Passe a super.lancarMagia o mesmo parâmetro recebido pelo método.");
                cursor.Expect(")", "SUPER001", "A magia base recebe somente o parâmetro Inimigo.");
                cursor.Expect(";", "SUPER001", "Finalize super.lancarMagia com ;.");
            }
            cursor.Expect("}", "SPELL004", overriding
                ? "Depois de super.lancarMagia, feche o método. O efeito elemental vem da especialização; comandos adicionais não são previstos nesta atividade."
                : "Nesta atividade, mantenha o corpo da magia base vazio; o motor de combate resolve o efeito.");
        }

        private static void ReadClassEnd(Cursor cursor)
        {
            int depth = 1;
            while (!cursor.End && depth > 0)
            {
                Token token = cursor.Read();
                if (token.Lexeme == "{") depth++;
                if (token.Lexeme == "}") depth--;
            }
            if (depth != 0) throw cursor.Fail("SPELL004", "Feche a classe com }.");
        }

        private static List<Token> Slice(IReadOnlyList<Token> tokens, int start, int end)
        {
            var slice = new List<Token>();
            for (int i = start; i < end; i++) slice.Add(tokens[i]);
            return slice;
        }

        private sealed class Cursor
        {
            private readonly IReadOnlyList<Token> tokens;
            public Cursor(IReadOnlyList<Token> tokens) { this.tokens = tokens; }
            public int Index { get; private set; }
            public bool End => Index >= tokens.Count;
            public bool Check(string text, int lookahead = 0) =>
                Index + lookahead < tokens.Count && tokens[Index + lookahead].Lexeme == text;
            public Token Read()
            {
                if (End) throw Fail("SPELL004", "Complete e feche a declaração de magia ou de classe.");
                return tokens[Index++];
            }
            public Token Expect(string text, string code, string detail)
            {
                if (!Check(text)) throw Fail(code, detail);
                return Read();
            }
            public Token ExpectKind(TokenKind kind, string code, string detail)
            {
                if (End || tokens[Index].Kind != kind) throw Fail(code, detail);
                return Read();
            }
            public SpellValidationException Fail(string code, string detail, Token token = null)
            {
                SourcePosition position = token != null ? token.Position : !End ? tokens[Index].Position :
                    tokens.Count > 0 ? tokens[tokens.Count - 1].Position : new SourcePosition(0, 1, 1);
                return new SpellValidationException(new Diagnostic(code, position, detail));
            }
        }

        private sealed class SpellValidationException : Exception
        {
            public SpellValidationException(Diagnostic diagnostic) { Diagnostic = diagnostic; }
            public Diagnostic Diagnostic { get; }
        }
    }
}
