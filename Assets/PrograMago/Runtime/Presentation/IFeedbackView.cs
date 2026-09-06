using PrograMago.Language;

namespace PrograMago.Presentation
{
    public interface IFeedbackView
    {
        void ShowError(Diagnostic diagnostic);

        void Clear();
    }
}
