using System;

namespace PrograMago.Domain
{
    public sealed class ExerciseDefinition
    {
        public ExerciseDefinition(string id, string expectedClassName, string instructions)
        {
            Id = RequireText(id, nameof(id));
            ExpectedClassName = RequireText(expectedClassName, nameof(expectedClassName));
            Instructions = RequireText(instructions, nameof(instructions));
        }

        public string Id { get; }

        public string ExpectedClassName { get; }

        public string Instructions { get; }

        private static string RequireText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A value is required.", parameterName);
            }

            return value;
        }
    }
}
