using System;

namespace PrograMago.Domain
{
    public sealed class BattleLessonContent
    {
        public BattleLessonContent(
            string title,
            string concept,
            string whatItIs,
            string purpose,
            string usageExample,
            string gameEffect,
            string task)
        {
            Title = RequireText(title, nameof(title));
            Concept = RequireText(concept, nameof(concept));
            WhatItIs = RequireText(whatItIs, nameof(whatItIs));
            Purpose = RequireText(purpose, nameof(purpose));
            UsageExample = RequireText(usageExample, nameof(usageExample));
            GameEffect = RequireText(gameEffect, nameof(gameEffect));
            Task = RequireText(task, nameof(task));
        }

        public string Title { get; }

        public string Concept { get; }

        public string WhatItIs { get; }

        public string Purpose { get; }

        public string UsageExample { get; }

        public string GameEffect { get; }

        public string Task { get; }

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
