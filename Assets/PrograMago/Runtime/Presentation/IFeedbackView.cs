using PrograMago.Language;

namespace PrograMago.Presentation
{
    public interface IFeedbackView
    {
        void ShowSuccess(string message);

        void ShowError(Diagnostic diagnostic);

        void Clear();
    }
}
