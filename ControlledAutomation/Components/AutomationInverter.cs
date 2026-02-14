using KSerialization;
using UnityEngine;
using System.Collections.Generic;

namespace ControlledAutomation.Components
{
    // Base component for automation signal inversion (not currently used, kept for potential future use)
    [SerializationConfig(MemberSerialization.OptIn)]
    public class AutomationInverter : KMonoBehaviour
    {
        [Serialize] private bool invertSignal = false;

        public bool InvertSignal
        {
            get => invertSignal;
            set { if (invertSignal != value) { invertSignal = value; OnInversionChanged(); } }
        }

        public bool? LastSentSignal { get; set; } = null;
        public HashedString PortId { get; set; } = HashedString.Invalid;

        protected virtual void OnInversionChanged()
        {
            UpdateLogicPort();
            UpdatePortTooltips();
        }

        protected virtual void UpdateLogicPort() { }
        protected virtual void UpdatePortTooltips() { }

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

        public override void OnSpawn()
        {
            base.OnSpawn();
            fastMap[gameObject] = this;
            LastSentSignal = null;
            UpdatePortTooltips();
        }

        public override void OnCleanUp()
        {
            fastMap.Remove(gameObject);
            base.OnCleanUp();
        }

        private static readonly Dictionary<GameObject, AutomationInverter> fastMap = new Dictionary<GameObject, AutomationInverter>();

        public static AutomationInverter Get(GameObject go)
        {
            fastMap.TryGetValue(go, out var inverter);
            return inverter;
        }
    }
}
