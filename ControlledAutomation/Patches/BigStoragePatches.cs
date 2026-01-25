using HarmonyLib;
using UnityEngine;
using ControlledAutomation.Components;
using ControlledAutomation.Options;

namespace ControlledAutomation.Patches
{
    // Adds threshold/inversion support to any mod using SmartReservoir, StorageLockerSmart, or Refrigerator
    // Works with Big Storage, Ronivan's Legacy, and similar mods

    [HarmonyPatch(typeof(SmartReservoir), "OnSpawn")]
    public static class SmartReservoir_OnSpawn_Patch
    {
        public static void Postfix(SmartReservoir __instance)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                __instance.gameObject.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(StorageLockerSmart), "OnSpawn")]
    public static class StorageLockerSmart_OnSpawn_Patch
    {
        public static void Postfix(StorageLockerSmart __instance)
        {
            __instance.gameObject.AddOrGet<StorageThresholds>();
        }
    }

    [HarmonyPatch(typeof(Refrigerator), "OnSpawn")]
    public static class Refrigerator_OnSpawn_Patch
    {
        public static void Postfix(Refrigerator __instance)
        {
            __instance.gameObject.AddOrGet<RefrigeratorThresholds>();
        }
    }
}
