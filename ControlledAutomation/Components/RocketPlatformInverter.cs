using KSerialization;
using UnityEngine;
using STRINGS;
using System.Collections.Generic;

namespace ControlledAutomation.Components
{
    /// <summary>
    /// Special component for Rocket Platform (LaunchPad) which has two automation outputs:
    /// - Output 1: Rocket Present
    /// - Output 2: Rocket Ready
    /// Each output can be independently inverted.
    /// </summary>
    [SerializationConfig(MemberSerialization.OptIn)]
    public class RocketPlatformInverter : KMonoBehaviour
    {
        [Serialize]
        private bool invertOutput1 = false;

        [Serialize]
        private bool invertOutput2 = false;

        /// <summary>
        /// Whether to invert the first output (Rocket Present).
        /// </summary>
        public bool InvertOutput1
        {
            get => invertOutput1;
            set
            {
                if (invertOutput1 != value)
                {
                    invertOutput1 = value;
                    UpdatePortTooltips();
                }
            }
        }

        /// <summary>
        /// Whether to invert the second output (Rocket Ready).
        /// </summary>
        public bool InvertOutput2
        {
            get => invertOutput2;
            set
            {
                if (invertOutput2 != value)
                {
                    invertOutput2 = value;
                    UpdatePortTooltips();
                }
            }
        }

        /// <summary>
        /// Last signal values sent to each port.
        /// </summary>
        public bool? LastSentSignal1 { get; set; } = null;
        public bool? LastSentSignal2 { get; set; } = null;

        private string originalActiveDescription1;
        private string originalInactiveDescription1;
        private string originalActiveDescription2;
        private string originalInactiveDescription2;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            fastMap[gameObject] = this;

            // Store original descriptions
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

            UpdatePortTooltips();
        }

        protected override void OnCleanUp()
        {
            fastMap.Remove(gameObject);
            base.OnCleanUp();
        }

        /// <summary>
        /// Applies inversion to output 1.
        /// </summary>
        public bool ApplyInversion1(bool signal)
        {
            return invertOutput1 ? !signal : signal;
        }

        /// <summary>
        /// Applies inversion to output 2.
        /// </summary>
        public bool ApplyInversion2(bool signal)
        {
            return invertOutput2 ? !signal : signal;
        }

        private void UpdatePortTooltips()
        {
            var ports = GetComponent<LogicPorts>();
            if (ports?.outputPortInfo == null)
                return;

            // Update first port tooltips
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

            // Update second port tooltips
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

        // Fast lookup map
        private static readonly Dictionary<GameObject, RocketPlatformInverter> fastMap
            = new Dictionary<GameObject, RocketPlatformInverter>();

        public static RocketPlatformInverter Get(GameObject go)
        {
            if (fastMap.TryGetValue(go, out var inverter))
                return inverter;
            return null;
        }
    }
}
