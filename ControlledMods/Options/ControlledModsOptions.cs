using System;
using System.IO;
using Newtonsoft.Json;
using PeterHan.PLib.Options;
using UnityEngine;

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
                return ReadFromCanonicalPath() ?? POptions.ReadSettings<ControlledModsOptions>() ?? new ControlledModsOptions();
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

        [Option("Workshop", "Open this mod's Steam Workshop page.", "KIN Underground Conduit")]
        [JsonIgnore]
        public Action<object> OpenKINUndergroundConduitWorkshop => _ => OpenSteamWorkshop(3347169088);

        // ========== Resource Sensor (Berkay) ==========

        [Option("Apply fixes to Resource Sensor",
            "When enabled and Berkay's Resource Sensor mod is loaded, applies the fixes: range visualization clears on deselect, liquids and gases support, and sidescreen options.",
            "Resource Sensor")]
        [JsonProperty]
        public bool EnableResourceSensor { get; set; } = true;

        [Option("Workshop", "Open this mod's Steam Workshop page.", "Resource Sensor")]
        [JsonIgnore]
        public Action<object> OpenResourceSensorWorkshop => _ => OpenSteamWorkshop(2911545239);

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

        [Option("Workshop", "Open this mod's Steam Workshop page.", "Free Resource Buildings")]
        [JsonIgnore]
        public Action<object> OpenFreeResourceBuildingsWorkshop => _ => OpenSteamWorkshop(2839006500);

        // ========== Customizable Plants ==========

        [Option("VineBranch max_age compatibility",
            "When enabled and the Customizable Plants mod is loaded, applies the max_age setting from Customizable Plants config to Vine Branch (ovagro). " +
            "E.g. max_age = 1 in Customizable Plants makes vine branches drop fruit immediately when harvest-ready, like other plants. Default off.",
            "Customizable Plants")]
        [JsonProperty]
        public bool EnableCustomizablePlantsVineBranchMaxAge { get; set; }

        [Option("Workshop", "Open this mod's Steam Workshop page.", "Customizable Plants")]
        [JsonIgnore]
        public Action<object> OpenCustomizablePlantsWorkshop => _ => OpenSteamWorkshop(1818145851);

        // ========== DuplicantRoomSensor (Pholith) ==========

        [Option("Enable Duplicant Room Sensor range compatibility",
            "When enabled and Pholith's DuplicantRoomSensor mod is loaded, adds a per-sensor range limit mode with side screen controls. " +
            "Range mode can optionally integrate with ShowRange visualization if that mod is installed.",
            "Duplicant Room Sensor")]
        [JsonProperty]
        public bool EnableDuplicantRoomSensorRangeCompatibility { get; set; } = true;

        [Option("Show range when sidescreen opens",
            "When enabled, the range overlay is displayed automatically when you open the Duplicant Room Sensor sidescreen (may cause a brief delay). " +
            "When disabled, the sidescreen opens immediately; use the \"Show Range\" button to display the overlay on demand.",
            "Duplicant Room Sensor")]
        [JsonProperty]
        public bool ShowRangeOnSidescreenOpen { get; set; }

        [Option("Workshop", "Open this mod's Steam Workshop page.", "Duplicant Room Sensor")]
        [JsonIgnore]
        public Action<object> OpenDuplicantRoomSensorWorkshop => _ => OpenSteamWorkshop(1921058858);

        // ========== Darkness Not Excluded Relit ==========

        [Option("Fix implied light bleeding through walls",
            "When enabled and Darkness Not Excluded Relit is loaded, implied lux smoothing is occlusion-aware. " +
            "Solid tiles and solid doors block fully, while mesh/airflow/pneumatic structures use built-in transmission values.",
            "Darkness Not Excluded Relit")]
        [JsonProperty]
        public bool EnableDarknessImpliedLightOcclusionFix { get; set; } = true;

        [Option("Minimum implied lux cutoff",
            "Implied lux below this value is treated as 0 to reduce faint glow artifacts.",
            "Darkness Not Excluded Relit")]
        [Limit(0, 200)]
        [JsonProperty]
        public int DarknessMinImpliedLuxCutoff { get; set; } = 1;

        [Option("Workshop", "Open this mod's Steam Workshop page.", "Darkness Not Excluded Relit")]
        [JsonIgnore]
        public Action<object> OpenDarknessNotExcludedRelitWorkshop => _ => OpenSteamWorkshop(3609476592);

        // ========== Signs, Tags and Ribbons (Pether) ==========

        [Option("Enable Signs, Tags and Ribbons compatibility",
            "When enabled and Pether's Signs, Tags and Ribbons mod is loaded, ControlledMods applies patches for that mod (e.g. adds extra small element tags such as Raw Natural Gas that appear in the Utilities build menu).",
            "Signs, Tags and Ribbons")]
        [JsonProperty]
        public bool EnableSignsTagsAndRibbons { get; set; }

        [Option("Workshop", "Open this mod's Steam Workshop page.", "Signs, Tags and Ribbons")]
        [JsonIgnore]
        public Action<object> OpenSignsTagsAndRibbonsWorkshop => _ => OpenSteamWorkshop(2883096049);

        // ========== Add more mod option sections here ==========
        // Save File Fixes section is in ControlledModsSaveFileFixOptions; that type is only registered when the feature is enabled.

        public ControlledModsOptions() { }

        /// <summary>Opens the Steam client to the Workshop page for the given published file ID (steam:// scheme).</summary>
        private static void OpenSteamWorkshop(ulong publishedFileId)
        {
            Application.OpenURL($"steam://url/CommunityFilePage/{publishedFileId}");
        }
    }
}
