using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PrograMago.Domain;
using PrograMago.Language;

namespace PrograMago.Application
{
    // The timeline exposes a deliberately small command language, not arbitrary C# execution.
    public static class CombatCodeCompiler
    {
        public static string[] DefaultBlocks() => new[]
            { "analisarAlvo();", "selecionarMagia();", "lancarMagia();" };

        public static string[] BattleBlocks() => new[] { "lancarMagia();" };

        public static bool TryCompileStrategy(string source, string instanceName,
            out CombatStrategy strategy, out string error)
        {
            strategy = null;
            error = null;
            if (string.IsNullOrWhiteSpace(instanceName))
            {
                error = "Conclua a instância do Mago antes de escrever a Estratégia.";
                return false;
            }

            string remaining = Regex.Replace(source ?? "", @"//[^\r\n]*|/\*[\s\S]*?\*/", " ").Trim();
            var formsByElement = new Dictionary<string, string>(StringComparer.Ordinal);
            if (!TryReadStrategyBranch(ref remaining, instanceName, first: true,
                out string element, out string form))
            {
                error = "Use if (alvo.getElemento().equals(\"gelo\")) para escolher uma forma.";
                return false;
            }

            while (true)
            {
                if (!IsElement(element) || !IsForm(form))
                {
                    error = "Use elementos gelo, fogo, água ou neutro e formas piromante, hidromante, eletromante ou neutro.";
                    return false;
                }
                if (formsByElement.ContainsKey(element))
                {
                    error = "Cada elemento pode aparecer uma única vez na Estratégia.";
                    return false;
                }
                formsByElement.Add(element, form);

                string beforeBranch = remaining;
                if (!TryReadStrategyBranch(ref remaining, instanceName, first: false,
                    out element, out form))
                {
                    remaining = beforeBranch;
                    break;
                }
            }

            if (!TryReadFallback(ref remaining, instanceName, out string fallbackForm) || !IsForm(fallbackForm))
            {
                error = "Finalize o if/else com uma forma para os outros elementos.";
                return false;
            }

            string attackPattern = @"^" + Regex.Escape(instanceName) +
                @"\s*\.\s*lancarMagia\s*\(\s*alvo\s*\)\s*;\s*$";
            if (!Regex.IsMatch(remaining, attackPattern, RegexOptions.Singleline))
            {
                error = "Depois do if/else, chame " + instanceName + ".lancarMagia(alvo);";
                return false;
            }

            strategy = new CombatStrategy(formsByElement, fallbackForm);
            return true;
        }

        private static bool TryReadStrategyBranch(ref string source, string instanceName,
            bool first, out string element, out string form)
        {
            element = null;
            form = null;
            string prefix = first ? @"^if" : @"^else\s+if";
            string pattern = prefix + @"\s*\(\s*alvo\s*\.\s*getElemento\s*\(\s*\)\s*\.\s*equals\s*\(\s*""(?<element>[^""]+)""\s*\)\s*\)\s*\{\s*" +
                Regex.Escape(instanceName) + @"\s*\.\s*selecionarForma\s*\(\s*""(?<form>[^""]+)""\s*\)\s*;\s*\}";
            Match match = Regex.Match(source, pattern, RegexOptions.Singleline);
            if (!match.Success) return false;
            element = match.Groups["element"].Value;
            form = match.Groups["form"].Value;
            source = source.Substring(match.Length).TrimStart();
            return true;
        }

        private static bool TryReadFallback(ref string source, string instanceName, out string form)
        {
            form = null;
            string pattern = @"^else\s*\{\s*" + Regex.Escape(instanceName) +
                @"\s*\.\s*selecionarForma\s*\(\s*""(?<form>[^""]+)""\s*\)\s*;\s*\}";
            Match match = Regex.Match(source, pattern, RegexOptions.Singleline);
            if (!match.Success) return false;
            form = match.Groups["form"].Value;
            source = source.Substring(match.Length).TrimStart();
            return true;
        }

        private static bool IsElement(string element) => element == "gelo" || element == "fogo" ||
            element == "água" || element == "neutro";

        private static bool IsForm(string form) => form == "neutro" || form == "piromante" ||
            form == "hidromante" || form == "eletromante";

