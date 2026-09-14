using System;

namespace PrograMago.Domain
{
    public sealed class LearningProgress
    {
        private readonly int playableBattleCount;
        private readonly int[] attemptCounts;
        private readonly int[] failedCounts;
        private readonly bool[] completed;

        public LearningProgress(LearningPath path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            int firstChapter = path.Battles[0].Chapter;
            while (playableBattleCount < path.Battles.Count &&
                   path.Battles[playableBattleCount].Chapter == firstChapter)
            {
                playableBattleCount++;
            }

            attemptCounts = new int[playableBattleCount];
            failedCounts = new int[playableBattleCount];
            completed = new bool[playableBattleCount];
            Stage = LearningStage.Editing;
        }

        public LearningPath Path { get; }

        public int CurrentBattleIndex { get; private set; }

        public BattleDefinition CurrentBattle => Path.Battles[CurrentBattleIndex];

        public LearningStage Stage { get; private set; }

        public int FailedAttempts { get; private set; }

        public string CurrentHint { get; private set; }

        public int TotalPhaseOneAttempts
        {
            get
            {
                int total = 0;
                foreach (int count in attemptCounts)
                {
                    total += count;
                }

                return total;
            }
        }

        public bool HasCompletedPhaseOne => completed[playableBattleCount - 1];

        public bool IsPhaseOneBoundary => CurrentBattleIndex == playableBattleCount - 1 &&
                                          playableBattleCount < Path.Battles.Count;

        public int GetAttemptCount(string battleId)
        {
            return attemptCounts[FindPlayableBattle(battleId)];
        }

        public bool IsCompleted(string battleId)
        {
            return completed[FindPlayableBattle(battleId)];
        }

        public void RegisterSubmission()
        {
            if (Stage == LearningStage.Editing)
            {
                attemptCounts[CurrentBattleIndex]++;
            }
        }

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
            completed[CurrentBattleIndex] = true;
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

            if (Path.Battles[CurrentBattleIndex + 1].Chapter != CurrentBattle.Chapter)
            {
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
            failedCounts[CurrentBattleIndex] = FailedAttempts;
            int hintIndex = Math.Min(FailedAttempts - 1, CurrentBattle.Hints.Count - 1);
            CurrentHint = CurrentBattle.Hints[hintIndex];
            return CurrentHint;
        }

        public PhaseOneSaveData CapturePhaseOne(string sourceCode, string approvedCode)
        {
            return CapturePhaseOne(sourceCode, approvedCode,
                new[] { sourceCode ?? string.Empty, string.Empty, string.Empty }, 0);
        }

        public PhaseOneSaveData CapturePhaseOne(
            string sourceCode, string approvedCode, string[] sourceBlocks, int activeBlock)
        {
            if (sourceBlocks == null || sourceBlocks.Length != 3 ||
                activeBlock < 0 || activeBlock >= 3)
            {
                throw new ArgumentException("Registro dos blocos de código inválido.");
            }

            var ids = new string[playableBattleCount];
            for (int index = 0; index < ids.Length; index++)
            {
                ids[index] = Path.Battles[index].Id;
            }

            return new PhaseOneSaveData
            {
                battleIds = ids,
                attemptCounts = (int[])attemptCounts.Clone(),
                failedCounts = (int[])failedCounts.Clone(),
                completed = (bool[])completed.Clone(),
                sourceCode = sourceCode ?? string.Empty,
                approvedCode = approvedCode ?? string.Empty,
                sourceBlocks = (string[])sourceBlocks.Clone(),
                activeBlock = activeBlock
            };
        }

        public void RestorePhaseOne(PhaseOneSaveData data)
        {
            if (data == null || (data.version != 1 && data.version != 2) ||
                data.battleIds?.Length != playableBattleCount ||
                data.attemptCounts?.Length != playableBattleCount ||
                data.failedCounts?.Length != playableBattleCount ||
                data.completed?.Length != playableBattleCount ||
                (data.version == 2 &&
                    (data.sourceBlocks?.Length != 3 || data.activeBlock < 0 || data.activeBlock >= 3)))
            {
                throw new ArgumentException("Registro da fase 1 inválido.", nameof(data));
            }

            bool foundPendingBattle = false;
            int firstPendingBattle = playableBattleCount;
            for (int index = 0; index < playableBattleCount; index++)
            {
                if (!string.Equals(data.battleIds[index], Path.Battles[index].Id,
                        StringComparison.Ordinal) ||
                    data.attemptCounts[index] < 0 || data.failedCounts[index] < 0 ||
                    data.failedCounts[index] > data.attemptCounts[index] ||
                    (data.completed[index] && data.attemptCounts[index] == 0) ||
                    (foundPendingBattle && data.completed[index]))
                {
                    throw new ArgumentException("Registro da fase 1 incompatível.", nameof(data));
                }

                if (!data.completed[index] && !foundPendingBattle)
                {
                    foundPendingBattle = true;
                    firstPendingBattle = index;
                }
            }

            Array.Copy(data.attemptCounts, attemptCounts, playableBattleCount);
            Array.Copy(data.failedCounts, failedCounts, playableBattleCount);
            Array.Copy(data.completed, completed, playableBattleCount);
            CurrentBattleIndex = foundPendingBattle ? firstPendingBattle : playableBattleCount - 1;
            FailedAttempts = foundPendingBattle ? failedCounts[CurrentBattleIndex] : 0;
            CurrentHint = FailedAttempts == 0
                ? null
                : CurrentBattle.Hints[Math.Min(FailedAttempts - 1, CurrentBattle.Hints.Count - 1)];
            Stage = foundPendingBattle
                ? LearningStage.Editing
                : LearningStage.VictoryReview;
        }

        private int FindPlayableBattle(string battleId)
        {
            for (int index = 0; index < playableBattleCount; index++)
            {
                if (string.Equals(Path.Battles[index].Id, battleId, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            throw new ArgumentException("Batalha fora da fase 1.", nameof(battleId));
        }
    }
}
