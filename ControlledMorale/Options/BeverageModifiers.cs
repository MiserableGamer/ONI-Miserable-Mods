using Newtonsoft.Json;

namespace ControlledMorale
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class BeverageModifiers
    {
        // Skills
        [JsonProperty] public bool ConstructionEnabled { get; set; } = false;
        [JsonProperty] public int  ConstructionValue   { get; set; } = 0;

        [JsonProperty] public bool DiggingEnabled { get; set; } = false;
        [JsonProperty] public int  DiggingValue   { get; set; } = 0;

        [JsonProperty] public bool MachineryEnabled { get; set; } = false;
        [JsonProperty] public int  MachineryValue   { get; set; } = 0;

        [JsonProperty] public bool AthleticsEnabled { get; set; } = false;
        [JsonProperty] public int  AthleticsValue   { get; set; } = 0;

        [JsonProperty] public bool LearningEnabled { get; set; } = false;
        [JsonProperty] public int  LearningValue   { get; set; } = 0;

        [JsonProperty] public bool CookingEnabled { get; set; } = false;
        [JsonProperty] public int  CookingValue   { get; set; } = 0;

        [JsonProperty] public bool CaringEnabled { get; set; } = false;
        [JsonProperty] public int  CaringValue   { get; set; } = 0;

        [JsonProperty] public bool StrengthEnabled { get; set; } = false;
        [JsonProperty] public int  StrengthValue   { get; set; } = 0;

        [JsonProperty] public bool ArtEnabled { get; set; } = false;
        [JsonProperty] public int  ArtValue   { get; set; } = 0;

        [JsonProperty] public bool BotanistEnabled { get; set; } = false;
        [JsonProperty] public int  BotanistValue   { get; set; } = 0;

        [JsonProperty] public bool RanchingEnabled { get; set; } = false;
        [JsonProperty] public int  RanchingValue   { get; set; } = 0;

        [JsonProperty] public bool SpaceNavigationEnabled { get; set; } = false;
        [JsonProperty] public int  SpaceNavigationValue   { get; set; } = 0;

        // Morale & Health
        [JsonProperty] public bool QualityOfLifeEnabled { get; set; } = false;
        [JsonProperty] public int  QualityOfLifeValue   { get; set; } = 0;

        [JsonProperty] public bool GermResistanceEnabled { get; set; } = false;
        [JsonProperty] public int  GermResistanceValue   { get; set; } = 0;

        // Physical
        [JsonProperty] public bool CarryAmountEnabled { get; set; } = false;
        [JsonProperty] public int  CarryAmountValue   { get; set; } = 0;

        [JsonProperty] public bool SneezynessEnabled { get; set; } = false;
        [JsonProperty] public int  SneezynessValue   { get; set; } = 0;

        [JsonProperty] public bool DiseaseCureSpeedEnabled { get; set; } = false;
        [JsonProperty] public int  DiseaseCureSpeedValue   { get; set; } = 0;
    }
}
