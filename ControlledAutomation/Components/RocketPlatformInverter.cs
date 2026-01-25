using KSerialization;
using UnityEngine;
using STRINGS;
using System.Collections.Generic;

namespace ControlledAutomation.Components
{
    // Rocket Platform has two automation outputs - each can be inverted independently
    [SerializationConfig(MemberSerialization.OptIn)]
    public class RocketPlatformInverter : KMonoBehaviour
    {
        [Serialize] private bool invertOutput1 = false;
        [Serialize] private bool invertOutput2 = false;

        public bool InvertOutput1
        {
            get => invertOutput1;
            set { if (invertOutput1 != value) { invertOutput1 = value; UpdatePortTooltips(); } }
        }

        public bool InvertOutput2
        {
            get => invertOutput2;
            set { if (invertOutput2 != value) { invertOutput2 = value; UpdatePortTooltips(); } }
        }

        public bool? LastSentSignal1 { get; set; } = null;
        public bool? LastSentSignal2 { get; set; } = null;

        private string originalActiveDescription1;
        private string originalInactiveDescription1;
        private string originalActiveDescription2;
        private string originalInactiveDescription2;

        private static readonly EventSystem.IntraObjectHandler<RocketPlatformInverter> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<RocketPlatformInverter>((component, data) => component.OnCopySettings(data));

        protected override void OnSpawn()
        {
            base.OnSpawn();
            fastMap[gameObject] = this;

            var ports = GetComponent<LogicPorts>();
            if (ports?.outputPortInfo != null)
            {
                if (ports.outputPortInfo.Length > 0)
                {
                    originalActiveDescription1 = ports.outputPortInfo[0].activeDescription;
                    originalInactiveDescription1 = ports.outputPortInfo[0].inactiveDescription;
                }
                if (ports.outputPortInfo.Length > 1)
                {
                    originalActiveDescription2 = ports.outputPortInfo[1].activeDescription;
                    originalInactiveDescription2 = ports.outputPortInfo[1].inactiveDescription;
                }
            }

            Subscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
            UpdatePortTooltips();
        }

        protected override void OnCleanUp()
        {
            Unsubscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
            fastMap.Remove(gameObject);
            base.OnCleanUp();
        }

        private void OnCopySettings(object data)
        {
            RocketPlatformInverter other = ((GameObject)data)?.GetComponent<RocketPlatformInverter>();
            if (other != null)
            {
                InvertOutput1 = other.InvertOutput1;
                InvertOutput2 = other.InvertOutput2;
            }
        }

        public bool ApplyInversion1(bool signal) => invertOutput1 ? !signal : signal;
        public bool ApplyInversion2(bool signal) => invertOutput2 ? !signal : signal;

        private void UpdatePortTooltips()
        {
            var ports = GetComponent<LogicPorts>();
            if (ports?.outputPortInfo == null) return;

            if (ports.outputPortInfo.Length > 0)
            {
                if (invertOutput1)
                {
                    ports.outputPortInfo[0].activeDescription = originalInactiveDescription1 ?? CONTROLLEDAUTOMATION.SENSOR_LOGIC_PORT_ACTIVE_INVERTED;
                    ports.outputPortInfo[0].inactiveDescription = originalActiveDescription1 ?? CONTROLLEDAUTOMATION.SENSOR_LOGIC_PORT_INACTIVE_INVERTED;
                }
                else
                {
                    ports.outputPortInfo[0].activeDescription = originalActiveDescription1;
                    ports.outputPortInfo[0].inactiveDescription = originalInactiveDescription1;
                }
            }

            if (ports.outputPortInfo.Length > 1)
            {
                if (invertOutput2)
                {
                    ports.outputPortInfo[1].activeDescription = originalInactiveDescription2 ?? CONTROLLEDAUTOMATION.SENSOR_LOGIC_PORT_ACTIVE_INVERTED;
                    ports.outputPortInfo[1].inactiveDescription = originalActiveDescription2 ?? CONTROLLEDAUTOMATION.SENSOR_LOGIC_PORT_INACTIVE_INVERTED;
                }
                else
                {
                    ports.outputPortInfo[1].activeDescription = originalActiveDescription2;
                    ports.outputPortInfo[1].inactiveDescription = originalInactiveDescription2;
                }
            }
        }

        private static readonly Dictionary<GameObject, RocketPlatformInverter> fastMap = new Dictionary<GameObject, RocketPlatformInverter>();

        public static RocketPlatformInverter Get(GameObject go)
        {
            fastMap.TryGetValue(go, out var inverter);
            return inverter;
        }
    }
}
