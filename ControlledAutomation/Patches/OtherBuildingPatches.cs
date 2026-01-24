using HarmonyLib;
using UnityEngine;
using ControlledAutomation.Components;
using ControlledAutomation.Options;

namespace ControlledAutomation.Patches
{
    /// <summary>
    /// Patches for other buildings that need inversion only.
    /// - Geotuner (GeoTuner)
    /// - Materials Study Terminal (NuclearResearchCenter)
    /// </summary>

    #region Geotuner

    [HarmonyPatch(typeof(GeoTunerConfig))]
    public class GeoTunerConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GeoTunerConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    #endregion

    #region Materials Study Terminal (NuclearResearchCenter)

    [HarmonyPatch(typeof(NuclearResearchCenterConfig))]
    public class NuclearResearchCenterConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(NuclearResearchCenterConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    #endregion
}
