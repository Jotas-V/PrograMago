using NUnit.Framework;
using PrograMago.Application;
using PrograMago.Domain;
using PrograMago.Language;
using PrograMago.Presentation;

namespace PrograMago.Tests.Presentation
{
    public sealed class GameplayPresenterTests
    {
        [Test]
        public void Battle_ValidCode_ClearsFeedbackAndRevealsWizard()
        {
            var editor = new FakeCodeEditorView { SourceCode = "public class Mago {}" };
            var feedback = new FakeFeedbackView();
            var arena = new FakeArenaView();
            GameplayPresenter presenter = CreatePresenter(editor, feedback, arena);

            presenter.Battle();

            Assert.That(feedback.WasCleared, Is.True);
            Assert.That(feedback.Diagnostic, Is.Null);
            Assert.That(arena.HasDeclaredClass, Is.True);
        }

        [Test]
        public void Battle_InvalidCode_ShowsDiagnosticAndKeepsWizardHidden()
        {
            var editor = new FakeCodeEditorView { SourceCode = "public class Bruxo {}" };
            var feedback = new FakeFeedbackView();
            var arena = new FakeArenaView();
            GameplayPresenter presenter = CreatePresenter(editor, feedback, arena);

            presenter.Battle();

            Assert.That(feedback.Diagnostic.Code, Is.EqualTo("CLASS001"));
            Assert.That(arena.HasDeclaredClass, Is.False);
        }

        [Test]
        public void Battle_InvalidCodeAfterSuccess_PreservesSessionButHidesCurrentPreview()
        {
            var editor = new FakeCodeEditorView { SourceCode = "public class Mago {}" };
            var feedback = new FakeFeedbackView();
            var arena = new FakeArenaView();
            GameplayPresenter presenter = CreatePresenter(editor, feedback, arena);
            presenter.Battle();
            editor.SourceCode = "class public Mago {}";

            presenter.Battle();

            Assert.That(feedback.Diagnostic.Code, Is.EqualTo("SYN001"));
            Assert.That(arena.HasDeclaredClass, Is.False);
        }

        [Test]
        public void Preview_ValidCode_RevealsWizardWithoutShowingFeedback()
        {
            var editor = new FakeCodeEditorView { SourceCode = "public class Mago {}" };
            var feedback = new FakeFeedbackView();
            var arena = new FakeArenaView();
            GameplayPresenter presenter = CreatePresenter(editor, feedback, arena);

            presenter.Preview();

            Assert.That(arena.HasDeclaredClass, Is.True);
            Assert.That(feedback.WasCleared, Is.True);
            Assert.That(feedback.Diagnostic, Is.Null);
        }

        [Test]
        public void Preview_InvalidCode_HidesWizardWithoutShowingFeedback()
        {
            var editor = new FakeCodeEditorView { SourceCode = "public class Bruxo {}" };
            var feedback = new FakeFeedbackView();
            var arena = new FakeArenaView();
            GameplayPresenter presenter = CreatePresenter(editor, feedback, arena);

            presenter.Preview();

            Assert.That(arena.HasDeclaredClass, Is.False);
            Assert.That(feedback.WasCleared, Is.True);
            Assert.That(feedback.Diagnostic, Is.Null);
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
            return new GameplayPresenter(submit, editor, feedback, arena);
        }

        private sealed class FakeCodeEditorView : ICodeEditorView
        {
            public string SourceCode { get; set; }
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
            public bool HasDeclaredClass { get; private set; }

            public void SetClassDeclared(bool hasDeclaredClass)
            {
                HasDeclaredClass = hasDeclaredClass;
            }
        }
    }
}
