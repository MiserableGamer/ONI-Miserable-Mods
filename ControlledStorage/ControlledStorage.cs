using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using PeterHan.PLib.UI;
using UnityEngine;
using ControlledStorage.UI;

namespace ControlledStorage
{
    // ControlledStorage - Unified storage control mod for Oxygen Not Included.
    // Features: Empty Storage, Controlled Filtering, Capacity Control, Delivery Control, No-Sweep Zones
    public sealed class ControlledStorageMod : UserMod2
    {
        public static ControlledStorageMod Instance { get; private set; }

        public override void OnLoad(Harmony harmony)
        {
            Instance = this;
            base.OnLoad(harmony);

            PUtil.InitLibrary();
            ConfigMigrationHelper.Migrate(ConfigMigrationHelper.OldConfigFolderName, ConfigMigrationHelper.NewConfigFolderName);
            new POptions().RegisterOptions(this, typeof(ControlledStorageOptions));
            ControlledStorageStrings.RegisterStrings();
            new PeterHan.PLib.PatchManager.PPatchManager(harmony).RegisterPatchClass(typeof(Patches.NoSweepZonePatches));
            Patches.NoSweepZonePatches.OnModLoad(harmony);
            harmony.PatchAll();
        }
    }

    // Reverted to Workshop/temp code - uses PLib (will crash on U57 in test env with limited mods)
    [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
    public static class DetailsScreen_OnPrefabInit_Patch
    {
        private static DetailsScreen _lastDetailsScreen;

        internal static bool Prepare() => ControlledStorageOptions.Instance.EnableDeliveryControl;

        internal static void Postfix(DetailsScreen __instance)
        {
            if (__instance != _lastDetailsScreen)
            {
                _lastDetailsScreen = __instance;
                PUIUtils.AddSideScreenContentWithOrdering<DeliveryControlSideScreen>(typeof(TreeFilterableSideScreen).FullName, true, null);
            }
        }
    }
}
