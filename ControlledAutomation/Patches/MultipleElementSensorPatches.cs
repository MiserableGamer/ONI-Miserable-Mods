using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using ControlledAutomation.Components;
using ControlledAutomation.Options;

namespace ControlledAutomation.Patches
{
    // Adds inversion support to the "Multiple Elements Sensors" mod
    // Uses reflection to conditionally patch if the mod is installed

    public static class MultipleElementSensorPatches
    {
        private static bool patchesApplied = false;
        private static Assembly multiElementSensorAssembly = null;

        public static void TryApplyPatches(Harmony harmony)
        {
            if (patchesApplied) return;
            patchesApplied = true;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "MultipleElementSensor")
                {
                    multiElementSensorAssembly = assembly;
                    break;
                }
            }

            if (multiElementSensorAssembly == null)
                return;

            TryPatchComponent(harmony, "MultipleElementSensor.MultipleElementSensor");
            TryPatchComponent(harmony, "MultipleElementSensor.LogicElementsSensor");
        }

        private static void TryPatchComponent(Harmony harmony, string typeName)
        {
            try
            {
                var componentType = multiElementSensorAssembly.GetType(typeName);
                if (componentType == null) return;

                var onSpawnMethod = componentType.GetMethod("OnSpawn", 
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                
                if (onSpawnMethod == null) return;

                var postfixMethod = typeof(MultipleElementSensorPatches)
                    .GetMethod(nameof(OnSpawn_Postfix), BindingFlags.Static | BindingFlags.Public);

                harmony.Patch(onSpawnMethod, postfix: new HarmonyMethod(postfixMethod));
            }
            catch (Exception)
            {
                // Silently fail if patching doesn't work
            }
        }

        public static void OnSpawn_Postfix(Component __instance)
        {
            if (!InversionHelper.IsInversionEnabled()) return;
            __instance.gameObject.AddOrGet<SensorInverter>();
        }
    }
}
