using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;

namespace ControlledMorale
{
    public sealed class ControlledMoraleMod : UserMod2
    {
        public static ControlledMoraleMod Instance { get; private set; }

        public override void OnLoad(Harmony harmony)
        {
            Instance = this;
            base.OnLoad(harmony);
            PUtil.InitLibrary();
            harmony.PatchAll();
        }
    }
}
