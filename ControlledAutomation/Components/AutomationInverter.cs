using KSerialization;
using UnityEngine;
using System.Collections.Generic;

namespace ControlledAutomation.Components
{
    /// <summary>
    /// Base component that provides automation signal inversion capability.
    /// Attach to any building with a logic output port to enable signal inversion.
    /// </summary>
    [SerializationConfig(MemberSerialization.OptIn)]
    public class AutomationInverter : KMonoBehaviour
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
                    OnInversionChanged();
                }
            }
        }

        /// <summary>
        /// Last signal value that was actually sent to the port.
        /// Used to track state and avoid redundant updates.
        /// </summary>
        public bool? LastSentSignal { get; set; } = null;

        /// <summary>
        /// The port ID to control. Default is the first output port.
        /// </summary>
        public HashedString PortId { get; set; } = HashedString.Invalid;

        /// <summary>
        /// Called when inversion setting changes. Override to update port state.
        /// </summary>
        protected virtual void OnInversionChanged()
        {
            UpdateLogicPort();
            UpdatePortTooltips();
        }

        /// <summary>
        /// Override in derived classes to trigger logic port update.
        /// </summary>
        protected virtual void UpdateLogicPort()
        {
            // Derived classes should implement this to call the building's logic update method
        }

        /// <summary>
        /// Override in derived classes to update port tooltip descriptions.
        /// </summary>
        protected virtual void UpdatePortTooltips()
        {
            // Derived classes should implement this
        }

        /// <summary>
        /// Applies inversion to a signal value if inversion is enabled.
        /// </summary>
        public bool ApplyInversion(bool signal)
        {
            return invertSignal ? !signal : signal;
        }

        /// <summary>
        /// Sends a signal to the logic port, applying inversion if enabled.
        /// Returns true if the signal was actually sent (i.e., it was different from last time).
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

        protected override void OnSpawn()
        {
            base.OnSpawn();
            fastMap[gameObject] = this;
            LastSentSignal = null;
            UpdatePortTooltips();
        }

        protected override void OnCleanUp()
        {
            fastMap.Remove(gameObject);
            base.OnCleanUp();
        }

        // Fast lookup map for GetComponent optimization
        private static readonly Dictionary<GameObject, AutomationInverter> fastMap 
            = new Dictionary<GameObject, AutomationInverter>();

        public static AutomationInverter Get(GameObject go)
        {
            if (fastMap.TryGetValue(go, out var inverter))
                return inverter;
            return null;
        }
    }
}
