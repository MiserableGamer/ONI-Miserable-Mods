using HarmonyLib;
using UnityEngine;
using ControlledAutomation.Components;
using ControlledAutomation.Options;

namespace ControlledAutomation.Patches
{
    public static class InversionHelper
    {
        public static bool IsInversionEnabled()
        {
            try { return ControlledAutomationOptions.Instance?.EnableAutomationInversion ?? true; }
            catch { return true; }
        }
    }

    // Core patch that applies signal inversion
    [HarmonyPatch(typeof(LogicPorts), nameof(LogicPorts.SendSignal), new System.Type[] { typeof(HashedString), typeof(int) })]
    public static class LogicPorts_SendSignal_Patch
    {
        public static void Prefix(LogicPorts __instance, ref int new_value)
        {
            if (!InversionHelper.IsInversionEnabled())
                return;

            var inverter = SensorInverter.Get(__instance.gameObject) 
                ?? __instance.gameObject.GetComponent<SensorInverter>();
            
            if (inverter?.InvertSignal == true)
                new_value = (new_value != 0) ? 0 : 1;
        }
    }

    // Only sensors without vanilla above/below threshold controls benefit from inversion
    // Element sensors detect presence/absence without thresholds, so inversion is useful

    #region Element Sensors (Vanilla)

    [HarmonyPatch(typeof(LiquidConduitElementSensorConfig), nameof(LiquidConduitElementSensorConfig.DoPostConfigureComplete))]
    public static class LiquidConduitElementSensorConfig_Patch
    {
        public static void Postfix(GameObject go) => go.AddOrGet<SensorInverter>();
    }

    [HarmonyPatch(typeof(GasConduitElementSensorConfig), nameof(GasConduitElementSensorConfig.DoPostConfigureComplete))]
    public static class GasConduitElementSensorConfig_Patch
    {
        public static void Postfix(GameObject go) => go.AddOrGet<SensorInverter>();
    }

    [HarmonyPatch(typeof(SolidConduitElementSensorConfig), nameof(SolidConduitElementSensorConfig.DoPostConfigureComplete))]
    public static class SolidConduitElementSensorConfig_Patch
    {
        public static void Postfix(GameObject go) => go.AddOrGet<SensorInverter>();
    }

    [HarmonyPatch(typeof(LogicElementSensorLiquidConfig), nameof(LogicElementSensorLiquidConfig.DoPostConfigureComplete))]
    public static class LogicElementSensorLiquidConfig_Patch
    {
        public static void Postfix(GameObject go) => go.AddOrGet<SensorInverter>();
    }

    [HarmonyPatch(typeof(LogicElementSensorGasConfig), nameof(LogicElementSensorGasConfig.DoPostConfigureComplete))]
    public static class LogicElementSensorGasConfig_Patch
    {
        public static void Postfix(GameObject go) => go.AddOrGet<SensorInverter>();
    }

    #endregion

    #region Limit Valves

    [HarmonyPatch(typeof(LiquidLimitValveConfig), nameof(LiquidLimitValveConfig.DoPostConfigureComplete))]
    public static class LiquidLimitValveConfig_Patch
    {
        public static void Postfix(GameObject go) => go.AddOrGet<SensorInverter>();
    }

    [HarmonyPatch(typeof(GasLimitValveConfig), nameof(GasLimitValveConfig.DoPostConfigureComplete))]
    public static class GasLimitValveConfig_Patch
    {
        public static void Postfix(GameObject go) => go.AddOrGet<SensorInverter>();
    }

    [HarmonyPatch(typeof(SolidLimitValveConfig), nameof(SolidLimitValveConfig.DoPostConfigureComplete))]
    public static class SolidLimitValveConfig_Patch
    {
        public static void Postfix(GameObject go) => go.AddOrGet<SensorInverter>();
    }

    #endregion

    #region Other Sensors Without Threshold Controls

    [HarmonyPatch(typeof(LogicDuplicantSensorConfig), nameof(LogicDuplicantSensorConfig.DoPostConfigureComplete))]
    public static class LogicDuplicantSensorConfig_Patch
    {
        public static void Postfix(GameObject go) => go.AddOrGet<SensorInverter>();
    }

    [HarmonyPatch(typeof(LogicTimeOfDaySensorConfig), nameof(LogicTimeOfDaySensorConfig.DoPostConfigureComplete))]
    public static class LogicTimeOfDaySensorConfig_Patch
    {
        public static void Postfix(GameObject go) => go.AddOrGet<SensorInverter>();
    }

    [HarmonyPatch(typeof(LogicCounterConfig), nameof(LogicCounterConfig.DoPostConfigureComplete))]
    public static class LogicCounterConfig_Patch
    {
        public static void Postfix(GameObject go) => go.AddOrGet<SensorInverter>();
    }

    [HarmonyPatch(typeof(LogicClusterLocationSensorConfig), nameof(LogicClusterLocationSensorConfig.DoPostConfigureComplete))]
    public static class LogicClusterLocationSensorConfig_Patch
    {
        public static void Postfix(GameObject go) => go.AddOrGet<SensorInverter>();
    }

    #endregion
}
