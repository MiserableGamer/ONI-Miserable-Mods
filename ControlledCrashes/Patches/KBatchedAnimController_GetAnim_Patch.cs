using System;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    [HarmonyPatch(typeof(KBatchedAnimController), "GetAnim")]
    public class KBatchedAnimController_GetAnim_Patch
    {
        public static Exception Finalizer(Exception __exception, KBatchedAnimController __instance, int index)
        {
            if (__exception != null && __exception is ArgumentOutOfRangeException)
            {
                GameObject gameObject = __instance?.gameObject;
                string entityInfo = CrashTracker.GetEntityInfo(gameObject);
                int cell = gameObject != null ? Grid.PosToCell(gameObject) : -1;
                string cellInfo = CrashTracker.GetCellInfo(cell);
                string entityKey = string.Format("AnimIndex_{0}", gameObject?.GetInstanceID());
                int count = CrashTracker.IncrementCrash(entityKey);

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [MEDIUM] KBatchedAnimController.GetAnim crash:");
                Debug.LogWarning("  Object: " + entityInfo);
                Debug.LogWarning("  Location: " + cellInfo);
                Debug.LogWarning(string.Format("  Requested Animation Index: {0}", index));
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                Debug.LogWarning("  Exception: " + __exception.Message);
                Debug.LogWarning("  Cause: Animation files missing or corrupted - mod installation incomplete");

                if (count >= 3)
                {
                    Debug.LogWarning("  RECOMMENDATION: This object has missing animation files. Check for 'animationSetFound: False' warnings earlier in the log.");
                    Debug.LogWarning("  Common fix: Verify mod files through Steam Workshop or reinstall problematic mods (e.g., SpaceTreeBranch, custom buildings).");
                }

                return null;
            }

            return __exception;
        }
    }
}
