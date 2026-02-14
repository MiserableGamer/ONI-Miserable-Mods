using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using UnityEngine;

namespace ControlledWarnings
{
    public class ControlledWarningsMod : KMod.UserMod2
    {
        public const bool EnableDebugLogs = false;

        public static void DebugLog(string message)
        {
            if (EnableDebugLogs)
                Debug.Log($"[ControlledWarnings] {message}");
        }

        public static void DebugLogWarning(string message)
        {
            if (EnableDebugLogs)
                Debug.LogWarning($"[ControlledWarnings] {message}");
        }

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary();
            ConfigMigrationHelper.Migrate("ControlledWarnings.dll", "ControlledWarnings");
            new POptions().RegisterOptions(this, typeof(Options.ControlledWarningsOptions));
            harmony.PatchAll();
            Debug.Log("[ControlledWarnings] Loaded");
        }
    }
}
