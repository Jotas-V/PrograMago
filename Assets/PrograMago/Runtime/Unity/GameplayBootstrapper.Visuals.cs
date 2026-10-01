using System.Collections.Generic;
using PrograMago.Domain;
using UnityEngine;

namespace PrograMago.UnityIntegration
{
    public sealed partial class GameplayBootstrapper
    {
        private readonly List<CombatProjectileView> projectiles = new List<CombatProjectileView>();
        private readonly Dictionary<CombatElement, GameObject> projectilePrefabs = new Dictionary<CombatElement, GameObject>();
        private readonly Queue<CombatEvent> trace = new Queue<CombatEvent>();
        private float nextTraceTime;
        private bool HasCombatVisuals => projectiles.Exists(item => item != null) ||
            movingActors.Count > 0 || IsAttackPlaying(wizardInstance) || enemyMarkers.Exists(IsAttackPlaying);

        private static bool IsAttackPlaying(GameObject actor)
        {
            if (actor == null || !actor.activeInHierarchy) return false;
            Animator animator = actor.GetComponent<Animator>();
            if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
                return false;
            return animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") ||
                (animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName("Attack"));
        }

        private void PresentCombatStep(CombatEvent step)
        {
            if (step.BlockIndex >= 0) trace.Enqueue(step);
            if (step.Kind == CombatEventKind.Hit || step.Kind == CombatEventKind.Ineffective)
            {
                if (step.Actor == "Mago") PlayWizardAttack();
                if (step.Kind == CombatEventKind.Hit)
                    FlashCombatTarget(FindCombatActor(step.Target)?.gameObject, step.Element);
            }
            if (step.Kind != CombatEventKind.Hit && step.Kind != CombatEventKind.Ineffective) return;
            if (!projectilePrefabs.TryGetValue(step.Element, out GameObject prefab))
            {
                prefab = Resources.Load<GameObject>("Combat/" + step.Element + "Projectile");
                projectilePrefabs[step.Element] = prefab;
            }
            if (prefab == null) { Debug.LogError("Prefab de magia ausente: " + step.Element); return; }
            Transform source = FindCombatActor(step.Actor);
            Transform target = FindCombatActor(step.Target);
            if (source == null || target == null) return;
            var projectile = Instantiate(prefab).GetComponent<CombatProjectileView>();
            projectile.Launch(source.position, target.position, () => combat != null && combat.IsPaused);
            projectiles.Add(projectile);
        }

        private Transform FindCombatActor(string actor)
        {
            if (actor == "Mago") return wizardInstance == null ? null : wizardInstance.transform;
            for (int i = 0; i < CurrentEnemies.Count && i < enemyMarkers.Count; i++)
                if (CurrentEnemies[i].VariableName == actor) return enemyMarkers[i].transform;
            return null;
        }

        private void UpdateCombatPresentation()
        {
            projectiles.RemoveAll(item => item == null);
            if (combat != null && !combat.IsPaused && trace.Count > 0 && Time.unscaledTime >= nextTraceTime)
            {
                CombatEvent step = trace.Dequeue();
                RefreshCombatActionButtons();
                var activeButton = TimelineButtonAt(step.BlockIndex);
                if (activeButton != null)
                    activeButton.targetGraphic.color =
                        step.Kind == CombatEventKind.NoTarget || step.Kind == CombatEventKind.MissingSpell
                        ? new Color32(170, 59, 68, 255) : new Color32(57, 132, 105, 255);
                nextTraceTime = Time.unscaledTime + 0.15f;
            }
            CompleteCombatWhenVisualsFinish();
        }

        private void CompleteCombatWhenVisualsFinish()
        {
            if (combat == null || combat.Outcome != CombatOutcome.Victory || HasCombatVisuals) return;
            HideCombatControls();
            if (!ReportBattleVictory()) SetInteractionEnabled(true);
            combat = null;
            trace.Clear();
        }

        private void ClearCombatPresentation()
        {
            StopWizardMove();
            foreach (var projectile in projectiles) if (projectile != null) Destroy(projectile.gameObject);
            projectiles.Clear();
            trace.Clear();
            nextTraceTime = 0;
        }
    }
}
