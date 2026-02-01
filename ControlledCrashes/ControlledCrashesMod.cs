using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;

namespace ControlledCrashes
{
    public sealed class ControlledCrashesMod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary();
            
            harmony.PatchAll();
        }
    }
}
