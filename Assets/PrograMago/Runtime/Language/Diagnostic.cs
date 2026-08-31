namespace PrograMago.Language
{
    public sealed class Diagnostic
    {
        public Diagnostic(string code, SourcePosition position, string detail)
        {
            Code = code;
            Position = position;
            Detail = detail;
        }

        public string Code { get; }

        public SourcePosition Position { get; }

        public string Detail { get; }
    }
}
