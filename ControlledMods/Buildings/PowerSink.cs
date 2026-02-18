using KSerialization;
using UnityEngine;

namespace ControlledMods.Buildings
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class PowerSink : KMonoBehaviour, ISliderControl, ISingleSliderControl, ISim200ms
    {
        public const float MAX_CONSUMPTION = 40000f;

        public static readonly Color SINK_TINT = new Color(1f, 0.35f, 0.35f, 1f);

        [Serialize]
        [SerializeField]
        public float currentConsumption = PowerSinkConfig.DEFAULT_WATTAGE;

        [MyCmpGet]
        private EnergyConsumer energyConsumer;

        [MyCmpGet]
        private Operational operational;

        [MyCmpGet]
        private KBatchedAnimController kbac;

        [MyCmpAdd]
        private CopyBuildingSettings copyBuildingSettings;

        private bool tintApplied;

        private static readonly EventSystem.IntraObjectHandler<PowerSink> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<PowerSink>(
                (component, data) => component.OnCopySettings(data));

        public override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            ApplyTint();

            if (energyConsumer != null)
                energyConsumer.BaseWattageRating = currentConsumption;
        }

        public void Sim200ms(float dt)
        {
            if (operational != null)
                operational.SetActive(operational.IsOperational);

            if (energyConsumer != null)
                energyConsumer.BaseWattageRating = currentConsumption;

            if (!tintApplied)
                ApplyTint();
        }

        private void ApplyTint()
        {
            if (kbac != null)
            {
                kbac.TintColour = SINK_TINT;
                tintApplied = true;
            }
        }

        private void OnCopySettings(object data)
        {
            var other = ((GameObject)data).GetComponent<PowerSink>();
            if (other != null)
                currentConsumption = other.currentConsumption;
        }

        // ISliderControl
        public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.CONTROLLEDMODS_POWERSINK.TITLE";
        public string SliderUnits => STRINGS.UI.UNITSUFFIXES.ELECTRICAL.WATT;

        public int SliderDecimalPlaces(int index) => 0;
        public float GetSliderMin(int index) => 0f;
        public float GetSliderMax(int index) => MAX_CONSUMPTION;
        public float GetSliderValue(int index) => currentConsumption;

        public void SetSliderValue(float value, int index)
        {
            currentConsumption = value;
            if (energyConsumer != null)
                energyConsumer.BaseWattageRating = value;
        }

        public string GetSliderTooltipKey(int index) =>
            "STRINGS.UI.UISIDESCREENS.CONTROLLEDMODS_POWERSINK.TOOLTIP";

        public string GetSliderTooltip(int index) =>
            "Adjust how much power this building consumes";
    }
}
