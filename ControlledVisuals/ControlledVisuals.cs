using HarmonyLib;
using PeterHan.PLib.Core;
using ControlledVisuals.Helper;
using ControlledVisuals.Patches;

namespace ControlledVisuals
{
    public class ControlledVisualsMod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            PUtil.InitLibrary();

            ConfigMigrationHelper.Migrate(ConfigMigrationHelper.OldConfigFolderName, ConfigMigrationHelper.NewConfigFolderName);

            // Conduit animation options removed for now - mod only applies conveyor-behind-drywall fix
            // ConduitFlowVisualizerPatches.ApplyPatches(harmony);

            harmony.PatchAll();
        }
    }
}
