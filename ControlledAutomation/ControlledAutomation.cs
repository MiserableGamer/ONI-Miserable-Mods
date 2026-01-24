using HarmonyLib;
using PeterHan.PLib.Core;

namespace ControlledAutomation
{
    public class ControlledAutomationMod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            // Initialize PLib
            PUtil.InitLibrary();

            // Apply Harmony patches
            harmony.PatchAll();
        }
    }
}
