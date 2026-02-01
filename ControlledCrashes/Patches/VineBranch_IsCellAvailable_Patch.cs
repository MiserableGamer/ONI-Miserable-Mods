using System;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    [HarmonyPatch(typeof(VineBranch), "IsCellAvailable")]
    public class VineBranch_IsCellAvailable_Patch
    {
        public static Exception Finalizer(Exception __exception, GameObject questionerObj, int cell, ref bool __result)
        {
            if (__exception != null)
            {
                string entityInfo = CrashTracker.GetEntityInfo(questionerObj);
                string cellInfo = CrashTracker.GetCellInfo(cell);
                string entityKey = string.Format("VineBranch_{0}_{1}", questionerObj?.GetInstanceID(), cell);
                int count = CrashTracker.IncrementCrash(entityKey);

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [MEDIUM] VineBranch.IsCellAvailable crash:");
                Debug.LogWarning("  Vine: " + entityInfo);
                Debug.LogWarning("  Target: " + cellInfo);
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                Debug.LogWarning("  Exception: " + __exception.Message);

                if (count >= 5)
                {
                    Debug.LogWarning(string.Format("  RECOMMENDATION: This vine has crashed {0} times. Consider digging up the vine plant.", count));
                }

                __result = false;
                return null;
            }

            return __exception;
        }
    }
}
