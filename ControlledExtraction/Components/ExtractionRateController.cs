using KSerialization;
using ControlledExtraction.Options;
using UnityEngine;

namespace ControlledExtraction.Components
{
    // Controls water input rate and scales oil/gas output proportionally
    [SerializationConfig(MemberSerialization.OptIn)]
    public class ExtractionRateController : KMonoBehaviour
    {
        public const float VANILLA_WATER_RATE = 1f;
        public const float VANILLA_OIL_RATIO = 3.3333333f;

        [Serialize]
        private float waterInputRate = -1f;

#pragma warning disable CS0649
        [MyCmpReq] private ElementConverter elementConverter;
        [MyCmpReq] private ConduitConsumer conduitConsumer;
#pragma warning restore CS0649

        private OilWellCap oilWellCap;
        private float baseGasRate;

        public float WaterInputRate
        {
            get => waterInputRate < 0 ? ControlledExtractionOptions.Instance.DefaultWaterRate : waterInputRate;
            set
            {
                float newValue = Mathf.Clamp(value,
                    ControlledExtractionOptions.Instance.MinWaterRate,
                    ControlledExtractionOptions.Instance.MaxWaterRate);
                
                if (Mathf.Approximately(newValue, waterInputRate)) return;
                
                waterInputRate = newValue;
                ApplyRates();
            }
        }

        public float ExtractionMultiplier => WaterInputRate / VANILLA_WATER_RATE;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe<ExtractionRateController>(-905833192, OnCopySettingsDelegate);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            oilWellCap = GetComponent<OilWellCap>();
            if (oilWellCap != null)
                baseGasRate = oilWellCap.addGasRate;
            
            ApplyRates();
        }

        private void ApplyRates()
        {
            if (elementConverter == null) return;

            float currentRate = WaterInputRate;
            float multiplier = ExtractionMultiplier;

            if (elementConverter.consumedElements?.Length > 0)
            {
                var consumed = elementConverter.consumedElements[0];
                consumed.MassConsumptionRate = currentRate;
                elementConverter.consumedElements[0] = consumed;
            }

            // Oil output (3.33:1 ratio with water)
            if (elementConverter.outputElements?.Length > 0)
            {
                var output = elementConverter.outputElements[0];
                output.massGenerationRate = currentRate * VANILLA_OIL_RATIO;
                elementConverter.outputElements[0] = output;
            }

            if (conduitConsumer != null)
            {
                conduitConsumer.consumptionRate = Mathf.Max(2f, currentRate * 2f);
                conduitConsumer.capacityKG = Mathf.Max(10f, currentRate * 10f);
            }

            if (oilWellCap != null && baseGasRate > 0)
                oilWellCap.addGasRate = baseGasRate * multiplier;

            // Oil storage scales with rate to prevent overflow at high extraction rates
            var actualStorage = GetComponent<Storage>();
            if (actualStorage != null)
            {
                float baseCapacity = ControlledExtractionOptions.Instance.MaxOilStorage;
                actualStorage.capacityKg = baseCapacity * Mathf.Max(1f, multiplier);
            }

            // Ronivan's Legacy compatibility - scale their hardcoded limits
            ScaleRonivansLegacyLimits(multiplier);
        }

        private void ScaleRonivansLegacyLimits(float multiplier)
        {
            // PipedOptionalExhaust - RL sets capacity to 20f for oil
            foreach (var component in GetComponents<KMonoBehaviour>())
            {
                var type = component.GetType();
                
                if (type.Name == "PipedOptionalExhaust")
                {
                    var capacityField = type.GetField("capacity");
                    if (capacityField != null)
                    {
                        float baseValue = 20f; // RL's hardcoded value
                        float scaledValue = baseValue * Mathf.Max(1f, multiplier);
                        capacityField.SetValue(component, scaledValue);
                    }
                }
                
                if (type.Name == "ElementThresholdOperational")
                {
                    var thresholdField = type.GetField("Threshold");
                    if (thresholdField != null)
                    {
                        float baseValue = 20f; // RL's hardcoded value for oil
                        float scaledValue = baseValue * Mathf.Max(1f, multiplier);
                        thresholdField.SetValue(component, scaledValue);
                    }
                }
            }
        }

        private void OnCopySettings(object data)
        {
            var source = ((GameObject)data).GetComponent<ExtractionRateController>();
            if (source != null)
                WaterInputRate = source.WaterInputRate;
        }

        private static readonly EventSystem.IntraObjectHandler<ExtractionRateController> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<ExtractionRateController>((c, d) => c.OnCopySettings(d));
    }
}
