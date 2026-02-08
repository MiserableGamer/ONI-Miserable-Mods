using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace AdvancedWattageSensor.Options
{
    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile(POptions.CONFIG_FILE_NAME, false, true)]
    public sealed class AdvancedWattageSensorOptions
    {
        private static AdvancedWattageSensorOptions _instance;
        public static AdvancedWattageSensorOptions Instance
        {
            get
            {
                if (_instance == null)
                    _instance = POptions.ReadSettings<AdvancedWattageSensorOptions>() ?? new AdvancedWattageSensorOptions();
                return _instance;
            }
        }

        [Option("Warning Threshold (%)", "Percentage of threshold wattage at which the monitor display turns red.\nFor example, 10% means the display turns red when usage reaches 90% of the threshold.", "Power Monitor")]
        [Limit(1, 50)]
        [JsonProperty]
        public int WarningPercent { get; set; } = 10;
    }
}
