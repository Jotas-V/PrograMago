using System;
using PrograMago.Application;
using PrograMago.Domain;
using PrograMago.Language;

namespace PrograMago.Presentation
{
    public sealed class LearningFlowPresenter
    {
        private readonly HoldToRestartController restartController;
        private readonly ICodeEditorView editor;
        private readonly IFeedbackView feedback;
        private readonly IArenaView arena;
        private readonly ILearningFlowView view;

        public LearningFlowPresenter(
            LearningProgress progress,
            HoldToRestartController restartController,
            ICodeEditorView editor,
            IFeedbackView feedback,
            IArenaView arena,
            ILearningFlowView view)
        {
            Progress = progress ?? throw new ArgumentNullException(nameof(progress));
            this.restartController = restartController ?? throw new ArgumentNullException(nameof(restartController));
            this.editor = editor ?? throw new ArgumentNullException(nameof(editor));
            this.feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
            this.arena = arena ?? throw new ArgumentNullException(nameof(arena));
            this.view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public LearningProgress Progress { get; }

        public void Initialize()
        {
            RenderCurrentBattle();
            view.HideVictory();
            view.HideRestartProgress();
            view.SetInteractionEnabled(true);
        }

        public void HandleSubmission(SubmitCodeResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (!result.IsSuccess)
            {
                RevealNextHint();
                return;
            }

            if (!result.SatisfiedCriterion.HasValue ||
                !Progress.TryStartBattle(result.SatisfiedCriterion.Value))
            {
                RevealNextHint();
                feedback.ShowError(new Diagnostic(
                    "FLOW001",
                    new SourcePosition(0, 1, 1),
                    "O código ainda não atende ao objetivo da batalha atual."));
                return;
            }

            view.SetInteractionEnabled(false);
        }

        public bool ReportBattleVictory()
        {
            if (!Progress.ReportVictory())
            {
                return false;
            }

            bool isFinalBattle = Progress.CurrentBattleIndex == Progress.Path.Battles.Count - 1;
            view.ShowVictory(Progress.CurrentBattle.Victory, isFinalBattle);
            return true;
        }

        public bool ReportBattleDefeat()
        {
            if (!Progress.ReportDefeat())
            {
                return false;
            }

            ResetCurrentBattleViews();
            return true;
        }

        public bool NextBattle()
        {
            if (!Progress.ContinueAfterVictory())
            {
                return false;
            }

            editor.SourceCode = string.Empty;
            arena.SetClassDeclared(false);
            feedback.Clear();
            view.HideVictory();
            view.ShowHint(string.Empty);
            view.HideRestartProgress();
            view.SetInteractionEnabled(true);
            RenderCurrentBattle();
            return true;
        }

        public bool RestartCurrentBattle()
        {
            if (!Progress.RestartCurrentBattle())
            {
                return false;
            }

            ResetCurrentBattleViews();
            return true;
        }

        public bool UpdateRestartHold(bool isPressed, float unscaledDeltaTime)
        {
            if (Progress.Stage == LearningStage.VictoryReview ||
                Progress.Stage == LearningStage.JourneyCompleted)
            {
                restartController.Update(false, unscaledDeltaTime);
                view.HideRestartProgress();
                return false;
            }

            bool triggered = restartController.Update(isPressed, unscaledDeltaTime);
            if (isPressed)
            {
                view.ShowRestartProgress(restartController.Progress);
            }
            else
            {
                view.HideRestartProgress();
            }

            if (!triggered)
            {
                return false;
            }

            bool restarted = RestartCurrentBattle();
            view.ShowRestartProgress(restartController.Progress);
            return restarted;
        }

        private void RenderCurrentBattle()
        {
            view.ShowBattle(
                Progress.CurrentBattle,
                Progress.CurrentBattleIndex + 1,
                Progress.Path.Battles.Count);
        }

        private void RevealNextHint()
        {
            string hint = Progress.RegisterFailedAttempt();
            if (hint != null)
            {
                view.ShowHint(hint);
            }
        }

        private void ResetCurrentBattleViews()
        {
            arena.SetClassDeclared(false);
            feedback.Clear();
            view.HideVictory();
            view.HideRestartProgress();
            view.SetInteractionEnabled(true);
        }
    }
}
