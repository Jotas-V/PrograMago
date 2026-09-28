using System;
using PrograMago.Language;

namespace PrograMago.Application
{
    /// <summary>Identifies code newly inserted inside Mago without allowing edits to approved text.</summary>
    public static class MagoSourceInsertion
    {
        public static bool TryExtract(string approvedSource, string draft,
            out string insertedCode, out string error)
        {
            insertedCode = string.Empty;
            error = null;
            if (string.IsNullOrEmpty(approvedSource) || draft == null)
            {
                error = "O código aprovado do Mago não está disponível.";
                return false;
            }

            TokenizationResult parsed = new CodeTokenizer().Tokenize(approvedSource);
            if (!parsed.IsSuccess)
            {
                error = "Não foi possível localizar o fim da classe Mago aprovada.";
                return false;
            }

            int opening = -1;
            int closeOffset = -1;
            for (int index = 0; index + 3 < parsed.Tokens.Count; index++)
            {
                if (parsed.Tokens[index].Kind != TokenKind.PublicKeyword ||
                    parsed.Tokens[index + 1].Kind != TokenKind.ClassKeyword ||
                    parsed.Tokens[index + 2].Lexeme != "Mago" ||
                    parsed.Tokens[index + 3].Kind != TokenKind.LeftBrace)
                    continue;

                opening = index + 3;
                break;
            }

            if (opening < 0)
            {
                error = "Não foi possível localizar a classe Mago aprovada.";
                return false;
            }

            int depth = 0;
            for (int index = opening; index < parsed.Tokens.Count; index++)
            {
                Token token = parsed.Tokens[index];
                if (token.Kind == TokenKind.LeftBrace) depth++;
                else if (token.Kind == TokenKind.RightBrace && --depth == 0)
                {
                    closeOffset = token.Position.Offset;
                    break;
                }
            }

            if (closeOffset < 0 || closeOffset > approvedSource.Length)
            {
                error = "Não foi possível localizar a chave final da classe Mago.";
                return false;
            }

            string prefix = approvedSource.Substring(0, closeOffset);
            string suffix = approvedSource.Substring(closeOffset);
            if (draft.Length < prefix.Length + suffix.Length ||
                !draft.StartsWith(prefix, StringComparison.Ordinal) ||
                !draft.EndsWith(suffix, StringComparison.Ordinal))
            {
                error = "O código já aprovado está protegido. Acrescente os novos métodos dentro da classe Mago, antes da chave final.";
                return false;
            }

            insertedCode = draft.Substring(prefix.Length, draft.Length - prefix.Length - suffix.Length);
            return true;
        }
    }
}
