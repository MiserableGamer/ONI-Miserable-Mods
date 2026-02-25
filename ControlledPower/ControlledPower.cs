using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;

namespace ControlledPower
{
    public sealed class ControlledPowerMod : UserMod2
    {
        public static ControlledPowerMod Instance { get; private set; }

        public override void OnLoad(Harmony harmony)
        {
            Instance = this;
            base.OnLoad(harmony);

            PUtil.InitLibrary();
            harmony.PatchAll();
        }
    }
}
