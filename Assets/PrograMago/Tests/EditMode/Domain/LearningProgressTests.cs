using System;
using System.Collections.Generic;
using NUnit.Framework;
using PrograMago.Domain;

namespace PrograMago.Tests.Domain
{
    public sealed class LearningProgressTests
    {
        [Test]
        public void DomainAssembly_ExposesLearningPath()
        {
            System.Type type = typeof(ExerciseDefinition).Assembly.GetType(
                "PrograMago.Domain.LearningPath");

            Assert.That(type, Is.Not.Null);
        }

        [Test]
        public void DomainAssembly_ExposesLearningProgress()
        {
            System.Type type = typeof(ExerciseDefinition).Assembly.GetType(
                "PrograMago.Domain.LearningProgress");

            Assert.That(type, Is.Not.Null);
        }

        [Test]
        public void DomainAssembly_ExposesLearningStage()
        {
            System.Type type = typeof(ExerciseDefinition).Assembly.GetType(
                "PrograMago.Domain.LearningStage");

            Assert.That(type, Is.Not.Null);
        }

        [Test]
        public void LearningPath_ExposesOrderedBattleContract()
        {
            System.Type type = typeof(LearningPath);

            Assert.That(type.GetConstructor(new[] { typeof(IEnumerable<BattleDefinition>) }), Is.Not.Null);
            Assert.That(
                type.GetProperty("Battles")?.PropertyType,
                Is.EqualTo(typeof(IReadOnlyList<BattleDefinition>)));
        }

        [Test]
        public void LearningProgress_ExposesStateMachineContract()
        {
            System.Type type = typeof(LearningProgress);

            Assert.That(type.GetConstructor(new[] { typeof(LearningPath) }), Is.Not.Null);
            Assert.That(type.GetProperty("Path")?.PropertyType, Is.EqualTo(typeof(LearningPath)));
            Assert.That(type.GetProperty("CurrentBattleIndex")?.PropertyType, Is.EqualTo(typeof(int)));
            Assert.That(type.GetProperty("CurrentBattle")?.PropertyType, Is.EqualTo(typeof(BattleDefinition)));
            Assert.That(type.GetProperty("Stage")?.PropertyType, Is.EqualTo(typeof(LearningStage)));
            Assert.That(type.GetProperty("FailedAttempts")?.PropertyType, Is.EqualTo(typeof(int)));
            Assert.That(type.GetProperty("CurrentHint")?.PropertyType, Is.EqualTo(typeof(string)));
            Assert.That(type.GetMethod("TryStartBattle")?.ReturnType, Is.EqualTo(typeof(bool)));
            Assert.That(type.GetMethod("ReportVictory")?.ReturnType, Is.EqualTo(typeof(bool)));
            Assert.That(type.GetMethod("ReportDefeat")?.ReturnType, Is.EqualTo(typeof(bool)));
            Assert.That(type.GetMethod("RestartCurrentBattle")?.ReturnType, Is.EqualTo(typeof(bool)));
            Assert.That(type.GetMethod("ContinueAfterVictory")?.ReturnType, Is.EqualTo(typeof(bool)));
            Assert.That(type.GetMethod("RegisterFailedAttempt")?.ReturnType, Is.EqualTo(typeof(string)));
        }

        [Test]
        public void LearningStage_DefinesTheApprovedFlow()
        {
            Assert.That(System.Enum.GetNames(typeof(LearningStage)), Is.EquivalentTo(new[]
            {
                "Editing",
                "BattleInProgress",
                "VictoryReview",
                "JourneyCompleted"
            }));
        }

        [Test]
        public void LearningPath_UnorderedBattles_SortsAndCopiesDefinitions()
        {
            var battles = new List<BattleDefinition>
            {
                CreateBattle("second", 2, ValidationCriterion.AddPrivateAttributes),
                CreateBattle("first", 1, ValidationCriterion.DeclareMagoClass)
            };

            var path = new LearningPath(battles);
            battles.Clear();

            Assert.That(path.Battles, Has.Count.EqualTo(2));
            Assert.That(path.Battles[0].Id, Is.EqualTo("first"));
            Assert.That(path.Battles[1].Id, Is.EqualTo("second"));
        }

