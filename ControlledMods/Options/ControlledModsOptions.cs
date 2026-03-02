using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PeterHan.PLib.Options;

namespace ControlledMods.Options
{
    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile("ControlledMods.json", true, true)]
    [RestartRequired]
    [ModInfo("https://github.com/MiserableGamer/ONI-Miserable-Mods", collapse: true)]
    public sealed class ControlledModsOptions
    {
        public static ControlledModsOptions Instance
        {
            get
            {
                try
                {
                    string plibPath = POptions.GetConfigFilePath(typeof(ControlledModsOptions));
                    ConfigMigrationHelper.MigrateConfigFromFilePath(plibPath);
                }
                catch { /* ignore */ }
                var instance = ReadFromCanonicalPath() ?? POptions.ReadSettings<ControlledModsOptions>() ?? new ControlledModsOptions();
                DefaultMissingOptionsToOff(instance);
                return instance;
            }
        }

        /// <summary>Read options from the canonical config path (ControlledMods folder, not ControlledMods.dll)
        /// so we never create or use the .dll folder.</summary>
        private static ControlledModsOptions ReadFromCanonicalPath()
        {
            string path = null;
            try
            {
                path = ConfigMigrationHelper.GetCanonicalConfigPath(POptions.GetConfigFilePath(typeof(ControlledModsOptions)));
            }
            catch { /* no path */ }
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;
            try
            {
                return JsonConvert.DeserializeObject<ControlledModsOptions>(File.ReadAllText(path));
            }
            catch
            {
                return null;
            }
        }

        // When config file exists but is from an older version, any option not present in the file is treated as disabled.
        // We merge missing keys into the existing file (instead of overwriting) so we never wipe values PLib may have
        // saved under different key names (e.g. after the user enables an option and the game restarts).
        // All reads/writes use the canonical path (ControlledMods, not ControlledMods.dll) so the .dll folder is never created.
        private static void DefaultMissingOptionsToOff(ControlledModsOptions instance)
        {
            string path = null;
            try
            {
                path = ConfigMigrationHelper.GetCanonicalConfigPath(POptions.GetConfigFilePath(typeof(ControlledModsOptions)));
            }
            catch { /* no path */ }
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;
            JObject jo;
            try
            {
                jo = JObject.Parse(File.ReadAllText(path));
            }
            catch
            {
                return;
            }
            bool anyMissing = false;
            foreach (var prop in typeof(ControlledModsOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.PropertyType != typeof(bool) || !prop.CanWrite)
                    continue;
                var jsonAttr = prop.GetCustomAttribute<JsonPropertyAttribute>();
                string key = jsonAttr?.PropertyName ?? prop.Name;
                if (string.IsNullOrEmpty(key))
                    key = prop.Name;
                if (jo[key] == null)
                {
                    prop.SetValue(instance, false);
                    jo[key] = false;
                    anyMissing = true;
                }
            }
            if (anyMissing)
            {
                try
                {
                    var dir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    File.WriteAllText(path, JsonConvert.SerializeObject(jo, Formatting.Indented));
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"[ControlledMods] Could not write options after adding missing keys: {ex.Message}");
                }
            }
        }

        // ========== KIN Underground Conduit ==========

        [Option("Fix Power Terminal + logic wire crash",
            "When enabled, prevents a crash when a logic wire is built in the same cell as an Underground Conduit Power Terminal. " +
            "Requires KIN Underground Conduit mod. Disable only if you do not use power terminals or want to risk the crash.",
            "KIN Underground Conduit")]
        [JsonProperty]
        public bool FixPowerTerminalLogicWireCrash { get; set; } = true;

        [Option("Enable Copy Settings for conduit terminals/senders/receivers",
            "When enabled, the vanilla Copy Settings tool works for Power/Logic/Liquid/Gas/Solid/Radbolt terminals and senders/receivers (channel is copied). " +
            "Requires KIN Underground Conduit mod.",
            "KIN Underground Conduit")]
        [JsonProperty]
        public bool EnableCopySettingsForConduits { get; set; } = true;

        [Option("Tint Logic Terminal light by signal",
            "When enabled, the light on the Logic Terminal when a channel is selected reflects the logic output: green signal = standard light, red signal = light tinted red. " +
            "Requires KIN Underground Conduit mod.",
            "KIN Underground Conduit")]
        [JsonProperty]
        public bool TintLogicTerminalLight { get; set; } = true;

        // ========== Resource Sensor (Berkay) ==========

        [Option("Apply fixes to Resource Sensor",
            "When enabled and Berkay's Resource Sensor mod is loaded, applies the fixes: range visualization clears on deselect, liquids and gases support, and sidescreen options.",
            "Resource Sensor")]
        [JsonProperty]
        public bool EnableResourceSensor { get; set; } = true;

        // ========== Free Resource Buildings (castrolol) ==========

        [Option("Fix Free Energy Generator wattage slider",
            "When enabled and the Free Resource Buildings mod is loaded, the wattage slider on the Free Energy Generator actually controls power output (vanilla bug: slider had no effect).",
            "Free Resource Buildings")]
        [JsonProperty]
        public bool FixFreeEnergyGeneratorSlider { get; set; } = true;

        [Option("Add Power Sink building",
            "When enabled and the Free Resource Buildings mod is loaded, adds a Power Sink building (the reverse of the Power Box) that consumes power at a configurable rate via a slider. " +
            "Useful for testing power systems. Found in the Power category of the build menu.",
            "Free Resource Buildings")]
        [JsonProperty]
        public bool AddPowerSinkBuilding { get; set; } = true;

        // ========== Customizable Plants ==========

        [Option("VineBranch max_age compatibility",
            "When enabled and the Customizable Plants mod is loaded, applies the max_age setting from Customizable Plants config to Vine Branch (ovagro). " +
            "E.g. max_age = 1 in Customizable Plants makes vine branches drop fruit immediately when harvest-ready, like other plants. Default off.",
            "Customizable Plants")]
        [JsonProperty]
        public bool EnableCustomizablePlantsVineBranchMaxAge { get; set; }

        // ========== DuplicantRoomSensor (Pholith) ==========

        [Option("Enable Duplicant Room Sensor range compatibility",
            "When enabled and Pholith's DuplicantRoomSensor mod is loaded, adds a per-sensor range limit mode with side screen controls. " +
            "Range mode can optionally integrate with ShowRange visualization if that mod is installed.",
            "Duplicant Room Sensor")]
        [JsonProperty]
        public bool EnableDuplicantRoomSensorRangeCompatibility { get; set; } = true;

        // ========== Add more mod option sections here ==========
        // Save File Fixes section is in ControlledModsSaveFileFixOptions; that type is only registered when the feature is enabled.

        public ControlledModsOptions() { }
    }
}
