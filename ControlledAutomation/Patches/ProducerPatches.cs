using HarmonyLib;
using UnityEngine;
using ControlledAutomation.Components;
using ControlledAutomation.Options;

namespace ControlledAutomation.Patches
{
    // Producer buildings - critter traps and diamond press

    [HarmonyPatch(typeof(GroundTrapConfig), nameof(GroundTrapConfig.DoPostConfigureComplete))]
    public static class GroundTrapConfig_Patch
    {
        public static void Postfix(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(AirTrapConfig), nameof(AirTrapConfig.DoPostConfigureComplete))]
    public static class AirTrapConfig_Patch
    {
        public static void Postfix(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(WaterTrapConfig), nameof(WaterTrapConfig.DoPostConfigureComplete))]
    public static class WaterTrapConfig_Patch
    {
        public static void Postfix(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(DiamondPressConfig), nameof(DiamondPressConfig.DoPostConfigureComplete))]
    public static class DiamondPressConfig_Patch
    {
        public static void Postfix(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }
}
