using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ControlledVisuals.Options
{
    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile(SharedConfigLocation: true)]
    [RestartRequired]
    public sealed class ControlledVisualsOptions : SingletonOptions<ControlledVisualsOptions>
    {
        [JsonProperty]
        public ConduitAnimationQuality ConduitAnimation { get; set; }

        [Option("Enable Clean Edges",
                "Converts many natural neutronium edge tiles to abyssalite while preserving outer borders and geyser bases. Runs once per save when enabled.",
                "World")]
        [JsonProperty]
        public bool EnableCleanEdges { get; set; }

        [Option("Clean Edges Border Size",
                "Thickness of the neutronium border to preserve on the top, left, and right map edges.",
                "World")]
        [Limit(1, 20)]
        [JsonProperty]
        public int CleanEdgesBorderSize { get; set; }

        [Option("Clean Edges Abyssalite Mass (kg)",
                "Mass applied to abyssalite tiles created by Clean Edges.",
                "World")]
        [Limit(1, 20000)]
        [JsonProperty]
        public int CleanEdgesAbyssaliteMassKg { get; set; }

        [Option("Reconvert on Next Save Load",
                "Forces Clean Edges to run once on the next save load (only when Enable Clean Edges is on), then automatically resets this option to off.",
                "World")]
        [JsonProperty]
        public bool ReconvertOnNextSaveLoad { get; set; }

        [Option("Show all temperature units",
                "When enabled, temperature tooltips and overlays show your chosen unit first, then the other two units in parentheses (e.g. 25 °C (77 °F, 298 K)). Your unit is set in Options → Units.",
                "Visual")]
        [JsonProperty]
        public bool ShowAllTemperatureUnits { get; set; }

        public ControlledVisualsOptions()
        {
            ConduitAnimation = ConduitAnimationQuality.Full;
            EnableCleanEdges = false;
            CleanEdgesBorderSize = 2;
            CleanEdgesAbyssaliteMassKg = 2;
            ReconvertOnNextSaveLoad = false;
            ShowAllTemperatureUnits = false;
        }

        public enum ConduitAnimationQuality
        {
            [Option("STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.FULL",
                    "STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.FULL_TOOLTIP")]
            Full,

            [Option("STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.REDUCED",
                    "STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.REDUCED_TOOLTIP")]
            Reduced,

            [Option("STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.MINIMAL",
                    "STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.MINIMAL_TOOLTIP")]
            Minimal
        }
    }
}
