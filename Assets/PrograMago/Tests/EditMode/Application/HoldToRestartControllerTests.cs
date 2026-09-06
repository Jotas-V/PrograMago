using System;
using NUnit.Framework;
using PrograMago.Application;

namespace PrograMago.Tests.Application
{
    public sealed class HoldToRestartControllerTests
    {
        [Test]
        public void ApplicationAssembly_ExposesHoldToRestartController()
        {
            System.Type type = typeof(SubmitCodeUseCase).Assembly.GetType(
                "PrograMago.Application.HoldToRestartController");

            Assert.That(type, Is.Not.Null);
        }

        [Test]
        public void HoldToRestartController_ExposesTimingContract()
        {
            System.Type type = typeof(HoldToRestartController);

            Assert.That(type.GetConstructor(new[] { typeof(float) }), Is.Not.Null);
            Assert.That(type.GetProperty("HoldDurationSeconds")?.PropertyType, Is.EqualTo(typeof(float)));
            Assert.That(type.GetProperty("ElapsedSeconds")?.PropertyType, Is.EqualTo(typeof(float)));
            Assert.That(type.GetProperty("Progress")?.PropertyType, Is.EqualTo(typeof(float)));
            Assert.That(type.GetProperty("IsWaitingForRelease")?.PropertyType, Is.EqualTo(typeof(bool)));
            Assert.That(
                type.GetMethod("Update", new[] { typeof(bool), typeof(float) })?.ReturnType,
                Is.EqualTo(typeof(bool)));
        }

        [Test]
        public void Constructor_NonPositiveDuration_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HoldToRestartController(0f));
        }

        [Test]
        public void Update_HeldForLessThanDuration_ReportsProgressWithoutTriggering()
        {
            var controller = new HoldToRestartController(5f);

            bool triggered = controller.Update(true, 4f);

            Assert.That(triggered, Is.False);
            Assert.That(controller.HoldDurationSeconds, Is.EqualTo(5f));
            Assert.That(controller.ElapsedSeconds, Is.EqualTo(4f));
            Assert.That(controller.Progress, Is.EqualTo(0.8f).Within(0.001f));
            Assert.That(controller.IsWaitingForRelease, Is.False);
        }

        [Test]
        public void Update_ReleasedBeforeDuration_CancelsProgress()
        {
            var controller = new HoldToRestartController(5f);
            controller.Update(true, 3f);

            bool triggered = controller.Update(false, 0.1f);

            Assert.That(triggered, Is.False);
            Assert.That(controller.ElapsedSeconds, Is.Zero);
            Assert.That(controller.Progress, Is.Zero);
        }

        [Test]
        public void Update_ReachesDuration_TriggersOnlyOnceUntilRelease()
        {
            var controller = new HoldToRestartController(5f);
            controller.Update(true, 2f);

            bool firstTrigger = controller.Update(true, 3f);
            bool repeatedTrigger = controller.Update(true, 1f);

            Assert.That(firstTrigger, Is.True);
            Assert.That(repeatedTrigger, Is.False);
            Assert.That(controller.ElapsedSeconds, Is.EqualTo(5f));
            Assert.That(controller.Progress, Is.EqualTo(1f));
            Assert.That(controller.IsWaitingForRelease, Is.True);
        }

        [Test]
        public void Update_ReleasedAfterTrigger_AllowsAnotherHold()
        {
            var controller = new HoldToRestartController(5f);
            controller.Update(true, 5f);

            controller.Update(false, 0.1f);
            bool triggeredAgain = controller.Update(true, 5f);

            Assert.That(triggeredAgain, Is.True);
        }
    }
}
