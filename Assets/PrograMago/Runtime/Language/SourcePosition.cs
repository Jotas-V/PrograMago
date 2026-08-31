namespace PrograMago.Language
{
    public readonly struct SourcePosition
    {
        public SourcePosition(int offset, int line, int column)
        {
            Offset = offset;
            Line = line;
            Column = column;
        }

        public int Offset { get; }

        public int Line { get; }

        public int Column { get; }
    }
}
