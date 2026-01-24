using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using ControlledAutomation.Options;

namespace ControlledAutomation
{
    public class ControlledAutomationMod : KMod.UserMod2
    {
        public const bool EnableDebugLogs = false;

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            // Initialize PLib
            PUtil.InitLibrary(false);
            new POptions().RegisterOptions(this, typeof(ControlledAutomationOptions));

            DebugLog("ControlledAutomation loaded");
        }

        public static void DebugLog(string message)
        {
            if (EnableDebugLogs)
                Debug.Log($"[ControlledAutomation] {message}");
        }

        public static void Log(string message)
        {
            Debug.Log($"[ControlledAutomation] {message}");
        }
    }
}
