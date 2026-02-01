using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    [HarmonyPatch]
    public class PowerTerminal_OnOperationalChanged_Patch
    {
        public static bool Prepare()
        {
            return Type.GetType("UndergroundConduit.PowerTerminal, UndergroundConduit") != null;
        }

        public static MethodBase TargetMethod()
        {
            Type type = Type.GetType("UndergroundConduit.PowerTerminal, UndergroundConduit");
            if (type == null)
            {
                return null;
            }

            return AccessTools.Method(type, "OnOperationalChanged");
        }

        public static Exception Finalizer(Exception __exception, object __instance)
        {
            if (__exception != null && __exception is InvalidCastException)
            {
                Component component = __instance as Component;
                GameObject gameObject = component?.gameObject;
                string entityInfo = CrashTracker.GetEntityInfo(gameObject);
                int cell = gameObject != null ? Grid.PosToCell(gameObject) : -1;
                string cellInfo = CrashTracker.GetCellInfo(cell);
                string entityKey = string.Format("PowerTerminal_{0}", gameObject?.GetInstanceID());
                int count = CrashTracker.IncrementCrash(entityKey);

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [MEDIUM] PowerTerminal.OnOperationalChanged InvalidCastException:");
                Debug.LogWarning("  Terminal: " + entityInfo);
                Debug.LogWarning("  Location: " + cellInfo);
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                Debug.LogWarning("  Exception: " + __exception.Message);
                Debug.LogWarning("  Mod: Underground Conduit");

                if (count >= 5)
                {
                    Debug.LogWarning(string.Format("  RECOMMENDATION: This Power Terminal has crashed {0} times. Consider deconstructing and rebuilding at {1}", count, cellInfo));
                }

                return null;
            }

            return __exception;
        }
    }
}
