namespace STRINGS
{
    public class CONTROLLEDAUTOMATION
    {
        public class BUILDINGS
        {
            public class PREFABS
            {
                public class TEMPERATURERANGESENSOR
                {
                    public static LocString NAME = UI.FormatAsLink("Adv. Thermo Sensor", "TEMPERATURERANGESENSOR");
                    public static LocString DESC = "An advanced temperature sensor that activates when the ambient temperature is within a specified range.";
                    public static LocString EFFECT = "Sends a " + UI.FormatAsAutomationState("Green Signal", UI.AutomationState.Active) + " when the temperature is within the configured range.\n\nCan be inverted to activate outside the range instead.";
                    public static LocString LOGIC_PORT = "Temperature Range";
                    public static LocString LOGIC_PORT_ACTIVE = "Sends a " + UI.FormatAsAutomationState("Green Signal", UI.AutomationState.Active) + " when the temperature is within the specified range";
                    public static LocString LOGIC_PORT_INACTIVE = "Sends a " + UI.FormatAsAutomationState("Red Signal", UI.AutomationState.Standby) + " when the temperature is outside the specified range";
                }
            }
        }

        public class UISIDESCREENS
        {
            public class TEMPERATURERANGESIDESCREEN
            {
                public static LocString TITLE = "Temperature Range";
                public static LocString CURRENT_TEMP = "Current Temperature: {0}";
                public static LocString CENTER_TEMP = "Center Temperature";
                public static LocString DEGREES_BELOW = "Degrees Below Center";
                public static LocString DEGREES_ABOVE = "Degrees Above Center";
                public static LocString ACTIVE_RANGE = "Active Range: {0} to {1}";
                public static LocString INVERT = "Activate Outside Range";
                public static LocString INVERT_TOOLTIP = "When enabled, sends a Green Signal when temperature is OUTSIDE the range instead of inside.";
            }
        }

        // Threshold descriptions
        public static LocString LOGIC_PORT_ACTIVE = "Sends a " + UI.FormatAsAutomationState("Green Signal", UI.AutomationState.Active) + " when storage is <b>High Threshold</b> full, until <b>Low Threshold</b> is reached again";
        public static LocString LOGIC_PORT_INACTIVE = "Sends a " + UI.FormatAsAutomationState("Red Signal", UI.AutomationState.Standby) + " when storage is less than <b>Low Threshold</b> full, until <b>High Threshold</b> is reached again";
        public static LocString LOGIC_PORT_ACTIVE_INVERTED = "Sends a " + UI.FormatAsAutomationState("Green Signal", UI.AutomationState.Active) + " when storage is less than <b>Low Threshold</b> full, until <b>High Threshold</b> is reached again";
        public static LocString LOGIC_PORT_INACTIVE_INVERTED = "Sends a " + UI.FormatAsAutomationState("Red Signal", UI.AutomationState.Standby) + " when storage is <b>High Threshold</b> full, until <b>Low Threshold</b> is reached again";

        // Threshold tooltips
        public static LocString ACTIVATE_TOOLTIP = "Sends a " + UI.FormatAsAutomationState("Green Signal", UI.AutomationState.Active) + " when storage is <b>{0}%</b> full, until it is less than <b>{1}% (Low Threshold)</b> full";
        public static LocString DEACTIVATE_TOOLTIP = "Sends a " + UI.FormatAsAutomationState("Red Signal", UI.AutomationState.Standby) + " when storage is less than <b>{0}%</b> full, until it is <b>{1}% (High Threshold)</b> full";
        public static LocString ACTIVATE_TOOLTIP_INVERTED = "Sends a " + UI.FormatAsAutomationState("Red Signal", UI.AutomationState.Standby) + " when storage is <b>{0}%</b> full, until it is less than <b>{1}% (Low Threshold)</b> full";
        public static LocString DEACTIVATE_TOOLTIP_INVERTED = "Sends a " + UI.FormatAsAutomationState("Green Signal", UI.AutomationState.Active) + " when storage is less than <b>{0}%</b> full, until it is <b>{1}% (High Threshold)</b> full";

        // Inversion checkboxes
        public static LocString INVERT_CHECKBOX = "Invert Automation Signal";
        public static LocString INVERT_CHECKBOX_TOOLTIP = "When enabled, inverts the automation output signal (Green becomes Red, Red becomes Green).";
        public static LocString INVERT_CHECKBOX_STORAGE = "Send Green Signal When Low";
        public static LocString INVERT_CHECKBOX_STORAGE_TOOLTIP = "When enabled, sends a " + UI.FormatAsAutomationState("Green Signal", UI.AutomationState.Active) + " when storage is low instead of when it is sufficiently full.";

        // Sensor inversion
        public static LocString SENSOR_LOGIC_PORT_ACTIVE_INVERTED = "Sends a " + UI.FormatAsAutomationState("Green Signal", UI.AutomationState.Active) + " when condition is NOT met (inverted)";
        public static LocString SENSOR_LOGIC_PORT_INACTIVE_INVERTED = "Sends a " + UI.FormatAsAutomationState("Red Signal", UI.AutomationState.Standby) + " when condition IS met (inverted)";

        // Rocket Platform
        public static LocString ROCKET_OUTPUT_1_LABEL = "Output 1: Rocket Present";
        public static LocString ROCKET_OUTPUT_2_LABEL = "Output 2: Rocket Ready";
        public static LocString ROCKET_INVERT_OUTPUT_1 = "Invert 'Rocket Present' Signal";
        public static LocString ROCKET_INVERT_OUTPUT_1_TOOLTIP = "When enabled, inverts the first automation output signal that indicates rocket presence.";
        public static LocString ROCKET_INVERT_OUTPUT_2 = "Invert 'Rocket Ready' Signal";
        public static LocString ROCKET_INVERT_OUTPUT_2_TOOLTIP = "When enabled, inverts the second automation output signal that indicates rocket readiness.";

        // Sidescreen titles
        public static LocString SIDESCREEN_TITLE = "Automation Settings";
        public static LocString SIDESCREEN_TITLE_THRESHOLDS = "Threshold Settings";
    }
}
