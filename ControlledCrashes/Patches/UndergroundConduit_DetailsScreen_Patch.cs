using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    [HarmonyPatch]
    public class UndergroundConduit_DetailsScreen_Patch
    {
        public static bool Prepare()
        {
            Type type = Type.GetType("UndergroundConduit.Mod, UndergroundConduit");
            if (type == null)
            {
                return false;
            }

            Type nestedType = type.GetNestedType("DetailsScreen_OnSelectObject_Patch", BindingFlags.Public | BindingFlags.NonPublic);
            return nestedType != null;
        }

        public static MethodBase TargetMethod()
        {
            Type type = Type.GetType("UndergroundConduit.Mod, UndergroundConduit");
            if (type == null)
            {
                return null;
            }

            Type nestedType = type.GetNestedType("DetailsScreen_OnSelectObject_Patch", BindingFlags.Public | BindingFlags.NonPublic);
            if (nestedType == null)
            {
                return null;
            }

            return AccessTools.Method(nestedType, "Prefix");
        }

        public static Exception Finalizer(Exception __exception, DetailsScreen __instance)
        {
            if (__exception != null && __exception is NullReferenceException)
            {
                string targetInfo = "DetailsScreen";
                int cell = -1;

                if (__instance != null && __instance.target != null)
                {
                    targetInfo = CrashTracker.GetEntityInfo(__instance.target);
                    cell = Grid.PosToCell(__instance.target);
                }

                string cellInfo = CrashTracker.GetCellInfo(cell);
                int? targetId = __instance?.target?.GetInstanceID();
                string entityKey = string.Format("DetailsScreen_{0}", targetId.GetValueOrDefault(-1));
                int count = CrashTracker.IncrementCrash(entityKey);

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [LOW] Underground Conduit DetailsScreen NullReferenceException:");
                Debug.LogWarning("  Selected Object: " + targetInfo);
                Debug.LogWarning("  Location: " + cellInfo);
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                Debug.LogWarning("  Exception: " + __exception.Message);
                Debug.LogWarning("  Mod: Underground Conduit");

                if (count >= 3)
                {
                    Debug.LogWarning("  RECOMMENDATION: Details panel crashes repeatedly for this object. This is a known Underground Conduit mod issue.");
                }

                return null;
            }

            return __exception;
        }
    }
}
