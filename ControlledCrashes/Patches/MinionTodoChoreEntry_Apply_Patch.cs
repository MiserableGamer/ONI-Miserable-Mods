using System;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    [HarmonyPatch(typeof(MinionTodoChoreEntry), "Apply")]
    public class MinionTodoChoreEntry_Apply_Patch
    {
        public static bool Prefix(MinionTodoChoreEntry __instance, Chore.Precondition.Context context)
        {
            try
            {
                if (context.chore == null)
                {
                    string minionName = context.consumerState?.consumer?.name ?? "Unknown";
                    Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] MinionTodoChoreEntry: Chore is null");
                    Debug.LogWarning("  Minion: " + minionName);
                    Debug.LogWarning("  Skipping to prevent crash");
                    return false;
                }

                if (context.consumerState == null || context.consumerState.consumer == null)
                {
                    string choreType = context.chore?.choreType?.Name ?? "Unknown";
                    Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] MinionTodoChoreEntry: Consumer is null");
                    Debug.LogWarning("  Chore: " + choreType);
                    Debug.LogWarning("  Skipping to prevent crash");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] MinionTodoChoreEntry.Apply crashed in Prefix: " + ex.Message);
                return false;
            }
        }

        public static Exception Finalizer(Exception __exception, Chore.Precondition.Context context)
        {
            if (__exception != null)
            {
                string minionName = context.consumerState?.consumer?.name ?? "Unknown Minion";
                string choreType = context.chore?.choreType?.Name ?? "Unknown Chore";
                GameObject target = context.chore?.target?.gameObject;
                string entityInfo = CrashTracker.GetEntityInfo(target);
                string entityKey = string.Format("MinionTodo_{0}", context.consumerState?.consumer?.GetInstanceID());
                int count = CrashTracker.IncrementCrash(entityKey);

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [HIGH] MinionTodoChoreEntry.Apply crash:");
                Debug.LogWarning("  Minion: " + minionName);
                Debug.LogWarning("  Chore: " + choreType);
                Debug.LogWarning("  Target: " + entityInfo);
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                Debug.LogWarning("  Exception: " + __exception.Message);

                return null;
            }

            return __exception;
        }
    }
}
