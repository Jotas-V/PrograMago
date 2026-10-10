using UnityEngine;
namespace PrograMago.UnityIntegration
{
    public sealed class MenuDuelAnimation : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image wizard, dummy, projectile;
        [SerializeField] private Sprite[] idleFrames, attackFrames;
        private float elapsed;
        private Vector2 projectileOrigin, dummyOrigin;
        private void Start()
        {
            Canvas.ForceUpdateCanvases();
            projectileOrigin = projectile.rectTransform.anchoredPosition;
            dummyOrigin = dummy.rectTransform.anchoredPosition;
        }
        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float cycle = elapsed % 3.2f;
            bool attacking = cycle >= .5f && cycle < 1.4f;
            var frames = attacking ? attackFrames : idleFrames;
            if (frames.Length > 0) wizard.sprite = frames[(int)((attacking ? cycle - .5f : elapsed) / .14f) % frames.Length];
            float flight = Mathf.InverseLerp(1.05f, 1.85f, cycle);
            projectile.enabled = cycle >= 1.05f && cycle < 1.85f;
            Vector2 travel = projectile.transform.parent.InverseTransformVector(dummy.transform.position - projectile.transform.position);
            travel += projectile.rectTransform.anchoredPosition - projectileOrigin;
            projectile.rectTransform.anchoredPosition = projectileOrigin + travel * flight + new Vector2(0, Mathf.Sin(flight * Mathf.PI) * 24);
            bool impact = cycle >= 1.85f && cycle < 2.08f;
            dummy.color = impact ? new Color(1f, .75f, .45f) : Color.white;
            dummy.rectTransform.anchoredPosition = dummyOrigin + (impact ? new Vector2(Mathf.Sin(cycle * 80) * 6, 0) : Vector2.zero);
        }
    }
}
