using System;
using PrograMago.Application;

namespace PrograMago.Presentation
{
    public sealed class GameplayPresenter
    {
        private readonly SubmitCodeUseCase submitCode;
        private readonly RestartSessionUseCase restartSession;
        private readonly ICodeEditorView editor;
        private readonly IFeedbackView feedback;
        private readonly IArenaView arena;
        private readonly string starterCode;

        public GameplayPresenter(
            SubmitCodeUseCase submitCode,
            RestartSessionUseCase restartSession,
            ICodeEditorView editor,
            IFeedbackView feedback,
            IArenaView arena,
            string starterCode)
        {
            this.submitCode = submitCode ?? throw new ArgumentNullException(nameof(submitCode));
            this.restartSession = restartSession ?? throw new ArgumentNullException(nameof(restartSession));
            this.editor = editor ?? throw new ArgumentNullException(nameof(editor));
            this.feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
            this.arena = arena ?? throw new ArgumentNullException(nameof(arena));
            this.starterCode = starterCode ?? throw new ArgumentNullException(nameof(starterCode));
        }

        public void Submit()
        {
            SubmitCodeResult result = submitCode.Execute(editor.SourceCode);
            arena.SetClassDeclared(result.HasDeclaredClass);

            if (result.IsSuccess)
            {
                feedback.ShowSuccess("Classe Mago declarada com sucesso!");
                return;
            }

            feedback.ShowError(result.Diagnostic);
        }

        public void Restart()
        {
            bool hasDeclaredClass = restartSession.Execute();
            editor.SourceCode = starterCode;
            feedback.Clear();
            arena.SetClassDeclared(hasDeclaredClass);
        }
    }
}
