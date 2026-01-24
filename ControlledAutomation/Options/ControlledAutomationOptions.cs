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
        [Option("Enable Storage Thresholds", "Adds high/low threshold sliders to Smart Storage, Reservoirs, Smart Battery, Refrigerator, and Radbolt Chamber.", "Features")]
        [JsonProperty]
        public bool EnableStorageThresholds { get; set; } = true;

        [Option("Enable Automation Inversion", "Adds option to invert automation output signals on supported buildings and sensors.", "Features")]
        [JsonProperty]
        public bool EnableAutomationInversion { get; set; } = true;

        [Option("Reduced Smart Storage Power", "Reduces Smart Storage Bin required power to 20W (from 60W).", "Tweaks")]
        [JsonProperty]
        public bool ReducedSmartStoragePower { get; set; } = false;

        public override string ToString()
        {
            return $"ControlledAutomationOptions[thresholds={EnableStorageThresholds}, inversion={EnableAutomationInversion}, reducedPower={ReducedSmartStoragePower}]";
        }

        public void OnOptionsChanged()
        {
            Instance = (ControlledAutomationOptions)this.MemberwiseClone();
        }

        public IEnumerable<IOptionsEntry> CreateOptions()
        {
            return null;
        }
    }
}
