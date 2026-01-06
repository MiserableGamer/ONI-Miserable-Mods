using Newtonsoft.Json;
using PeterHan.PLib.Options;
using ControlledMods.ModDetection;

namespace ControlledMods.Options
{
    // Main options class - all options at top level, grouped by category
    // Options are always visible; descriptions indicate if target mod is required
    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile("ControlledMods.json", true, true)]
    [RestartRequired]
    [ModInfo("https://github.com/MiserableGamer/ONI-Miserable-Mods", collapse: true)]
    public sealed class ControlledModsOptions
    {
        private static ControlledModsOptions _instance;
        public static ControlledModsOptions Instance
        {
            get
            {
                if (_instance == null)
                    _instance = POptions.ReadSettings<ControlledModsOptions>() ?? new ControlledModsOptions();
                return _instance;
            }
        }

        // Helper to get status text for descriptions
        public static string GetModStatus(bool isLoaded) => isLoaded ? "✓ Mod Detected" : "✗ Mod Not Detected";

        // ========== Ronivan's Legacy - Reservoirs ==========

        [Option("Medium Gas Reservoir Capacity (kg)",
            "Maximum storage capacity for the Medium Gas Reservoir.\n" +
            "Requires: Ronivan's Legacy mod\n" +
            "Default: 750 kg",
            "Ronivan's Legacy - Reservoirs",
            Format = "F0")]
        [Limit(750, 1000000)]
        [JsonProperty]
        public float MedGasReservoirCapacity { get; set; } = 750f;

        [Option("Medium Liquid Reservoir Capacity (kg)",
            "Maximum storage capacity for the Medium Liquid Reservoir.\n" +
            "Requires: Ronivan's Legacy mod\n" +
            "Default: 7500 kg",
            "Ronivan's Legacy - Reservoirs",
            Format = "F0")]
        [Limit(7500, 1000000)]
        [JsonProperty]
        public float MedLiquidReservoirCapacity { get; set; } = 7500f;

        [Option("Small Gas Reservoir Capacity (kg)",
            "Maximum storage capacity for the Small Gas Reservoir (floor and inverted variants).\n" +
            "Requires: Ronivan's Legacy mod\n" +
            "Default: 250 kg",
            "Ronivan's Legacy - Reservoirs",
            Format = "F0")]
        [Limit(250, 50000)]
        [JsonProperty]
        public float SmallGasReservoirCapacity { get; set; } = 250f;

        [Option("Small Liquid Reservoir Capacity (kg)",
            "Maximum storage capacity for the Small Liquid Reservoir (floor and inverted variants).\n" +
            "Requires: Ronivan's Legacy mod\n" +
            "Default: 2500 kg",
            "Ronivan's Legacy - Reservoirs",
            Format = "F0")]
        [Limit(2500, 50000)]
        [JsonProperty]
        public float SmallLiquidReservoirCapacity { get; set; } = 2500f;

        [Option("Wall Gas Tank Capacity (kg)",
            "Maximum storage capacity for the Wall Gas Tank.\n" +
            "Requires: Ronivan's Legacy mod\n" +
            "Default: 150 kg",
            "Ronivan's Legacy - Reservoirs",
            Format = "F0")]
        [Limit(150, 25000)]
        [JsonProperty]
        public float WallGasTankCapacity { get; set; } = 150f;

        [Option("Wall Liquid Tank Capacity (kg)",
            "Maximum storage capacity for the Wall Liquid Tank.\n" +
            "Requires: Ronivan's Legacy mod\n" +
            "Default: 1500 kg",
            "Ronivan's Legacy - Reservoirs",
            Format = "F0")]
        [Limit(1500, 25000)]
        [JsonProperty]
        public float WallLiquidTankCapacity { get; set; } = 1500f;

        // ========== Add more mod sections here ==========

        public ControlledModsOptions() { }
    }
}
