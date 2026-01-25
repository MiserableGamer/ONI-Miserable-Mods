using HarmonyLib;
using UnityEngine;
using ControlledAutomation.Components;
using ControlledAutomation.Options;

namespace ControlledAutomation.Patches
{
    // Storage buildings that need thresholds added

    #region Smart Storage Bin

    [HarmonyPatch(typeof(StorageLockerSmartConfig), nameof(StorageLockerSmartConfig.DoPostConfigureComplete))]
    public static class StorageLockerSmartConfig_Patch
    {
        [HarmonyPrefix]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableStorageThresholds)
                go.AddOrGet<StorageThresholds>();
        }
    }

    [HarmonyPatch(typeof(StorageLockerSmart), "UpdateLogicAndActiveState")]
    public static class StorageLockerSmart_Patch
    {
        private delegate float FloatDelegate(FilteredStorage storage);
        private static readonly FloatDelegate getAmountStoredMethod =
            AccessTools.MethodDelegate<FloatDelegate>(AccessTools.Method(typeof(FilteredStorage), "GetAmountStored"));
        private static readonly FloatDelegate getMaxCapacityMethod =
            AccessTools.MethodDelegate<FloatDelegate>(AccessTools.Method(typeof(FilteredStorage), "GetMaxCapacityMinusStorageMargin"));

        public static bool Prefix(StorageLockerSmart __instance,
            FilteredStorage ___filteredStorage, Operational ___operational, LogicPorts ___ports)
        {
            ThresholdsBase component = ThresholdsBase.Get(__instance.gameObject);
            if (component == null)
                return true;

            float stored = getAmountStoredMethod(___filteredStorage);
            float capacity = getMaxCapacityMethod(___filteredStorage);
            bool isOperational = component.IsActuallyOperational(___operational);
            bool activated = component.UpdateLogicState(stored / capacity);
            bool flag = activated && isOperational;

            if (flag != component.LastSetFlag)
            {
                ___ports.SendSignal(FilteredStorage.FULL_PORT_ID, flag ? 1 : 0);
                component.LastSetFlag = flag;
            }

            ___filteredStorage.SetLogicMeter(flag);
            ___operational.SetActive(isOperational);
            return false;
        }
    }

    #endregion

    #region Refrigerator

    [HarmonyPatch(typeof(RefrigeratorConfig), nameof(RefrigeratorConfig.DoPostConfigureComplete))]
    public static class RefrigeratorConfig_Patch
    {
        [HarmonyPrefix]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableStorageThresholds)
                go.AddOrGet<RefrigeratorThresholds>();
        }
    }

    [HarmonyPatch(typeof(Refrigerator), "UpdateLogicCircuit")]
    public static class Refrigerator_Patch
    {
        private delegate float FloatDelegate(FilteredStorage storage);
        private static readonly FloatDelegate getAmountStoredMethod =
            AccessTools.MethodDelegate<FloatDelegate>(AccessTools.Method(typeof(FilteredStorage), "GetAmountStored"));
        private static readonly FloatDelegate getMaxCapacityMethod =
            AccessTools.MethodDelegate<FloatDelegate>(AccessTools.Method(typeof(FilteredStorage), "GetMaxCapacityMinusStorageMargin"));

        public static bool Prefix(Refrigerator __instance,
            FilteredStorage ___filteredStorage, Operational ___operational, LogicPorts ___ports)
        {
            ThresholdsBase component = ThresholdsBase.Get(__instance.gameObject);
            if (component == null)
                return true;

            float stored = getAmountStoredMethod(___filteredStorage);
            float capacity = getMaxCapacityMethod(___filteredStorage);
            bool isOperational = component.IsActuallyOperational(___operational);
            bool activated = component.UpdateLogicState(stored / capacity);
            bool flag = activated && isOperational;

            if (flag != component.LastSetFlag)
            {
                ___ports.SendSignal(FilteredStorage.FULL_PORT_ID, flag ? 1 : 0);
                component.LastSetFlag = flag;
            }

            ___filteredStorage.SetLogicMeter(flag);
            return false;
        }
    }

    #endregion
}
