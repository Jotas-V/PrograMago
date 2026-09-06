using System;
using System.Collections.Generic;

namespace PrograMago.Domain
{
    public sealed class LearningPath
    {
        public LearningPath(IEnumerable<BattleDefinition> battles)
        {
            if (battles == null)
            {
                throw new ArgumentNullException(nameof(battles));
            }

            var definitions = new List<BattleDefinition>(battles);
            if (definitions.Count == 0)
            {
                throw new ArgumentException("At least one battle is required.", nameof(battles));
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            var orders = new HashSet<int>();
            foreach (BattleDefinition battle in definitions)
            {
                if (battle == null)
                {
                    throw new ArgumentException("Battle definitions cannot be null.", nameof(battles));
                }

                if (!ids.Add(battle.Id) || !orders.Add(battle.Order))
                {
                    throw new ArgumentException("Battle ids and orders must be unique.", nameof(battles));
                }
            }

            definitions.Sort((left, right) => left.Order.CompareTo(right.Order));
            Battles = definitions.AsReadOnly();
        }

        public IReadOnlyList<BattleDefinition> Battles { get; }
    }
}
