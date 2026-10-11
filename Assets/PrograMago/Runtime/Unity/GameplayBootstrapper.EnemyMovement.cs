using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrograMago.UnityIntegration
{
    public sealed partial class GameplayBootstrapper
    {
        private readonly Dictionary<GameObject, Coroutine> enemyMoves = new Dictionary<GameObject, Coroutine>();

        private void AnimateEnemyMove(GameObject actor, int cell)
        {
            if (actor == null) return;
            if (enemyMoves.TryGetValue(actor, out var previous) && previous != null) StopCoroutine(previous);
            enemyMoves[actor] = StartCoroutine(MoveEnemy(actor, cell));
        }
        private IEnumerator MoveEnemy(GameObject actor, int cell)
        {
            movingActors.Add(actor);
            var animator = actor.GetComponent<Animator>();
            if (animator != null) { animator.SetBool("Walking", true); animator.speed = 1; }
            var start = actor.transform.position;
            var end = GetActorCellPosition(actor, cell);
            float elapsed = 0;
            while (actor != null && elapsed < .44f) {
                bool paused = combat != null && combat.IsPaused;
                if (animator != null) animator.speed = paused ? 0 : 1;
                if (!paused) {
                    elapsed += Time.unscaledDeltaTime;
                    actor.transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(elapsed / .44f));
                }
                yield return null;
            }
            if (actor != null) {
                actor.transform.position = end;
                if (animator != null) { animator.SetBool("Walking", false); animator.speed = CharacterAnimationSpeed; }
            }
            movingActors.Remove(actor); enemyMoves.Remove(actor);
        }
        private void StopEnemyMoves()
        {
            foreach (var move in enemyMoves) {
                if (move.Value != null) StopCoroutine(move.Value);
                if (move.Key != null) {
                    var animator = move.Key.GetComponent<Animator>();
                    if (animator != null) { animator.SetBool("Walking", false); animator.speed = CharacterAnimationSpeed; }
                }
                movingActors.Remove(move.Key);
            }
            enemyMoves.Clear();
        }
    }
}
