using System;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    [HarmonyPatch(typeof(FetchAreaChore), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(Chore.Precondition.Context) })]
    public class FetchAreaChore_Constructor_Patch
    {
        public static void Prefix(ref Chore.Precondition.Context context)
        {
            if (context.masterPriority.priority_value <= 0)
            {
                GameObject gameObject = context.chore?.target?.gameObject;
                string entityInfo = CrashTracker.GetEntityInfo(gameObject);
                string choreType = context.chore?.choreType?.Name ?? "Unknown";
                int cell = gameObject != null ? Grid.PosToCell(gameObject) : -1;
                string cellInfo = CrashTracker.GetCellInfo(cell);
                string entityKey = string.Format("FetchAreaChore_{0}", gameObject?.GetInstanceID());
                int count = CrashTracker.IncrementCrash(entityKey);

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [HIGH] Invalid FetchAreaChore priority detected:");
                Debug.LogWarning(string.Format("  Priority: {0} (INVALID - fixing to 5)", context.masterPriority.priority_value));
                Debug.LogWarning("  Chore Type: " + choreType);
                Debug.LogWarning("  Target: " + entityInfo);
                Debug.LogWarning("  Location: " + cellInfo);
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));

                StackTrace stackTrace = new StackTrace(true);
                bool foundSuspectMod = false;

                foreach (StackFrame frame in stackTrace.GetFrames())
                {
                    MethodBase method = frame.GetMethod();
                    if (method != null)
                    {
                        string assemblyName = method.DeclaringType?.Assembly?.GetName()?.Name;
                        if (assemblyName != null && assemblyName != "Assembly-CSharp" && 
                            assemblyName != "ControlledCrashes" && assemblyName != "0Harmony")
                        {
                            Debug.LogWarning(string.Format("  SUSPECT MOD FOUND: {0} - Method: {1}.{2}", 
                                assemblyName, method.DeclaringType?.Name, method.Name));
                            foundSuspectMod = true;
                        }
                    }
                }

                if (!foundSuspectMod)
                {
                    Debug.LogWarning("  Source: Vanilla game bug (no suspect mods found)");
                }

                if (count >= 3)
                {
                    if (choreType != null && (choreType.Contains("Farm") || choreType.Contains("farm")))
                    {
                        Debug.LogWarning("  RECOMMENDATION: Farm tile at " + cellInfo + " has priority issues. Dismantle and rebuild the farm tile to fix blank priority display and errand creation.");
                    }
                    else
                    {
                        Debug.LogWarning("  RECOMMENDATION: Building at " + cellInfo + " repeatedly has priority issues. Consider deconstructing and rebuilding.");
                    }
                }

                PrioritySetting fixedPriority = new PrioritySetting(context.masterPriority.priority_class, 5);
                context.masterPriority = fixedPriority;
            }
        }
    }
}
