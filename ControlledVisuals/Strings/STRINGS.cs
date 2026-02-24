namespace ControlledVisuals.Strings
{
    public static class STRINGS
    {
        public static class UI
        {
            public static class CONTROLLEDVISUALS
            {
                public static class OPTIONS
                {
                    public static LocString CATEGORY_VISUAL = "Visual";

                    public static class CONDUITANIMATION
                    {
                        public static LocString NAME = "Pipe Animation Quality";
                        public static LocString TOOLTIP = "Controls the visual fidelity of liquid and gas pipe animations.\n\n<i>No changes to actual pipe mechanics will occur - this only affects visuals.</i>\n\n<b>Performance Impact: <color=#FF8827>Medium</color></b>";

                        public static LocString FULL = "Full";
                        public static LocString FULL_TOOLTIP = "Pipe animation quality is unchanged from the base game.\nAnimations update every frame.";

                        public static LocString REDUCED = "Reduced";
                        public static LocString REDUCED_TOOLTIP = "Pipe animations update at 10 FPS (every 0.1 seconds).\nWhen zoomed far out, updates reduce to 1 FPS.\n\n<i>Recommended for mid-game colonies.</i>";

                        public static LocString MINIMAL = "Minimal";
                        public static LocString MINIMAL_TOOLTIP = "Pipe animations update at 2 FPS (every 0.5 seconds).\nWhen zoomed far out, updates reduce to 1 FPS.\n\n<i>Recommended for large colonies with extensive pipe networks.</i>";
                    }
                }
            }
        }
    }
}
