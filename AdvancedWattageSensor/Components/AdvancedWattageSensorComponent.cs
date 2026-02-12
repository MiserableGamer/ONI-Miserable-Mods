using System.Collections.Generic;
using AdvancedWattageSensor.Options;
using KSerialization;
using STRINGS;
using UnityEngine;

namespace AdvancedWattageSensor.Components
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class AdvancedWattageSensorComponent : Switch, ISaveLoadable, IThresholdSwitch, ISim200ms
    {
        // Static registry so the monitor panel can find all labeled sensors
        private static readonly HashSet<AdvancedWattageSensorComponent> allSensors = new HashSet<AdvancedWattageSensorComponent>();
        public static IReadOnlyCollection<AdvancedWattageSensorComponent> AllSensors => allSensors;

        public override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe<AdvancedWattageSensorComponent>(-905833192, OnCopySettingsDelegate);
        }

        private void OnCopySettings(object data)
        {
            var source = ((GameObject)data).GetComponent<AdvancedWattageSensorComponent>();
            if (source != null)
            {
                Threshold = source.Threshold;
                ActivateAboveThreshold = source.ActivateAboveThreshold;
                useWarningThreshold = source.useWarningThreshold;
                sendGreenOnWarning = source.sendGreenOnWarning;
            }
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            OnToggle += OnSwitchToggled;

            // Set up the meter visual (needle showing wattage relative to threshold)
            var animController = GetComponent<KBatchedAnimController>();
            meter = new MeterController(animController, "needle", "meter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer,
                "meter_fill", "meter_OL");
            animController.Play("on", KAnim.PlayMode.Loop, 1f, 0f);

            UpdateVisualState(true);
            UpdateLogicCircuit();
            wasOn = switchedOn;

            allSensors.Add(this);
        }

        public override void OnCleanUp()
        {
            allSensors.Remove(this);
            base.OnCleanUp();
        }

        public bool HasLabel => !string.IsNullOrEmpty(sensorLabel);

        public void SetLabel(string label)
        {
            sensorLabel = label ?? "";
        }

        public void Sim200ms(float dt)
        {
            float wattsUsed = Game.Instance.circuitManager.GetWattsUsedByCircuit(
                Game.Instance.circuitManager.GetCircuitID(Grid.PosToCell(this)));

            if (wattsUsed < 0f)
                return;

            currentWattage = wattsUsed;

            // Track cycle average: accumulate samples and reset when a new cycle starts
            int currentCycle = GameClock.Instance != null ? GameClock.Instance.GetCycle() : -1;
            if (currentCycle != lastTrackedCycle)
            {
                if (lastTrackedCycle >= 0 && cycleSampleCount > 0)
                    lastCycleAverageWattage = cycleSampleSum / cycleSampleCount;
                cycleSampleSum = 0f;
                cycleSampleCount = 0;
                lastTrackedCycle = currentCycle;
            }
            cycleSampleSum += currentWattage;
            cycleSampleCount++;

            // Smooth the reading for the meter display
            averageWattage = averageWattage * 0.95f + currentWattage * 0.05f;

            UpdateVisualState(false);

            // When warning threshold mode is enabled, trigger at the percentage-adjusted
            // threshold and use the per-sensor signal polarity instead of the slider direction
            float effectiveThreshold = thresholdWattage;
            bool activateAbove = activateOnHigherThan;

            if (useWarningThreshold)
            {
                int warningPercent = AdvancedWattageSensorOptions.Instance.WarningPercent;
                effectiveThreshold = thresholdWattage * (1f - warningPercent / 100f);
                activateAbove = sendGreenOnWarning;
            }

            if (activateAbove)
            {
                if ((currentWattage > effectiveThreshold && !IsSwitchedOn) ||
                    (currentWattage <= effectiveThreshold && IsSwitchedOn))
                {
                    Toggle();
                }
            }
            else
            {
                if ((currentWattage >= effectiveThreshold && IsSwitchedOn) ||
                    (currentWattage < effectiveThreshold && !IsSwitchedOn))
                {
                    Toggle();
                }
            }
        }

        public float GetWattageUsed()
        {
            return currentWattage;
        }

        private void OnSwitchToggled(bool toggled_on)
        {
            UpdateVisualState(false);
            UpdateLogicCircuit();
        }

        private void UpdateLogicCircuit()
        {
            GetComponent<LogicPorts>().SendSignal(LogicSwitch.PORT_ID, switchedOn ? 1 : 0);
        }

        private void UpdateVisualState(bool force = false)
        {
            if (meter == null)
                return;

            // Show wattage relative to threshold on the meter needle
            if (thresholdWattage >= 1f)
                meter.SetPositionPercent(Mathf.Clamp01(averageWattage / thresholdWattage));
            else
                meter.SetPositionPercent(1f);
        }

        public override void UpdateSwitchStatus()
        {
            var statusItem = switchedOn
                ? Db.Get().BuildingStatusItems.LogicSensorStatusActive
                : Db.Get().BuildingStatusItems.LogicSensorStatusInactive;
            GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Power, statusItem, null);
        }

        // IThresholdSwitch implementation
        public float Threshold
        {
            get => thresholdWattage;
            set
            {
                thresholdWattage = value;
                dirty = true;
            }
        }

        public bool ActivateAboveThreshold
        {
            get => activateOnHigherThan;
            set
            {
                activateOnHigherThan = value;
                dirty = true;
            }
        }

        public float CurrentValue => GetWattageUsed();
        public float RangeMin => minWattage;
        public float RangeMax => maxWattage;

        public float GetRangeMinInputField() => minWattage;
        public float GetRangeMaxInputField() => maxWattage;

        public LocString Title => STRINGS.UI.UISIDESCREENS.WATTAGESWITCHSIDESCREEN.TITLE;
        public LocString ThresholdValueName => STRINGS.UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.WATTAGE;
        public string AboveToolTip => STRINGS.UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.WATTAGE_TOOLTIP_ABOVE;
        public string BelowToolTip => STRINGS.UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.WATTAGE_TOOLTIP_BELOW;

        public string Format(float value, bool units) =>
            GameUtil.GetFormattedWattage(value, GameUtil.WattageFormatterUnit.Watts, units);

        public float ProcessedSliderValue(float input) => Mathf.Round(input);
        public float ProcessedInputValue(float input) => input;
        public LocString ThresholdValueUnits() => STRINGS.UI.UNITSUFFIXES.ELECTRICAL.WATT;

        public ThresholdScreenLayoutType LayoutType => ThresholdScreenLayoutType.SliderBar;
        public int IncrementScale => 1;

        public NonLinearSlider.Range[] GetRanges => new[]
        {
            new NonLinearSlider.Range(5f, 5f),
            new NonLinearSlider.Range(35f, 1000f),
            new NonLinearSlider.Range(50f, 3000f),
            new NonLinearSlider.Range(10f, maxWattage)
        };

        // Fields
        [Serialize] public string sensorLabel = "";
        [Serialize] public float thresholdWattage;
        [Serialize] public bool activateOnHigherThan;
        [Serialize] public bool dirty = true;
        [Serialize] public bool useWarningThreshold;
        [Serialize] public bool sendGreenOnWarning;

        private readonly float minWattage = 0f;
        private readonly float maxWattage = 1.5f * Wire.GetMaxWattageAsFloat(Wire.WattageRating.Max50000);
        public float currentWattage { get; private set; }
        public float lastCycleAverageWattage { get; private set; }
        private float averageWattage;
        private bool wasOn;
        private MeterController meter;

        // Cycle average tracking
        private float cycleSampleSum;
        private int cycleSampleCount;
        private int lastTrackedCycle = -1;

#pragma warning disable CS0649, CS0169 // [MyCmpAdd] fields are assigned by the game's component system
        [MyCmpAdd]
        private CopyBuildingSettings copyBuildingSettings;
#pragma warning restore CS0649, CS0169

        private static readonly EventSystem.IntraObjectHandler<AdvancedWattageSensorComponent> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<AdvancedWattageSensorComponent>(
                (component, data) => component.OnCopySettings(data));
    }
}
