using HarmonyLib;
using UnityEngine;
using ControlledAutomation.Components;
using ControlledAutomation.Options;

namespace ControlledAutomation.Patches
{
    /// <summary>
    /// Patches for storage buildings that need BOTH thresholds and inversion.
    /// - Smart Storage Bin (StorageLockerSmart)
    /// - Refrigerator
    /// </summary>

    #region Smart Storage Bin

    [HarmonyPatch(typeof(StorageLockerSmartConfig))]
    public class StorageLockerSmartConfig_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(StorageLockerSmartConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableStorageThresholds)
                go.AddOrGet<StorageThresholds>();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StorageLockerSmartConfig.CreateBuildingDef))]
        public static void CreateBuildingDef(ref BuildingDef __result)
        {
            if (ControlledAutomationOptions.Instance.ReducedSmartStoragePower)
                __result.EnergyConsumptionWhenActive = 20f;
        }
    }

    // OnCopySettings is inherited from StorageLocker
    [HarmonyPatch(typeof(StorageLocker))]
    public class StorageLocker_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(StorageLocker.OnCopySettings))]
        public static void OnCopySettings(StorageLocker __instance, object data)
        {
            GameObject otherGameObject = (GameObject)data;
            if (otherGameObject != null)
            {
                StorageThresholds component = __instance.gameObject.GetComponent<StorageThresholds>();
                StorageThresholds otherComponent = otherGameObject.GetComponent<StorageThresholds>();
                if (component != null && otherComponent != null)
                {
                    component.InvertSignal = otherComponent.InvertSignal;
                    component.ActivateValue = otherComponent.ActivateValue;
                    component.DeactivateValue = otherComponent.DeactivateValue;
                }
            }
        }
    }

    [HarmonyPatch(typeof(StorageLockerSmart))]
    public class StorageLockerSmart_Patch
    {
        private delegate float FloatDelegate(FilteredStorage storage);
        private static readonly FloatDelegate getAmountStoredMethod
            = AccessTools.MethodDelegate<FloatDelegate>(
                AccessTools.Method(typeof(FilteredStorage), "GetAmountStored"));
        private static readonly FloatDelegate getMaxCapacityMethod
            = AccessTools.MethodDelegate<FloatDelegate>(
                AccessTools.Method(typeof(FilteredStorage), "GetMaxCapacityMinusStorageMargin"));

        [HarmonyPrefix]
        [HarmonyPatch("UpdateLogicAndActiveState")]
        public static bool UpdateLogicAndActiveState(StorageLockerSmart __instance, 
            FilteredStorage ___filteredStorage, Operational ___operational, LogicPorts ___ports)
        {
            ThresholdsBase component = ThresholdsBase.Get(__instance.gameObject);
            if (component == null)
                return true; // Run original

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
            return false; // Skip original
        }
    }

    #endregion

    #region Refrigerator

    [HarmonyPatch(typeof(RefrigeratorConfig))]
    public class RefrigeratorConfig_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(RefrigeratorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableStorageThresholds)
                go.AddOrGet<RefrigeratorThresholds>();
        }
    }

    [HarmonyPatch(typeof(Refrigerator))]
    public class Refrigerator_Patch
    {
        private delegate float FloatDelegate(FilteredStorage storage);
        private static readonly FloatDelegate getAmountStoredMethod
            = AccessTools.MethodDelegate<FloatDelegate>(
                AccessTools.Method(typeof(FilteredStorage), "GetAmountStored"));
        private static readonly FloatDelegate getMaxCapacityMethod
            = AccessTools.MethodDelegate<FloatDelegate>(
                AccessTools.Method(typeof(FilteredStorage), "GetMaxCapacityMinusStorageMargin"));

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Refrigerator.OnCopySettings))]
        public static void OnCopySettings(Refrigerator __instance, object data)
        {
            GameObject otherGameObject = (GameObject)data;
            if (otherGameObject != null)
            {
                RefrigeratorThresholds component = __instance.gameObject.GetComponent<RefrigeratorThresholds>();
                RefrigeratorThresholds otherComponent = otherGameObject.GetComponent<RefrigeratorThresholds>();
                if (component != null && otherComponent != null)
                {
                    component.InvertSignal = otherComponent.InvertSignal;
                    component.ActivateValue = otherComponent.ActivateValue;
                    component.DeactivateValue = otherComponent.DeactivateValue;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("UpdateLogicCircuit")]
        public static bool UpdateLogicCircuit(Refrigerator __instance, 
            FilteredStorage ___filteredStorage, Operational ___operational, LogicPorts ___ports)
        {
            ThresholdsBase component = ThresholdsBase.Get(__instance.gameObject);
            if (component == null)
                return true; // Run original

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
            return false; // Skip original
        }
    }

    #endregion
}
