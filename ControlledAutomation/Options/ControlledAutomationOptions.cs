using PeterHan.PLib.Options;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ControlledAutomation.Options
{
    [JsonObject(MemberSerialization.OptIn)]
    [ModInfo("https://github.com/MiserableGamer/ONI-Miserable-Mods")]
    [ConfigFile(SharedConfigLocation: true)]
    public sealed class ControlledAutomationOptions : SingletonOptions<ControlledAutomationOptions>, IOptions
    {
        [Option("Enable Storage Thresholds", "Adds high/low threshold sliders to storage buildings.", "Features")]
        [JsonProperty]
        public bool EnableStorageThresholds { get; set; } = true;

        [Option("Enable Automation Inversion", "Adds option to invert automation output signals.", "Features")]
        [JsonProperty]
        public bool EnableAutomationInversion { get; set; } = true;

        public void OnOptionsChanged() => Instance = (ControlledAutomationOptions)MemberwiseClone();
        public IEnumerable<IOptionsEntry> CreateOptions() => null;
    }
}
