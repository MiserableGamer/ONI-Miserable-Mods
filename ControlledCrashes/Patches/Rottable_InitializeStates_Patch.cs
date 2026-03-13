using System;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    /// <summary>
    /// Fixes NullReferenceException in Rottable.root.Stale.ToggleTag when objects are destroyed
    /// during Pickupable.Absorb. The ToggleTag action accesses smi.master.gameObject which can
    /// be null during destruction.
    /// 
    /// The crash occurs when:
    /// 1. Pickupable.Absorb is called (merging items)
    /// 2. StateMachineController.OnTargetDestroyed triggers
    /// 3. State machine tries to execute ToggleTag actions during cleanup
    /// 4. smi.master.gameObject is null, causing NullReferenceException
    /// </summary>
    [HarmonyPatch(typeof(Rottable), "InitializeStates")]
    public class Rottable_InitializeStates_Patch
    {
        public static void Postfix(Rottable __instance)
        {
            try
            {
                // Replace the Stale state's ToggleTag RemoveTag action with a null-safe version.
                // Vanilla does: this.Stale.ToggleTag(GameTags.RotModifierTags.Stale) which adds
                // Enter "AddTag(Stale)" and Exit "RemoveTag(Stale)". The RemoveTag callback
                // accesses smi.master.gameObject which is null during Pickupable.Absorb destruction.
                var staleState = __instance.Stale;
                if (staleState == null) return;

                // Remove the crashing RemoveTag(Stale) action from exitActions
                const string removeTagActionName = "RemoveTag(Stale)";
                if (staleState.exitActions != null)
                {
                    for (int i = staleState.exitActions.Count - 1; i >= 0; i--)
                    {
                        if (staleState.exitActions[i].name == removeTagActionName)
                        {
                            staleState.exitActions.RemoveAt(i);
                            break;
                        }
                    }
                }

                // Add null-safe Exit handler to replace the removed action
                staleState.Exit((smi) =>
                {
                    try
                    {
                        if (smi?.master == null || smi.master.gameObject == null)
                            return;

                        var prefabId = smi.master.gameObject.GetComponent<KPrefabID>();
                        if (prefabId != null && prefabId.HasTag(GameTags.RotModifierTags.Stale))
                        {
                            prefabId.RemoveTag(GameTags.RotModifierTags.Stale);
                        }
                    }
                    catch (NullReferenceException)
                    {
                        // Object is being destroyed - silently ignore
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("[ControlledCrashes] Rottable Stale exit error: " + ex.Message);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ControlledCrashes] Failed to patch Rottable.InitializeStates: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Catches NullReferenceException from state machine action execution when objects
    /// are destroyed. The ToggleTag action in Rottable.Stale tries to access 
    /// smi.master.gameObject which is null during destruction.
    /// 
    /// This patch catches exceptions at the StateMachineController level, which is called
    /// when objects are destroyed and triggers state machine cleanup.
    /// </summary>
    [HarmonyPatch(typeof(StateMachineController), "OnTargetDestroyed")]
    public class StateMachineController_OnTargetDestroyed_Patch
    {
        public static Exception Finalizer(Exception __exception, StateMachineController __instance, object data)
        {
            if (__exception != null && __exception is NullReferenceException)
            {
                int instanceId = __instance?.gameObject?.GetInstanceID() ?? 0;
                string entityKey = string.Format("StateMachineController_Destroyed_{0}", instanceId);
                int count = CrashTracker.IncrementCrash(entityKey);

                string entityInfo = CrashTracker.GetEntityInfo(__instance?.gameObject);

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [HIGH] StateMachineController.OnTargetDestroyed crash:");
                Debug.LogWarning("  Entity: " + entityInfo);
                Debug.LogWarning("  Exception: NullReferenceException in state machine action");
                Debug.LogWarning("  Cause: Object destroyed (likely Pickupable.Absorb), state machine action accessing null");
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                Debug.LogWarning("  Note: Common with Rottable ToggleTag when items are merged/absorbed");

                // Suppress the exception - the object is being destroyed anyway
                return null;
            }

            return __exception;
        }
    }


}
