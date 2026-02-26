using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;

namespace BlankProjectSteamTest
{
    public sealed class BlankProjectSteamTestMod : UserMod2
    {
        public static BlankProjectSteamTestMod Instance { get; private set; }

        public override void OnLoad(Harmony harmony)
        {
            Instance = this;
            base.OnLoad(harmony);

            PUtil.InitLibrary();
            harmony.PatchAll();
        }
    }
}
