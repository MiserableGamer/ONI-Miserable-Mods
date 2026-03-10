using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ControlledFramerate.Options
{
    public enum SmoothingLevel
    {
        [Option("Low", "Slow, gentle speed changes")]
        Low,
        [Option("Medium", "Balanced speed adjustment")]
        Medium,
        [Option("High", "Fast, responsive speed changes")]
        High
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class SaveSpeedProfile
    {
        [JsonProperty]
        public float SlowSpeed { get; set; } = 1f;

        [JsonProperty]
        public float MediumSpeed { get; set; } = 2f;

        [JsonProperty]
        public float FastSpeed { get; set; } = 3f;

        [JsonProperty]
        public float BenchmarkMaxFound { get; set; } = 0f;

        [JsonProperty]
        public string LastBenchmarkDate { get; set; } = "";

        [JsonProperty]
        public bool AdaptiveEnabled { get; set; } = false;

        [JsonProperty]
        public bool MonitorEnabled { get; set; } = false;
    }

    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile(POptions.CONFIG_FILE_NAME, false, true)]
    public sealed class ControlledFramerateOptions
    {
        private static ControlledFramerateOptions _instance;
        public static ControlledFramerateOptions Instance
        {
            get
            {
                if (_instance == null)
                {
                    try
                    {
                        string plibPath = POptions.GetConfigFilePath(typeof(ControlledFramerateOptions));
                        ConfigMigrationHelper.MigrateConfigFromFilePath(plibPath);
                    }
                    catch { }
                    _instance = SafeReadSettings() ?? new ControlledFramerateOptions();
                }
                return _instance;
            }
        }

        public static void Reload()
        {
            try
            {
                string plibPath = POptions.GetConfigFilePath(typeof(ControlledFramerateOptions));
                ConfigMigrationHelper.MigrateConfigFromFilePath(plibPath);
            }
            catch { /* ignore */ }
            _instance = SafeReadSettings() ?? new ControlledFramerateOptions();
        }

        public static void Save()
        {
            if (_instance == null) return;
            try
            {
                string path = ConfigMigrationHelper.GetCanonicalConfigPath(
                    POptions.GetConfigFilePath(typeof(ControlledFramerateOptions)));
                if (string.IsNullOrEmpty(path)) return;

                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string json = JsonConvert.SerializeObject(_instance, Formatting.Indented);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                ControlledFramerateMod.Log($"Failed to save config: {ex}");
            }
        }

        private static ControlledFramerateOptions SafeReadSettings()
        {
            ControlledFramerateOptions result = null;

            try
            {
                string path = ConfigMigrationHelper.GetCanonicalConfigPath(
                    POptions.GetConfigFilePath(typeof(ControlledFramerateOptions)));
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    if (!string.IsNullOrEmpty(json))
                        result = JsonConvert.DeserializeObject<ControlledFramerateOptions>(json);
                }
            }
            catch { }

            if (result == null)
            {
                try
                {
                    result = POptions.ReadSettings<ControlledFramerateOptions>();
                }
                catch { }
            }

            result?.SanitiseAfterLoad();
            return result;
        }

        internal void SanitiseAfterLoad()
        {
            if (SaveProfiles == null)
                SaveProfiles = new Dictionary<string, SaveSpeedProfile>();

            SlowSpeed = Clamp(SlowSpeed, 0.5f, 20f, 1f);
            MediumSpeed = Clamp(MediumSpeed, 0.5f, 20f, 2f);
            FastSpeed = Clamp(FastSpeed, 0.5f, 20f, 3f);

            DesiredFps = ClampInt(DesiredFps, 10, 120, 30);
            MinimumFps = ClampInt(MinimumFps, 5, 60, 20);
            if (MinimumFps >= DesiredFps)
                MinimumFps = Math.Max(5, DesiredFps - 5);

            BenchmarkMaxSpeed = Clamp(BenchmarkMaxSpeed, 3f, 30f, 10f);
            BenchmarkStepSize = Clamp(BenchmarkStepSize, 0.5f, 5f, 1f);
            AcceptableThreshold = ClampInt(AcceptableThreshold, 0, 50, 10);
            SaveIgnoreWindow = Clamp(SaveIgnoreWindow, 1f, 10f, 3f);
        }

        private static float Clamp(float value, float min, float max, float fallback)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return fallback;
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static int ClampInt(int value, int min, int max, int fallback)
        {
            if (value < min || value > max) return (value < min) ? min : max;
            return value;
        }

        [Option("Slow Speed", "Speed multiplier for the first (slow) button.")]
        [Limit(0.5, 20)]
        [JsonProperty]
        public float SlowSpeed { get; set; } = 1f;

        [Option("Medium Speed", "Speed multiplier for the second (medium) button.")]
        [Limit(0.5, 20)]
        [JsonProperty]
        public float MediumSpeed { get; set; } = 2f;

        [Option("Fast Speed", "Speed multiplier for the third (fast) button.")]
        [Limit(0.5, 20)]
        [JsonProperty]
        public float FastSpeed { get; set; } = 3f;

        [Option("Desired FPS", "Target FPS for adaptive mode and benchmark pass criteria.")]
        [Limit(10, 120)]
        [JsonProperty]
        public int DesiredFps { get; set; } = 30;

        [Option("Minimum FPS", "Hard floor — adaptive mode aggressively throttles below this.")]
        [Limit(5, 60)]
        [JsonProperty]
        public int MinimumFps { get; set; } = 20;

        [Option("Benchmark Max Speed", "Starting speed for the benchmark ramp-down test.")]
        [Limit(3, 30)]
        [JsonProperty]
        public float BenchmarkMaxSpeed { get; set; } = 10f;

        [Option("Benchmark Step Size", "Speed reduction per benchmark step.")]
        [Limit(0.5, 5)]
        [JsonProperty]
        public float BenchmarkStepSize { get; set; } = 1f;

        [Option("Acceptable FPS Threshold (%)", "How far below the target FPS is still acceptable. E.g. 10 means 10% below target counts as a pass (27 FPS passes for a 30 FPS target).")]
        [Limit(0, 50)]
        [JsonProperty]
        public int AcceptableThreshold { get; set; } = 10;

        [Option("Adjustment Smoothing", "How quickly adaptive mode changes speed.")]
        [JsonProperty]
        public SmoothingLevel AdjustmentSmoothing { get; set; } = SmoothingLevel.Medium;

        [Option("Save Ignore Window", "Seconds to freeze speed adjustments during and after autosave.")]
        [Limit(1, 10)]
        [JsonProperty]
        public float SaveIgnoreWindow { get; set; } = 3f;

        [JsonProperty]
        public Dictionary<string, SaveSpeedProfile> SaveProfiles { get; set; } = new Dictionary<string, SaveSpeedProfile>();

        public SaveSpeedProfile GetCurrentSaveProfile()
        {
            string saveName = SpeedStateManager.GetCurrentSaveName();
            if (string.IsNullOrEmpty(saveName)) return null;
            if (SaveProfiles == null) return null;
            SaveProfiles.TryGetValue(saveName, out var profile);
            return profile;
        }

        public void ApplyBenchmarkResults(float maxSpeed)
        {
            string saveName = SpeedStateManager.GetCurrentSaveName();

            float slow = 1f;
            float medium = Math.Max(1f, (float)Math.Floor(maxSpeed * 0.5f * 2f) / 2f);
            float fast = maxSpeed;

            if (medium >= fast) medium = Math.Max(1f, fast - 0.5f);

            SlowSpeed = slow;
            MediumSpeed = medium;
            FastSpeed = fast;

            if (!string.IsNullOrEmpty(saveName))
            {
                if (SaveProfiles == null)
                    SaveProfiles = new Dictionary<string, SaveSpeedProfile>();

                var profile = new SaveSpeedProfile
                {
                    SlowSpeed = slow,
                    MediumSpeed = medium,
                    FastSpeed = fast,
                    BenchmarkMaxFound = maxSpeed,
                    LastBenchmarkDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm")
                };
                SaveProfiles[saveName] = profile;

                while (SaveProfiles.Count > 50)
                {
                    string oldest = null;
                    System.DateTime oldestDate = System.DateTime.MaxValue;
                    foreach (var kvp in SaveProfiles)
                    {
                        if (System.DateTime.TryParse(kvp.Value.LastBenchmarkDate, out var date) && date < oldestDate)
                        {
                            oldestDate = date;
                            oldest = kvp.Key;
                        }
                    }
                    if (oldest != null) SaveProfiles.Remove(oldest);
                    else break;
                }
            }

            Save();
        }

        public ControlledFramerateOptions()
        {
        }
    }
}
