using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;

namespace ControlledAssignments
{
    public sealed class ControlledAssignmentsMod : UserMod2
    {
        public static ControlledAssignmentsMod Instance { get; private set; }

        public override void OnLoad(Harmony harmony)
        {
            Instance = this;
            base.OnLoad(harmony);

            PUtil.InitLibrary();
            harmony.PatchAll();
        }
    }
}
