using UnityEngine;

namespace ControlledFramerate.Core
{
    // EMA over unscaledDeltaTime, immune to Time.timeScale
    public static class FpsMonitor
    {
        private const float SmoothingAlpha = 0.05f;
        private const int MinFramesBeforeValid = 30;

        private static float smoothedFrameTime = 0.033f;
        private static int frameCount = 0;

        public static float SmoothedFps => smoothedFrameTime > 0f ? 1f / smoothedFrameTime : 0f;
        public static float SmoothedFrameTimeMs => smoothedFrameTime * 1000f;
        public static float InstantFps => Time.unscaledDeltaTime > 0f ? 1f / Time.unscaledDeltaTime : 0f;
        public static bool IsValid => frameCount >= MinFramesBeforeValid;

        public static void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f || dt > 1f) return;

            frameCount++;
            smoothedFrameTime = SmoothingAlpha * dt + (1f - SmoothingAlpha) * smoothedFrameTime;
        }

        public static void Reset()
        {
            smoothedFrameTime = 0.033f;
            frameCount = 0;
        }
    }
}
