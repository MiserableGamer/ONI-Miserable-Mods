using HarmonyLib;
using UnityEngine;
using ControlledAutomation.Components;
using ControlledAutomation.Options;

namespace ControlledAutomation.Patches
{
    // Other buildings - Geotuner, Materials Study Terminal

    [HarmonyPatch(typeof(GeoTunerConfig), nameof(GeoTunerConfig.DoPostConfigureComplete))]
    public static class GeoTunerConfig_Patch
    {
        public static void Postfix(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(NuclearResearchCenterConfig), nameof(NuclearResearchCenterConfig.DoPostConfigureComplete))]
    public static class NuclearResearchCenterConfig_Patch
    {
        public static void Postfix(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }
}
