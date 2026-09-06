using PrograMago.Application;
using PrograMago.Domain;
using NUnit.Framework;
using PrograMago.Language;
using PrograMago.Presentation;

namespace PrograMago.Tests.Presentation
{
    public sealed class LearningFlowPresenterTests
    {
        [Test]
        public void PresentationAssembly_ExposesLearningFlowPresenter()
        {
            System.Type type = typeof(GameplayPresenter).Assembly.GetType(
                "PrograMago.Presentation.LearningFlowPresenter");

            Assert.That(type, Is.Not.Null);
        }

        [Test]
        public void PresentationAssembly_ExposesLearningFlowView()
        {
            System.Type type = typeof(GameplayPresenter).Assembly.GetType(
                "PrograMago.Presentation.ILearningFlowView");

            Assert.That(type, Is.Not.Null);
        }

        [Test]
        public void LearningFlowView_ExposesPedagogicalUiContract()
        {
            System.Type type = typeof(ILearningFlowView);

            Assert.That(type.GetMethod("ShowBattle")?.ReturnType, Is.EqualTo(typeof(void)));
            Assert.That(type.GetMethod("ShowHint")?.ReturnType, Is.EqualTo(typeof(void)));
            Assert.That(type.GetMethod("ShowVictory")?.ReturnType, Is.EqualTo(typeof(void)));
            Assert.That(type.GetMethod("HideVictory")?.ReturnType, Is.EqualTo(typeof(void)));
            Assert.That(type.GetMethod("SetInteractionEnabled")?.ReturnType, Is.EqualTo(typeof(void)));
            Assert.That(type.GetMethod("ShowRestartProgress")?.ReturnType, Is.EqualTo(typeof(void)));
            Assert.That(type.GetMethod("HideRestartProgress")?.ReturnType, Is.EqualTo(typeof(void)));
        }

        [Test]
        public void LearningFlowPresenter_ExposesProgressionContract()
        {
            System.Type type = typeof(LearningFlowPresenter);

            Assert.That(type.GetConstructor(new[]
            {
                typeof(LearningProgress),
                typeof(HoldToRestartController),
                typeof(ICodeEditorView),
                typeof(IFeedbackView),
                typeof(IArenaView),
                typeof(ILearningFlowView)
            }), Is.Not.Null);
            Assert.That(type.GetProperty("Progress")?.PropertyType, Is.EqualTo(typeof(LearningProgress)));
            Assert.That(type.GetMethod("Initialize")?.ReturnType, Is.EqualTo(typeof(void)));
            Assert.That(type.GetMethod("HandleSubmission")?.ReturnType, Is.EqualTo(typeof(void)));
            Assert.That(type.GetMethod("ReportBattleVictory")?.ReturnType, Is.EqualTo(typeof(bool)));
            Assert.That(type.GetMethod("ReportBattleDefeat")?.ReturnType, Is.EqualTo(typeof(bool)));
            Assert.That(type.GetMethod("NextBattle")?.ReturnType, Is.EqualTo(typeof(bool)));
            Assert.That(type.GetMethod("RestartCurrentBattle")?.ReturnType, Is.EqualTo(typeof(bool)));
            Assert.That(type.GetMethod("UpdateRestartHold")?.ReturnType, Is.EqualTo(typeof(bool)));
        }

        [Test]
        public void Initialize_FirstBattle_RendersContentAndEditingState()
        {
            var editor = new FakeCodeEditorView();
            var feedback = new FakeFeedbackView();
            var arena = new FakeArenaView();
            var view = new FakeLearningFlowView();
            LearningFlowPresenter presenter = CreatePresenter(editor, feedback, arena, view);

            presenter.Initialize();

            Assert.That(view.Battle.Id, Is.EqualTo("first"));
            Assert.That(view.BattleNumber, Is.EqualTo(1));
            Assert.That(view.BattleCount, Is.EqualTo(2));
            Assert.That(view.VictoryHidden, Is.True);
            Assert.That(view.RestartProgressHidden, Is.True);
            Assert.That(view.InteractionEnabled, Is.True);
        }

        [Test]
        public void HandleSubmission_InvalidCode_RevealsNextHint()
        {
            var view = new FakeLearningFlowView();
            LearningFlowPresenter presenter = CreatePresenter(
                new FakeCodeEditorView(), new FakeFeedbackView(), new FakeArenaView(), view);
            presenter.Initialize();
            var diagnostic = new Diagnostic("CLASS001", new SourcePosition(0, 1, 1), "Nome inválido.");

            presenter.HandleSubmission(SubmitCodeResult.Failure(false, diagnostic));

            Assert.That(presenter.Progress.FailedAttempts, Is.EqualTo(1));
            Assert.That(view.Hint, Is.EqualTo("Dica 1"));
            Assert.That(presenter.Progress.Stage, Is.EqualTo(LearningStage.Editing));
        }

