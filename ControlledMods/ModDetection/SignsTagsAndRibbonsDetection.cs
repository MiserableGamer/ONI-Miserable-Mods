namespace ControlledMods.ModDetection
{
    // Detection for Pether's Signs, Tags and Ribbons mod
    public static class SignsTagsAndRibbonsDetection
    {
        public const string DisplayName = "Signs, Tags and Ribbons";

        public static bool Loaded { get; private set; }

        public static void Detect()
        {
            Loaded = ModDetector.DetectByType("SignsTagsAndRibbons.SmallElementTagConfig")
                || ModDetector.DetectByAssembly("SignsTagsAndRibbons");
        }
    }
}
