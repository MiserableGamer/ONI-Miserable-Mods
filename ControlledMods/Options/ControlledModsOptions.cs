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
                if (_instance == null)
                {
                    _instance = POptions.ReadSettings<ControlledModsOptions>() ?? new ControlledModsOptions();
                    // Options missing from the config file (e.g. after an update) default to off
                    DefaultMissingOptionsToOff(_instance);
                }
                return _instance;
            }
        }

        // When config file exists but is from an older version, any option not present in the file is treated as disabled.
        private static void DefaultMissingOptionsToOff(ControlledModsOptions instance)
        {
            string path = null;
            try
            {
                path = POptions.GetConfigFilePath(typeof(ControlledModsOptions));
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
            foreach (var prop in typeof(ControlledModsOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.PropertyType != typeof(bool) || !prop.CanWrite)
                    continue;
                var jsonAttr = prop.GetCustomAttribute<JsonPropertyAttribute>();
                string key = jsonAttr?.PropertyName ?? prop.Name;
                if (string.IsNullOrEmpty(key))
                    key = prop.Name;
                if (jo[key] == null)
                    prop.SetValue(instance, false);
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

        // ========== Add more mod option sections here ==========

        public ControlledModsOptions() { }

        /// <summary>Opens the Steam client to the Workshop page for the given published file ID (steam:// scheme).</summary>
        private static void OpenSteamWorkshop(ulong publishedFileId)
        {
            Application.OpenURL($"steam://url/CommunityFilePage/{publishedFileId}");
        }
    }
}
