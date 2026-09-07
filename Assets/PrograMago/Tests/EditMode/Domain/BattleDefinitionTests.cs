using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PrograMago.Domain;

namespace PrograMago.Tests.Domain
{
    public sealed class BattleDefinitionTests
    {
        [Test]
        public void DomainAssembly_ExposesBattleDefinition()
        {
            System.Type battleDefinition = typeof(ExerciseDefinition).Assembly.GetType(
                "PrograMago.Domain.BattleDefinition");

            Assert.That(battleDefinition, Is.Not.Null);
        }

        [Test]
        public void DomainAssembly_ExposesBattleVictoryContent()
        {
            System.Type victoryContent = typeof(ExerciseDefinition).Assembly.GetType(
                "PrograMago.Domain.BattleVictoryContent");

            Assert.That(victoryContent, Is.Not.Null);
        }

        [Test]
        public void DomainAssembly_ExposesValidationCriterion()
        {
            System.Type validationCriterion = typeof(ExerciseDefinition).Assembly.GetType(
                "PrograMago.Domain.ValidationCriterion");

            Assert.That(validationCriterion, Is.Not.Null);
        }

        [Test]
        public void BattleCompletionMode_DefinesCodeAndCombatResolution()
        {
            Assert.That(Enum.GetNames(typeof(BattleCompletionMode)), Is.EquivalentTo(new[]
            {
                "OnCodeValidated",
                "OnCombatVictory"
            }));
        }

        [Test]
        public void DomainAssembly_ExposesBattleLessonContent()
        {
            System.Type lessonContent = typeof(ExerciseDefinition).Assembly.GetType(
                "PrograMago.Domain.BattleLessonContent");

            Assert.That(lessonContent, Is.Not.Null);
        }

        [Test]
        public void BattleLessonContent_ExposesPedagogicalTextContract()
        {
            System.Type type = typeof(BattleLessonContent);

            Assert.That(type.GetConstructor(new[]
            {
                typeof(string), typeof(string), typeof(string), typeof(string),
                typeof(string), typeof(string), typeof(string)
            }), Is.Not.Null);
            Assert.That(
                new[] { "Title", "Concept", "WhatItIs", "Purpose", "UsageExample", "GameEffect", "Task" }
                    .All(property => type.GetProperty(property)?.PropertyType == typeof(string)),
                Is.True);
        }

        [Test]
        public void BattleVictoryContent_ExposesReviewTextContract()
        {
            System.Type type = typeof(BattleVictoryContent);

            Assert.That(type.GetConstructor(new[]
            {
                typeof(string), typeof(string), typeof(string)
            }), Is.Not.Null);
            Assert.That(
                new[] { "Title", "Achievement", "Review" }
                    .All(property => type.GetProperty(property)?.PropertyType == typeof(string)),
                Is.True);
        }

        [Test]
        public void BattleDefinition_ExposesOrderedBattleContract()
        {
            System.Type type = typeof(BattleDefinition);

            Assert.That(type.GetConstructor(new[]
            {
                typeof(string),
                typeof(int),
                typeof(int),
                typeof(BattleLessonContent),
                typeof(IEnumerable<string>),
                typeof(ValidationCriterion),
                typeof(BattleVictoryContent)
            }), Is.Not.Null);
            Assert.That(type.GetProperty("Id")?.PropertyType, Is.EqualTo(typeof(string)));
            Assert.That(type.GetProperty("Chapter")?.PropertyType, Is.EqualTo(typeof(int)));
            Assert.That(type.GetProperty("Order")?.PropertyType, Is.EqualTo(typeof(int)));
            Assert.That(type.GetProperty("Lesson")?.PropertyType, Is.EqualTo(typeof(BattleLessonContent)));
            Assert.That(type.GetProperty("Hints")?.PropertyType, Is.EqualTo(typeof(IReadOnlyList<string>)));
            Assert.That(type.GetProperty("Criterion")?.PropertyType, Is.EqualTo(typeof(ValidationCriterion)));
            Assert.That(type.GetProperty("Victory")?.PropertyType, Is.EqualTo(typeof(BattleVictoryContent)));
            Assert.That(
                type.GetProperty("CompletionMode")?.PropertyType,
                Is.EqualTo(typeof(BattleCompletionMode)));
        }

        [Test]
        public void ValidationCriterion_DefinesTheEightApprovedBattles()
        {
            Assert.That(System.Enum.GetNames(typeof(ValidationCriterion)), Is.EquivalentTo(new[]
            {
                "DeclareMagoClass",
                "AddPrivateAttributes",
                "ConstructAndInstantiateMago",
                "ConstructAndInstantiateEnemy",
                "DefineAndCallSpellMethod",
                "ExtendMago",
                "OverrideSpellWithSuper",
                "UsePolymorphicMagoReference"
            }));
        }

        [Test]
        public void BattleLessonContent_ValidTexts_PreservesPedagogicalContent()
        {
            var lesson = new BattleLessonContent(
                "O nascimento do Mago",
                "Classes",
                "Uma classe é um molde para objetos.",
                "Ela reúne estado e comportamento.",
                "public class Mago {}",
                "A classe permite materializar o mago na arena.",
                "Declare a classe pública Mago.");

            Assert.That(lesson.Title, Is.EqualTo("O nascimento do Mago"));
            Assert.That(lesson.Concept, Is.EqualTo("Classes"));
            Assert.That(lesson.WhatItIs, Is.EqualTo("Uma classe é um molde para objetos."));
            Assert.That(lesson.Purpose, Is.EqualTo("Ela reúne estado e comportamento."));
            Assert.That(lesson.UsageExample, Is.EqualTo("public class Mago {}"));
            Assert.That(lesson.GameEffect, Is.EqualTo("A classe permite materializar o mago na arena."));
            Assert.That(lesson.Task, Is.EqualTo("Declare a classe pública Mago."));
        }

