namespace ControlledMods.ModDetection
{
    // Detection for KIN's Underground Conduit mod
    public static class UndergroundConduitDetection
    {
        public const string DisplayName = "KIN Underground Conduit";

        public static bool Loaded { get; private set; }

        public static void Detect()
        {
            Loaded = ModDetector.DetectByType("UndergroundConduit.Mod");
        }
    }
}
