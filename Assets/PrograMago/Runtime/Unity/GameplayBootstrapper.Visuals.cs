using System.Collections;
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
        private const float WizardCastDuration = 0.5f;
        private const float WizardCastReleaseProgress = 2f / 3f;
        private readonly Queue<CombatEvent> pendingWizardCasts = new Queue<CombatEvent>();
        private Coroutine wizardCastRoutine;
        private bool wizardCastActive;
        private AnimatorUpdateMode wizardCastPreviousUpdateMode;
        private float WizardAnimationPlaybackSpeed => wizardCastActive
            ? combat != null && combat.IsPaused ? 0f : 1f
            : CharacterAnimationSpeed;
        private float nextTraceTime;
        private bool HasCombatVisuals => wizardCastRoutine != null || pendingWizardCasts.Count > 0 ||
            projectiles.Exists(item => item != null) ||
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
            if (step.Kind != CombatEventKind.Hit && step.Kind != CombatEventKind.Ineffective) return;
            if (step.Actor == "Mago")
            {
                pendingWizardCasts.Enqueue(step);
                if (wizardCastRoutine == null) wizardCastRoutine = StartCoroutine(PresentWizardCasts());
                return;
            }
            LaunchCombatProjectile(step);
        }

        private IEnumerator PresentWizardCasts()
        {
            // Let StartCoroutine return its handle before a fallback can complete the queue.
            yield return null;
            while (pendingWizardCasts.Count > 0)
            {
                CombatEvent step = pendingWizardCasts.Dequeue();
                Animator animator = wizardInstance == null ? null : wizardInstance.GetComponent<Animator>();
                if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
                {
                    LaunchCombatProjectile(step);
                    continue;
                }
                wizardCastPreviousUpdateMode = animator.updateMode;
                wizardCastActive = true;
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                PlayWizardAttack();
                bool released = false;
                float elapsed = 0f;
                while (elapsed < WizardCastDuration)
                {
                    yield return null;
                    if (combat != null && combat.IsPaused) continue;
                    elapsed += Time.unscaledDeltaTime;
                    if (!released && animator != null &&
                        animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
                        animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= WizardCastReleaseProgress)
                    {
                        LaunchCombatProjectile(step);
                        released = true;
                    }
                }
                if (!released) LaunchCombatProjectile(step);
                FinishWizardCast();
            }
            wizardCastRoutine = null;
        }

        private void FinishWizardCast()
        {
            if (!wizardCastActive) return;
            wizardCastActive = false;
            Animator animator = wizardInstance == null ? null : wizardInstance.GetComponent<Animator>();
            if (animator == null || !animator.isActiveAndEnabled) return;
            animator.updateMode = wizardCastPreviousUpdateMode;
            animator.speed = CharacterAnimationSpeed;
            animator.Play("Idle", 0, 0f);
            animator.Update(0f);
        }

        private void LaunchCombatProjectile(CombatEvent step)
        {
            if (!projectilePrefabs.TryGetValue(step.Element, out GameObject prefab))
            {
                prefab = Resources.Load<GameObject>("Combat/" + step.Element + "Projectile");
                projectilePrefabs[step.Element] = prefab;
            }
            if (prefab == null) { Debug.LogError("Prefab de magia ausente: " + step.Element); return; }
            Transform source = FindCombatActor(step.Actor);
            Transform target = FindCombatActor(step.Target);
            if (source == null || target == null) return;
            if (step.Kind == CombatEventKind.Hit)
                FlashCombatTarget(target.gameObject, step.Element);
            var projectile = Instantiate(prefab).GetComponent<CombatProjectileView>();
            projectile.Launch(CombatActorVisualCenter(source), CombatActorVisualCenter(target),
                () => combat != null && combat.IsPaused);
            projectiles.Add(projectile);
        }
        private static Vector3 CombatActorVisualCenter(Transform actor)
        {
            SpriteRenderer sprite = actor.GetComponentInChildren<SpriteRenderer>();
            return sprite != null ? sprite.bounds.center : actor.position;
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
            if (wizardCastActive && wizardInstance != null)
            {
                Animator animator = wizardInstance.GetComponent<Animator>();
                if (animator != null) animator.speed = WizardAnimationPlaybackSpeed;
            }
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
            if (wizardCastRoutine != null) StopCoroutine(wizardCastRoutine);
            wizardCastRoutine = null;
            pendingWizardCasts.Clear();
            FinishWizardCast();
            StopWizardMove();
            foreach (var projectile in projectiles) if (projectile != null) Destroy(projectile.gameObject);
            projectiles.Clear();
            trace.Clear();
            nextTraceTime = 0;
        }
    }
}
