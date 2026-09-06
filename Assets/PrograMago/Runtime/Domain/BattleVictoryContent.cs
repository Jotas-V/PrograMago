using System;

namespace PrograMago.Domain
{
    public sealed class BattleVictoryContent
    {
        public BattleVictoryContent(string title, string achievement, string review)
        {
            Title = RequireText(title, nameof(title));
            Achievement = RequireText(achievement, nameof(achievement));
            Review = RequireText(review, nameof(review));
        }

        public string Title { get; }

        public string Achievement { get; }

        public string Review { get; }

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
