namespace STRINGS
{
    public static class CONTROLLEDCONDUITS
    {
        public class BUILDINGS
        {
            public class PREFABS
            {
                public class GASCONDUITVOLUMESENSOR
                {
                    public static LocString NAME = UI.FormatAsLink("Gas Conduit Volume Sensor", "GasConduitVolumeSensor");
                    public static LocString DESC = "Detects the mass of the gas packet in the conduit at this cell. Use to find sub-1g packets or to automate by packet size.";
                    public static LocString EFFECT = "Sends an automation signal when the conduit packet mass is above or below the set threshold.";
                    public static LocString LOGIC_PORT = "Packet Mass";
                    public static LocString LOGIC_PORT_ACTIVE = "Sends a " + UI.FormatAsAutomationState("Green Signal", UI.AutomationState.Active) + " when packet mass meets the threshold";
                    public static LocString LOGIC_PORT_INACTIVE = "Otherwise sends a " + UI.FormatAsAutomationState("Red Signal", UI.AutomationState.Standby);
                }
                public class LIQUIDCONDUITVOLUMESENSOR
                {
                    public static LocString NAME = UI.FormatAsLink("Liquid Conduit Volume Sensor", "LiquidConduitVolumeSensor");
                    public static LocString DESC = "Detects the mass of the liquid packet in the conduit at this cell. Use to find sub-1g packets or to automate by packet size.";
                    public static LocString EFFECT = "Sends an automation signal when the conduit packet mass is above or below the set threshold.";
                    public static LocString LOGIC_PORT = "Packet Mass";
                    public static LocString LOGIC_PORT_ACTIVE = "Sends a " + UI.FormatAsAutomationState("Green Signal", UI.AutomationState.Active) + " when packet mass meets the threshold";
                    public static LocString LOGIC_PORT_INACTIVE = "Otherwise sends a " + UI.FormatAsAutomationState("Red Signal", UI.AutomationState.Standby);
                }
                public class SOLIDCONDUITVOLUMESENSOR
                {
                    public static LocString NAME = UI.FormatAsLink("Conveyor Volume Sensor", "SolidConduitVolumeSensor");
                    public static LocString DESC = "Detects the mass of the conveyor packet at this cell. Use to find sub-1g packets or to automate by packet size.";
                    public static LocString EFFECT = "Sends an automation signal when the conveyor packet mass is above or below the set threshold.";
                    public static LocString LOGIC_PORT = "Packet Mass";
                    public static LocString LOGIC_PORT_ACTIVE = "Sends a " + UI.FormatAsAutomationState("Green Signal", UI.AutomationState.Active) + " when packet mass meets the threshold";
                    public static LocString LOGIC_PORT_INACTIVE = "Otherwise sends a " + UI.FormatAsAutomationState("Red Signal", UI.AutomationState.Standby);
                }
            }
        }

        // Explicit keys so ToString() resolves via Strings.Get(key) instead of "MISSING".
        public static LocString THRESHOLD_SIDESCREEN_TITLE = new LocString("Packet mass", "STRINGS.CONTROLLEDCONDUITS.THRESHOLD_SIDESCREEN_TITLE");
        public static LocString THRESHOLD_PACKET_MASS = new LocString("Packet mass", "STRINGS.CONTROLLEDCONDUITS.THRESHOLD_PACKET_MASS");
        public static string THRESHOLD_ABOVE_TOOLTIP = "Send Green Signal when packet mass is above the threshold.";
        public static string THRESHOLD_BELOW_TOOLTIP = "Send Green Signal when packet mass is below the threshold.";

        public static LocString IGNORE_EMPTY_TITLE = new LocString("Ignore Empty (below only)", "STRINGS.CONTROLLEDCONDUITS.IGNORE_EMPTY_TITLE");
        // Section title distinct from checkbox label so sidescreen header isn't redundant.
        public static LocString VOLUME_SENSOR_OPTIONS_TITLE = new LocString("Options", "STRINGS.CONTROLLEDCONDUITS.VOLUME_SENSOR_OPTIONS_TITLE");
        public static string IGNORE_EMPTY_LABEL = "Ignore Empty (below only)";
        public static string IGNORE_EMPTY_TOOLTIP = "When enabled with \"Below\" mode: treat an empty conduit as inactive (do not send Green). Only send Green when there is some mass but it is below the threshold.";
    }
}
