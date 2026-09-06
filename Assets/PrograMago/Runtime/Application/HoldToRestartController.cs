using System;

namespace PrograMago.Application
{
    public sealed class HoldToRestartController
    {
        public HoldToRestartController(float holdDurationSeconds)
        {
            if (holdDurationSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(holdDurationSeconds));
            }

            HoldDurationSeconds = holdDurationSeconds;
        }

        public float HoldDurationSeconds { get; }

        public float ElapsedSeconds { get; private set; }

        public float Progress => ElapsedSeconds / HoldDurationSeconds;

        public bool IsWaitingForRelease { get; private set; }

        public bool Update(bool isPressed, float unscaledDeltaTime)
        {
            if (unscaledDeltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(unscaledDeltaTime));
            }

            if (!isPressed)
            {
                ElapsedSeconds = 0f;
                IsWaitingForRelease = false;
                return false;
            }

            if (IsWaitingForRelease)
            {
                return false;
            }

            ElapsedSeconds = Math.Min(
                HoldDurationSeconds,
                ElapsedSeconds + unscaledDeltaTime);
            if (ElapsedSeconds < HoldDurationSeconds)
            {
                return false;
            }

            IsWaitingForRelease = true;
            return true;
        }
    }
}
