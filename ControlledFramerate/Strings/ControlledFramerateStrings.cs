namespace ControlledFramerate.Strings
{
    public static class ControlledFramerateStrings
    {
        public const string ModPrefix = "[ControlledFramerate] ";

        public const string BenchmarkButtonText = "BENCH";
        public const string AdaptiveButtonText = "ADAPT";

        public const string BenchmarkTooltip = "Run FPS benchmark to find optimal speeds for this save";
        public const string AdaptiveTooltipDisabled = "Adaptive speed (run benchmark first to enable)";
        public const string AdaptiveTooltipOff = "Enable adaptive speed (adjusts game speed to maintain target FPS)";
        public const string AdaptiveTooltipOn = "Disable adaptive speed (use fixed speeds)";

        public const string BenchmarkOverlayTitle = "FPS Benchmark";
        public const string BenchmarkTesting = "Testing speed {0:F1}x...";
        public const string BenchmarkSettling = "Settling...";
        public const string BenchmarkMeasuring = "Measuring FPS...";
        public const string BenchmarkCurrentFps = "Current FPS: {0:F0}";
        public const string BenchmarkTargetFps = "Target FPS: {0}";
        public const string BenchmarkStep = "Step {0} of {1}";
        public const string BenchmarkComplete = "Benchmark complete! Max speed: {0:F1}x";
        public const string BenchmarkFailed = "No speed met the target FPS. Using 1x.";
        public const string BenchmarkCancelled = "Benchmark cancelled.";

        public const string SpeedModDetected = "Speed mod detected: {0}. ControlledFramerate will override its speed settings.";
        public const string SaveProfileLoaded = "Loaded speed profile for save '{0}': {1:F1}/{2:F1}/{3:F1}";
        public const string SaveProfileNotFound = "No speed profile for save '{0}'. Using defaults: {1:F1}/{2:F1}/{3:F1}";
        public const string AdaptiveEnabled = "Adaptive speed enabled (target: {0} FPS, min: {1} FPS)";
        public const string AdaptiveDisabled = "Adaptive speed disabled";
    }
}
