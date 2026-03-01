namespace ControlledMods.ModDetection
{
    // Detection for Pholith's DuplicantRoomSensor mod
    public static class DuplicantRoomSensorDetection
    {
        public const string DisplayName = "Duplicant Room Sensor (Pholith)";

        public static bool Loaded { get; private set; }

        public static void Detect()
        {
            Loaded = ModDetector.DetectByType("DuplicantRoomSensor.LogicDuplicantCountSensor")
                || ModDetector.DetectByAssembly("DuplicantRoomSensor");
        }
    }
}
