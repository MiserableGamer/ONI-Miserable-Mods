using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace MorningExercise.Options
{
    [JsonObject(MemberSerialization.OptIn)]
    [ModInfo("Morning Exercise", "https://github.com/MiserableGamer/ONI-Miserable-Mods")]
    [ConfigFile(SharedConfigLocation: true)]
    [RestartRequired]
    public class MorningExerciseOptions
    {
        // Standard Duplicants section
        [JsonProperty]
        [Option("Enable Exercise", "Whether standard Duplicants are allowed to exercise", "1. Dupes")]
        public bool EnableStandardExercise { get; set; } = true;

        [JsonProperty]
        [Option("Exercise Duration (seconds)", "How long duplicants exercise on the Manual Exerciser", "1. Dupes")]
        [Limit(5, 120)]
        public int ExerciseDuration { get; set; } = 20;

        [JsonProperty]
        [Option("Buff Duration (seconds)", "How long the Warm Up buff lasts for regular Duplicants after exercising", "1. Dupes")]
        [Limit(60, 999)]
        public int BuffDuration { get; set; } = 360;

        [JsonProperty]
        [Option("Athletics Bonus", "The Athletics bonus granted by the Warm Up buff for regular Duplicants", "1. Dupes")]
        [Limit(1, 10)]
        public int AthleticsBonus { get; set; } = 3;

        // Bionic Duplicants section
        [JsonProperty]
        [Option("Enable Exercise", "Whether Bionic Duplicants are allowed to exercise", "2. Boops")]
        public bool EnableBionicExercise { get; set; } = true;

        [JsonProperty]
        [Option("Buff Duration (seconds)", "How long the Warm Up buff lasts for Bionic Duplicants after exercising", "2. Boops")]
        [Limit(60, 999)]
        public int BionicBuffDuration { get; set; } = 720;

        [JsonProperty]
        [Option("Morale Bonus", "The Morale bonus granted by the Warm Up buff for Bionic Duplicants", "2. Boops")]
        [Limit(1, 10)]
        public int BionicMoraleBonus { get; set; } = 2;

        // Experience Settings section
        [JsonProperty]
        [Option("Enable Experience Gain", "Whether exercising grants permanent Athletics attribute experience (like Manual Generator)", "Experience Settings")]
        public bool EnableExperienceGain { get; set; } = true;

        [JsonProperty]
        [Option("Experience Multiplier", "Multiplier for Athletics experience gained per second of exercise (1 = same as Manual Generator)", "Experience Settings")]
        [Limit(0, 10)]
        public int ExperienceMultiplier { get; set; } = 1;

        // Visual Settings section
        [JsonProperty]
        [Option("Animation Variant", "Which animation variant to use for the Manual Exerciser building", "Visual Settings")]
        public AnimationVariant AnimationVariant { get; set; } = AnimationVariant.VariantA;

        public MorningExerciseOptions()
        {
            EnableStandardExercise = true;
            ExerciseDuration = 20;
            BuffDuration = 360;
            EnableBionicExercise = true;
            BionicBuffDuration = 720;
            BionicMoraleBonus = 2;
            EnableExperienceGain = true;
            ExperienceMultiplier = 1;
            AnimationVariant = AnimationVariant.VariantA;
        }
    }

    public enum AnimationVariant
    {
        [Option("Standard", "Use the standard Manual Generator animation")]
        Standard = 0,
        [Option("Variant A", "Use Variant A of the Manual Generator animation")]
        VariantA = 1,
        [Option("Variant B", "Use Variant B of the Manual Generator animation")]
        VariantB = 2
    }
}
