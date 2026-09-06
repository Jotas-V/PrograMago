using System;

namespace PrograMago.Domain
{
    public sealed class LearningProgress
    {
        public LearningProgress(LearningPath path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Stage = LearningStage.Editing;
        }

        public LearningPath Path { get; }

        public int CurrentBattleIndex { get; private set; }

        public BattleDefinition CurrentBattle => Path.Battles[CurrentBattleIndex];

        public LearningStage Stage { get; private set; }

        public int FailedAttempts { get; private set; }

        public string CurrentHint { get; private set; }

        public bool TryStartBattle(ValidationCriterion criterion)
        {
            if (Stage != LearningStage.Editing || CurrentBattle.Criterion != criterion)
            {
                return false;
            }

            Stage = LearningStage.BattleInProgress;
            return true;
        }

        public bool ReportVictory()
        {
            if (Stage != LearningStage.BattleInProgress)
            {
                return false;
            }

            Stage = LearningStage.VictoryReview;
            return true;
        }

        public bool ReportDefeat()
        {
            if (Stage != LearningStage.BattleInProgress)
            {
                return false;
            }

            Stage = LearningStage.Editing;
            return true;
        }

        public bool RestartCurrentBattle()
        {
            if (Stage == LearningStage.VictoryReview || Stage == LearningStage.JourneyCompleted)
            {
                return false;
            }

            Stage = LearningStage.Editing;
            return true;
        }

        public bool ContinueAfterVictory()
        {
            if (Stage != LearningStage.VictoryReview)
            {
                return false;
            }

            if (CurrentBattleIndex == Path.Battles.Count - 1)
            {
                Stage = LearningStage.JourneyCompleted;
                return false;
            }

            CurrentBattleIndex++;
            FailedAttempts = 0;
            CurrentHint = null;
            Stage = LearningStage.Editing;
            return true;
        }

        public string RegisterFailedAttempt()
        {
            if (Stage != LearningStage.Editing)
            {
                return CurrentHint;
            }

            FailedAttempts++;
            int hintIndex = Math.Min(FailedAttempts - 1, CurrentBattle.Hints.Count - 1);
            CurrentHint = CurrentBattle.Hints[hintIndex];
            return CurrentHint;
        }
    }
}
