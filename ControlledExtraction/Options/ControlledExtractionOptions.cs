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
        private const string CAT_OIL_WELL = "Oil Well Cap";
        private const string CAT_OIL_REFINERY = "Oil Refinery";
        private const string CAT_ETHANOL = "Ethanol Distillery";
        private const string CAT_COAL_GEN = "Coal Generator";
        private const string CAT_WOOD_GEN = "Wood Burner";
        private const string CAT_PETROL_GEN = "Petroleum Generator";
        private const string CAT_NATGAS_GEN = "Natural Gas Generator";

        // Oil Well Cap
        [Option("Default Water Rate (kg/s)", "Default water input rate for new wells.\nVanilla: 1 kg/s", CAT_OIL_WELL)]
        [Limit(0.01, 100)]
        [JsonProperty]
        public float DefaultWaterRate { get; set; } = 1f;

        [Option("Minimum Water Rate (kg/s)", "Minimum slider value.", CAT_OIL_WELL)]
        [Limit(0.001, 10)]
        [JsonProperty]
        public float MinWaterRate { get; set; } = 0.01f;

        [Option("Maximum Water Rate (kg/s)", "Maximum slider value.\nNote: Pipes max at 10 kg/s.", CAT_OIL_WELL)]
        [Limit(1, 1000)]
        [JsonProperty]
        public float MaxWaterRate { get; set; } = 100f;

        [Option("Backpressure Threshold (%)", "When dupes come to vent pressure.", CAT_OIL_WELL)]
        [Limit(0, 100)]
        [JsonProperty]
        public float BackpressureThreshold { get; set; } = 75f;

        [Option("Max Gas Storage (kg)", "Gas capacity before overpressure.\nVanilla: 80 kg", CAT_OIL_WELL)]
        [Limit(10, 10000)]
        [JsonProperty]
        public float MaxGasStorage { get; set; } = 80f;

        [Option("Base Oil Storage (kg)", "Base oil buffer capacity.\nScales with extraction rate to prevent overflow.\nVanilla: 50 kg", CAT_OIL_WELL)]
        [Limit(50, 100000)]
        [JsonProperty]
        public float MaxOilStorage { get; set; } = 50f;

        [Option("Add Gas Output Port", "Adds gas pipe output for automatic venting.\nRequires restart.", CAT_OIL_WELL)]
        [JsonProperty]
        public bool AddGasOutputPort { get; set; } = false;

        [Option("Add Liquid Output Port", "Adds liquid pipe output for direct oil extraction.\nAuto-disabled if Ronivan's Legacy is detected (it adds its own).\nRequires restart.", CAT_OIL_WELL)]
        [JsonProperty]
        public bool AddLiquidOutputPort { get; set; } = false;

        // Oil Refinery
        [Option("Add Methane Gas Output Port", "Gas output at (-1, 3).\nRequires restart.", CAT_OIL_REFINERY)]
        [JsonProperty]
        public bool OilRefineryMethanePort { get; set; } = false;

        // Ethanol Distillery
        [Option("Add CO2 Gas Output Port", "Gas output at (2, 2).\nRequires restart.", CAT_ETHANOL)]
        [JsonProperty]
        public bool EthanolCO2Port { get; set; } = false;

        [Option("Add Solid Output Port", "Conveyor output for Polluted Dirt at (0, 0).\nRequires restart.", CAT_ETHANOL)]
        [JsonProperty]
        public bool EthanolSolidOutput { get; set; } = false;

        [Option("Add Solid Input Port", "Conveyor input for Lumber at (2, 0).\nRequires restart.", CAT_ETHANOL)]
        [JsonProperty]
        public bool EthanolSolidInput { get; set; } = false;

        // Coal Generator
        [Option("Add CO2 Output Port", "Gas output at (1, 1).\nRequires restart.", CAT_COAL_GEN)]
        [JsonProperty]
        public bool CoalGenCO2Port { get; set; } = false;

        [Option("Add Solid Input Port", "Conveyor input for Coal at (1, 0).\nRequires restart.", CAT_COAL_GEN)]
        [JsonProperty]
        public bool CoalGenSolidInput { get; set; } = false;

        // Wood Burner
        [Option("Add CO2 Output Port", "Gas output at (0, 1).\nRequires restart.", CAT_WOOD_GEN)]
        [JsonProperty]
        public bool WoodGenCO2Port { get; set; } = false;

        [Option("Add Solid Input Port", "Conveyor input for Lumber at (0, 0).\nRequires restart.", CAT_WOOD_GEN)]
        [JsonProperty]
        public bool WoodGenSolidInput { get; set; } = false;

        // Petroleum Generator
        [Option("Add CO2 Output Port", "Gas output at (0, 1).\nRequires restart.", CAT_PETROL_GEN)]
        [JsonProperty]
        public bool PetrolGenCO2Port { get; set; } = false;

        [Option("Add Polluted Water Output Port", "Liquid output at (1, 1).\nRequires restart.", CAT_PETROL_GEN)]
        [JsonProperty]
        public bool PetrolGenPWaterPort { get; set; } = false;

        // Natural Gas Generator
        [Option("Add Polluted Water Output Port", "Liquid output at (1, 1).\nRequires restart.", CAT_NATGAS_GEN)]
        [JsonProperty]
        public bool NatGasGenPWaterPort { get; set; } = false;

        public ControlledExtractionOptions() { }
    }
}
