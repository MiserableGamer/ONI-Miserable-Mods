using KSerialization;
using UnityEngine;
using STRINGS;
using System.Collections.Generic;

namespace ControlledAutomation.Components
{
    // Base component for high/low threshold support with IActivationRangeTarget interface
    [SerializationConfig(MemberSerialization.OptIn)]
    public abstract class ThresholdsBase : KMonoBehaviour, IActivationRangeTarget, ISim4000ms
    {
        [Serialize] private bool invertSignal = false;
        [Serialize] private int activateValue = 100;
        [Serialize] private int deactivateValue = 99;
        [Serialize] private bool activated;

        public bool? LastSetFlag { get; set; } = null;

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

        // IActivationRangeTarget - note: game naming is backwards, "Activate" = High, "Deactivate" = Low
        public float ActivateValue
        {
            get => activateValue;
            set { activateValue = (int)value; UpdateLogicCircuit(); }
        }

        public float DeactivateValue
        {
            get => deactivateValue;
            set { deactivateValue = (int)value; UpdateLogicCircuit(); }
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

        public string ActivationRangeTitleText => BUILDINGS.PREFABS.SMARTRESERVOIR.SIDESCREEN_TITLE;
        public string ActivateSliderLabelText => BUILDINGS.PREFABS.SMARTRESERVOIR.SIDESCREEN_DEACTIVATE;
        public string DeactivateSliderLabelText => BUILDINGS.PREFABS.SMARTRESERVOIR.SIDESCREEN_ACTIVATE;

        private static readonly EventSystem.IntraObjectHandler<ThresholdsBase> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<ThresholdsBase>((component, data) => component.OnCopySettings(data));

        public bool UpdateLogicState(float percentFull)
        {
            float num = Mathf.RoundToInt(percentFull * 100f);
            
            if (activated)
            {
                if (invertSignal)
                {
                    if (num >= (float)activateValue)
                        activated = false;
                }
                else
                {
                    if (num <= (float)deactivateValue)
                        activated = false;
                }
            }
            else
            {
                if (invertSignal)
                {
                    if (num <= (float)deactivateValue)
                        activated = true;
                }
                else
                {
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
            Subscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
            UpdateLogicPortTooltip();
        }

        protected override void OnCleanUp()
        {
            Unsubscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
            fastMap.Remove(gameObject);
            base.OnCleanUp();
        }

        private void OnCopySettings(object data)
        {
            ThresholdsBase other = ((GameObject)data)?.GetComponent<ThresholdsBase>();
            if (other != null)
            {
                invertSignal = other.invertSignal;
                activateValue = other.activateValue;
                deactivateValue = other.deactivateValue;
                UpdateLogicCircuit();
                UpdateLogicPortTooltip();
            }
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

        protected abstract void UpdateLogicCircuit();

        // Handles rocket interior storage that may be restricted
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

        public void Sim4000ms(float dt)
        {
            if (!gameObject.GetMyWorld().IsModuleInterior)
                return;
            UpdateLogicCircuit();
        }

        // Fast lookup
        private static readonly Dictionary<GameObject, ThresholdsBase> fastMap = new Dictionary<GameObject, ThresholdsBase>();

        public static ThresholdsBase Get(GameObject go)
        {
            fastMap.TryGetValue(go, out var thresholds);
            return thresholds;
        }
    }
}
