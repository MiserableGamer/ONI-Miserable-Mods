using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ControlledExtraction.Options
{
    [JsonObject(MemberSerialization.OptIn)]
    [ModInfo("Controlled Extraction", "https://github.com/MiserableGamer/ONI-Miserable-Mods")]
    [ConfigFile("ControlledExtraction.json", true, true)]
    [RestartRequired]
    public class ControlledExtractionOptions : SingletonOptions<ControlledExtractionOptions>
    {
        // Category constants for PLib grouping
        private const string CAT_OIL_WELL = "Oil Well Cap";
        private const string CAT_OIL_REFINERY = "Oil Refinery";
        private const string CAT_ETHANOL = "Ethanol Distillery";
        private const string CAT_COAL_GEN = "Coal Generator";
        private const string CAT_WOOD_GEN = "Wood Burner";
        private const string CAT_PETROL_GEN = "Petroleum Generator";
        private const string CAT_NATGAS_GEN = "Natural Gas Generator";

        // ========== Oil Well Cap ==========

        [Option("Default Water Rate (kg/s)",
            "Default water input rate for new Oil Well Caps.\nVanilla default is 1 kg/s.",
            CAT_OIL_WELL)]
        [Limit(0.01, 100)]
        [JsonProperty]
        public float DefaultWaterRate { get; set; } = 1f;

        [Option("Minimum Water Rate (kg/s)",
            "Minimum value for the water rate slider.\nAllows fine-tuning at low extraction rates.",
            CAT_OIL_WELL)]
        [Limit(0.001, 10)]
        [JsonProperty]
        public float MinWaterRate { get; set; } = 0.01f;

        [Option("Maximum Water Rate (kg/s)",
            "Maximum value for the water rate slider.\nNote: Liquid pipes max at 10 kg/s, so higher values require multiple inputs or accepting the pipe as bottleneck.",
            CAT_OIL_WELL)]
        [Limit(1, 1000)]
        [JsonProperty]
        public float MaxWaterRate { get; set; } = 100f;

        [Option("Backpressure Threshold (%)",
            "When gas pressure reaches this percentage, duplicants will come to release pressure.\nApplies to all Oil Well Caps.",
            CAT_OIL_WELL)]
        [Limit(0, 100)]
        [JsonProperty]
        public float BackpressureThreshold { get; set; } = 75f;

        [Option("Max Gas Storage (kg)",
            "Maximum gas pressure before overpressure.\nIncrease this if using high extraction rates to reduce venting frequency.\nVanilla: 80 kg",
            CAT_OIL_WELL)]
        [Limit(10, 10000)]
        [JsonProperty]
        public float MaxGasStorage { get; set; } = 80f;

        [Option("Max Oil Storage (kg)",
            "Maximum crude oil storage capacity.\nIncrease this if oil is backing up faster than pipes can handle.\nVanilla: ~50 kg (default storage)",
            CAT_OIL_WELL)]
        [Limit(50, 100000)]
        [JsonProperty]
        public float MaxOilStorage { get; set; } = 50f;

        [Option("Add Gas Output Port",
            "Adds a gas pipe output to Oil Well Caps for automatic venting.\nNo more duplicant labor needed!\nRequires game restart to take effect.",
            CAT_OIL_WELL)]
        [JsonProperty]
        public bool AddGasOutputPort { get; set; } = false;

        [Option("Add Liquid Output Port",
            "Adds a liquid pipe output to Oil Well Caps for direct oil extraction.\nNo more mopping or pumps needed!\nRequires game restart to take effect.",
            CAT_OIL_WELL)]
        [JsonProperty]
        public bool AddLiquidOutputPort { get; set; } = false;

        // ========== Oil Refinery ==========

        [Option("Add Methane Gas Output Port",
            "Adds a gas pipe output for Natural Gas (Methane) at position (-1, 3).\nRequires game restart.",
            CAT_OIL_REFINERY)]
        [JsonProperty]
        public bool OilRefineryMethanePort { get; set; } = false;

        // ========== Ethanol Distillery ==========

        [Option("Add CO2 Gas Output Port",
            "Adds a gas pipe output for Carbon Dioxide at position (2, 2).\nRequires game restart.",
            CAT_ETHANOL)]
        [JsonProperty]
        public bool EthanolCO2Port { get; set; } = false;

        [Option("Add Solid Output Port",
            "Adds a conveyor output for Polluted Dirt at position (0, 0).\nRequires game restart.",
            CAT_ETHANOL)]
        [JsonProperty]
        public bool EthanolSolidOutput { get; set; } = false;

        [Option("Add Solid Input Port",
            "Adds a conveyor input for Lumber at position (2, 0).\nRequires game restart.",
            CAT_ETHANOL)]
        [JsonProperty]
        public bool EthanolSolidInput { get; set; } = false;

        // ========== Coal Generator ==========

        [Option("Add CO2 Output Port",
            "Adds a gas pipe output for Carbon Dioxide at position (1, 1).\nRequires game restart.",
            CAT_COAL_GEN)]
        [JsonProperty]
        public bool CoalGenCO2Port { get; set; } = false;

        // ========== Wood Burner ==========

        [Option("Add CO2 Output Port",
            "Adds a gas pipe output for Carbon Dioxide at position (0, 1).\nRequires game restart.",
            CAT_WOOD_GEN)]
        [JsonProperty]
        public bool WoodGenCO2Port { get; set; } = false;

        // ========== Petroleum Generator ==========

        [Option("Add CO2 Output Port",
            "Adds a gas pipe output for Carbon Dioxide at position (0, 1).\nRequires game restart.",
            CAT_PETROL_GEN)]
        [JsonProperty]
        public bool PetrolGenCO2Port { get; set; } = false;

        [Option("Add Polluted Water Output Port",
            "Adds a liquid pipe output for Polluted Water at position (1, 1).\nRequires game restart.",
            CAT_PETROL_GEN)]
        [JsonProperty]
        public bool PetrolGenPWaterPort { get; set; } = false;

        // ========== Natural Gas Generator ==========

        [Option("Add Polluted Water Output Port",
            "Adds a liquid pipe output for Polluted Water at position (1, 1).\nRequires game restart.",
            CAT_NATGAS_GEN)]
        [JsonProperty]
        public bool NatGasGenPWaterPort { get; set; } = false;

        public ControlledExtractionOptions() { }
    }
}
