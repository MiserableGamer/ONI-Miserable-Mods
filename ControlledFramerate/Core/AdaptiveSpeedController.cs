using ControlledFramerate.Options;
using UnityEngine;

namespace ControlledFramerate.Core
{
    public static class AdaptiveSpeedController
    {
        private const float DeadZoneAboveDesired = 5f;
        private const int CooldownFrames = 30;
        private const float MinTimeScale = 0.5f;

        private static int cooldownRemaining = 0;
        private static float currentAdaptiveSpeed = 1f;

        public static float CurrentAdaptiveSpeed => currentAdaptiveSpeed;

        public static void Update(int selectedSpeedButton)
        {
            if (SpeedStateManager.CurrentMode != SpeedStateManager.SpeedMode.Adaptive) return;
            if (SpeedStateManager.IsBenchmarkRunning) return;
            if (SpeedStateManager.IsInSaveGracePeriod) return;
            if (!FpsMonitor.IsValid) return;
            if (SpeedControlScreen.Instance != null && SpeedControlScreen.Instance.IsPaused) return;

            var opts = ControlledFramerateOptions.Instance;
            float ceiling = SpeedStateManager.GetSpeedForButton(selectedSpeedButton);
            float fps = FpsMonitor.SmoothedFps;
            float desired = opts.DesiredFps;
            float minimum = opts.MinimumFps;

            float rampRate = GetRampRate(opts.AdjustmentSmoothing);

            if (fps < minimum)
            {
                currentAdaptiveSpeed -= rampRate * 3f;
                cooldownRemaining = CooldownFrames;
            }
            else if (fps < desired)
            {
                currentAdaptiveSpeed -= rampRate;
                cooldownRemaining = CooldownFrames;
            }
            else if (fps > desired + DeadZoneAboveDesired)
            {
                if (cooldownRemaining <= 0)
                    currentAdaptiveSpeed += rampRate * 0.5f;
            }

            if (cooldownRemaining > 0)
                cooldownRemaining--;

            currentAdaptiveSpeed = Mathf.Clamp(currentAdaptiveSpeed, MinTimeScale, ceiling);
            Time.timeScale = currentAdaptiveSpeed;
        }

        public static void SetSpeed(float speed)
        {
            currentAdaptiveSpeed = speed;
            cooldownRemaining = 0;
        }

        public static void Reset()
        {
            currentAdaptiveSpeed = 1f;
            cooldownRemaining = 0;
        }

        private static float GetRampRate(int level)
        {
            switch (level)
            {
                case 0: return 0.01f;
                case 1: return 0.03f;
                case 2: return 0.06f;
                default: return 0.03f;
            }
        }
    }
}
