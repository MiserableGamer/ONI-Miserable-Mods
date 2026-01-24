using KSerialization;
using UnityEngine;
using STRINGS;
using System.Collections.Generic;

namespace ControlledAutomation.Components
{
    /// <summary>
    /// Base component that provides high/low threshold support with IActivationRangeTarget interface.
    /// Also includes automation signal inversion.
    /// Used for storage buildings that need threshold controls.
    /// </summary>
    [SerializationConfig(MemberSerialization.OptIn)]
    public abstract class ThresholdsBase : KMonoBehaviour, IActivationRangeTarget, ISim4000ms
    {
        [Serialize]
        private bool invertSignal = false;

        [Serialize]
        private int activateValue = 100; // High threshold

        [Serialize]
        private int deactivateValue = 99; // Low threshold

        [Serialize]
        private bool activated;

        /// <summary>
        /// Last signal value sent to the logic port.
        /// </summary>
        public bool? LastSetFlag { get; set; } = null;

        /// <summary>
        /// Whether to invert the signal (send green when low instead of when high).
        /// </summary>
        public bool InvertSignal
        {
            get => invertSignal;
            set
            {
                invertSignal = value;
                UpdateLogicCircuit();
                UpdateLogicPortTooltip();
            }
        }

        // IActivationRangeTarget implementation
        // Note: The interface naming is confusing in the game code.
        // "Activate" actually means "High" threshold and "Deactivate" means "Low" threshold.

        public float ActivateValue
        {
            get => activateValue;
            set
            {
                activateValue = (int)value;
                UpdateLogicCircuit();
            }
        }

        public float DeactivateValue
        {
            get => deactivateValue;
            set
            {
                deactivateValue = (int)value;
                UpdateLogicCircuit();
            }
        }

        public float MinValue => 0f;
        public float MaxValue => 100f;
        public bool UseWholeNumbers => true;

        public string ActivateTooltip => invertSignal
            ? CONTROLLEDAUTOMATION.ACTIVATE_TOOLTIP_INVERTED
            : CONTROLLEDAUTOMATION.ACTIVATE_TOOLTIP;

        public string DeactivateTooltip => invertSignal
            ? CONTROLLEDAUTOMATION.DEACTIVATE_TOOLTIP_INVERTED
            : CONTROLLEDAUTOMATION.DEACTIVATE_TOOLTIP;

        // Reuse existing game strings for slider labels
        public string ActivationRangeTitleText => BUILDINGS.PREFABS.SMARTRESERVOIR.SIDESCREEN_TITLE;
        public string ActivateSliderLabelText => BUILDINGS.PREFABS.SMARTRESERVOIR.SIDESCREEN_DEACTIVATE; // "High Threshold:"
        public string DeactivateSliderLabelText => BUILDINGS.PREFABS.SMARTRESERVOIR.SIDESCREEN_ACTIVATE; // "Low Threshold:"

        /// <summary>
        /// Updates the activated state based on current fill percentage.
        /// Returns the new activated state.
        /// </summary>
        public bool UpdateLogicState(float percentFull)
        {
            float num = Mathf.RoundToInt(percentFull * 100f);
            
            if (activated)
            {
                // Currently activated, check if we should deactivate
                if (invertSignal)
                {
                    // Inverted: deactivate when reaching high threshold
                    if (num >= (float)activateValue)
                        activated = false;
                }
                else
                {
                    // Normal: deactivate when falling below low threshold
                    if (num <= (float)deactivateValue)
                        activated = false;
                }
            }
            else
            {
                // Currently not activated, check if we should activate
                if (invertSignal)
                {
                    // Inverted: activate when falling below low threshold
                    if (num <= (float)deactivateValue)
                        activated = true;
                }
                else
                {
                    // Normal: activate when reaching high threshold
                    if (num >= (float)activateValue)
                        activated = true;
                }
            }
            
            return activated;
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            fastMap[gameObject] = this;
            LastSetFlag = null;
            UpdateLogicPortTooltip();
        }

        protected override void OnCleanUp()
        {
            fastMap.Remove(gameObject);
            base.OnCleanUp();
        }

        private void UpdateLogicPortTooltip()
        {
            LogicPorts ports = GetComponent<LogicPorts>();
            if (ports?.outputPortInfo == null || ports.outputPortInfo.Length == 0)
                return;

            ports.outputPortInfo[0].activeDescription = invertSignal
                ? CONTROLLEDAUTOMATION.LOGIC_PORT_ACTIVE_INVERTED
                : CONTROLLEDAUTOMATION.LOGIC_PORT_ACTIVE;
            ports.outputPortInfo[0].inactiveDescription = invertSignal
                ? CONTROLLEDAUTOMATION.LOGIC_PORT_INACTIVE_INVERTED
                : CONTROLLEDAUTOMATION.LOGIC_PORT_INACTIVE;
        }

        /// <summary>
        /// Override in derived classes to trigger the building's logic update.
        /// </summary>
        protected abstract void UpdateLogicCircuit();

        /// <summary>
        /// Checks if the building is actually operational, ignoring rocket usage restriction.
        /// This prevents restricted storage inside rockets from always signaling false.
        /// </summary>
        public bool IsActuallyOperational(Operational operational)
        {
            if (operational.IsOperational)
                return true;

            foreach (var flag in operational.Flags)
            {
                if (!flag.Value && flag.Key != RocketUsageRestriction.rocketUsageAllowed)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Periodic check for buildings inside rockets that may be restricted.
        /// </summary>
        public void Sim4000ms(float dt)
        {
            if (!gameObject.GetMyWorld().IsModuleInterior)
                return;
            UpdateLogicCircuit();
        }

        // Fast lookup map
        private static readonly Dictionary<GameObject, ThresholdsBase> fastMap
            = new Dictionary<GameObject, ThresholdsBase>();

        public static ThresholdsBase Get(GameObject go)
        {
            if (fastMap.TryGetValue(go, out var thresholds))
                return thresholds;
            return null;
        }
    }
}