        [Test]
        public void HandleSubmission_MatchingCriterion_StartsBattleWithoutVictory()
        {
            var view = new FakeLearningFlowView();
            LearningFlowPresenter presenter = CreatePresenter(
                new FakeCodeEditorView(), new FakeFeedbackView(), new FakeArenaView(), view);
            presenter.Initialize();

            presenter.HandleSubmission(SubmitCodeResult.Success(
                true,
                ValidationCriterion.DeclareMagoClass));

            Assert.That(presenter.Progress.Stage, Is.EqualTo(LearningStage.BattleInProgress));
            Assert.That(view.InteractionEnabled, Is.False);
            Assert.That(view.Victory, Is.Null);
        }

        [Test]
        public void HandleSubmission_MismatchedCriterion_ShowsFlowDiagnosticAndStaysEditing()
        {
            var feedback = new FakeFeedbackView();
            var view = new FakeLearningFlowView();
            LearningFlowPresenter presenter = CreatePresenter(
                new FakeCodeEditorView(), feedback, new FakeArenaView(), view);
            presenter.Initialize();

            presenter.HandleSubmission(SubmitCodeResult.Success(
                true,
                ValidationCriterion.AddPrivateAttributes));

            Assert.That(presenter.Progress.Stage, Is.EqualTo(LearningStage.Editing));
            Assert.That(feedback.Diagnostic.Code, Is.EqualTo("FLOW001"));
            Assert.That(view.Hint, Is.EqualTo("Dica 1"));
        }

        [Test]
        public void ReportBattleVictory_ActiveBattle_ShowsReusableReview()
        {
            var view = new FakeLearningFlowView();
            LearningFlowPresenter presenter = CreatePresenter(
                new FakeCodeEditorView(), new FakeFeedbackView(), new FakeArenaView(), view);
            presenter.Initialize();
            presenter.HandleSubmission(SubmitCodeResult.Success(
                true,
                ValidationCriterion.DeclareMagoClass));

            bool accepted = presenter.ReportBattleVictory();

            Assert.That(accepted, Is.True);
            Assert.That(view.Victory.Title, Is.EqualTo("Vitória!"));
            Assert.That(view.IsFinalBattle, Is.False);
            Assert.That(presenter.Progress.Stage, Is.EqualTo(LearningStage.VictoryReview));
        }

        [Test]
        public void NextBattle_AfterVictory_ClearsEditorAndLoadsNextContent()
        {
            var editor = new FakeCodeEditorView { SourceCode = "public class Mago {}" };
            var feedback = new FakeFeedbackView();
            var arena = new FakeArenaView { HasDeclaredClass = true };
            var view = new FakeLearningFlowView();
            LearningFlowPresenter presenter = CreatePresenter(editor, feedback, arena, view);
            presenter.Initialize();
            presenter.HandleSubmission(SubmitCodeResult.Success(
                true,
                ValidationCriterion.DeclareMagoClass));
            presenter.ReportBattleVictory();

            bool advanced = presenter.NextBattle();

            Assert.That(advanced, Is.True);
            Assert.That(editor.SourceCode, Is.Empty);
            Assert.That(arena.HasDeclaredClass, Is.False);
            Assert.That(feedback.WasCleared, Is.True);
            Assert.That(view.VictoryHidden, Is.True);
            Assert.That(view.Battle.Id, Is.EqualTo("second"));
            Assert.That(view.BattleNumber, Is.EqualTo(2));
            Assert.That(view.InteractionEnabled, Is.True);
        }

        [Test]
        public void ReportBattleVictory_FinalBattle_MarksReviewAsFinal()
        {
            var view = new FakeLearningFlowView();
            LearningFlowPresenter presenter = CreatePresenterForPath(
                new LearningPath(new[]
                {
                    CreateBattle("only", 1, ValidationCriterion.DeclareMagoClass)
                }),
                new FakeCodeEditorView(),
                new FakeFeedbackView(),
                new FakeArenaView(),
                view);
            presenter.Initialize();
            presenter.HandleSubmission(SubmitCodeResult.Success(
                true,
                ValidationCriterion.DeclareMagoClass));

            presenter.ReportBattleVictory();

            Assert.That(view.IsFinalBattle, Is.True);
        }

        [Test]
        public void ReportBattleDefeat_ActiveBattle_PreservesCodeAndRestartsViews()
        {
            var editor = new FakeCodeEditorView { SourceCode = "public class Mago {}" };
            var feedback = new FakeFeedbackView();
            var arena = new FakeArenaView { HasDeclaredClass = true };
            var view = new FakeLearningFlowView();
            LearningFlowPresenter presenter = CreatePresenter(editor, feedback, arena, view);
            presenter.Initialize();
            presenter.HandleSubmission(SubmitCodeResult.Success(
                true,
                ValidationCriterion.DeclareMagoClass));

            bool restarted = presenter.ReportBattleDefeat();

            Assert.That(restarted, Is.True);
            Assert.That(editor.SourceCode, Is.EqualTo("public class Mago {}"));
            Assert.That(arena.HasDeclaredClass, Is.False);
            Assert.That(feedback.WasCleared, Is.True);
            Assert.That(view.InteractionEnabled, Is.True);
            Assert.That(presenter.Progress.Stage, Is.EqualTo(LearningStage.Editing));
        }

