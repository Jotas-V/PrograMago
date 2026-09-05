using NUnit.Framework;
using PrograMago.Application;
using PrograMago.Domain;
using PrograMago.Language;
using PrograMago.Presentation;

namespace PrograMago.Tests.Presentation
{
    public sealed class GameplayPresenterTests
    {
        private const string StarterCode = "public class Mago {\n\n}";

        [Test]
        public void Submit_ValidCode_ShowsSuccessAndRevealsWizard()
        {
            var editor = new FakeCodeEditorView { SourceCode = "public class Mago {}" };
            var feedback = new FakeFeedbackView();
            var arena = new FakeArenaView();
            GameplayPresenter presenter = CreatePresenter(editor, feedback, arena);

            presenter.Submit();

            Assert.That(feedback.SuccessMessage, Is.Not.Empty);
            Assert.That(feedback.Diagnostic, Is.Null);
            Assert.That(arena.HasDeclaredClass, Is.True);
        }

        [Test]
        public void Restart_AfterSuccess_RestoresInitialPresentationState()
        {
            var editor = new FakeCodeEditorView { SourceCode = "public class Mago {}" };
            var feedback = new FakeFeedbackView();
            var arena = new FakeArenaView();
            GameplayPresenter presenter = CreatePresenter(editor, feedback, arena);
            presenter.Submit();

            presenter.Restart();

            Assert.That(editor.SourceCode, Is.EqualTo(StarterCode));
            Assert.That(feedback.WasCleared, Is.True);
            Assert.That(arena.HasDeclaredClass, Is.False);
        }

        [Test]
        public void Submit_InvalidCode_ShowsDiagnosticAndKeepsWizardHidden()
        {
            var editor = new FakeCodeEditorView { SourceCode = "public class Bruxo {}" };
            var feedback = new FakeFeedbackView();
            var arena = new FakeArenaView();
            GameplayPresenter presenter = CreatePresenter(editor, feedback, arena);

            presenter.Submit();

            Assert.That(feedback.SuccessMessage, Is.Null);
            Assert.That(feedback.Diagnostic.Code, Is.EqualTo("CLASS001"));
            Assert.That(arena.HasDeclaredClass, Is.False);
        }

        [Test]
        public void Submit_InvalidCodeAfterSuccess_PreservesWizardAndShowsDiagnostic()
        {
            var editor = new FakeCodeEditorView { SourceCode = "public class Mago {}" };
            var feedback = new FakeFeedbackView();
            var arena = new FakeArenaView();
            GameplayPresenter presenter = CreatePresenter(editor, feedback, arena);
            presenter.Submit();
            editor.SourceCode = "class public Mago {}";

            presenter.Submit();

            Assert.That(feedback.SuccessMessage, Is.Null);
            Assert.That(feedback.Diagnostic.Code, Is.EqualTo("SYN001"));
            Assert.That(arena.HasDeclaredClass, Is.True);
        }

        private static GameplayPresenter CreatePresenter(
            ICodeEditorView editor,
            IFeedbackView feedback,
            IArenaView arena)
        {
            var session = new LearningSession(new ExerciseDefinition(
                "declare-mago-class",
                "Mago",
                "Declare a classe Mago."));
            var submit = new SubmitCodeUseCase(
                new CodeTokenizer(),
                new ClassDeclarationValidator(),
                session);
            var restart = new RestartSessionUseCase(session);
            return new GameplayPresenter(submit, restart, editor, feedback, arena, StarterCode);
        }

        private sealed class FakeCodeEditorView : ICodeEditorView
        {
            public string SourceCode { get; set; }
        }

        private sealed class FakeFeedbackView : IFeedbackView
        {
            public string SuccessMessage { get; private set; }

            public Diagnostic Diagnostic { get; private set; }

            public bool WasCleared { get; private set; }

            public void ShowSuccess(string message)
            {
                SuccessMessage = message;
                Diagnostic = null;
            }

            public void ShowError(Diagnostic diagnostic)
            {
                Diagnostic = diagnostic;
                SuccessMessage = null;
            }

            public void Clear()
            {
                WasCleared = true;
                SuccessMessage = null;
                Diagnostic = null;
            }
        }

        private sealed class FakeArenaView : IArenaView
        {
            public bool HasDeclaredClass { get; private set; }

            public void SetClassDeclared(bool hasDeclaredClass)
            {
                HasDeclaredClass = hasDeclaredClass;
            }
        }
    }
}
