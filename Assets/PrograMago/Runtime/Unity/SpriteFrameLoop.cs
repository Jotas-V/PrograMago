using UnityEngine;

namespace PrograMago.UnityIntegration
{
    public sealed class SpriteFrameLoop : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private Sprite[] frames;
        [SerializeField, Min(0.02f)] private float secondsPerFrame = 0.16f;

        private int currentFrame;
        private float elapsed;

        public void Configure(SpriteRenderer renderer, Sprite[] animationFrames, float frameDuration)
        {
            target = renderer;
            frames = animationFrames;
            secondsPerFrame = Mathf.Max(0.02f, frameDuration);
            currentFrame = 0;
            elapsed = 0f;
            ApplyFrame();
        }

        private void OnEnable()
        {
            currentFrame = 0;
            elapsed = 0f;
            ApplyFrame();
        }

        private void Update()
        {
            if (target == null || frames == null || frames.Length < 2) return;
            elapsed += Time.unscaledDeltaTime;
            while (elapsed >= secondsPerFrame)
            {
                elapsed -= secondsPerFrame;
                currentFrame = (currentFrame + 1) % frames.Length;
                ApplyFrame();
            }
        }

        private void ApplyFrame()
        {
            if (target != null && frames != null && frames.Length > 0)
                target.sprite = frames[Mathf.Clamp(currentFrame, 0, frames.Length - 1)];
        }
    }
}
