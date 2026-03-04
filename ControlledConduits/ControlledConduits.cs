using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;

namespace ControlledConduits
{
    public sealed class ControlledConduitsMod : UserMod2
    {
        public static ControlledConduitsMod Instance { get; private set; }

        public override void OnLoad(Harmony harmony)
        {
            Instance = this;
            base.OnLoad(harmony);

            PUtil.InitLibrary();
            harmony.PatchAll();
        }
    }
}