        public static bool TryCompileBattle(IReadOnlyList<string> blocks, string instanceName,
            out CombatAction[] actions, out int[] sourceBlocks, out int errorBlock, out string error)
        {
            actions = Array.Empty<CombatAction>();
            sourceBlocks = Array.Empty<int>();
            errorBlock = -1;
            error = null;
            if (blocks == null || blocks.Count != 1)
            {
                error = "Use um único bloco Atacar na timeline.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(instanceName))
            {
                error = "A instância do Mago precisa estar pronta antes da batalha.";
                return false;
            }

            var compiled = new List<CombatAction>();
            var origins = new List<int>();
            for (int block = 0; block < blocks.Count; block++)
            {
                errorBlock = block;
                string code = Regex.Replace(blocks[block] ?? "", @"//[^\r\n]*|/\*[\s\S]*?\*/", " ");
                TokenizationResult tokens = new CodeTokenizer().Tokenize(code);
                error = "Use lancarMagia(); ou " + instanceName + ".lancarMagia(); em cada bloco.";
                if (!tokens.IsSuccess || (tokens.Tokens.Count != 4 && tokens.Tokens.Count != 6)) return false;
                int nameIndex = tokens.Tokens.Count == 4 ? 0 : 2;
                if (tokens.Tokens.Count == 6 &&
                    (tokens.Tokens[0].Lexeme != instanceName || tokens.Tokens[1].Lexeme != ".")) return false;
                if (tokens.Tokens[nameIndex].Lexeme != "lancarMagia" ||
                    tokens.Tokens[nameIndex + 1].Lexeme != "(" ||
                    tokens.Tokens[nameIndex + 2].Lexeme != ")" ||
                    tokens.Tokens[nameIndex + 3].Lexeme != ";") return false;
                compiled.Add(CombatAction.Cast);
                origins.Add(block);
            }
            actions = compiled.ToArray();
            sourceBlocks = origins.ToArray();
            errorBlock = -1;
            error = null;
            return true;
        }
        public static bool TryCompile(IReadOnlyList<string> blocks, out CombatAction[] actions,
            out int[] sourceBlocks, out int errorBlock, out string error)
        {
            var compiled = new List<CombatAction>();
            var origins = new List<int>();
            actions = Array.Empty<CombatAction>();
            sourceBlocks = Array.Empty<int>();
            errorBlock = -1;
            error = null;
            if (blocks == null || blocks.Count == 0 || blocks.Count > 16)
            {
                error = "Use de 1 a 16 blocos de ação.";
                return false;
            }
            for (int block = 0; block < blocks.Count; block++)
            {
                string code = Regex.Replace(blocks[block] ?? "", @"//[^\r\n]*|/\*[\s\S]*?\*/", " ");
                TokenizationResult tokens = new CodeTokenizer().Tokenize(code);
                errorBlock = block;
                error = "Use analisarAlvo();, selecionarMagia(); ou lancarMagia();, sem parâmetros. Cada chamada termina com ;.";
                if (!tokens.IsSuccess || tokens.Tokens.Count % 4 != 0) return false;
                for (int index = 0; index < tokens.Tokens.Count; index += 4)
                {
                    var items = tokens.Tokens;
                    if (items[index + 1].Lexeme != "(" || items[index + 2].Lexeme != ")" || items[index + 3].Lexeme != ";") return false;
                    CombatAction action;
                    switch (items[index].Lexeme)
                    {
                        case "analisarAlvo": action = CombatAction.AnalyzeTarget; break;
                        case "selecionarMagia": action = CombatAction.SelectSpell; break;
                        case "lancarMagia": action = CombatAction.Attack; break;
                        default: return false;
                    }
                    if (compiled.Count >= 48) { error = "Use no máximo 48 chamadas por ciclo."; return false; }
                    compiled.Add(action);
                    origins.Add(block);
                }
            }
            if (!compiled.Contains(CombatAction.Attack))
            {
                error = "Adicione um bloco com lancarMagia(); para atacar.";
                return false;
            }
            actions = compiled.ToArray();
            sourceBlocks = origins.ToArray();
            errorBlock = -1;
            error = null;
            return true;
        }
    }
}
