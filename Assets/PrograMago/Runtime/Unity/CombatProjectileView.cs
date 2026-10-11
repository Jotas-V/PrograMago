using System;
using UnityEngine;

namespace PrograMago.UnityIntegration
{
    // Presentation only: the engine has already resolved damage and outcome.
    public sealed class CombatProjectileView : MonoBehaviour
    {
        [SerializeField] private float flightDuration = 0.45f;
        [SerializeField] private float impactDuration = 0.25f;
        private Vector3 origin;
        private Vector3 destination;
        private Vector3 originalScale;
        private float elapsed;
        private Func<bool> paused;
        private SpriteRenderer sprite;
        private Color initialColor;
        private bool launched;
        private Action finished;
        private Action impacted;
        private bool arrived;

        public void Launch(Vector3 start, Vector3 end, Func<bool> isPaused, Action onFinished = null, Action onImpact = null)
        {
            origin = start;
            destination = end;
            paused = isPaused;
            finished = onFinished;
            impacted = onImpact;
            arrived = false;
            elapsed = 0;
            originalScale = transform.localScale;
            sprite = GetComponentInChildren<SpriteRenderer>();
            initialColor = sprite.color;
            transform.position = start;
            launched = true;
        }

        private void OnDestroy()
        {
            finished?.Invoke();
            finished = null;
        }

        private void Update()
        {
            if (!launched || (paused != null && paused())) return;
            elapsed += Time.unscaledDeltaTime;
            if (elapsed < flightDuration)
            {
                float t = elapsed / flightDuration;
                transform.position = Vector3.Lerp(origin, destination, t);
                transform.localScale = originalScale * (1f + 0.08f * Mathf.Sin(t * 20f));
            }
            else
            {
                if (!arrived) {
                    arrived = true;
                    impacted?.Invoke();
                    impacted = null;
                }
                float t = Mathf.Clamp01((elapsed - flightDuration) / impactDuration);
                transform.position = destination;
                transform.localScale = originalScale * (1f + t * 2.5f);
                sprite.color = Color.Lerp(Color.white, new Color(initialColor.r, initialColor.g, initialColor.b, 0f), t);
                if (t >= 1f) Destroy(gameObject);
            }
        }
    }
}
