using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ControlledVisuals.Options
{
    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile(SharedConfigLocation: true)]
    [RestartRequired]
    public sealed class ControlledVisualsOptions : SingletonOptions<ControlledVisualsOptions>
    {
        [Option("STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.NAME",
                "STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.TOOLTIP",
                "STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CATEGORY_VISUAL")]
        [JsonProperty]
        public ConduitAnimationQuality ConduitAnimation { get; set; }

        public ControlledVisualsOptions()
        {
            ConduitAnimation = ConduitAnimationQuality.Full;
        }

        public enum ConduitAnimationQuality
        {
            [Option("STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.FULL",
                    "STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.FULL_TOOLTIP")]
            Full,

            [Option("STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.REDUCED",
                    "STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.REDUCED_TOOLTIP")]
            Reduced,

            [Option("STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.MINIMAL",
                    "STRINGS.UI.CONTROLLEDVISUALS.OPTIONS.CONDUITANIMATION.MINIMAL_TOOLTIP")]
            Minimal
        }
    }
}
