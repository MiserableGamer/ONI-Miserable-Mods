namespace ControlledMods.ModDetection
{
    // Detection for Darkness Not Excluded Relit
    public static class DarknessNotExcludedRelitDetection
    {
        public const string DisplayName = "Darkness Not Excluded Relit";

        public static bool Loaded { get; private set; }

        public static void Detect()
        {
            Loaded = ModDetector.DetectByType("DarknessNotIncluded.Darkness.Behavior")
                || ModDetector.DetectByAssembly("DarknessNotExcluded");
        }
    }
}
