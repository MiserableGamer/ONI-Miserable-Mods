using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using ControlledVisuals.Helper;
using ControlledVisuals.Options;
using ControlledVisuals.Patches;
using CVOptions = ControlledVisuals.Options.ControlledVisualsOptions;

namespace ControlledVisuals
{
    public class ControlledVisualsMod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            PUtil.InitLibrary();

            ConfigMigrationHelper.Migrate(ConfigMigrationHelper.OldConfigFolderName, ConfigMigrationHelper.NewConfigFolderName);
            new POptions().RegisterOptions(this, typeof(CVOptions));

            // Conduit throttling patch remains disabled; only the active fixes are applied via PatchAll.
            // ConduitFlowVisualizerPatches.ApplyPatches(harmony);

            harmony.PatchAll();
        }
    }
}
