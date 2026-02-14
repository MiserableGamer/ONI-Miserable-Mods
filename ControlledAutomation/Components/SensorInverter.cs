using KSerialization;
using UnityEngine;
using STRINGS;
using System.Collections.Generic;

namespace ControlledAutomation.Components
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class SensorInverter : KMonoBehaviour
    {
        [Serialize]
        private bool invertSignal = false;

        public bool InvertSignal
        {
            get => invertSignal;
            set
            {
                if (invertSignal != value)
                {
                    invertSignal = value;
                    UpdatePortTooltips();
                    TriggerLogicUpdate();
                }
            }
        }

        public bool? LastSentSignal { get; set; } = null;

        private string originalActiveDescription;
        private string originalInactiveDescription;

        private static readonly EventSystem.IntraObjectHandler<SensorInverter> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<SensorInverter>((component, data) => component.OnCopySettings(data));

        public override void OnSpawn()
        {
            base.OnSpawn();
            fastMap[gameObject] = this;
            LastSentSignal = null;
            
            var ports = GetComponent<LogicPorts>();
            if (ports?.outputPortInfo != null && ports.outputPortInfo.Length > 0)
            {
                originalActiveDescription = ports.outputPortInfo[0].activeDescription;
                originalInactiveDescription = ports.outputPortInfo[0].inactiveDescription;
            }
            
            Subscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
            UpdatePortTooltips();
        }

        public override void OnCleanUp()
        {
            Unsubscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
            fastMap.Remove(gameObject);
            base.OnCleanUp();
        }

        private void OnCopySettings(object data)
        {
            SensorInverter other = ((GameObject)data)?.GetComponent<SensorInverter>();
            if (other != null)
                InvertSignal = other.InvertSignal;
        }

        public bool ApplyInversion(bool signal) => invertSignal ? !signal : signal;

        public bool SendSignal(LogicPorts ports, HashedString portId, bool rawSignal)
        {
            bool finalSignal = ApplyInversion(rawSignal);
            
            if (finalSignal != LastSentSignal)
            {
                ports.SendSignal(portId, finalSignal ? 1 : 0);
                LastSentSignal = finalSignal;
                return true;
            }
            return false;
        }

        private void UpdatePortTooltips()
        {
            var ports = GetComponent<LogicPorts>();
            if (ports?.outputPortInfo == null || ports.outputPortInfo.Length == 0)
                return;

            if (invertSignal)
            {
                ports.outputPortInfo[0].activeDescription = originalInactiveDescription ?? CONTROLLEDAUTOMATION.SENSOR_LOGIC_PORT_ACTIVE_INVERTED;
                ports.outputPortInfo[0].inactiveDescription = originalActiveDescription ?? CONTROLLEDAUTOMATION.SENSOR_LOGIC_PORT_INACTIVE_INVERTED;
            }
            else
            {
                ports.outputPortInfo[0].activeDescription = originalActiveDescription;
                ports.outputPortInfo[0].inactiveDescription = originalInactiveDescription;
            }
        }

        protected virtual void TriggerLogicUpdate()
        {
            var ports = GetComponent<LogicPorts>();
            if (ports == null)
                return;

            HashedString portId = LogicSwitch.PORT_ID;
            if (ports.outputPortInfo != null && ports.outputPortInfo.Length > 0)
                portId = ports.outputPortInfo[0].id;

            // Get current output and calculate what the raw (uninverted) value should be
            int currentOutput = ports.GetOutputValue(portId);
            
            // Since we just toggled, the OLD state was !invertSignal
            bool oldInversionWasOn = !invertSignal;
            int rawValue = oldInversionWasOn ? (currentOutput != 0 ? 0 : 1) : currentOutput;
            
            ports.SendSignal(portId, rawValue);
        }

        // Fast lookup
        private static readonly Dictionary<GameObject, SensorInverter> fastMap = new Dictionary<GameObject, SensorInverter>();

        public static SensorInverter Get(GameObject go)
        {
            fastMap.TryGetValue(go, out var inverter);
            return inverter;
        }
    }
}
