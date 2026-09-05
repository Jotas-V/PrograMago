using System;
using PrograMago.Application;

namespace PrograMago.Presentation
{
    public sealed class GameplayPresenter
    {
        private readonly SubmitCodeUseCase submitCode;
        private readonly ICodeEditorView editor;
        private readonly IFeedbackView feedback;
        private readonly IArenaView arena;

        public GameplayPresenter(
            SubmitCodeUseCase submitCode,
            ICodeEditorView editor,
            IFeedbackView feedback,
            IArenaView arena)
        {
            this.submitCode = submitCode ?? throw new ArgumentNullException(nameof(submitCode));
            this.editor = editor ?? throw new ArgumentNullException(nameof(editor));
            this.feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
            this.arena = arena ?? throw new ArgumentNullException(nameof(arena));
        }

        public void Battle()
        {
            SubmitCodeResult result = submitCode.Execute(editor.SourceCode);
            arena.SetClassDeclared(result.IsSuccess);

            if (result.IsSuccess)
            {
                feedback.ShowSuccess("Classe Mago declarada com sucesso!");
                return;
            }

            feedback.ShowError(result.Diagnostic);
        }

        public void Preview()
        {
            feedback.Clear();
            arena.SetClassDeclared(submitCode.CanPreview(editor.SourceCode));
        }
    }
}
