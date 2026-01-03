using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using CopyTileTool.Logic;

namespace CopyTileTool
{
    public class CopyTileMod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            PUtil.InitLibrary();
            CopyTileStrings.Register();

            harmony.PatchAll();
        }
    }
}

