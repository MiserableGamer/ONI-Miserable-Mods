namespace ControlledMods.ModDetection
{
    // Detection for Delivery Temperature Limit mod (llunak original or QoooLiO Fixed)
    public static class DeliveryTemperatureLimitDetection
    {
        public const string DisplayName = "Delivery Temperature Limit";

        public static bool Loaded { get; private set; }

        public static void Detect()
        {
            Loaded = ModDetector.DetectByType("DeliveryTemperatureLimit.TemperatureLimit")
                || ModDetector.DetectByAssembly("DeliveryTemperatureLimit");
        }
    }
}