        [Test]
        public void BattleVictoryContent_ValidTexts_PreservesReview()
        {
            var victory = new BattleVictoryContent(
                "Primeira batalha concluída!",
                "Você criou seu primeiro mago.",
                "Classes funcionam como moldes para criar objetos.");

            Assert.That(victory.Title, Is.EqualTo("Primeira batalha concluída!"));
            Assert.That(victory.Achievement, Is.EqualTo("Você criou seu primeiro mago."));
            Assert.That(victory.Review, Is.EqualTo("Classes funcionam como moldes para criar objetos."));
        }

        [Test]
        public void BattleDefinition_ValidData_PreservesOrderedDefinition()
        {
            BattleLessonContent lesson = CreateLesson();
            var hints = new List<string> { "Comece com public.", "Depois escreva class." };
            BattleVictoryContent victory = CreateVictory();

            var battle = new BattleDefinition(
                "mago-class",
                1,
                1,
                lesson,
                hints,
                ValidationCriterion.DeclareMagoClass,
                victory,
                BattleCompletionMode.OnCodeValidated);

            hints[0] = "alterada externamente";

            Assert.That(battle.Id, Is.EqualTo("mago-class"));
            Assert.That(battle.Chapter, Is.EqualTo(1));
            Assert.That(battle.Order, Is.EqualTo(1));
            Assert.That(battle.Lesson, Is.SameAs(lesson));
            Assert.That(battle.Hints, Is.EqualTo(new[] { "Comece com public.", "Depois escreva class." }));
            Assert.That(battle.Criterion, Is.EqualTo(ValidationCriterion.DeclareMagoClass));
            Assert.That(battle.Victory, Is.SameAs(victory));
            Assert.That(battle.CompletionMode, Is.EqualTo(BattleCompletionMode.OnCodeValidated));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void BattleLessonContent_BlankRequiredText_Throws(int blankIndex)
        {
            string[] texts =
            {
                "Título", "Conceito", "O que é", "Para que serve",
                "Como usar", "Efeito no jogo", "Tarefa"
            };
            texts[blankIndex] = " ";

            Assert.Throws<ArgumentException>(() => new BattleLessonContent(
                texts[0], texts[1], texts[2], texts[3], texts[4], texts[5], texts[6]));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void BattleVictoryContent_BlankRequiredText_Throws(int blankIndex)
        {
            string[] texts = { "Vitória", "Conquista", "Revisão" };
            texts[blankIndex] = string.Empty;

            Assert.Throws<ArgumentException>(() => new BattleVictoryContent(
                texts[0], texts[1], texts[2]));
        }

        [TestCase(null, 1, 1)]
        [TestCase(" ", 1, 1)]
        [TestCase("battle", 0, 1)]
        [TestCase("battle", 1, 0)]
        public void BattleDefinition_InvalidIdentityOrPosition_Throws(
            string id,
            int chapter,
            int order)
        {
            Assert.That(
                () => new BattleDefinition(
                    id,
                    chapter,
                    order,
                    CreateLesson(),
                    new[] { "Dica" },
                    ValidationCriterion.DeclareMagoClass,
                    CreateVictory()),
                Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        public void BattleDefinition_EmptyOrBlankHints_Throws()
        {
            Assert.Throws<ArgumentException>(() => new BattleDefinition(
                "battle", 1, 1, CreateLesson(), Array.Empty<string>(),
                ValidationCriterion.DeclareMagoClass, CreateVictory()));
            Assert.Throws<ArgumentException>(() => new BattleDefinition(
                "battle", 1, 1, CreateLesson(), new[] { "Dica", " " },
                ValidationCriterion.DeclareMagoClass, CreateVictory()));
        }

        [Test]
        public void BattleDefinition_MissingLessonHintsOrVictory_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new BattleDefinition(
                "battle", 1, 1, null, new[] { "Dica" },
                ValidationCriterion.DeclareMagoClass, CreateVictory()));
            Assert.Throws<ArgumentNullException>(() => new BattleDefinition(
                "battle", 1, 1, CreateLesson(), null,
                ValidationCriterion.DeclareMagoClass, CreateVictory()));
            Assert.Throws<ArgumentNullException>(() => new BattleDefinition(
                "battle", 1, 1, CreateLesson(), new[] { "Dica" },
                ValidationCriterion.DeclareMagoClass, null));
        }

        [Test]
        public void BattleDefinition_UndefinedCriterion_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BattleDefinition(
                "battle", 1, 1, CreateLesson(), new[] { "Dica" },
                (ValidationCriterion)999, CreateVictory()));
        }

        [Test]
        public void BattleDefinition_UndefinedCompletionMode_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BattleDefinition(
                "battle", 1, 1, CreateLesson(), new[] { "Dica" },
                ValidationCriterion.DeclareMagoClass, CreateVictory(),
                (BattleCompletionMode)999));
        }

        private static BattleLessonContent CreateLesson()
        {
            return new BattleLessonContent(
                "O nascimento do Mago",
                "Classes",
                "Uma classe é um molde para objetos.",
                "Ela reúne estado e comportamento.",
                "public class Mago {}",
                "A classe permite materializar o mago na arena.",
                "Declare a classe pública Mago.");
        }

        private static BattleVictoryContent CreateVictory()
        {
            return new BattleVictoryContent(
                "Primeira batalha concluída!",
                "Você criou seu primeiro mago.",
                "Classes funcionam como moldes para criar objetos.");
        }
    }
}
