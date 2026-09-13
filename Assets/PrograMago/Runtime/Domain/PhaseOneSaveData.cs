using System;

namespace PrograMago.Domain
{
    [Serializable]
    public sealed class PhaseOneSaveData
    {
        public int version = 1;
        public string[] battleIds;
        public int[] attemptCounts;
        public int[] failedCounts;
        public bool[] completed;
        public string sourceCode;
        public string approvedCode;
    }
}
