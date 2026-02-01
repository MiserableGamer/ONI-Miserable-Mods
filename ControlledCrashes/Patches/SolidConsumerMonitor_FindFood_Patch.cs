using System;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    [HarmonyPatch(typeof(SolidConsumerMonitor), "FindFood")]
    public class SolidConsumerMonitor_FindFood_Patch
    {
        public static Exception Finalizer(Exception __exception, SolidConsumerMonitor.Instance smi)
        {
            if (__exception != null && __exception is NullReferenceException)
            {
                GameObject gameObject = smi?.gameObject;
                string entityInfo = CrashTracker.GetEntityInfo(gameObject);
                int cell = gameObject != null ? Grid.PosToCell(gameObject) : -1;
                string cellInfo = CrashTracker.GetCellInfo(cell);
                string species = "Unknown";

                if (gameObject != null)
                {
                    KPrefabID prefabId = gameObject.GetComponent<KPrefabID>();
                    if (prefabId != null)
                    {
                        species = prefabId.PrefabTag.ToString();
                    }
                }

                string entityKey = string.Format("CritterFood_{0}", gameObject?.GetInstanceID());
                int count = CrashTracker.IncrementCrash(entityKey);

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [HIGH] SolidConsumerMonitor.FindFood crash:");
                Debug.LogWarning("  Critter: " + entityInfo);
                Debug.LogWarning("  Species: " + species);
                Debug.LogWarning("  Location: " + cellInfo);
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                Debug.LogWarning("  Exception: " + __exception.Message);
                Debug.LogWarning("  Cause: Food GameObject was null when critter tried to pathfind to it (likely destroyed/consumed between checks)");

                if (count >= 5)
                {
                    Debug.LogWarning(string.Format("  RECOMMENDATION: This {0} has crashed {1} times while looking for food. May indicate food source issues or corrupted critter data.", species, count));
                }

                return null;
            }

            return __exception;
        }
    }
}
