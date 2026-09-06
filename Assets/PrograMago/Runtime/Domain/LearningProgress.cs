namespace PrograMago.Domain
{
    public sealed class LearningProgress
    {
        public LearningProgress(LearningPath path)
        {
        }

        public LearningPath Path { get; }

        public int CurrentBattleIndex { get; }

        public BattleDefinition CurrentBattle { get; }

        public LearningStage Stage { get; }

        public int FailedAttempts { get; }

        public string CurrentHint { get; }

        public bool TryStartBattle(ValidationCriterion criterion)
        {
            return false;
        }

        public bool ReportVictory()
        {
            return false;
        }

        public bool ReportDefeat()
        {
            return false;
        }

        public bool RestartCurrentBattle()
        {
            return false;
        }

        public bool ContinueAfterVictory()
        {
            return false;
        }

        public string RegisterFailedAttempt()
        {
            return null;
        }
    }
}
