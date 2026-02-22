namespace ControlledMods.ModDetection
{
    // Detection for Customizable Plants mod (Truinto / FumiK)
    public static class CustomizablePlantsDetection
    {
        public const string DisplayName = "Customizable Plants";

        public static bool Loaded { get; private set; }

        public static void Detect()
        {
            Loaded = ModDetector.DetectByType("CustomizePlants.CustomizePlantsState")
                || ModDetector.DetectByAssembly("CustomizePlants");
        }
    }
}
