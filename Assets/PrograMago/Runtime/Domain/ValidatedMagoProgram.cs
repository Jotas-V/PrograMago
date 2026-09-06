using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PrograMago.Domain
{
    public sealed class ValidatedMagoProgram
    {
        public ValidatedMagoProgram(
            string className,
            IEnumerable<string> attributeNames,
            string instanceName,
            IDictionary<string, int> initialValues,
            int pointBudget)
        {
            ClassName = className;
            AttributeNames = new List<string>(attributeNames ?? Array.Empty<string>()).AsReadOnly();
            InstanceName = instanceName;
            var values = new Dictionary<string, int>(
                initialValues ?? new Dictionary<string, int>(),
                StringComparer.Ordinal);
            InitialValues = new ReadOnlyDictionary<string, int>(values);
            TotalPoints = values.Values.Sum();
            RemainingPoints = pointBudget - TotalPoints;
        }

        public string ClassName { get; }

        public IReadOnlyList<string> AttributeNames { get; }

        public string InstanceName { get; }

        public IReadOnlyDictionary<string, int> InitialValues { get; }

        public int TotalPoints { get; }

        public int RemainingPoints { get; }
    }
}
