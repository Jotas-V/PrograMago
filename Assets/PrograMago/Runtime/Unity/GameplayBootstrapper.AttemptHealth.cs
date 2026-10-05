using PrograMago.Domain;
using UnityEngine;

namespace PrograMago.UnityIntegration
{
    public sealed partial class GameplayBootstrapper
    {
        private int attemptLife = -1;
        private int attemptMaximumLife;
        private bool IsFinalEncounter => learningFlowPresenter != null &&
            learningFlowPresenter.Progress.CurrentBattle.Criterion == ValidationCriterion.UsePolymorphicMagoReference;

        private void BeginFinalAttempt()
        {
            if (!IsFinalEncounter || attemptLife >= 0 || CurrentMago == null) return;
            attemptMaximumLife = CurrentMago.Vida;
            attemptLife = attemptMaximumLife;
        }

        private void PenalizeInvalidSubmission()
        {
            if (!IsFinalEncounter) return;
            BeginFinalAttempt();
            if (attemptLife < 0) return;
            int damage = Mathf.Max(1, Mathf.CeilToInt(attemptMaximumLife * 0.2f));
            attemptLife = Mathf.Max(0, attemptLife - damage);
            feedbackText.text += $"\nCódigo inválido: perdeu {damage} de vida. Vida: {attemptLife}/{attemptMaximumLife}.";
            RenderWizard();
            if (attemptLife == 0)
            {
                combatDefeatOverlay.SetActive(true);
                SetInteractionEnabled(false);
            }
            SaveProgress();
        }

        private void ResetFinalAttempt()
        {
            attemptLife = -1;
            attemptMaximumLife = 0;
        }

        private void ShowAttemptLife()
        {
            if (!IsFinalEncounter || attemptLife < 0 || magoStatsText == null || CurrentMago == null) return;
            magoStatsText.text = $"Mago {CurrentMago.InstanceName} — Vida: {attemptLife}/{attemptMaximumLife}\n" +
                $"Dano: {CurrentMago.Dano} · Alcance: {CurrentMago.Alcance} casas\n" +
                $"Iniciativa: {CurrentMago.Iniciativa} · Velocidade: {CurrentMago.VelocidadeAtaque}";
        }
    }
}
