using System;
using System.IO;
using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace SkillsAndStatsProgressFIXED
{
    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile(POptions.CONFIG_FILE_NAME, false, true)]
    [RestartRequired]
    public sealed class Config
    {
        private static Config _instance;
        public static Config Cfg
        {
            get
            {
                if (_instance == null)
                {
                    ConfigMigrationHelper.MigrateConfigFromFilePath(
                        POptions.GetConfigFilePath(typeof(Config)));
                    _instance = SafeReadSettings() ?? new Config();
                }
                return _instance;
            }
        }

        public static void Reload()
        {
            ConfigMigrationHelper.MigrateConfigFromFilePath(
                POptions.GetConfigFilePath(typeof(Config)));
            _instance = SafeReadSettings() ?? new Config();
        }

        private static Config SafeReadSettings()
        {
            try
            {
                string canonical = ConfigMigrationHelper.GetCanonicalConfigPath(
                    POptions.GetConfigFilePath(typeof(Config)));
                if (!string.IsNullOrEmpty(canonical) && File.Exists(canonical))
                {
                    string json = File.ReadAllText(canonical);
                    return JsonConvert.DeserializeObject<Config>(json);
                }
                return POptions.ReadSettings<Config>();
            }
            catch (Exception) { return null; }
        }

        #region Display Options

        [Option("Highlight Changing Stats",
            "Highlight attributes that are currently gaining experience.",
            "Display")]
        [JsonProperty]
        public bool EnabledFirstFeature { get; set; } = true;

        [Option("Show Max XP for Skill",
            "Show the maximum experience needed alongside current skill XP.",
            "Display")]
        [JsonProperty]
        public bool ShowMaxExpForSkill { get; set; } = true;

        [Option("Show Max XP for Stats",
            "Show the maximum experience needed alongside current stat XP.",
            "Display")]
        [JsonProperty]
        public bool ShowMaxExpForStats { get; set; } = true;

        [Option("Show Required XP",
            "Show required XP to next level instead of current XP total.",
            "Display")]
        [JsonProperty]
        public bool ShowRequiredXp { get; set; } = true;

        [Option("Shrink Stat Name (chars)",
            "Truncate stat names to this many characters. 0 = no truncation.",
            "Display")]
        [Limit(0, 20)]
        [JsonProperty]
        public int ShrinkStatNameToXchar { get; set; } = 0;

        [Option("Alter Tab Sort Order",
            "Switch to the stats tab when selecting a duplicant.",
            "Display")]
        [JsonProperty]
        public bool AlterSortOrder { get; set; } = false;

        [Option("High Precision XP Values",
            "Show XP values with full decimal precision. When off (default), values are rounded to whole numbers for cleaner display.",
            "Display")]
        [JsonProperty]
        public bool HighPrecisionXP { get; set; } = false;

        #endregion

        #region Speed & Travel

        [Option("Show Actual Speed",
            "Display the duplicant's current movement speed and position.",
            "Speed & Travel")]
        [JsonProperty]
        public bool ShowActualSpeed { get; set; } = true;

        [Option("Average Speed Interval",
            "Time window in seconds for average speed calculation. Negative = disabled.",
            "Speed & Travel")]
        [JsonProperty]
        public float AvgSpeedInterval { get; set; } = 30f;

        [Option("Show Travel Distance",
            "Display distance traveled by navigation type.",
            "Speed & Travel")]
        [JsonProperty]
        public bool ShowTravelPath { get; set; } = true;

        #endregion

        #region Tracking

        [Option("Enable Delta Tracking",
            "Track and display XP changes over time intervals.",
            "Tracking")]
        [JsonProperty]
        public bool EnableComplexFeature { get; set; } = false;

        [Option("Tracking Interval (seconds)",
            "How often to snapshot XP values for delta calculation.",
            "Tracking")]
        [Limit(60, 6000)]
        [JsonProperty]
        public int IntervalSecond { get; set; } = 600;

        [Option("Sample Rate (seconds)",
            "How frequently to poll XP values within each interval.",
            "Tracking")]
        [Limit(1, 60)]
        [JsonProperty]
        public int GetEveryXSecond { get; set; } = 5;

        [Option("Show Debug Tracking Info",
            "Display internal tracking counters (for troubleshooting).",
            "Tracking")]
        [JsonProperty]
        public bool EnableAdditionalInfo { get; set; } = false;

        #endregion

        #region Radiation

        [Option("Show Radiation Info",
            "Display radiation balance, recovery, exposure and resistance.",
            "Radiation")]
        [JsonProperty]
        public bool ShowRadiationInfo { get; set; } = true;

        #endregion

        #region Workable Info

        [Option("Show Workable Info",
            "Display pop-up text showing work efficiency on buildings.",
            "Workable Info")]
        [JsonProperty]
        public bool ShowWorkableInfo { get; set; } = false;

        [Option("Selected Dupe Only",
            "Only show workable info for the currently selected duplicant.",
            "Workable Info")]
        [JsonProperty]
        public bool ShowWorkableOnlyForSelectedDuplicant { get; set; } = true;

        [Option("Only Show Result",
            "Only show the final result report, not the in-progress report.",
            "Workable Info")]
        [JsonProperty]
        public bool WorkableShowOnlyResultReport { get; set; } = false;

        [Option("In-Progress Report Speed",
            "Speed at which the in-progress pop-up text rises.",
            "Workable Info")]
        [Limit(0.01, 1.0)]
        [JsonProperty]
        public float WorkableInfoReport1Speed { get; set; } = 0.1f;

        [Option("Result Report Speed",
            "Speed at which the result pop-up text rises.",
            "Workable Info")]
        [Limit(0.01, 1.0)]
        [JsonProperty]
        public float WorkableInfoReport2Speed { get; set; } = 0.1f;

        [Option("In-Progress Report Duration",
            "How long the in-progress pop-up text stays visible (seconds).",
            "Workable Info")]
        [Limit(1.0, 60.0)]
        [JsonProperty]
        public float WorkableInfoReport1Time { get; set; } = 10f;

        [Option("Result Report Duration",
            "How long the result pop-up text stays visible (seconds).",
            "Workable Info")]
        [Limit(1.0, 60.0)]
        [JsonProperty]
        public float WorkableInfoReport2Time { get; set; } = 10f;

        [Option("Report Font Size",
            "Font size for workable info pop-up text.",
            "Workable Info")]
        [Limit(10.0, 60.0)]
        [JsonProperty]
        public float WorkableReportFontSize { get; set; } = 30f;

        #endregion

        #region Advanced

        [Option("Debug Logging",
            "Enable debug logging to Player.log.",
            "Advanced")]
        [JsonProperty]
        public bool DebugInfo { get; set; } = false;

        #endregion

        // Colors can't be edited via PLib Options UI, so keep reasonable defaults
        // and don't expose them (users can edit config.json directly if needed)
        [JsonProperty]
        public float[] WorkableInfoReport1Color { get; set; } = new float[] { 0f, 1f, 0f, 1f };

        [JsonProperty]
        public float[] WorkableInfoReport2Color { get; set; } = new float[] { 0f, 1f, 1f, 1f };

        // Helper to get UnityEngine.Color from the float arrays
        public UnityEngine.Color GetReport1Color()
        {
            var c = WorkableInfoReport1Color;
            return c != null && c.Length >= 4
                ? new UnityEngine.Color(c[0], c[1], c[2], c[3])
                : UnityEngine.Color.green;
        }

        public UnityEngine.Color GetReport2Color()
        {
            var c = WorkableInfoReport2Color;
            return c != null && c.Length >= 4
                ? new UnityEngine.Color(c[0], c[1], c[2], c[3])
                : UnityEngine.Color.cyan;
        }
    }
}
