using static STRINGS.UI;

namespace ControlledMods.ResourceSensor.UI
{
    public static class UISIDESCREENS
    {
        public static class RESOURCE_SENSOR_SIDE_SCREEN
        {
            public static readonly LocString TITLE = "Resource Sensor";
            public static readonly LocString VALUE_NAME = "Value";
            public static LocString SLIDER_TOOLTIP => $"Resources further than {FormatAsKeyWord("{0}")} tiles will not be counted.";
        }
    }
}
