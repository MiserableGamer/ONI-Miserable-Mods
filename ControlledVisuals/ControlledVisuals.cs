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
            try
            {
                string path = POptions.GetConfigFilePath(typeof(ControlledVisualsOptions));
                ConfigMigrationHelper.MigrateConfigFromFilePath(path);
            }
            catch { /* ignore */ }

            // Initialize debug overlay (only active in DEBUG builds)
            DevDebug.Init();

            // Conduit throttling patch remains disabled; only the active fixes are applied via PatchAll.
            // ConduitFlowVisualizerPatches.ApplyPatches(harmony);

            harmony.PatchAll();
        }
    }
}