        [Test]
        public void LearningPath_MissingOrEmptyBattles_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new LearningPath(null));
            Assert.Throws<ArgumentException>(() => new LearningPath(Array.Empty<BattleDefinition>()));
        }

        [Test]
        public void LearningPath_DuplicateIdOrOrder_Throws()
        {
            Assert.Throws<ArgumentException>(() => new LearningPath(new[]
            {
                CreateBattle("same", 1, ValidationCriterion.DeclareMagoClass),
                CreateBattle("same", 2, ValidationCriterion.AddPrivateAttributes)
            }));
            Assert.Throws<ArgumentException>(() => new LearningPath(new[]
            {
                CreateBattle("first", 1, ValidationCriterion.DeclareMagoClass),
                CreateBattle("second", 1, ValidationCriterion.AddPrivateAttributes)
            }));
        }

        [Test]
        public void Constructor_ValidPath_StartsEditingFirstBattle()
        {
            LearningPath path = CreateTwoBattlePath();

            var progress = new LearningProgress(path);

            Assert.That(progress.Path, Is.SameAs(path));
            Assert.That(progress.CurrentBattleIndex, Is.Zero);
            Assert.That(progress.CurrentBattle.Id, Is.EqualTo("first"));
            Assert.That(progress.Stage, Is.EqualTo(LearningStage.Editing));
            Assert.That(progress.FailedAttempts, Is.Zero);
            Assert.That(progress.CurrentHint, Is.Null);
        }

        [Test]
        public void Constructor_MissingPath_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new LearningProgress(null));
        }

        [Test]
        public void TryStartBattle_MatchingCriterion_StartsBattle()
        {
            var progress = new LearningProgress(CreateTwoBattlePath());

            bool started = progress.TryStartBattle(ValidationCriterion.DeclareMagoClass);

            Assert.That(started, Is.True);
            Assert.That(progress.Stage, Is.EqualTo(LearningStage.BattleInProgress));
        }

        [Test]
        public void TryStartBattle_MismatchedCriterion_StaysInEditing()
        {
            var progress = new LearningProgress(CreateTwoBattlePath());

            bool started = progress.TryStartBattle(ValidationCriterion.AddPrivateAttributes);

            Assert.That(started, Is.False);
            Assert.That(progress.Stage, Is.EqualTo(LearningStage.Editing));
        }

        [Test]
        public void ReportVictory_ActiveBattle_OpensVictoryReview()
        {
            var progress = new LearningProgress(CreateTwoBattlePath());
            progress.TryStartBattle(ValidationCriterion.DeclareMagoClass);

            bool accepted = progress.ReportVictory();

            Assert.That(accepted, Is.True);
            Assert.That(progress.Stage, Is.EqualTo(LearningStage.VictoryReview));
        }

        [Test]
        public void ReportVictory_WhileEditing_IsRejected()
        {
            var progress = new LearningProgress(CreateTwoBattlePath());

            bool accepted = progress.ReportVictory();

            Assert.That(accepted, Is.False);
            Assert.That(progress.Stage, Is.EqualTo(LearningStage.Editing));
        }

        [Test]
        public void ContinueAfterVictory_ReviewingBattle_LoadsNextBattle()
        {
            var progress = new LearningProgress(CreateTwoBattlePath());
            progress.RegisterFailedAttempt();
            progress.TryStartBattle(ValidationCriterion.DeclareMagoClass);
            progress.ReportVictory();

            bool hasNextBattle = progress.ContinueAfterVictory();

            Assert.That(hasNextBattle, Is.True);
            Assert.That(progress.CurrentBattleIndex, Is.EqualTo(1));
            Assert.That(progress.CurrentBattle.Id, Is.EqualTo("second"));
            Assert.That(progress.Stage, Is.EqualTo(LearningStage.Editing));
            Assert.That(progress.FailedAttempts, Is.Zero);
            Assert.That(progress.CurrentHint, Is.Null);
        }

        [Test]
        public void ContinueAfterVictory_FinalBattle_CompletesJourney()
        {
            var progress = new LearningProgress(new LearningPath(new[]
            {
                CreateBattle("only", 1, ValidationCriterion.DeclareMagoClass)
            }));
            progress.TryStartBattle(ValidationCriterion.DeclareMagoClass);
            progress.ReportVictory();

            bool hasNextBattle = progress.ContinueAfterVictory();

            Assert.That(hasNextBattle, Is.False);
            Assert.That(progress.Stage, Is.EqualTo(LearningStage.JourneyCompleted));
            Assert.That(progress.CurrentBattle.Id, Is.EqualTo("only"));
        }

        [Test]
        public void ContinueAfterVictory_WhileEditing_IsRejectedWithoutAdvancing()
        {
            var progress = new LearningProgress(CreateTwoBattlePath());

            bool hasNextBattle = progress.ContinueAfterVictory();

            Assert.That(hasNextBattle, Is.False);
            Assert.That(progress.CurrentBattleIndex, Is.Zero);
            Assert.That(progress.Stage, Is.EqualTo(LearningStage.Editing));
        }

        [Test]
        public void RegisterFailedAttempt_RevealsAndClampsProgressiveHints()
        {
            var progress = new LearningProgress(CreateTwoBattlePath());

            string first = progress.RegisterFailedAttempt();
            string second = progress.RegisterFailedAttempt();
            progress.RegisterFailedAttempt();
            string fourth = progress.RegisterFailedAttempt();
            string fifth = progress.RegisterFailedAttempt();

            Assert.That(first, Is.EqualTo("Dica 1"));
            Assert.That(second, Is.EqualTo("Dica 2"));
            Assert.That(fourth, Is.EqualTo("Dica 4"));
            Assert.That(fifth, Is.EqualTo("Dica 4"));
            Assert.That(progress.FailedAttempts, Is.EqualTo(5));
            Assert.That(progress.CurrentHint, Is.EqualTo("Dica 4"));
        }

        [Test]
        public void ReportDefeat_ActiveBattle_ReturnsToEditingAndPreservesHints()
        {
            var progress = new LearningProgress(CreateTwoBattlePath());
            progress.RegisterFailedAttempt();
            progress.TryStartBattle(ValidationCriterion.DeclareMagoClass);

            bool restarted = progress.ReportDefeat();

            Assert.That(restarted, Is.True);
            Assert.That(progress.Stage, Is.EqualTo(LearningStage.Editing));
            Assert.That(progress.CurrentBattleIndex, Is.Zero);
            Assert.That(progress.FailedAttempts, Is.EqualTo(1));
            Assert.That(progress.CurrentHint, Is.EqualTo("Dica 1"));
        }

        [Test]
        public void RestartCurrentBattle_DuringBattle_ReturnsToEditingAndPreservesHints()
        {
            var progress = new LearningProgress(CreateTwoBattlePath());
            progress.RegisterFailedAttempt();
            progress.TryStartBattle(ValidationCriterion.DeclareMagoClass);

            bool restarted = progress.RestartCurrentBattle();

            Assert.That(restarted, Is.True);
            Assert.That(progress.Stage, Is.EqualTo(LearningStage.Editing));
            Assert.That(progress.FailedAttempts, Is.EqualTo(1));
            Assert.That(progress.CurrentHint, Is.EqualTo("Dica 1"));
        }

        [Test]
        public void RestartCurrentBattle_DuringVictoryReview_IsRejected()
        {
            var progress = new LearningProgress(CreateTwoBattlePath());
            progress.TryStartBattle(ValidationCriterion.DeclareMagoClass);
            progress.ReportVictory();

            bool restarted = progress.RestartCurrentBattle();

            Assert.That(restarted, Is.False);
            Assert.That(progress.Stage, Is.EqualTo(LearningStage.VictoryReview));
        }

        private static LearningPath CreateTwoBattlePath()
        {
            return new LearningPath(new[]
            {
                CreateBattle("first", 1, ValidationCriterion.DeclareMagoClass),
                CreateBattle("second", 2, ValidationCriterion.AddPrivateAttributes)
            });
        }

        private static BattleDefinition CreateBattle(
            string id,
            int order,
            ValidationCriterion criterion)
        {
            return new BattleDefinition(
                id,
                1,
                order,
                new BattleLessonContent(
                    $"Batalha {order}",
                    "Conceito",
                    "O que é.",
                    "Para que serve.",
                    "Como usar.",
                    "Efeito no jogo.",
                    "Tarefa."),
                new[] { "Dica 1", "Dica 2", "Dica 3", "Dica 4" },
                criterion,
                new BattleVictoryContent(
                    "Vitória!",
                    "Conquista.",
                    "Revisão."));
        }
    }
}
