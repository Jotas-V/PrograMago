using System;
using System.Collections.Generic;

namespace PrograMago.Domain
{
    /// <summary>
    /// Immutable, validated form choices compiled from the player's strategy block.
    /// The combat engine owns when the strategy is evaluated and what the chosen
    /// spell does in the arena.
    /// </summary>
    public sealed class CombatStrategy
    {
        private static readonly Dictionary<string, CombatElement> FormSpells =
            new Dictionary<string, CombatElement>(StringComparer.Ordinal)
            {
                ["neutro"] = CombatElement.Neutral,
                ["piromante"] = CombatElement.Fire,
                ["hidromante"] = CombatElement.Water,
                ["eletromante"] = CombatElement.Electric
            };

        private readonly Dictionary<string, string> formsByElement;
        private readonly string fallbackForm;

        public CombatStrategy(IReadOnlyDictionary<string, string> formsByElement, string fallbackForm)
        {
            if (!FormSpells.ContainsKey(fallbackForm ?? string.Empty))
                throw new ArgumentException("Forma elemental desconhecida.", nameof(fallbackForm));

            this.formsByElement = new Dictionary<string, string>(StringComparer.Ordinal);
            if (formsByElement != null)
            {
                foreach (KeyValuePair<string, string> choice in formsByElement)
                {
                    if (string.IsNullOrWhiteSpace(choice.Key) || !FormSpells.ContainsKey(choice.Value ?? string.Empty))
                        throw new ArgumentException("A estratégia contém uma condição ou forma inválida.", nameof(formsByElement));
                    if (!this.formsByElement.TryAdd(choice.Key, choice.Value))
                        throw new ArgumentException("A estratégia repete uma condição elemental.", nameof(formsByElement));
                }
            }

            this.fallbackForm = fallbackForm;
        }

        public string FormFor(string targetElement)
        {
            return targetElement != null && formsByElement.TryGetValue(targetElement, out string form)
                ? form
                : fallbackForm;
        }

        public CombatElement SpellFor(string targetElement)
        {
            return FormSpells[FormFor(targetElement)];
        }
    }
}
