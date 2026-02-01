using System;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    [HarmonyPatch(typeof(MinionBrain), "AddAnimTracker")]
    public class MinionBrain_AddAnimTracker_Patch
    {
        public static Exception Finalizer(Exception __exception, MinionBrain __instance, GameObject go)
        {
            if (__exception != null && __exception is NullReferenceException)
            {
                GameObject gameObject = __instance?.gameObject;
                string entityInfo = CrashTracker.GetEntityInfo(gameObject);
                string itemInfo = CrashTracker.GetEntityInfo(go);
                string minionName = "Unknown";

                if (gameObject != null)
                {
                    MinionIdentity identity = gameObject.GetComponent<MinionIdentity>();
                    if (identity != null)
                    {
                        minionName = identity.name;
                    }
                }

                string entityKey = string.Format("MinionAnimTracker_{0}", gameObject?.GetInstanceID());
                int count = CrashTracker.IncrementCrash(entityKey);

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [MEDIUM] MinionBrain.AddAnimTracker crash:");
                Debug.LogWarning("  Minion: " + minionName);
                Debug.LogWarning("  Minion Object: " + entityInfo);
                Debug.LogWarning("  Item Being Tracked: " + itemInfo);
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                Debug.LogWarning("  Exception: " + __exception.Message);
                Debug.LogWarning("  Cause: Trying to track animation for GameObject that was destroyed (common with bionic dupes and electrobanks)");

                if (count >= 5)
                {
                    Debug.LogWarning(string.Format("  RECOMMENDATION: {0} has crashed {1} times trying to track items. This may be caused by Popup Control mod or timing issues with bionic dupes.", minionName, count));
                }

                return null;
            }

            return __exception;
        }
    }
}
