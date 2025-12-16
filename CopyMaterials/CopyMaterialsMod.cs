using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;

namespace CopyMaterials
{
    public class CopyMaterialsMod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            // Initialise PLib
            PUtil.InitLibrary();
            CopyMaterialsStrings.Register();

            // Apply Harmony patches if needed
            harmony.PatchAll();
        }
    }
}