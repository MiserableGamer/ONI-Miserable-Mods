using System;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    [HarmonyPatch(typeof(Growing), "ConsumeGrowthUnits")]
    public class Growing_ConsumeGrowthUnits_Patch
    {
        public static Exception Finalizer(Exception __exception, Growing __instance, float units_to_consume, float unit_maturity_ratio)
        {
            if (__exception != null)
            {
                GameObject gameObject = __instance?.gameObject;
                string entityInfo = CrashTracker.GetEntityInfo(gameObject);
                int cell = gameObject != null ? Grid.PosToCell(gameObject) : -1;
                string cellInfo = CrashTracker.GetCellInfo(cell);
                Crop crop = gameObject?.GetComponent<Crop>();
                string cropId = crop?.cropId ?? "Unknown";
                string entityKey = string.Format("Growing_{0}", gameObject?.GetInstanceID());
                int count = CrashTracker.IncrementCrash(entityKey);

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [CRITICAL] Growing.ConsumeGrowthUnits crash:");
                Debug.LogWarning("  Plant: " + entityInfo);
                Debug.LogWarning("  Crop Type: " + cropId);
                Debug.LogWarning("  Location: " + cellInfo);
                Debug.LogWarning(string.Format("  Units to Consume: {0}, Maturity Ratio: {1}", units_to_consume, unit_maturity_ratio));
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                Debug.LogWarning("  Exception: " + __exception.Message);

                if (count >= 3)
                {
                    Debug.LogWarning(string.Format("  RECOMMENDATION: This plant has crashed {0} times. Consider uprooting and replanting at {1}", count, cellInfo));
                }

                return null;
            }

            return __exception;
        }
    }
}
