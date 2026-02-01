using System;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    [HarmonyPatch(typeof(FetchAreaChore.StatesInstance), "DeliverComplete")]
    public class FetchAreaChore_DeliverComplete_Patch
    {
        public static Exception Finalizer(Exception __exception, FetchAreaChore.StatesInstance __instance)
        {
            if (__exception != null && __exception is ArgumentOutOfRangeException)
            {
                string choreInfo = "Unknown Chore";
                int cell = -1;

                if (__instance?.gameObject != null)
                {
                    choreInfo = CrashTracker.GetEntityInfo(__instance.gameObject);
                    cell = Grid.PosToCell(__instance.gameObject);
                }

                string cellInfo = CrashTracker.GetCellInfo(cell);
                string entityKey = string.Format("FetchDeliver_{0}", cell);
                int count = CrashTracker.IncrementCrash(entityKey);

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [HIGH] FetchAreaChore.DeliverComplete crash:");
                Debug.LogWarning("  Chore: " + choreInfo);
                Debug.LogWarning("  Location: " + cellInfo);
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                Debug.LogWarning("  Exception: " + __exception.Message);
                Debug.LogWarning("  Note: ArgumentOutOfRangeException during resource delivery - likely caused by very small resource amounts");

                if (count >= 3)
                {
                    Debug.LogWarning("  RECOMMENDATION: Delivery crashes repeatedly at " + cellInfo + ". Try deconstructing and rebuilding nearby storage containers.");
                }

                return null;
            }

            return __exception;
        }
    }
}
