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

        // ========== Resource Sensor (Berkay) ==========

        [Option("Apply fixes to Resource Sensor",
            "When enabled and Berkay's Resource Sensor mod is loaded, applies the fixes: range visualization clears on deselect, liquids and gases support, and sidescreen options.",
            "Resource Sensor")]
        [JsonProperty]
        public bool EnableResourceSensor { get; set; } = true;

        // ========== Add more mod option sections here ==========

        public ControlledModsOptions() { }

        /// <summary>Opens the Steam client to the Workshop page for the given published file ID (steam:// scheme).</summary>
        private static void OpenSteamWorkshop(ulong publishedFileId)
        {
            Application.OpenURL($"steam://url/CommunityFilePage/{publishedFileId}");
        }
    }
}
