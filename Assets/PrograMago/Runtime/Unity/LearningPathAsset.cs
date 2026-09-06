using System;
using PrograMago.Domain;
using UnityEngine;

namespace PrograMago.UnityIntegration
{
    [CreateAssetMenu(fileName = "LearningPath", menuName = "PrograMago/Learning Path")]
    public sealed class LearningPathAsset : ScriptableObject
    {
        [SerializeField] private BattleRecord[] battles = Array.Empty<BattleRecord>();

        public LearningPath ToDomain()
        {
            return new LearningPath(Array.ConvertAll(
                battles ?? Array.Empty<BattleRecord>(),
                battle => battle.ToDomain()));
        }

        [Serializable]
        private sealed class BattleRecord
        {
            [SerializeField] private string id;
            [SerializeField] private int chapter;
            [SerializeField] private int order;
            [SerializeField] private string title;
            [SerializeField] private string concept;
            [SerializeField] private string whatItIs;
            [SerializeField] private string purpose;
            [SerializeField] private string usageExample;
            [SerializeField] private string gameEffect;
            [SerializeField] private string task;
            [SerializeField] private string[] hints = Array.Empty<string>();
            [SerializeField] private ValidationCriterion criterion;
            [SerializeField] private string victoryTitle;
            [SerializeField] private string victoryAchievement;
            [SerializeField] private string victoryReview;

            public BattleDefinition ToDomain()
            {
                return new BattleDefinition(
                    id,
                    chapter,
                    order,
                    new BattleLessonContent(
                        title,
                        concept,
                        whatItIs,
                        purpose,
                        usageExample,
                        gameEffect,
                        task),
                    hints,
                    criterion,
                    new BattleVictoryContent(
                        victoryTitle,
                        victoryAchievement,
                        victoryReview));
            }
        }
    }
}
