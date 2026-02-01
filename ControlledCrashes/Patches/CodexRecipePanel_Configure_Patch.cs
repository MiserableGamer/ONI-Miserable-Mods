using System;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    [HarmonyPatch(typeof(CodexRecipePanel), "Configure")]
    public class CodexRecipePanel_Configure_Patch
    {
        public static Exception Finalizer(Exception __exception, CodexRecipePanel __instance)
        {
            if (__exception != null && __exception is NullReferenceException)
            {
                string entityKey = "CodexRecipePanel";
                int count = CrashTracker.IncrementCrash(entityKey);

                Debug.LogWarning("[ControlledCrashes] [" + CrashTracker.GetTimestamp() + "] [LOW] CodexRecipePanel.Configure crash:");
                Debug.LogWarning(string.Format("  Crash Count: {0} time(s)", count));
                Debug.LogWarning("  Exception: " + __exception.Message);
                Debug.LogWarning("  Cause: Outdated mod trying to display Codex recipe information with null data");
                Debug.LogWarning("  Common culprit: Ronivan's Legacy - Industrial Revolution (hasn't been updated for current game version)");

                if (count >= 3)
                {
                    Debug.LogWarning(string.Format("  RECOMMENDATION: Codex has crashed {0} times. An outdated mod is incompatible with the current Codex UI.", count));
                    Debug.LogWarning("  Fix: Disable Ronivan's Legacy or other mods that modify the Codex/recipe panels.");
                }

                return null;
            }

            return __exception;
        }
    }
}
