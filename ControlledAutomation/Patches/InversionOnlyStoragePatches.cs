using HarmonyLib;
using UnityEngine;
using ControlledAutomation.Components;
using ControlledAutomation.Options;

namespace ControlledAutomation.Patches
{
    /// <summary>
    /// Patches for storage buildings that ALREADY have thresholds, just need inversion.
    /// - Smart Battery (BatterySmart)
    /// - Liquid Reservoir
    /// - Gas Reservoir
    /// - Radbolt Chamber (HEPBattery)
    /// </summary>

    #region Smart Battery

    [HarmonyPatch(typeof(BatterySmartConfig))]
    public class BatterySmartConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(BatterySmartConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(BatterySmart))]
    public class BatterySmart_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(BatterySmart.OnCopySettings))]
        public static void OnCopySettings(BatterySmart __instance, object data)
        {
            GameObject otherGameObject = (GameObject)data;
            if (otherGameObject != null)
            {
                SensorInverter component = SensorInverter.Get(__instance.gameObject);
                SensorInverter otherComponent = SensorInverter.Get(otherGameObject);
                if (component != null && otherComponent != null)
                {
                    component.InvertSignal = otherComponent.InvertSignal;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("UpdateLogicCircuit")]
        public static void UpdateLogicCircuit_Postfix(BatterySmart __instance, LogicPorts ___ports)
        {
            SensorInverter inverter = SensorInverter.Get(__instance.gameObject);
            if (inverter == null || !inverter.InvertSignal)
                return;

            // The original method already sent a signal - we need to invert it
            // Read the current port state and invert if needed
            int cell = __instance.GetComponent<Building>().GetLogicOutputCellByIndex(0);
            bool currentSignal = Game.Instance.logicCircuitSystem.GetNetworkForCell(cell)?.OutputValue == 1;
            
            // Only re-send if we're actually inverting
            if (inverter.InvertSignal)
            {
                ___ports.SendSignal(BatterySmart.PORT_ID, currentSignal ? 0 : 1);
            }
        }
    }

    #endregion

    #region Liquid Reservoir

    [HarmonyPatch(typeof(LiquidReservoirConfig))]
    public class LiquidReservoirConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LiquidReservoirConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    #endregion

    #region Gas Reservoir

    [HarmonyPatch(typeof(GasReservoirConfig))]
    public class GasReservoirConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GasReservoirConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    #endregion

    #region SmartReservoir (handles both Liquid and Gas Reservoir logic)

    [HarmonyPatch(typeof(SmartReservoir))]
    public class SmartReservoir_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SmartReservoir.OnCopySettings))]
        public static void OnCopySettings(SmartReservoir __instance, object data)
        {
            GameObject otherGameObject = (GameObject)data;
            if (otherGameObject != null)
            {
                SensorInverter component = SensorInverter.Get(__instance.gameObject);
                SensorInverter otherComponent = SensorInverter.Get(otherGameObject);
                if (component != null && otherComponent != null)
                {
                    component.InvertSignal = otherComponent.InvertSignal;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("UpdateLogicCircuit")]
        public static void UpdateLogicCircuit_Postfix(SmartReservoir __instance, LogicPorts ___logicPorts)
        {
            SensorInverter inverter = SensorInverter.Get(__instance.gameObject);
            if (inverter == null || !inverter.InvertSignal)
                return;

            // Read the current port state and invert it
            int cell = __instance.GetComponent<Building>().GetLogicOutputCellByIndex(0);
            bool currentSignal = Game.Instance.logicCircuitSystem.GetNetworkForCell(cell)?.OutputValue == 1;
            
            if (inverter.InvertSignal)
            {
                ___logicPorts.SendSignal(SmartReservoir.PORT_ID, currentSignal ? 0 : 1);
            }
        }
    }

    #endregion

    #region Radbolt Chamber (HEPBattery)

    [HarmonyPatch(typeof(HEPBatteryConfig))]
    public class HEPBatteryConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(HEPBatteryConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(HighEnergyParticleStorage))]
    public class HighEnergyParticleStorage_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(HighEnergyParticleStorage.OnCopySettings))]
        public static void OnCopySettings(HighEnergyParticleStorage __instance, object data)
        {
            GameObject otherGameObject = (GameObject)data;
            if (otherGameObject != null)
            {
                SensorInverter component = SensorInverter.Get(__instance.gameObject);
                SensorInverter otherComponent = SensorInverter.Get(otherGameObject);
                if (component != null && otherComponent != null)
                {
                    component.InvertSignal = otherComponent.InvertSignal;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("UpdateLogicCircuit")]
        public static void UpdateLogicCircuit_Postfix(HighEnergyParticleStorage __instance, LogicPorts ___ports)
        {
            SensorInverter inverter = SensorInverter.Get(__instance.gameObject);
            if (inverter == null || !inverter.InvertSignal)
                return;

            // Read the current port state and invert it
            int cell = __instance.GetComponent<Building>().GetLogicOutputCellByIndex(0);
            bool currentSignal = Game.Instance.logicCircuitSystem.GetNetworkForCell(cell)?.OutputValue == 1;
            
            if (inverter.InvertSignal)
            {
                ___ports.SendSignal(HighEnergyParticleStorage.PORT_ID, currentSignal ? 0 : 1);
            }
        }
    }

    #endregion
}
