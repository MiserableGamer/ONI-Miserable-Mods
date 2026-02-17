namespace ControlledMods.ModDetection
{
    // Detection for castrolol's Free Resource Buildings mod
    public static class FreeResourceBuildingsDetection
    {
        public const string DisplayName = "Free Resource Buildings (castrolol)";

        public static bool Loaded { get; private set; }

        public static void Detect()
        {
            Loaded = ModDetector.DetectByType("FreeResourceBuildingsPatches.Mod")
                || ModDetector.DetectByType("FreeResourceBuildings.FreeEnergyGenerator")
                || ModDetector.DetectByAssembly("FreeResourceBuildings");
        }
    }
}
