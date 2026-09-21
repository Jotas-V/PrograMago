using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PrograMago.Domain;
using PrograMago.Language;

namespace PrograMago.Application
{
    public sealed class MagoMethodBook
    {
        public static readonly string[] Names = { "setAlcance", "setVida", "setDano", "setIniciativa", "setVelocidadeAtaque", "lancarMagia" };
        public static readonly string[] Attributes = { "alcance", "vida", "dano", "iniciativa", "velocidadeAtaque" };
        public static readonly string[] Examples = {
            "public void setAlcance(int valor) { this.alcance = valor; }",
            "public void setVida(int valor) { this.vida = valor; }",
            "public void setDano(int valor) { this.dano = valor; }",
            "public void setIniciativa(int valor) { this.iniciativa = valor; }",
            "public void setVelocidadeAtaque(int valor) { this.velocidadeAtaque = valor; }",
            "public void lancarMagia() { atacar(); }" };
        private readonly List<string> sources = new List<string>();
        public IReadOnlyList<string> ApprovedSources => sources.AsReadOnly();
        public bool IsComplete => sources.Count == Names.Length;
        public bool IsLearned(string name) { int index = Array.IndexOf(Names, name); return index >= 0 && index < sources.Count; }

        public MagoMethodBook(IEnumerable<string> saved = null)
        {
            if (saved != null) foreach (string source in saved) if (!TryApprove(source, out _)) break;
        }

        public bool TryApprove(string source, out string error)
        {
            error = "Todos os métodos já foram aprovados. Edite as chamadas na Preparação.";
            if (IsComplete) return false;
            int index = sources.Count;
            error = "Implemente " + Names[index] + " conforme o exemplo. O método deve usar seu próprio parâmetro e atributo.";
            var parsed = Tokenize(source);
            if (!parsed.IsSuccess) return false;
            var tokens = parsed.Tokens;
            if (index < Attributes.Length)
            {
                if (tokens.Count != 15 || tokens[5].Kind != TokenKind.Identifier) return false;
                string parameter = tokens[5].Lexeme;
                string[] expected = { "public", "void", Names[index], "(", "int", parameter, ")", "{", "this", ".", Attributes[index], "=", parameter, ";", "}" };
                for (int i = 0; i < expected.Length; i++) if (tokens[i].Lexeme != expected[i]) return false;
            }
            else
            {
                string[] expected = { "public", "void", "lancarMagia", "(", ")", "{", "atacar", "(", ")", ";", "}" };
                if (tokens.Count != expected.Length) return false;
                for (int i = 0; i < expected.Length; i++) if (tokens[i].Lexeme != expected[i]) return false;
            }
            sources.Add(source);
            error = null;
            return true;
        }

        internal static TokenizationResult Tokenize(string source) => new CodeTokenizer().Tokenize(
            Regex.Replace(source ?? "", @"//[^\r\n]*|/\*[\s\S]*?\*/", " "));
    }

    public sealed class PreparationResult
    {
        public bool IsSuccess => Mago != null;
        public MagoState Mago { get; internal set; }
        public int TotalPoints { get; internal set; }
        public string Error { get; internal set; }
    }

    public static class MagoPreparation
    {
        public static PreparationResult Evaluate(MagoState initial, MagoMethodBook methods, string source)
        {
            var result = new PreparationResult();
            if (initial == null) { result.Error = "Primeiro conclua a classe e a instância do Mago."; return result; }
            var values = new Dictionary<string, int> { ["vida"] = initial.Vida, ["dano"] = initial.Dano,
                ["alcance"] = initial.Alcance, ["iniciativa"] = initial.Iniciativa, ["velocidadeAtaque"] = initial.VelocidadeAtaque };
            result.TotalPoints = values.Values.Sum();
            var parsed = MagoMethodBook.Tokenize(source);
            if (!parsed.IsSuccess || parsed.Tokens.Count % 7 != 0)
            { result.Error = "Use " + initial.InstanceName + ".setAtributo(valor); na Preparação."; return result; }
            var tokens = parsed.Tokens;
            for (int i = 0; i < tokens.Count; i += 7)
            {
                int setter = Array.IndexOf(MagoMethodBook.Names, tokens[i + 2].Lexeme);
                if (tokens[i].Lexeme != initial.InstanceName || tokens[i + 1].Lexeme != "." ||
                    tokens[i + 3].Lexeme != "(" || tokens[i + 5].Lexeme != ")" || tokens[i + 6].Lexeme != ";" ||
                    setter < 0 || setter >= MagoMethodBook.Attributes.Length ||
                    tokens[i + 4].Kind != TokenKind.IntegerLiteral || !int.TryParse(tokens[i + 4].Lexeme, out int value))
                { result.Error = "Chamada inválida na linha " + tokens[i].Position.Line + ". Use um setter do objeto " + initial.InstanceName + "."; return result; }
                if (!methods.IsLearned(tokens[i + 2].Lexeme))
                { result.Error = "Implemente e aprove " + tokens[i + 2].Lexeme + " na área Métodos antes de chamá-lo."; return result; }
                values[MagoMethodBook.Attributes[setter]] = value;
            }
            // Validate only the final draft. The live Mago is immutable and never partially updated.
            long total = values.Values.Aggregate(0L, (sum, value) => sum + value);
            result.TotalPoints = (int)Math.Min(int.MaxValue, total);
            foreach (var pair in values)
                if (pair.Value < 1 || pair.Value > 15)
                { result.Error = pair.Key + " deve terminar entre 1 e 15."; return result; }
            if (total > 25)
            { result.Error = total + "/25 — redistribua " + (total - 25) + " pontos."; return result; }
            result.Mago = MagoState.FromValidatedProgram(new ValidatedMagoProgram("Mago", values.Keys, initial.InstanceName, values, 25));
            return result;
        }
    }
}