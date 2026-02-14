using System;
using System.IO;
using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ControlledWarnings.Options
{
    [JsonObject(MemberSerialization.OptIn)]
    [ModInfo("Controlled Warnings", "https://github.com/MiserableGamer/ONI-Mods")]
    [ConfigFile(SharedConfigLocation: true)]
    public class ControlledWarningsOptions
    {
        private static ControlledWarningsOptions _instance;
        public static ControlledWarningsOptions Instance
        {
            get
            {
                if (_instance == null)
                {
                    ConfigMigrationHelper.MigrateConfigFromFilePath(
                        POptions.GetConfigFilePath(typeof(ControlledWarningsOptions)));
                    _instance = SafeReadSettings() ?? new ControlledWarningsOptions();
                }
                return _instance;
            }
        }

        public static void Reload()
        {
            ConfigMigrationHelper.MigrateConfigFromFilePath(
                POptions.GetConfigFilePath(typeof(ControlledWarningsOptions)));
            _instance = SafeReadSettings() ?? new ControlledWarningsOptions();
        }

        private static ControlledWarningsOptions SafeReadSettings()
        {
            try
            {
                string canonical = ConfigMigrationHelper.GetCanonicalConfigPath(
                    POptions.GetConfigFilePath(typeof(ControlledWarningsOptions)));
                if (!string.IsNullOrEmpty(canonical) && File.Exists(canonical))
                {
                    string json = File.ReadAllText(canonical);
                    return JsonConvert.DeserializeObject<ControlledWarningsOptions>(json);
                }
                return POptions.ReadSettings<ControlledWarningsOptions>();
            }
            catch (Exception) { return null; }
        }

        [Option("Enable Trapped Alerts", "Show notification when a duplicant is trapped", "Trapped Duplicant")]
        [JsonProperty]
        public bool EnableTrappedAlert { get; set; } = true;

        [Option("Persistent Alert", "Alert stays until duplicant is freed or dismissed", "Trapped Duplicant")]
        [JsonProperty]
        public bool TrappedPersistent { get; set; } = true;

        [Option("Alert Cooldown (seconds)", "Minimum seconds before re-alerting for the same duplicant", "Trapped Duplicant")]
        [Limit(30, 600)]
        [JsonProperty]
        public float TrappedCooldown { get; set; } = 120f;

        [Option("Enable Critical Escalation", "Escalate to red alert if trapped duplicant is also suffocating", "Critical Alerts")]
        [JsonProperty]
        public bool EnableCriticalEscalation { get; set; } = true;

        [Option("Pause on Critical", "Pause the game when a trapped duplicant starts suffocating", "Critical Alerts")]
        [JsonProperty]
        public bool PauseOnCritical { get; set; } = false;
    }
}