        [Test]
        public void UpdateRestartHold_ContinuousFiveSeconds_RestartsAndPreservesCode()
        {
            var editor = new FakeCodeEditorView { SourceCode = "public class Mago {}" };
            var arena = new FakeArenaView { HasDeclaredClass = true };
            var view = new FakeLearningFlowView();
            LearningFlowPresenter presenter = CreatePresenter(
                editor, new FakeFeedbackView(), arena, view);
            presenter.Initialize();
            presenter.HandleSubmission(SubmitCodeResult.Success(
                true,
                ValidationCriterion.DeclareMagoClass));

            bool restarted = presenter.UpdateRestartHold(true, 5f);

            Assert.That(restarted, Is.True);
            Assert.That(editor.SourceCode, Is.EqualTo("public class Mago {}"));
            Assert.That(arena.HasDeclaredClass, Is.False);
            Assert.That(view.RestartProgress, Is.EqualTo(1f));
            Assert.That(presenter.Progress.Stage, Is.EqualTo(LearningStage.Editing));
        }

        [Test]
        public void UpdateRestartHold_ReleasedBeforeFiveSeconds_HidesProgress()
        {
            var view = new FakeLearningFlowView();
            LearningFlowPresenter presenter = CreatePresenter(
                new FakeCodeEditorView(), new FakeFeedbackView(), new FakeArenaView(), view);
            presenter.Initialize();

            presenter.UpdateRestartHold(true, 2.5f);
            presenter.UpdateRestartHold(false, 0.1f);

            Assert.That(view.RestartProgressHidden, Is.True);
        }

        private static LearningFlowPresenter CreatePresenter(
            ICodeEditorView editor,
            IFeedbackView feedback,
            IArenaView arena,
            ILearningFlowView view)
        {
            return CreatePresenterForPath(
                new LearningPath(new[]
                {
                    CreateBattle("first", 1, ValidationCriterion.DeclareMagoClass),
                    CreateBattle("second", 2, ValidationCriterion.AddPrivateAttributes)
                }),
                editor,
                feedback,
                arena,
                view);
        }

        private static LearningFlowPresenter CreatePresenterForPath(
            LearningPath path,
            ICodeEditorView editor,
            IFeedbackView feedback,
            IArenaView arena,
            ILearningFlowView view)
        {
            return new LearningFlowPresenter(
                new LearningProgress(path),
                new HoldToRestartController(5f),
                editor,
                feedback,
                arena,
                view);
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
                    $"Batalha {order}", "Conceito", "O que é.", "Para que serve.",
                    "Como usar.", "Efeito.", "Tarefa."),
                new[] { "Dica 1", "Dica 2", "Dica 3", "Dica 4" },
                criterion,
                new BattleVictoryContent("Vitória!", "Conquista.", "Revisão."));
        }

        private sealed class FakeCodeEditorView : ICodeEditorView
        {
            public string SourceCode { get; set; } = string.Empty;
        }

        private sealed class FakeFeedbackView : IFeedbackView
        {
            public Diagnostic Diagnostic { get; private set; }

            public bool WasCleared { get; private set; }

            public void ShowError(Diagnostic diagnostic)
            {
                Diagnostic = diagnostic;
            }

            public void Clear()
            {
                WasCleared = true;
                Diagnostic = null;
            }
        }

        private sealed class FakeArenaView : IArenaView
        {
            public bool HasDeclaredClass { get; set; }

            public void SetClassDeclared(bool hasDeclaredClass)
            {
                HasDeclaredClass = hasDeclaredClass;
            }
        }

        private sealed class FakeLearningFlowView : ILearningFlowView
        {
            public BattleDefinition Battle { get; private set; }

            public int BattleNumber { get; private set; }

            public int BattleCount { get; private set; }

            public string Hint { get; private set; }

            public BattleVictoryContent Victory { get; private set; }

            public bool IsFinalBattle { get; private set; }

            public bool VictoryHidden { get; private set; }

            public bool InteractionEnabled { get; private set; }

            public float RestartProgress { get; private set; }

            public bool RestartProgressHidden { get; private set; }

            public void ShowBattle(BattleDefinition battle, int battleNumber, int battleCount)
            {
                Battle = battle;
                BattleNumber = battleNumber;
                BattleCount = battleCount;
            }

            public void ShowHint(string hint)
            {
                Hint = hint;
            }

            public void ShowVictory(BattleVictoryContent content, bool isFinalBattle)
            {
                Victory = content;
                IsFinalBattle = isFinalBattle;
                VictoryHidden = false;
            }

            public void HideVictory()
            {
                VictoryHidden = true;
                Victory = null;
            }

            public void SetInteractionEnabled(bool isEnabled)
            {
                InteractionEnabled = isEnabled;
            }

            public void ShowRestartProgress(float progress)
            {
                RestartProgress = progress;
                RestartProgressHidden = false;
            }

            public void HideRestartProgress()
            {
                RestartProgressHidden = true;
                RestartProgress = 0f;
            }
        }
    }
}
