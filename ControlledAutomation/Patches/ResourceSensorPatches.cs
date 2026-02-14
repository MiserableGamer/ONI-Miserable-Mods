using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using ControlledAutomation.Components;

namespace ControlledAutomation.Patches
{
    // Adds inversion support to the "Resource Sensor FIXED" mod
    // Uses AccessTools.TypeByName to conditionally patch if the mod is installed

    public static class ResourceSensorPatches
    {
        private static bool patchesApplied = false;

        public static void TryApplyPatches(Harmony harmony)
        {
            if (patchesApplied) return;
            patchesApplied = true;

            try
            {
                var configType = AccessTools.TypeByName("ResourceSensorFIXED.LogicResourceSensorConfig");
                if (configType == null) return;

                var doPostMethod = configType.GetMethod("DoPostConfigureComplete",
                    BindingFlags.Instance | BindingFlags.Public);

                if (doPostMethod == null) return;

                var postfixMethod = typeof(ResourceSensorPatches)
                    .GetMethod(nameof(DoPostConfigureComplete_Postfix), BindingFlags.Static | BindingFlags.Public);

                harmony.Patch(doPostMethod, postfix: new HarmonyMethod(postfixMethod));
            }
            catch (Exception)
            {
                // Silently fail if patching doesn't work
            }
        }

        public static void DoPostConfigureComplete_Postfix(GameObject go)
        {
            if (!InversionHelper.IsInversionEnabled()) return;
            go.AddOrGet<SensorInverter>();
        }
    }
}
