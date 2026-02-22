using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ControlledMods.Options
{
    /// <summary>
    /// Save File Fixes options only. Registered only when the Save File Fixes feature is enabled,
    /// so this section is hidden from the mod options UI when the feature is disabled.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile("ControlledMods.json", true, true)]
    [RestartRequired]
    public sealed class ControlledModsSaveFileFixOptions
    {
        [Option("Advanced Coolers", "Relocate this mod's config to the shared config folder.", "Save File Fixes")]
        [JsonProperty]
        public bool EnableSaveFileFix_AdvancedCoolers { get; set; }

        [Option("AI Improvements", "Relocate this mod's config to the shared config folder.", "Save File Fixes")]
        [JsonProperty]
        public bool EnableSaveFileFix_AiImprovements { get; set; }

        [Option("Customizable Speed", "Relocate this mod's config to the shared config folder.", "Save File Fixes")]
        [JsonProperty]
        public bool EnableSaveFileFix_CustomizableSpeed { get; set; }

        [Option("Default Building Settings", "Relocate this mod's config to the shared config folder.", "Save File Fixes")]
        [JsonProperty]
        public bool EnableSaveFileFix_DefaultBuildingSettings { get; set; }

        [Option("Drag Area More Visible", "Relocate this mod's config to the shared config folder.", "Save File Fixes")]
        [JsonProperty]
        public bool EnableSaveFileFix_DragAreaMoreVisible { get; set; }

        [Option("Drains", "Relocate this mod's config to the shared config folder.", "Save File Fixes")]
        [JsonProperty]
        public bool EnableSaveFileFix_Drains { get; set; }

        [Option("Critical Notifications Pauser (Notifications Pause)", "Relocate this mod's config to the shared config folder.", "Save File Fixes")]
        [JsonProperty]
        public bool EnableSaveFileFix_NotificationsPause { get; set; }

        [Option("Popup Control (Toast Control)", "Relocate this mod's config to the shared config folder.", "Save File Fixes")]
        [JsonProperty]
        public bool EnableSaveFileFix_ToastControl { get; set; }

        [Option("Wall Pumps", "Relocate this mod's config to the shared config folder.", "Save File Fixes")]
        [JsonProperty]
        public bool EnableSaveFileFix_WallPumps { get; set; }
    }
}
