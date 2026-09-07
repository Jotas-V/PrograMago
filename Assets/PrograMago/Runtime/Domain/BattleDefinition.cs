using System;
using System.Collections.Generic;

namespace PrograMago.Domain
{
    public sealed class BattleDefinition
    {
        public BattleDefinition(
            string id,
            int chapter,
            int order,
            BattleLessonContent lesson,
            IEnumerable<string> hints,
            ValidationCriterion criterion,
            BattleVictoryContent victory)
            : this(
                id,
                chapter,
                order,
                lesson,
                hints,
                criterion,
                victory,
                BattleCompletionMode.OnCombatVictory)
        {
        }

        public BattleDefinition(
            string id,
            int chapter,
            int order,
            BattleLessonContent lesson,
            IEnumerable<string> hints,
            ValidationCriterion criterion,
            BattleVictoryContent victory,
            BattleCompletionMode completionMode)
        {
            Id = RequireText(id, nameof(id));
            if (chapter <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chapter));
            }

            if (order <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(order));
            }

            Chapter = chapter;
            Order = order;
            Lesson = lesson ?? throw new ArgumentNullException(nameof(lesson));
            if (hints == null)
            {
                throw new ArgumentNullException(nameof(hints));
            }

            var validatedHints = new List<string>();
            foreach (string hint in hints)
            {
                validatedHints.Add(RequireText(hint, nameof(hints)));
            }

            if (validatedHints.Count == 0)
            {
                throw new ArgumentException("At least one hint is required.", nameof(hints));
            }

            if (!Enum.IsDefined(typeof(ValidationCriterion), criterion))
            {
                throw new ArgumentOutOfRangeException(nameof(criterion));
            }

            if (!Enum.IsDefined(typeof(BattleCompletionMode), completionMode))
            {
                throw new ArgumentOutOfRangeException(nameof(completionMode));
            }

            Hints = validatedHints.AsReadOnly();
            Criterion = criterion;
            Victory = victory ?? throw new ArgumentNullException(nameof(victory));
            CompletionMode = completionMode;
        }

        public string Id { get; }

        public int Chapter { get; }

        public int Order { get; }

        public BattleLessonContent Lesson { get; }

        public IReadOnlyList<string> Hints { get; }

        public ValidationCriterion Criterion { get; }

        public BattleVictoryContent Victory { get; }

        public BattleCompletionMode CompletionMode { get; }

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
