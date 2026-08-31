using System;

namespace PrograMago.Domain
{
    public sealed class ValidatedClassDeclaration
    {
        public ValidatedClassDeclaration(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("A class name is required.", nameof(name));
            }

            Name = name;
        }

        public string Name { get; }
    }
}
