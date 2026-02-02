using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;

namespace BlankProject
{
    public sealed class BlankProjectMod : UserMod2
    {
        public static BlankProjectMod Instance { get; private set; }

        public override void OnLoad(Harmony harmony)
        {
            Instance = this;
            base.OnLoad(harmony);

            PUtil.InitLibrary();
            harmony.PatchAll();
        }
    }
}
