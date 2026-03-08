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

                    public static class ENABLECLEANEDGES
                    {
                        public static LocString NAME = "Enable Clean Edges";
                        public static LocString TOOLTIP = "Converts many natural neutronium edge tiles to abyssalite while preserving outer borders and geyser bases.\n\n<i>Runs once per save when enabled.</i>";
                    }

                    public static class CLEANEDGESBORDERSIZE
                    {
                        public static LocString NAME = "Clean Edges Border Size";
                        public static LocString TOOLTIP = "Thickness of the neutronium border to preserve on the top, left, and right map edges.";
                    }

                    public static class CLEANEDGESABYSSALITEMASSKG
                    {
                        public static LocString NAME = "Clean Edges Abyssalite Mass (kg)";
                        public static LocString TOOLTIP = "Mass applied to abyssalite tiles created by Clean Edges.";
                    }

                    public static class SHOWALLTEMPERATUREUNITS
                    {
                        public static LocString NAME = "Show all temperature units";
                        public static LocString TOOLTIP = "When enabled, temperature tooltips and overlays show your chosen unit first, then the other two units in parentheses (e.g. 25 °C (77 °F, 298 K)). Your unit is set in Options → Units.";
                    }
                }
            }
        }
    }
}
