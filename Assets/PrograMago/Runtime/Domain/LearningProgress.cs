using System;

namespace PrograMago.Domain
{
    public sealed class LearningProgress
    {
        private readonly int phaseOneBattleCount;
        private readonly int playableBattleCount;
        private readonly int[] attemptCounts;
        private readonly int[] failedCounts;
        private readonly bool[] completed;

        public LearningProgress(LearningPath path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            int firstChapter = path.Battles[0].Chapter;
            while (phaseOneBattleCount < path.Battles.Count &&
                   path.Battles[phaseOneBattleCount].Chapter == firstChapter)
            {
                phaseOneBattleCount++;
            }

            playableBattleCount = phaseOneBattleCount;
            if (playableBattleCount < path.Battles.Count &&
                path.Battles[playableBattleCount].Criterion ==
                    ValidationCriterion.ConstructAndInstantiateEnemy)
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
                for (int index = 0; index < phaseOneBattleCount; index++)
                {
                    total += attemptCounts[index];
                }

                return total;
            }
        }

        public bool HasCompletedPhaseOne => completed[phaseOneBattleCount - 1];

        public bool IsPhaseOneBoundary => CurrentBattleIndex == phaseOneBattleCount - 1 &&
                                          phaseOneBattleCount < Path.Battles.Count;

        public bool CanContinueAfterVictory => Stage == LearningStage.VictoryReview &&
            (CurrentBattleIndex == Path.Battles.Count - 1 ||
             CurrentBattleIndex + 1 < playableBattleCount);

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

