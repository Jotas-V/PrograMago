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
