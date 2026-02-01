using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using PeterHan.PLib.UI;
using ControlledStorage.UI;

namespace ControlledStorage
{
    /// <summary>
    /// ControlledStorage - Unified storage control mod for Oxygen Not Included.
    /// 
    /// Features:
    /// - Empty Storage: Create tasks to empty storage buildings
    /// - Controlled Filtering: Configure which item categories are "standard" vs "non-standard"
    /// - Capacity Control: Increase character limit on storage capacity input
    /// - Delivery Control: Control dupe/sweeper deposit and extract permissions (Phase 3)
    /// - No-Sweep Zones: Mark areas where dupes won't sweep from (Phase 4)
    /// </summary>
    public sealed class ControlledStorageMod : UserMod2
    {
        public static ControlledStorageMod Instance { get; private set; }
        
        public override void OnLoad(Harmony harmony)
        {
            Instance = this;
            base.OnLoad(harmony);
            
            PUtil.InitLibrary();
            new POptions().RegisterOptions(this, typeof(ControlledStorageOptions));
            ControlledStorageStrings.RegisterStrings();
            new PeterHan.PLib.PatchManager.PPatchManager(harmony).RegisterPatchClass(typeof(Patches.NoSweepZonePatches));
            Patches.NoSweepZonePatches.OnModLoad(harmony);
            harmony.PatchAll();
        }
    }
    
    /// <summary>
    /// Register DeliveryControlSideScreen via PLib UI (Peter Han).
    /// </summary>
    [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
    public static class DetailsScreen_OnPrefabInit_Patch
    {
        private static DetailsScreen _lastDetailsScreen;

        internal static bool Prepare() => ControlledStorageOptions.Instance.EnableDeliveryControl;

        internal static void Postfix(DetailsScreen __instance)
        {
            // Add only when DetailsScreen instance changes (new screen or recreated after save/load).
            // Prevents duplicates when OnPrefabInit runs multiple times on same instance.
            if (__instance != _lastDetailsScreen)
            {
                _lastDetailsScreen = __instance;
                PUIUtils.AddSideScreenContentWithOrdering<DeliveryControlSideScreen>(
                    typeof(TreeFilterableSideScreen).FullName, insertBefore: true);
            }
        }
    }
}
