namespace ControlledMods.ModDetection
{
    // Detection for Peter Han's ShowRange mod
    public static class ShowRangeDetection
    {
        public const string DisplayName = "ShowRange (Peter Han)";

        public static bool Loaded { get; private set; }

        public static void Detect()
        {
            Loaded = ModDetector.DetectByType("PeterHan.ShowRange.SimVisualizerParams")
                || ModDetector.DetectByAssembly("ShowRange");
        }
    }
}
