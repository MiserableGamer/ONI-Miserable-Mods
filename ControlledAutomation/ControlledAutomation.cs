using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using ControlledAutomation.Options;
using ControlledAutomation.Patches;
using System.Collections.Generic;

namespace ControlledAutomation
{
    public class ControlledAutomationMod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary(false);
            new POptions().RegisterOptions(this, typeof(ControlledAutomationOptions));
        }

        public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods)
        {
            base.OnAllModsLoaded(harmony, mods);

            // Apply conditional patches for other mods (assemblies are now loaded)
            MultipleElementSensorPatches.TryApplyPatches(harmony);
            ResourceSensorPatches.TryApplyPatches(harmony);
        }
    }
}
