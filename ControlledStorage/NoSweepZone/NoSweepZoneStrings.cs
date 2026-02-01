using System.Collections.Generic;

namespace ControlledStorage.NoSweepZone
{
    public class UI
    {
        public class STRINGS
        {
            public const string TOOL_TITLE = "Set No-Sweep Zones";
            public const string TOOL_HOVER_CARD_TITLE = "No-Sweep Zone Tool";
            public const string TOOL_DESCRIPTION = "Set No-Sweep Zones {Hotkey}";
            public const string TOOL_ICON = "CONTROLLEDSTORAGE.NOSWEEPZONE.TOOL.ICON";
            public const string SET_VISUALIZER_ICON = "CONTROLLEDSTORAGE.NOSWEEPZONE.VISUALIZER.SET.ICON";
            public const string CANCEL_VISUALIZER_ICON = "CONTROLLEDSTORAGE.NOSWEEPZONE.VISUALIZER.CANCEL.ICON";

            public static readonly KeyValuePair<string, string> OVERLAY_NAME =
                new KeyValuePair<string, string>("CONTROLLEDSTORAGE.NOSWEEPZONE.OVERLAY.NAME", "No-Sweep Zone Overlay");
            public static readonly KeyValuePair<string, string> OVERLAY_DESCRIPTION =
                new KeyValuePair<string, string>("CONTROLLEDSTORAGE.NOSWEEPZONE.OVERLAY.DESCRIPTION", "Display no-sweep zone areas");
            public const string OVERLAY_ICON = "CONTROLLEDSTORAGE.NOSWEEPZONE.TOOL.ICON";
        }

        public class Actions
        {
            public static string OVERLAY_ACTION_KEY = "ControlledStorage.NoSweepZone.Action.Overlay";
            public static LocString OVERLAY_ACTION_TITLE = "No-Sweep Zone Overlay";
            public static string TOOL_ACTION_KEY = "ControlledStorage.NoSweepZone.Action.Tool";
            public static LocString TOOL_ACTION_TITLE = "No-Sweep Zone Tool";
        }
    }
}
