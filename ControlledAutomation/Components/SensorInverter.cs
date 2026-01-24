using KSerialization;
using UnityEngine;
using STRINGS;
using System.Collections.Generic;

namespace ControlledAutomation.Components
{
    /// <summary>
    /// Component that adds automation signal inversion to sensors and other buildings.
    /// Unlike ThresholdsBase, this doesn't implement IActivationRangeTarget.
    /// </summary>
    [SerializationConfig(MemberSerialization.OptIn)]
    public class SensorInverter : KMonoBehaviour
    {
        [Serialize]
        private bool invertSignal = false;

        /// <summary>
        /// Whether to invert the automation output signal.
        /// </summary>
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

        /// <summary>
        /// Last signal value that was sent to the port.
        /// </summary>
        public bool? LastSentSignal { get; set; } = null;

        /// <summary>
        /// Original active description from the port info.
        /// </summary>
        private string originalActiveDescription;

        /// <summary>
        /// Original inactive description from the port info.
        /// </summary>
        private string originalInactiveDescription;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            fastMap[gameObject] = this;
            LastSentSignal = null;
            
            // Store original descriptions
            var ports = GetComponent<LogicPorts>();
            if (ports?.outputPortInfo != null && ports.outputPortInfo.Length > 0)
            {
                originalActiveDescription = ports.outputPortInfo[0].activeDescription;
                originalInactiveDescription = ports.outputPortInfo[0].inactiveDescription;
            }
            
            UpdatePortTooltips();
        }

        protected override void OnCleanUp()
        {
            fastMap.Remove(gameObject);
            base.OnCleanUp();
        }

        /// <summary>
        /// Applies inversion to a signal if enabled.
        /// </summary>
        public bool ApplyInversion(bool signal)
        {
            return invertSignal ? !signal : signal;
        }

        /// <summary>
        /// Sends a signal to the logic port, applying inversion if enabled.
        /// </summary>
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
                // Swap descriptions when inverted
                ports.outputPortInfo[0].activeDescription = originalInactiveDescription ?? CONTROLLEDAUTOMATION.SENSOR_LOGIC_PORT_ACTIVE_INVERTED;
                ports.outputPortInfo[0].inactiveDescription = originalActiveDescription ?? CONTROLLEDAUTOMATION.SENSOR_LOGIC_PORT_INACTIVE_INVERTED;
            }
            else
            {
                // Restore original descriptions
                ports.outputPortInfo[0].activeDescription = originalActiveDescription;
                ports.outputPortInfo[0].inactiveDescription = originalInactiveDescription;
            }
        }

        /// <summary>
        /// Triggers a logic update on the attached component.
        /// Override this in subclasses if needed for specific building types.
        /// </summary>
        protected virtual void TriggerLogicUpdate()
        {
            // Most sensors will update automatically on next tick
            // Subclasses can override to force immediate update
        }

        // Fast lookup map
        private static readonly Dictionary<GameObject, SensorInverter> fastMap
            = new Dictionary<GameObject, SensorInverter>();

        public static SensorInverter Get(GameObject go)
        {
            if (fastMap.TryGetValue(go, out var inverter))
                return inverter;
            return null;
        }
    }
}
