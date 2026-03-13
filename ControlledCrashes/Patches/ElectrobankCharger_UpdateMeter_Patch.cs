using System;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    /// <summary>
    /// Fixes NullReferenceException in ElectrobankCharger.Instance.UpdateMeter when called from
    /// third-party patches (e.g. 3GuBsVisualFixesNTweaks UpdateSymbolSwap). Those patches may
    /// call GetComponent on null objects (e.g. Storage.items[0] or KBatchedAnimController) when
    /// the building is in an invalid state or during noBattery.Enter.
    ///
    /// The crash occurs when:
    /// 1. ElectrobankCharger transitions to noBattery (Enter) or other states
    /// 2. QueueElectrobank calls UpdateMeter
    /// 3. A mod's Postfix (e.g. UpdateSymbolSwap) runs and crashes on GetComponent
    /// </summary>
    [HarmonyPatch(typeof(ElectrobankCharger.Instance), nameof(ElectrobankCharger.Instance.UpdateMeter))]
    public class ElectrobankCharger_UpdateMeter_Patch
    {
        public static Exception Finalizer(Exception __exception, ElectrobankCharger.Instance __instance)
        {
            if (__exception != null && __exception is NullReferenceException)
            {
                int instanceId = __instance?.gameObject?.GetInstanceID() ?? __instance?.master?.gameObject?.GetInstanceID() ?? 0;
                string entityKey = string.Format("ElectrobankCharger_UpdateMeter_{0}", instanceId);
                int count = CrashTracker.IncrementCrash(entityKey);
                string entityInfo = CrashTracker.GetEntityInfo(__instance?.master?.gameObject);

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [HIGH] ElectrobankCharger.UpdateMeter crash:");
                Debug.LogWarning("  Entity: " + entityInfo);
                Debug.LogWarning("  Exception: NullReferenceException (often from 3GuBsVisualFixesNTweaks UpdateSymbolSwap)");
                Debug.LogWarning("  Cause: GetComponent on null (storage item, KBatchedAnimController, or SymbolOverrideController)");
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));

                return null;
            }

            return __exception;
        }
    }
}
