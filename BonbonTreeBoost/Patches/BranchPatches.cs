using HarmonyLib;
using UnityEngine;
using PeterHan.PLib.Options;

namespace BonbonTreeBoost
{
    [HarmonyPatch(typeof(SpaceTreeBranchConfig), "CreatePrefab")]
    public static class SpaceTreeBranch_DefPatch
    {
        public static void Postfix(GameObject __result)
        {
            if (__result == null)
                return;

            BonbonTreeBoostOptions options;
            try
            {
                options = POptions.ReadSettings<BonbonTreeBoostOptions>() ?? new BonbonTreeBoostOptions();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[BonbonTreeBoost] Failed to read config file, using defaults: {ex.Message}");
                options = new BonbonTreeBoostOptions();
            }

            var growthMultiplierData = __result.GetComponent<GrowthMultiplierData>();
            if (growthMultiplierData == null)
            {
                growthMultiplierData = __result.AddComponent<GrowthMultiplierData>();
            }
            growthMultiplierData.WildBranchMultiplier = options.WildBranchGrowthRate;
            growthMultiplierData.DomesticBranchMultiplier = options.DomesticBranchGrowthRate;
            growthMultiplierData.IsBranch = true;
            if (DebugFlags.EnableDebugLogs)
                Debug.Log($"[BonbonTreeBoost] Stored branch multipliers on SpaceTreeBranch: Wild={options.WildBranchGrowthRate}, Domestic={options.DomesticBranchGrowthRate}");
        }
    }

    [HarmonyPatch(typeof(SpaceTreeBranch.Instance), MethodType.Constructor, new System.Type[] { typeof(IStateMachineTarget), typeof(SpaceTreeBranch.Def) })]
    public static class SpaceTreeBranch_Instance_Constructor_Patch
    {
        public static void Postfix(SpaceTreeBranch.Instance __instance)
        {
            if (__instance == null || __instance.gameObject == null)
                return;

            BonbonTreeBoostOptions options;
            try
            {
                options = POptions.ReadSettings<BonbonTreeBoostOptions>() ?? new BonbonTreeBoostOptions();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[BonbonTreeBoost] Failed to read config file, using defaults: {ex.Message}");
                options = new BonbonTreeBoostOptions();
            }

            float wildMultiplier = options.WildBranchGrowthRate;
            float domesticMultiplier = options.DomesticBranchGrowthRate;

            if (DebugFlags.EnableDebugLogs)
                Debug.Log($"[BonbonTreeBoost] Applying branch multipliers to SpaceTreeBranch.Instance: Wild={wildMultiplier}, Domestic={domesticMultiplier}");

            if (Mathf.Approximately(wildMultiplier, 1.0f) && Mathf.Approximately(domesticMultiplier, 1.0f))
                return;

            if (__instance.baseGrowingRate != null && !Mathf.Approximately(domesticMultiplier, 1.0f))
            {
                float currentValue = __instance.baseGrowingRate.Value;
                __instance.baseGrowingRate.SetValue(currentValue * domesticMultiplier);
                if (DebugFlags.EnableDebugLogs)
                    Debug.Log($"[BonbonTreeBoost] Applied domestic multiplier {domesticMultiplier} to branch baseGrowingRate: {currentValue} -> {__instance.baseGrowingRate.Value}");
            }

            if (__instance.wildGrowingRate != null && !Mathf.Approximately(wildMultiplier, 1.0f))
            {
                float currentValue = __instance.wildGrowingRate.Value;
                __instance.wildGrowingRate.SetValue(currentValue * wildMultiplier);
                if (DebugFlags.EnableDebugLogs)
                    Debug.Log($"[BonbonTreeBoost] Applied wild multiplier {wildMultiplier} to branch wildGrowingRate: {currentValue} -> {__instance.wildGrowingRate.Value}");
            }
        }
    }

    [HarmonyPatch(typeof(SpaceTreeBranch), "AllowItToBeHarvestForWood")]
    public static class SpaceTreeBranch_AllowItToBeHarvestForWood_Patch
    {
        public static bool Prefix(SpaceTreeBranch.Instance smi)
        {
            if (smi == null)
                return true;

            BonbonTreeBoostOptions options;
            try
            {
                options = POptions.ReadSettings<BonbonTreeBoostOptions>() ?? new BonbonTreeBoostOptions();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[BonbonTreeBoost] Failed to read config file, using defaults: {ex.Message}");
                options = new BonbonTreeBoostOptions();
            }

            if (!options.AllowBranchHarvesting)
            {
                if (DebugFlags.EnableDebugLogs)
                    Debug.Log("[BonbonTreeBoost] Branch harvesting is disabled - branch will not be made harvestable");
                return false;
            }

            return true;
        }
    }
}

