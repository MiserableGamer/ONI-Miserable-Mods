using HarmonyLib;
using UnityEngine;

namespace ControlledStorage.Patches
{
    // Suppress vanilla "Colony Lacks Required Skill" so we can show our own targeted warnings (dupes vs bionics).
    [HarmonyPatch(typeof(Workable), nameof(Workable.SetShouldShowSkillPerkStatusItem))]
    public static class Workable_SetShouldShowSkillPerkStatusItem_Patch
    {
        internal static bool Prefix(Workable __instance)
        {
            if (__instance is EmptyStorageWorkable)
                return false;
            return true;
        }
    }
    
    // Add EmptyStorageSetting (and delivery control by type) to storage buildings.
    [HarmonyPatch(typeof(Storage), nameof(Storage.OnPrefabInit))]
    public static class Storage_OnPrefabInit_Patch
    {
        internal static void Postfix(Storage __instance)
        {
            // Skip dupe/minion internal storages
            if (__instance.gameObject.GetComponent<MinionIdentity>() != null)
                return;

            // Skip objects with special handling already
            var go = __instance.gameObject;
            if (go.GetComponent<CargoBay>() != null ||
                go.GetComponent<CargoBayCluster>() != null ||
                go.GetComponent<Dumpable>() != null ||
                go.GetComponent<DropAllWorkable>() != null ||
                go.GetComponent<HiveHarvestMonitor.Instance>() != null)
            {
                return;
            }

            // Add our components if not already present
            go.AddOrGet<EmptyStorageSetting>();
            
            // Add delivery control based on building type and options
            var opts = ControlledStorageOptions.Instance;
            if (opts.EnableDeliveryControlStorage && IsStorageBin(go))
            {
                go.AddOrGet<StorageDeliveryControl>();
            }
            else if (opts.EnableDeliveryControlFridges && IsFridge(go))
            {
                go.AddOrGet<StorageDeliveryControl>();
            }
        }

        // Component-based hooks - works for vanilla and modded buildings
        private static bool IsStorageBin(GameObject go) =>
            go.GetComponent<StorageLocker>() != null || go.GetComponent<StorageLockerSmart>() != null;

        private static bool IsFridge(GameObject go) =>
            go.GetComponent<Refrigerator>() != null || go.GetComponent<RationBox>() != null;
    }

    // Fridges use FilteredStorage, not Storage directly, so Storage.OnPrefabInit may not run.
    // Patch OnPrefabInit (before deserialization) to ensure settings are restored on load.

    [HarmonyPatch(typeof(RefrigeratorConfig), nameof(RefrigeratorConfig.DoPostConfigureComplete))]
    public static class RefrigeratorConfig_DoPostConfigureComplete_Patch
    {
        internal static void Postfix(GameObject go)
        {
            if (ControlledStorageOptions.Instance.EnableDeliveryControlFridges)
                go.AddOrGet<StorageDeliveryControl>();
        }
    }

    [HarmonyPatch(typeof(Refrigerator), "OnPrefabInit")]
    public static class Refrigerator_OnPrefabInit_Patch
    {
        internal static void Postfix(Refrigerator __instance)
        {
            if (ControlledStorageOptions.Instance.EnableDeliveryControlFridges)
                __instance.gameObject.AddOrGet<StorageDeliveryControl>();
        }
    }

    [HarmonyPatch(typeof(RationBoxConfig), nameof(RationBoxConfig.DoPostConfigureComplete))]
    public static class RationBoxConfig_DoPostConfigureComplete_Patch
    {
        internal static void Postfix(GameObject go)
        {
            if (ControlledStorageOptions.Instance.EnableDeliveryControlFridges)
                go.AddOrGet<StorageDeliveryControl>();
        }
    }

    [HarmonyPatch(typeof(RationBox), "OnPrefabInit")]
    public static class RationBox_OnPrefabInit_Patch
    {
        internal static void Postfix(RationBox __instance)
        {
            if (ControlledStorageOptions.Instance.EnableDeliveryControlFridges)
                __instance.gameObject.AddOrGet<StorageDeliveryControl>();
        }
    }

    // Conveyor loader (Inbox): add delivery control so dupes/sweepers can be restricted.
    [HarmonyPatch(typeof(SolidConduitInboxConfig), nameof(SolidConduitInboxConfig.DoPostConfigureComplete))]
    public static class SolidConduitInboxConfig_DoPostConfigureComplete_Patch
    {
        internal static void Postfix(GameObject go)
        {
            if (ControlledStorageOptions.Instance.EnableDeliveryControlLoaders)
                go.AddOrGet<StorageDeliveryControl>();
        }
    }

    // Bionic dupes with Tidying Booster can empty storage without the Tidy skill (options allow it).
    [HarmonyPatch(typeof(MinionResume), nameof(MinionResume.HasPerk), typeof(HashedString))]
    public static class MinionResume_HasPerk_Patch
    {
        internal static void Postfix(MinionResume __instance, HashedString perkId, ref bool __result)
        {
            // If already has perk, skip
            if (__result)
                return;

            // Only check for Groundskeeper perk (Tidy skill)
            var tidySkillId = Db.Get().SkillPerks.IncreaseStrengthGroundskeeper.Id;
            if (perkId != tidySkillId)
                return;

            var options = ControlledStorageOptions.Instance;
            if (options.ImmediateEmptying || !options.RequireSkills)
                return;

            // Check for bionic dupe with Tidying Booster
            var bionicUpgrades = __instance.gameObject.GetSMI<BionicUpgradesMonitor.Instance>();
            if (bionicUpgrades == null)
                return;

            Tag tidyingBoosterTag = new Tag("Booster_Tidy1");
            if (bionicUpgrades.CountBoosterAssignments(tidyingBoosterTag) > 0)
            {
                __result = true;
            }
        }
    }
}
