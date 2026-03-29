using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ControlledMorale
{
    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile(POptions.CONFIG_FILE_NAME, false, true)]
    [ModInfo("https://github.com/MiserableGamer/ONI-Miserable-Mods")]
    public sealed class ControlledMoraleOptions
    {
        private static ControlledMoraleOptions _instance;

        public static ControlledMoraleOptions Instance
        {
            get
            {
                if (_instance == null)
                    _instance = POptions.ReadSettings<ControlledMoraleOptions>() ?? new ControlledMoraleOptions();
                return _instance;
            }
        }

        public static void Invalidate() => _instance = null;

        // ── Per-beverage modifier data (serialized) ───────────────────────

        [JsonProperty] public BeverageModifiers Beer    { get; set; } = new BeverageModifiers
            { QualityOfLifeEnabled = true, QualityOfLifeValue = 2, AthleticsEnabled = true, AthleticsValue = -2 };
        [JsonProperty] public BeverageModifiers Wine    { get; set; } = new BeverageModifiers
            { QualityOfLifeEnabled = true, QualityOfLifeValue = 4, AthleticsEnabled = true, AthleticsValue = -4 };
        [JsonProperty] public BeverageModifiers Spirits { get; set; } = new BeverageModifiers
            { QualityOfLifeEnabled = true, QualityOfLifeValue = 6, AthleticsEnabled = true, AthleticsValue = -6 };

        // ── Options screen buttons (not serialized) ───────────────────────

        [Option("Beer Modifiers", "Open modifier settings for Beer.")]
        public System.Action<object> BeerButton =>
            _ => BeverageOptionsDialog.Show("Beer Modifiers", Beer);

        [Option("Wine Modifiers", "Open modifier settings for Wine.")]
        public System.Action<object> WineButton =>
            _ => BeverageOptionsDialog.Show("Wine Modifiers", Wine);

        [Option("Spirits Modifiers", "Open modifier settings for Spirits.")]
        public System.Action<object> SpiritsButton =>
            _ => BeverageOptionsDialog.Show("Spirits Modifiers", Spirits);
    }
}
