using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;

namespace BlankProject
{
    public sealed class BlankProjectMod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary();
            
            // If using PLib Options, uncomment:
            // new PeterHan.PLib.Options.POptions().RegisterOptions(this, typeof(BlankProjectOptions));
            
            harmony.PatchAll();
        }
    }
}
