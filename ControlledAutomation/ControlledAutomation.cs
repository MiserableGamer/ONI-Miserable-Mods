using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using ControlledAutomation.Options;
using ControlledAutomation.Patches;

namespace ControlledAutomation
{
    public class ControlledAutomationMod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary(false);
            ConfigMigrationHelper.Migrate(ConfigMigrationHelper.OldConfigFolderName, ConfigMigrationHelper.NewConfigFolderName);
            new POptions().RegisterOptions(this, typeof(ControlledAutomationOptions));
            
            MultipleElementSensorPatches.TryApplyPatches(harmony);
        }
    }
}
