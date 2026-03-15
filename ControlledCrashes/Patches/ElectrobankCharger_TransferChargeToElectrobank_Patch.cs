using System;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    // Fixes NullReferenceException in ElectrobankCharger.Instance.TransferChargeToElectrobank when
    // entering the full state. The crash occurs in Electrobank.Replace when targetElectrobank (the
    // actual parameter used, NOT storage.items[0]) is null, destroyed, or missing PrimaryElement/
    // Pickupable. targetElectrobank can become stale when items are removed during charging (e.g.
    // by dupes or mods like ControlledLoaders). When we skip or suppress, we must call
    // DequeueElectrobank to reset state; otherwise the building gets stuck in a glitch loop.
    [HarmonyPatch(typeof(ElectrobankCharger.Instance), "TransferChargeToElectrobank")]
    public class ElectrobankCharger_TransferChargeToElectrobank_Patch
    {
        private static void SkipAndReset(ElectrobankCharger.Instance instance, string entityKey, string issue)
        {
            int instanceId = instance?.gameObject?.GetInstanceID() ?? instance?.master?.gameObject?.GetInstanceID() ?? 0;
            int count = CrashTracker.IncrementCrash(entityKey);
            string entityInfo = CrashTracker.GetEntityInfo(instance?.master?.gameObject);

            Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [HIGH] ElectrobankCharger.TransferChargeToElectrobank crash:");
            Debug.LogWarning("  Entity: " + entityInfo);
            Debug.LogWarning("  Issue: " + issue);
            Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
            Debug.LogWarning("  Resetting state and skipping to prevent crash");

            instance?.DequeueElectrobank();
        }

        public static bool Prefix(ElectrobankCharger.Instance __instance)
        {
            try
            {
                if (__instance == null)
                {
                    Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] ElectrobankCharger.TransferChargeToElectrobank: Instance is null");
                    return false;
                }

                // Validate targetElectrobank - the actual parameter used by TransferChargeToElectrobank,
                // NOT storage.items[0]. targetElectrobank can differ (first EmptyPortableBattery) or
                // become stale when items are removed during charging.
                var target = __instance.targetElectrobank;
                if (target == null)
                {
                    int instanceId = __instance?.gameObject?.GetInstanceID() ?? __instance?.master?.gameObject?.GetInstanceID() ?? 0;
                    SkipAndReset(__instance, string.Format("ElectrobankCharger_TargetNull_{0}", instanceId),
                        "targetElectrobank is null or destroyed (item removed during charging?)");
                    return false;
                }

                // Electrobank.Replace requires PrimaryElement and Pickupable; null access throws
                if (target.GetComponent<PrimaryElement>() == null)
                {
                    int instanceId = __instance?.gameObject?.GetInstanceID() ?? __instance?.master?.gameObject?.GetInstanceID() ?? 0;
                    SkipAndReset(__instance, string.Format("ElectrobankCharger_NoPrimaryElement_{0}", instanceId),
                        "targetElectrobank missing PrimaryElement component");
                    return false;
                }

                if (target.GetComponent<Pickupable>() == null)
                {
                    int instanceId = __instance?.gameObject?.GetInstanceID() ?? __instance?.master?.gameObject?.GetInstanceID() ?? 0;
                    SkipAndReset(__instance, string.Format("ElectrobankCharger_NoPickupable_{0}", instanceId),
                        "targetElectrobank missing Pickupable component");
                    return false;
                }

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
                Debug.LogWarning("  Resetting state and skipping to prevent crash");

                __instance?.DequeueElectrobank();
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
                Debug.LogWarning("  Cause: targetElectrobank or component is null during replacement");
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                Debug.LogWarning("  Resetting state to prevent glitch loop");

                __instance?.DequeueElectrobank();
                return null;
            }

            return __exception;
        }
    }
}
