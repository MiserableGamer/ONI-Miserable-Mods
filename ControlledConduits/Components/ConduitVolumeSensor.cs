using System;
using HarmonyLib;
using KSerialization;
using STRINGS;
using UnityEngine;

namespace ControlledConduits.Components
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class ConduitVolumeSensor : ConduitThresholdSensor, IThresholdSwitch
    {
        [Serialize]
        private float lastValue;

        [Serialize]
        public bool ignoreEmpty;

        public float rangeMin = 0f;
        // Not used for display; max is derived from conduit type at runtime.
        public float rangeMax = 10f;

        // Read from flow manager at runtime so mods that change conduit capacity are reflected; solid has no game limit so we use a UI cap.
        private float ConduitMaxMass
        {
            get
            {
                if (conduitType == ConduitType.Gas)
                {
                    var flow = Game.Instance?.gasConduitFlow;
                    if (flow != null)
                    {
                        var v = Traverse.Create(flow).Field("MaxMass").GetValue<float>();
                        if (v > 0f) return v;
                    }
                    return ConduitFlow.MAX_GAS_MASS;
                }
                if (conduitType == ConduitType.Liquid)
                {
                    var flow = Game.Instance?.liquidConduitFlow;
                    if (flow != null)
                    {
                        var v = Traverse.Create(flow).Field("MaxMass").GetValue<float>();
                        if (v > 0f) return v;
                    }
                    return 10f;
                }
                return 20000f;
            }
        }

        public override float CurrentValue
        {
            get
            {
                int cell = Grid.PosToCell(this);
                float mass = 0f;
                if (conduitType == ConduitType.Gas || conduitType == ConduitType.Liquid)
                {
                    mass = Conduit.GetFlowManager(conduitType).GetContents(cell).mass;
                }
                else if (conduitType == ConduitType.Solid)
                {
                    var contents = SolidConduit.GetFlowManager().GetContents(cell);
                    var pickupable = SolidConduit.GetFlowManager().GetPickupable(contents.pickupableHandle);
                    if (pickupable != null && pickupable.PrimaryElement != null)
                        mass = pickupable.PrimaryElement.Mass;
                }
                if (mass > 0f)
                    lastValue = mass;
                return mass;
            }
        }

        public float RangeMin => rangeMin;
        public float RangeMax => ConduitMaxMass;

        public float GetRangeMinInputField() => rangeMin;
        public float GetRangeMaxInputField() => ConduitMaxMass;

        public LocString Title => CONTROLLEDCONDUITS.THRESHOLD_SIDESCREEN_TITLE;
        public LocString ThresholdValueName => CONTROLLEDCONDUITS.THRESHOLD_PACKET_MASS;
        public string AboveToolTip => CONTROLLEDCONDUITS.THRESHOLD_ABOVE_TOOLTIP;
        public string BelowToolTip => CONTROLLEDCONDUITS.THRESHOLD_BELOW_TOOLTIP;

        // IThresholdSwitch expects display scaled by magnitude (mg/g/kg) for current value; threshold is raw kg for the input field.
        public string Format(float value, bool units)
        {
            if (!units)
                return string.Format("{0:0.####}", value);

            if (value < 0.001f)
            {
                float mg = value * 1e6f;
                return string.Format("{0:0.##}", mg) + STRINGS.UI.UNITSUFFIXES.MASS.MILLIGRAM.ToString();
            }
            if (value < 1f)
            {
                float g = value * 1000f;
                return string.Format("{0:0.###}", g) + STRINGS.UI.UNITSUFFIXES.MASS.GRAM.ToString();
            }
            return string.Format("{0:0.##}", value) + STRINGS.UI.UNITSUFFIXES.MASS.KILOGRAM.ToString();
        }

        // Round to 0.1 g so slider matches threshold resolution.
        public float ProcessedSliderValue(float input) => Mathf.Round(input * 10000f) / 10000f;
        public float ProcessedInputValue(float input) => input;

        public LocString ThresholdValueUnits() => GameUtil.GetCurrentMassUnit(false);

        public ThresholdScreenLayoutType LayoutType => ThresholdScreenLayoutType.SliderBar;
        public int IncrementScale => 1;

        public NonLinearSlider.Range[] GetRanges => NonLinearSlider.GetDefaultRange(ConduitMaxMass);
    }
}
