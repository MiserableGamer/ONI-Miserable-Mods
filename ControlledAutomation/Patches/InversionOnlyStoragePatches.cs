using HarmonyLib;
using UnityEngine;
using ControlledAutomation.Components;
using ControlledAutomation.Options;

namespace ControlledAutomation.Patches
{
    // Storage buildings that already have thresholds - just need inversion

    [HarmonyPatch(typeof(BatterySmartConfig), nameof(BatterySmartConfig.DoPostConfigureComplete))]
    public static class BatterySmartConfig_Patch
    {
        public static void Postfix(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(LiquidReservoirConfig), nameof(LiquidReservoirConfig.DoPostConfigureComplete))]
    public static class LiquidReservoirConfig_Patch
    {
        public static void Postfix(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(GasReservoirConfig), nameof(GasReservoirConfig.DoPostConfigureComplete))]
    public static class GasReservoirConfig_Patch
    {
        public static void Postfix(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(HEPBatteryConfig), nameof(HEPBatteryConfig.DoPostConfigureComplete))]
    public static class HEPBatteryConfig_Patch
    {
        public static void Postfix(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }
}
