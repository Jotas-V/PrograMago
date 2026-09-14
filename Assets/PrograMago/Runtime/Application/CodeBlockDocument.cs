using System;
using System.Text;

namespace PrograMago.Application
{
    public sealed class CodeBlockDocument
    {
        public const int BlockCount = 3;

        private readonly string[] blocks = new string[BlockCount];

        public CodeBlockDocument()
        {
            for (int index = 0; index < BlockCount; index++) blocks[index] = string.Empty;
        }

        public int ActiveIndex { get; private set; }

        public string ActiveText => blocks[ActiveIndex];

        public string SourceCode
        {
            get
            {
                var source = new StringBuilder();
                foreach (string block in blocks)
                {
                    if (string.IsNullOrEmpty(block)) continue;
                    if (source.Length > 0) source.Append('\n');
                    source.Append(block);
                }

                return source.ToString();
            }
        }

        public void SetActiveText(string text)
        {
            blocks[ActiveIndex] = text ?? string.Empty;
        }

        public void Select(int index)
        {
            if (index < 0 || index >= BlockCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            ActiveIndex = index;
        }

        public string[] Snapshot()
        {
            return (string[])blocks.Clone();
        }

        public void Restore(string[] saved, int activeIndex)
        {
            if (saved == null || saved.Length != BlockCount)
            {
                throw new ArgumentException("São necessários três blocos.", nameof(saved));
            }

            Select(activeIndex);
            for (int index = 0; index < BlockCount; index++)
            {
                blocks[index] = saved[index] ?? string.Empty;
            }
        }

        public CodeBlockLocation Locate(int offset)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

            int combinedOffset = 0;
            int lastNonEmptyIndex = -1;
            for (int index = 0; index < BlockCount; index++)
            {
                string block = blocks[index];
                if (string.IsNullOrEmpty(block)) continue;

                if (lastNonEmptyIndex >= 0) combinedOffset++;
                int start = combinedOffset;
                int end = start + block.Length;
                if (offset <= end)
                {
                    return PositionInBlock(index + 1, block, Math.Max(0, offset - start));
                }

                combinedOffset = end;
                lastNonEmptyIndex = index;
            }

            if (lastNonEmptyIndex < 0) return new CodeBlockLocation(ActiveIndex + 1, 1, 1);
            return PositionInBlock(lastNonEmptyIndex + 1, blocks[lastNonEmptyIndex],
                blocks[lastNonEmptyIndex].Length);
        }

        private static CodeBlockLocation PositionInBlock(int number, string text, int offset)
        {
            int line = 1;
            int column = 1;
            for (int index = 0; index < offset && index < text.Length; index++)
            {
                if (text[index] == '\r')
                {
                    if (index + 1 < offset && text[index + 1] == '\n') index++;
                    line++;
                    column = 1;
                }
                else if (text[index] == '\n')
                {
                    line++;
                    column = 1;
                }
                else
                {
                    column++;
                }
            }

            return new CodeBlockLocation(number, line, column);
        }
    }

    public readonly struct CodeBlockLocation
    {
        public CodeBlockLocation(int blockNumber, int line, int column)
        {
            BlockNumber = blockNumber;
            Line = line;
            Column = column;
        }

        public int BlockNumber { get; }
        public int Line { get; }
        public int Column { get; }
    }
}
