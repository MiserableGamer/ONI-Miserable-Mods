using AdvancedWattageSensor.Options;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace AdvancedWattageSensor
{
    public sealed class AdvancedWattageSensorMod : UserMod2
    {
        public static AdvancedWattageSensorMod Instance { get; private set; }

        public override void OnLoad(Harmony harmony)
        {
            Instance = this;
            base.OnLoad(harmony);

            PUtil.InitLibrary();
            ConfigMigrationHelper.Migrate(ConfigMigrationHelper.OldConfigFolderName, ConfigMigrationHelper.NewConfigFolderName);
            new POptions().RegisterOptions(this, typeof(AdvancedWattageSensorOptions));
            harmony.PatchAll();
        }
    }
}
