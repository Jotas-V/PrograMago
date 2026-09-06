using PrograMago.Domain;

namespace PrograMago.Presentation
{
    public interface ILearningFlowView
    {
        void ShowBattle(BattleDefinition battle, int battleNumber, int battleCount);

        void ShowHint(string hint);

        void ShowVictory(BattleVictoryContent content, bool isFinalBattle);

        void HideVictory();

        void SetInteractionEnabled(bool isEnabled);

        void ShowRestartProgress(float progress);

        void HideRestartProgress();
    }
}
