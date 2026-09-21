using System;

namespace PrograMago.Domain
{
    [Serializable]
    public sealed class PhaseOneSaveData
    {
        public int version = 3;
        public string[] battleIds;
        public int[] attemptCounts;
        public int[] failedCounts;
        public bool[] completed;
        public string sourceCode;
        public string approvedCode;
        public string[] sourceBlocks;
        public int activeBlock;
        public int currentBattleIndex;
        public LearningStage stage;
        public string[] combatBlocks;
        public int[] timelineOrder;
        public string[] approvedMethods;
        public string methodDraft;
        public string preparationCode;
    }
}
