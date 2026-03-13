using System;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    /// <summary>
    /// Fixes NullReferenceException in ElectrobankCharger.Instance.TransferChargeToElectrobank
    /// when entering the full state. The crash occurs in Electrobank.Replace when trying to
    /// replace an empty electrobank with a charged one, but the storage, electrobank GameObject,
    /// or required components are null.
    /// 
    /// The crash occurs when:
    /// 1. ElectrobankCharger enters the full state
    /// 2. Enter action calls TransferChargeToElectrobank
    /// 3. Which calls Electrobank.ReplaceEmptyWithCharged
    /// 4. Which calls Electrobank.Replace
    /// 5. Something is null (storage, electrobank GameObject, or component)
    /// </summary>
    [HarmonyPatch(typeof(ElectrobankCharger.Instance), "TransferChargeToElectrobank")]
    public class ElectrobankCharger_TransferChargeToElectrobank_Patch
    {
        public static bool Prefix(ElectrobankCharger.Instance __instance)
        {
            try
            {
                // Null check: instance must exist
                if (__instance == null)
                {
                    Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] ElectrobankCharger.TransferChargeToElectrobank: Instance is null");
                    return false;
                }

                // Null check: storage must exist
                var storage = __instance.Storage;
                if (storage == null)
                {
                    int instanceId = __instance?.gameObject?.GetInstanceID() ?? __instance?.master?.gameObject?.GetInstanceID() ?? 0;
                    string entityKey = string.Format("ElectrobankCharger_StorageNull_{0}", instanceId);
                    int count = CrashTracker.IncrementCrash(entityKey);
                    string entityInfo = CrashTracker.GetEntityInfo(__instance?.master?.gameObject);

                    Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [HIGH] ElectrobankCharger.TransferChargeToElectrobank crash:");
                    Debug.LogWarning("  Entity: " + entityInfo);
                    Debug.LogWarning("  Issue: Storage is null");
                    Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                    Debug.LogWarning("  Skipping to prevent crash");

                    return false;
                }

                // Null check: storage must have items
                if (storage.items == null || storage.items.Count == 0)
                {
                    // No items to transfer - this is normal, not an error
                    return false;
                }

                // Null check: first item must exist and be valid
                var firstItem = storage.items[0];
                if (firstItem == null)
                {
                    int instanceId = __instance?.gameObject?.GetInstanceID() ?? __instance?.master?.gameObject?.GetInstanceID() ?? 0;
                    string entityKey = string.Format("ElectrobankCharger_ItemNull_{0}", instanceId);
                    int count = CrashTracker.IncrementCrash(entityKey);
                    string entityInfo = CrashTracker.GetEntityInfo(__instance?.master?.gameObject);

                    Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [HIGH] ElectrobankCharger.TransferChargeToElectrobank crash:");
                    Debug.LogWarning("  Entity: " + entityInfo);
                    Debug.LogWarning("  Issue: First storage item is null");
                    Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                    Debug.LogWarning("  Skipping to prevent crash");

                    return false;
                }

                // Check if the item is still in storage (might have been removed)
                if (!storage.items.Contains(firstItem))
                {
                    // Item was removed from storage - skip replacement
                    return false;
                }

                // Check if item has required components for replacement
                var prefabId = firstItem.GetComponent<KPrefabID>();
                if (prefabId == null)
                {
                    int instanceId = __instance?.gameObject?.GetInstanceID() ?? __instance?.master?.gameObject?.GetInstanceID() ?? 0;
                    string entityKey = string.Format("ElectrobankCharger_NoPrefabID_{0}", instanceId);
                    int count = CrashTracker.IncrementCrash(entityKey);
                    string entityInfo = CrashTracker.GetEntityInfo(__instance?.master?.gameObject);
                    string itemInfo = CrashTracker.GetEntityInfo(firstItem);

                    Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [HIGH] ElectrobankCharger.TransferChargeToElectrobank crash:");
                    Debug.LogWarning("  Entity: " + entityInfo);
                    Debug.LogWarning("  Item: " + itemInfo);
                    Debug.LogWarning("  Issue: Item missing KPrefabID component");
                    Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                    Debug.LogWarning("  Skipping to prevent crash");

                    return false;
                }

                // All checks passed - allow original method to run
                return true;
            }
            catch (Exception ex)
            {
                int instanceId = __instance?.gameObject?.GetInstanceID() ?? __instance?.master?.gameObject?.GetInstanceID() ?? 0;
                string entityKey = string.Format("ElectrobankCharger_Prefix_{0}", instanceId);
                int count = CrashTracker.IncrementCrash(entityKey);
                string entityInfo = CrashTracker.GetEntityInfo(__instance?.master?.gameObject);

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [HIGH] ElectrobankCharger.TransferChargeToElectrobank Prefix error:");
                Debug.LogWarning("  Entity: " + entityInfo);
                Debug.LogWarning("  Exception: " + ex.Message);
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                Debug.LogWarning("  Skipping to prevent crash");

                return false;
            }
        }

        public static Exception Finalizer(Exception __exception, ElectrobankCharger.Instance __instance)
        {
            if (__exception != null && __exception is NullReferenceException)
            {
                int instanceId = __instance?.gameObject?.GetInstanceID() ?? __instance?.master?.gameObject?.GetInstanceID() ?? 0;
                string entityKey = string.Format("ElectrobankCharger_Replace_{0}", instanceId);
                int count = CrashTracker.IncrementCrash(entityKey);
                string entityInfo = CrashTracker.GetEntityInfo(__instance?.master?.gameObject);
                string storageInfo = __instance?.Storage != null ? CrashTracker.GetEntityInfo(__instance.Storage.gameObject) : "null";

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [HIGH] ElectrobankCharger.TransferChargeToElectrobank crash:");
                Debug.LogWarning("  Entity: " + entityInfo);
                Debug.LogWarning("  Storage: " + storageInfo);
                Debug.LogWarning("  Exception: NullReferenceException in Electrobank.Replace");
                Debug.LogWarning("  Cause: Storage item or component is null during replacement");
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                Debug.LogWarning("  Note: This can happen when items are removed from storage during charging");

                // Suppress the exception - return false to indicate failure
                return null;
            }

            return __exception;
        }
    }
}
