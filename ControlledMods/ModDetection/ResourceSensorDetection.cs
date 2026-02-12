namespace ControlledMods.ModDetection
{
    // Detection for Berkay's Resource Sensor mod
    public static class ResourceSensorDetection
    {
        public const string DisplayName = "Resource Sensor (Berkay)";

        public static bool Loaded { get; private set; }

        public static void Detect()
        {
            Loaded = ModDetector.DetectByType("ResourceSensor.ResourceSensorMod")
                || ModDetector.DetectByAssembly("ResourceSensor");
        }
    }
}
