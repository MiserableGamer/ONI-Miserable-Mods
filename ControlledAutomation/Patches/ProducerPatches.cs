using HarmonyLib;
using UnityEngine;
using ControlledAutomation.Components;
using ControlledAutomation.Options;

namespace ControlledAutomation.Patches
{
    /// <summary>
    /// Patches for producer buildings that need inversion only.
    /// - Critter Trap (CreatureTrap)
    /// - Fish Trap (FishTrap)
    /// - Airbourne Critter Trap (FlyingCreatureBait)
    /// - Diamond Press (DiamondPress)
    /// </summary>

    #region Critter Trap

    [HarmonyPatch(typeof(CreatureTrapConfig))]
    public class CreatureTrapConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CreatureTrapConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(CreatureTrap))]
    public class CreatureTrap_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CreatureTrap.OnCopySettings))]
        public static void OnCopySettings(CreatureTrap __instance, object data)
        {
            CopyInverterSettings(__instance.gameObject, data as GameObject);
        }
    }

    #endregion

    #region Fish Trap

    [HarmonyPatch(typeof(FishTrapConfig))]
    public class FishTrapConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(FishTrapConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    #endregion

    #region Airbourne Critter Trap (FlyingCreatureBait)

    [HarmonyPatch(typeof(FlyingCreatureBaitConfig))]
    public class FlyingCreatureBaitConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(FlyingCreatureBaitConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    #endregion

    #region Diamond Press

    [HarmonyPatch(typeof(DiamondPressConfig))]
    public class DiamondPressConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(DiamondPressConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    #endregion

    #region Helper Methods

    public static class ProducerPatchHelpers
    {
        public static void CopyInverterSettings(GameObject target, GameObject source)
        {
            if (source == null) return;
            
            SensorInverter component = SensorInverter.Get(target);
            SensorInverter otherComponent = SensorInverter.Get(source);
            if (component != null && otherComponent != null)
            {
                component.InvertSignal = otherComponent.InvertSignal;
            }
        }
    }

    // Shorthand helper method used in patches
    internal static void CopyInverterSettings(GameObject target, GameObject source)
    {
        ProducerPatchHelpers.CopyInverterSettings(target, source);
    }

    #endregion
}
