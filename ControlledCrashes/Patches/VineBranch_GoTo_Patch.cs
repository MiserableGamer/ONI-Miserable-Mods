using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    // Targets vanilla crash: VineBranch.GoTo(root.undevelopedBranch.growing.wild) throws NullReferenceException
    // when the instance's GameObject is null (e.g. shortly after discovering a new vine). Intercept and skip
    // the transition so the game never calls KObjectManager.GetOrCreateObject(null).
    // RESTORE POINT: Remove this file and remove any reference to VineBranch_GoTo_Patch to revert.
    // TargetMethod used because GoTo is declared on generic base StateMachine<...>.GenericInstance; Harmony
    // does not resolve it when patching by VineBranch.Instance.
    [HarmonyPatch]
    public static class VineBranch_GoTo_Patch
    {
        public static MethodBase TargetMethod()
        {
            Type instanceType = typeof(VineBranch).GetNestedType("Instance", BindingFlags.Public | BindingFlags.NonPublic);
            if (instanceType == null)
                return null;
            return AccessTools.Method(instanceType, "GoTo", new Type[] { typeof(StateMachine.BaseState) });
        }

        public static bool Prefix(object __instance, StateMachine.BaseState base_state)
        {
            if (__instance == null)
                return false;

            var comp = __instance as Component;
            if (comp == null || comp.gameObject == null || !comp.gameObject)
            {
                string entityKey = string.Format("VineBranch_GoTo_{0}", __instance.GetHashCode());
                int count = CrashTracker.IncrementCrash(entityKey);
                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] Prevented VineBranch.GoTo crash (null/destroyed GameObject). Target state: " + (base_state?.name ?? "null") + ". Count: " + count);
                return false;
            }

            return true;
        }
    }
}