            if (CurrentBattleIndex + 1 >= playableBattleCount)
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
                activeBlock = activeBlock,
                currentBattleIndex = CurrentBattleIndex,
                stage = Stage
            };
        }

        public void RestorePhaseOne(PhaseOneSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.version < 4) data = UpgradeLegacySnapshot(data);

            int expectedCount = playableBattleCount;
            if (data.version != 4 ||
                data.battleIds?.Length != expectedCount ||
                data.attemptCounts?.Length != expectedCount ||
                data.failedCounts?.Length != expectedCount ||
                data.completed?.Length != expectedCount ||
                (data.version >= 2 &&
                    (data.sourceBlocks?.Length != 3 || data.activeBlock < 0 || data.activeBlock >= 3)))
            {
                throw new ArgumentException("Registro da fase 1 inválido.", nameof(data));
            }

            bool foundPendingBattle = false;
            int firstPendingBattle = expectedCount;
            for (int index = 0; index < expectedCount; index++)
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

            if (data.version == 4 &&
                (data.currentBattleIndex < 0 || data.currentBattleIndex >= playableBattleCount ||
                 !Enum.IsDefined(typeof(LearningStage), data.stage) ||
                 (data.stage == LearningStage.VictoryReview &&
                     (!data.completed[data.currentBattleIndex] ||
                      data.currentBattleIndex != firstPendingBattle - 1)) ||
                 ((data.stage == LearningStage.Editing ||
                   data.stage == LearningStage.BattleInProgress) &&
                     (data.currentBattleIndex != firstPendingBattle ||
                      data.currentBattleIndex >= playableBattleCount)) ||
                 (data.stage == LearningStage.JourneyCompleted &&
                     (firstPendingBattle != playableBattleCount ||
                      playableBattleCount != Path.Battles.Count ||
                      data.currentBattleIndex != playableBattleCount - 1))))
            {
                throw new ArgumentException("Registro da fase 1 incompatível.", nameof(data));
            }

            Array.Copy(data.attemptCounts, attemptCounts, expectedCount);
            Array.Copy(data.failedCounts, failedCounts, expectedCount);
            Array.Copy(data.completed, completed, expectedCount);
            CurrentBattleIndex = data.currentBattleIndex;
            Stage = data.stage == LearningStage.BattleInProgress
                ? LearningStage.Editing : data.stage;
            FailedAttempts = Stage == LearningStage.Editing ? failedCounts[CurrentBattleIndex] : 0;
            CurrentHint = FailedAttempts == 0
                ? null
                : CurrentBattle.Hints[Math.Min(FailedAttempts - 1, CurrentBattle.Hints.Count - 1)];
        }

        private PhaseOneSaveData UpgradeLegacySnapshot(PhaseOneSaveData legacy)
        {
            if (legacy.version < 1 || legacy.version > 3 ||
                legacy.battleIds == null || legacy.attemptCounts?.Length != legacy.battleIds.Length ||
                legacy.failedCounts?.Length != legacy.battleIds.Length ||
                legacy.completed?.Length != legacy.battleIds.Length ||
                (legacy.version >= 2 &&
                    (legacy.sourceBlocks?.Length != 3 || legacy.activeBlock < 0 || legacy.activeBlock >= 3)))
                throw new ArgumentException("Registro antigo da fase 1 inválido.", nameof(legacy));

            var ids = new string[playableBattleCount];
            var attempts = new int[playableBattleCount];
            var failed = new int[playableBattleCount];
            var done = new bool[playableBattleCount];
            var mapped = new bool[playableBattleCount];
            for (int index = 0; index < playableBattleCount; index++)
                ids[index] = Path.Battles[index].Id;

            int legacyCurrentTarget = -1;
            for (int index = 0; index < legacy.battleIds.Length; index++)
            {
                int target = FindPathBattleIndex(legacy.battleIds[index]);
                if (target < 0 || target >= playableBattleCount || mapped[target] ||
                    legacy.attemptCounts[index] < 0 || legacy.failedCounts[index] < 0 ||
                    legacy.failedCounts[index] > legacy.attemptCounts[index] ||
                    (legacy.completed[index] && legacy.attemptCounts[index] == 0))
                    throw new ArgumentException("Registro antigo da fase 1 incompatível.", nameof(legacy));

                mapped[target] = true;
                attempts[target] = legacy.attemptCounts[index];
                failed[target] = legacy.failedCounts[index];
                done[target] = legacy.completed[index];
                if (legacy.version == 3 && index == legacy.currentBattleIndex)
                    legacyCurrentTarget = target;
            }

            int oldEnemy = Array.IndexOf(legacy.battleIds, "enemy-object");
            bool reachedLegacyEnemy = oldEnemy >= 0 &&
                (legacy.version == 3 && legacy.currentBattleIndex == oldEnemy ||
                 legacy.attemptCounts[oldEnemy] > 0 || legacy.completed[oldEnemy]);
            if (reachedLegacyEnemy)
            {
                int setterIndex = FindPathBattleIndexByCriterion(ValidationCriterion.AddMagoSetters);
                if (setterIndex >= 0 && setterIndex < playableBattleCount)
                {
                    done[setterIndex] = true;
                    attempts[setterIndex] = Math.Max(attempts[setterIndex], 1);
                }
            }

            int currentBattleIndex;
            LearningStage stage;
            if (legacy.version == 3)
            {
                if (legacy.currentBattleIndex < 0 ||
                    legacy.currentBattleIndex >= legacy.battleIds.Length ||
                    legacyCurrentTarget < 0 || !Enum.IsDefined(typeof(LearningStage), legacy.stage))
                    throw new ArgumentException("Estado do registro antigo inválido.", nameof(legacy));
                currentBattleIndex = legacyCurrentTarget;
                stage = legacy.stage;
            }
            else
            {
                bool completedLegacySequence = Array.TrueForAll(legacy.completed, item => item);
                if (completedLegacySequence)
                {
                    currentBattleIndex = FindPathBattleIndex(legacy.battleIds[legacy.battleIds.Length - 1]);
                    stage = LearningStage.VictoryReview;
                }
                else
                {
                    currentBattleIndex = 0;
                    while (currentBattleIndex < playableBattleCount && done[currentBattleIndex])
                        currentBattleIndex++;
                    if (currentBattleIndex >= playableBattleCount)
                        currentBattleIndex = playableBattleCount - 1;
                    stage = LearningStage.Editing;
                }
            }

            string[] sourceBlocks = legacy.version >= 2
                ? (string[])legacy.sourceBlocks.Clone()
                : new[] { legacy.sourceCode ?? string.Empty, string.Empty, string.Empty };
            int activeBlock = legacy.version >= 2 ? legacy.activeBlock : 0;
            return new PhaseOneSaveData
            {
                version = 4,
                battleIds = ids,
                attemptCounts = attempts,
                failedCounts = failed,
                completed = done,
                sourceCode = legacy.sourceCode ?? string.Empty,
                approvedCode = legacy.approvedCode ?? string.Empty,
                sourceBlocks = sourceBlocks,
                activeBlock = activeBlock,
                currentBattleIndex = currentBattleIndex,
                stage = stage,
                combatBlocks = legacy.combatBlocks,
                timelineOrder = legacy.timelineOrder,
                approvedMethods = legacy.approvedMethods,
                methodDraft = legacy.methodDraft,
                preparationCode = legacy.preparationCode
            };
        }

        private int FindPathBattleIndex(string id)
        {
            for (int index = 0; index < Path.Battles.Count; index++)
                if (string.Equals(Path.Battles[index].Id, id, StringComparison.Ordinal)) return index;
            return -1;
        }

        private int FindPathBattleIndexByCriterion(ValidationCriterion criterion)
        {
            for (int index = 0; index < Path.Battles.Count; index++)
                if (Path.Battles[index].Criterion == criterion) return index;
            return -1;
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
