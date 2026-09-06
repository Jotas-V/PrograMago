using System;

namespace PrograMago.Domain
{
    public sealed class MagoValidationRules
    {
        public MagoValidationRules(
            int minimumAttributeValue,
            int maximumAttributeValue,
            int pointBudget)
        {
            if (minimumAttributeValue < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumAttributeValue));
            }

            if (maximumAttributeValue < minimumAttributeValue)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumAttributeValue));
            }

            if (pointBudget < minimumAttributeValue * 5)
            {
                throw new ArgumentOutOfRangeException(nameof(pointBudget));
            }

            MinimumAttributeValue = minimumAttributeValue;
            MaximumAttributeValue = maximumAttributeValue;
            PointBudget = pointBudget;
        }

        public int MinimumAttributeValue { get; }

        public int MaximumAttributeValue { get; }

        public int PointBudget { get; }
    }
}
