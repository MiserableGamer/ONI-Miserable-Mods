using KSerialization;
using UnityEngine;

namespace ControlledAutomation.Components
{
    // Temperature sensor that activates when temperature is within a configurable range
    [SerializationConfig(MemberSerialization.OptIn)]
    public class TemperatureRangeSensor : Switch, ISaveLoadable, ISim200ms
    {
        [MyCmpAdd]
        private CopyBuildingSettings copyBuildingSettings;

        private const int NumFrameDelay = 8;
        private float[] temperatures = new float[NumFrameDelay];
        private int simUpdateCounter;
        private float averageTemp;

        [Serialize][SerializeField] private float _centerTemperature = 293.15f;
        [Serialize][SerializeField] private float _degreesBelow = 10f;
        [Serialize][SerializeField] private float _degreesAbove = 10f;
        [Serialize][SerializeField] private bool _activateInsideRange = true;
        [Serialize] private bool dirty = true;

        public float centerTemperature { get => _centerTemperature; set => _centerTemperature = value; }
        public float degreesBelow { get => _degreesBelow; set => _degreesBelow = value; }
        public float degreesAbove { get => _degreesAbove; set => _degreesAbove = value; }
        public bool activateInsideRange { get => _activateInsideRange; set => _activateInsideRange = value; }

        public float minTemp;
        public float maxTemp = 373.15f;

        private bool wasOn;

        private static readonly EventSystem.IntraObjectHandler<TemperatureRangeSensor> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<TemperatureRangeSensor>((component, data) => component.OnCopySettings(data));

        public float LowerBound => centerTemperature - degreesBelow;
        public float UpperBound => centerTemperature + degreesAbove;
        public float GetTemperature() => averageTemp;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe(-905833192, OnCopySettingsDelegate);
        }

        private void OnCopySettings(object data)
        {
            var other = ((GameObject)data).GetComponent<TemperatureRangeSensor>();
            if (other != null)
            {
                centerTemperature = other.centerTemperature;
                degreesBelow = other.degreesBelow;
                degreesAbove = other.degreesAbove;
                activateInsideRange = other.activateInsideRange;
                dirty = true;
            }
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            OnToggle += OnSwitchToggled;
            UpdateVisualState(true);
            UpdateLogicCircuit();
            wasOn = switchedOn;
        }

        public void Sim200ms(float dt)
        {
            if (simUpdateCounter < NumFrameDelay && !dirty)
            {
                int cell = Grid.PosToCell(this);
                if (Grid.Mass[cell] > 0f)
                {
                    temperatures[simUpdateCounter] = Grid.Temperature[cell];
                    simUpdateCounter++;
                }
                return;
            }

            simUpdateCounter = 0;
            dirty = false;
            averageTemp = 0f;
            for (int i = 0; i < NumFrameDelay; i++)
                averageTemp += temperatures[i];
            averageTemp /= NumFrameDelay;

            bool isInRange = averageTemp >= LowerBound && averageTemp <= UpperBound;
            bool shouldBeOn = activateInsideRange ? isInRange : !isInRange;

            if (shouldBeOn != IsSwitchedOn)
                Toggle();
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
            if (wasOn != switchedOn || force)
            {
                wasOn = switchedOn;
                var anim = GetComponent<KBatchedAnimController>();
                anim.Play(switchedOn ? "on_pre" : "on_pst", KAnim.PlayMode.Once, 1f, 0f);
                anim.Queue(switchedOn ? "on" : "off", KAnim.PlayMode.Once, 1f, 0f);
            }
        }

        protected override void UpdateSwitchStatus()
        {
            var status = switchedOn 
                ? Db.Get().BuildingStatusItems.LogicSensorStatusActive 
                : Db.Get().BuildingStatusItems.LogicSensorStatusInactive;
            GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Power, status, null);
        }

        public void SetCenterTemperature(float temp)
        {
            centerTemperature = Mathf.Clamp(temp, minTemp, maxTemp);
            dirty = true;
        }

        public void SetDegreesBelow(float degrees)
        {
            degreesBelow = Mathf.Max(0f, degrees);
            dirty = true;
        }

        public void SetDegreesAbove(float degrees)
        {
            degreesAbove = Mathf.Max(0f, degrees);
            dirty = true;
        }

        public void SetActivateInsideRange(bool inside)
        {
            activateInsideRange = inside;
            dirty = true;
        }
    }
}
