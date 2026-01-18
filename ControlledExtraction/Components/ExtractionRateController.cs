using KSerialization;
using ControlledExtraction.Options;
using UnityEngine;

namespace ControlledExtraction.Components
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class ExtractionRateController : KMonoBehaviour
    {
        public const float VANILLA_WATER_RATE = 1f;
        public const float VANILLA_OIL_RATIO = 3.3333333f;

        [Serialize]
        private float waterInputRate = -1f;

        [MyCmpReq]
        private ElementConverter elementConverter;

        [MyCmpReq]
        private ConduitConsumer conduitConsumer;

        private OilWellCap oilWellCap;
        private float baseGasRate;

        public float WaterInputRate
        {
            get => waterInputRate < 0 ? ControlledExtractionOptions.Instance.DefaultWaterRate : waterInputRate;
            set
            {
                waterInputRate = Mathf.Clamp(value,
                    ControlledExtractionOptions.Instance.MinWaterRate,
                    ControlledExtractionOptions.Instance.MaxWaterRate);
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

            // Water consumption
            if (elementConverter.consumedElements?.Length > 0)
            {
                var consumed = elementConverter.consumedElements[0];
                consumed.MassConsumptionRate = WaterInputRate;
                elementConverter.consumedElements[0] = consumed;
            }

            // Oil output (maintains 3.33:1 ratio)
            if (elementConverter.outputElements?.Length > 0)
            {
                var output = elementConverter.outputElements[0];
                output.massGenerationRate = WaterInputRate * VANILLA_OIL_RATIO;
                elementConverter.outputElements[0] = output;
            }

            // Conduit capacity
            if (conduitConsumer != null)
            {
                conduitConsumer.consumptionRate = Mathf.Max(2f, WaterInputRate * 2f);
                conduitConsumer.capacityKG = Mathf.Max(10f, WaterInputRate * 10f);
            }

            // Gas buildup rate
            if (oilWellCap != null && baseGasRate > 0)
                oilWellCap.addGasRate = baseGasRate * ExtractionMultiplier;
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
