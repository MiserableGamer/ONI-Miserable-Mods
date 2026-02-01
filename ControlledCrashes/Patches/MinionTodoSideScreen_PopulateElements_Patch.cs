using System;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    [HarmonyPatch(typeof(MinionTodoSideScreen), "PopulateElements")]
    public class MinionTodoSideScreen_PopulateElements_Patch
    {
        public static bool Prefix(MinionTodoSideScreen __instance)
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] MinionTodoSideScreen.PopulateElements crashed in Prefix: " + ex.Message);
                return false;
            }
        }

        public static void Postfix(MinionTodoSideScreen __instance)
        {
        }

        public static Exception Finalizer(Exception __exception, MinionTodoSideScreen __instance)
        {
            if (__exception != null)
            {
                string instanceInfo = "Unknown";
                int instanceId = 0;

                if (__instance != null)
                {
                    instanceId = __instance.GetInstanceID();
                    instanceInfo = string.Format("UI Instance {0}", instanceId);
                }

                string entityKey = string.Format("TodoSideScreen_{0}", instanceId);
                int count = CrashTracker.IncrementCrash(entityKey);

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [HIGH] MinionTodoSideScreen.PopulateElements crash:");
                Debug.LogWarning("  UI Instance: " + instanceInfo);
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                Debug.LogWarning("  Exception: " + __exception.Message);

                if (count >= 3)
                {
                    Debug.LogWarning("  RECOMMENDATION: UI crashes repeatedly. This is usually caused by Chain Tool mod conflicts.");
                }

                return null;
            }

            return __exception;
        }
    }
}
