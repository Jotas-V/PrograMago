using System;

namespace PrograMago.Domain
{
    public sealed class LearningSession
    {
        public LearningSession(ExerciseDefinition exercise)
        {
            Exercise = exercise ?? throw new ArgumentNullException(nameof(exercise));
        }

        public ExerciseDefinition Exercise { get; }

        public bool HasDeclaredClass { get; private set; }

        public void Apply(ValidatedClassDeclaration declaration)
        {
            if (declaration == null)
            {
                throw new ArgumentNullException(nameof(declaration));
            }

            if (!string.Equals(
                    declaration.Name,
                    Exercise.ExpectedClassName,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException("The declaration does not belong to this exercise.");
            }

            HasDeclaredClass = true;
        }

        public void Restart()
        {
            HasDeclaredClass = false;
        }
    }
}
