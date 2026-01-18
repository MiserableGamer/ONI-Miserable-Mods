using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using ControlledExtraction.Options;

namespace ControlledExtraction
{
    public class ControlledExtractionMod : KMod.UserMod2
    {
        public static bool EnableDebugLogs = false;

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary();
            new POptions().RegisterOptions(this, typeof(ControlledExtractionOptions));
            harmony.PatchAll();
        }

        public static void Log(string message)
        {
            if (EnableDebugLogs)
            {
                Debug.Log($"[ControlledExtraction] {message}");
            }
        }
    }
}
