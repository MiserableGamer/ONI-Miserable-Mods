using HarmonyLib;
using UnityEngine;
using Klei.AI;
using Klei;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using System.Reflection;

namespace BonbonTreeBoost
{
    // Scale nectar storage capacity so higher production rates don't require constant dupe/port attendance.
    [HarmonyPatch(typeof(SpaceTreeConfig), nameof(SpaceTreeConfig.CreatePrefab))]
    public static class SpaceTreeConfig_CreatePrefab_Patch
    {
        private const float BASE_NECTAR_CAPACITY_KG = 20f; // Vanilla trunk storage (wiki)

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
                PUtil.LogWarning($"[BonbonTreeBoost] Failed to read config for nectar capacity, using defaults: {ex.Message}");
                return;
            }

            // Use same effective production scale as duration patch (including Production Balance and Advantage Multiplier)
            // so capacity matches whichever tree type has higher effective rate.
            int balanceMode = Mathf.Clamp(options.ProductionBalance, 1, 3);
            float advantageMultiplier = options.ProductionAdvantageMultiplier;

            float effectiveDomestic = options.DomesticProductionMultiplier;

            float effectiveWild;
            switch (balanceMode)
            {
                case 1: // Domestic advantage
                    effectiveWild = options.WildProductionMultiplier * 4f / advantageMultiplier;
                    break;
                case 2: // Equal production
                    effectiveWild = options.WildProductionMultiplier * 4f;
                    break;
                case 3: // Wild advantage
                    effectiveWild = options.WildProductionMultiplier * 4f * advantageMultiplier;
                    break;
                default:
                    effectiveWild = options.WildProductionMultiplier;
                    break;
            }

            float capacityScale = Mathf.Max(effectiveDomestic, effectiveWild);
            if (capacityScale <= 1f)
                return;

            var storage = __result.GetComponent<Storage>();
            if (storage == null)
                storage = __result.GetComponentInChildren<Storage>();
            if (storage == null)
                return;

            float newCapacity = BASE_NECTAR_CAPACITY_KG * capacityScale;
            storage.capacityKg = newCapacity;

            // UI "EFFECTS" line "Nectar: X kg" reads from DirectlyEdiblePlant_StorageElement.storageCapacity
            var nectarStorage = __result.GetComponent<DirectlyEdiblePlant_StorageElement>();
            if (nectarStorage != null)
                nectarStorage.storageCapacity = newCapacity;

            if (DebugFlags.EnableDebugLogs)
                PUtil.LogWarning($"[BonbonTreeBoost] Scaled Space Tree nectar storage: {BASE_NECTAR_CAPACITY_KG} -> {newCapacity} kg (scale {capacityScale}x)");
        }
    }

    [HarmonyPatch(typeof(EntityTemplates), "ExtendPlantToFertilizable")]
    public static class EntityTemplates_ExtendPlantToFertilizable_Patch
    {
        public static void Prefix(GameObject template, PlantElementAbsorber.ConsumeInfo[] fertilizers)
        {
            if (template == null || fertilizers == null || fertilizers.Length == 0)
                return;

            var prefabId = template.GetComponent<KPrefabID>();
            if (prefabId == null || prefabId.PrefabID().ToString() != "SpaceTree")
                return;

            BonbonTreeBoostOptions options;
            try
            {
                options = POptions.ReadSettings<BonbonTreeBoostOptions>() ?? new BonbonTreeBoostOptions();
            }
            catch (System.Exception ex)
            {
                PUtil.LogWarning($"[BonbonTreeBoost] Failed to read config file, using defaults: {ex.Message}");
                return;
            }

            // Apply multiplier to the base rate (0.16666667f from SpaceTreeConfig)
            // When multiplier is 1.0, this restores to base rate, allowing mod removal/disable to revert changes
            const float BASE_FERTILIZER_RATE = 0.16666667f; // SpaceTreeConfig.SNOW_RATE
            for (int i = 0; i < fertilizers.Length; i++)
            {
                var consumeInfo = fertilizers[i];
                consumeInfo.massConsumptionRate = BASE_FERTILIZER_RATE * options.FertilizerConsumptionRate;
                fertilizers[i] = consumeInfo;
            }

            if (DebugFlags.EnableDebugLogs)
                Debug.Log($"[BonbonTreeBoost] Applied fertilizer consumption multiplier {options.FertilizerConsumptionRate} to SpaceTree prefab via ExtendPlantToFertilizable (base: {BASE_FERTILIZER_RATE}, result: {BASE_FERTILIZER_RATE * options.FertilizerConsumptionRate})");
        }
    }

    [HarmonyPatch(typeof(SpaceTreePlant.Instance), "get_OptimalProductionDuration")]
    public static class SpaceTreePlant_Instance_OptimalProductionDuration_Patch
    {
        public static void Postfix(SpaceTreePlant.Instance __instance, ref float __result)
        {
            if (__instance == null || __instance.gameObject == null)
                return;

            var prefabId = __instance.gameObject.GetComponent<KPrefabID>();
            bool isSpaceTree = false;
            if (prefabId != null)
            {
                string prefabName = prefabId.PrefabID().ToString();
                isSpaceTree = (prefabName == "SpaceTree");
            }
            if (!isSpaceTree)
            {
                var spaceTreeDef = __instance.gameObject.GetDef<SpaceTreePlant.Def>();
                isSpaceTree = (spaceTreeDef != null);
            }

            if (!isSpaceTree)
                return;

            BonbonTreeBoostOptions options;
            try
            {
                options = POptions.ReadSettings<BonbonTreeBoostOptions>() ?? new BonbonTreeBoostOptions();
            }
            catch (System.Exception ex)
            {
                PUtil.LogWarning($"[BonbonTreeBoost] Failed to read config file, using defaults: {ex.Message}");
                return;
            }

            bool isWild = __instance.IsWildPlanted;
            float productionMultiplier = isWild ? options.WildProductionMultiplier : options.DomesticProductionMultiplier;
            
            if (Mathf.Approximately(productionMultiplier, 1.0f))
                return;

            int balanceMode = Mathf.Clamp(options.ProductionBalance, 1, 3);
            float advantageMultiplier = options.ProductionAdvantageMultiplier;
            
            // Game uses OptimalProductionDuration in a way that makes rate ~ 1/duration²,
            // so halving duration gives 4x rate. Apply sqrt(multiplier) so 2x option => 2x rate.
            float durationDivisor;
            if (isWild)
            {
                switch (balanceMode)
                {
                    case 1: // Domestic advantage
                        durationDivisor = Mathf.Sqrt(productionMultiplier * 4f / advantageMultiplier);
                        break;
                    case 2: // Equal production
                        durationDivisor = Mathf.Sqrt(productionMultiplier * 4f);
                        break;
                    case 3: // Wild advantage
                        durationDivisor = Mathf.Sqrt(productionMultiplier * 4f * advantageMultiplier);
                        break;
                    default:
                        durationDivisor = Mathf.Sqrt(productionMultiplier);
                        break;
                }
            }
            else
            {
                durationDivisor = Mathf.Sqrt(productionMultiplier);
            }

            float originalResult = __result;
            __result /= durationDivisor;
            
            if (DebugFlags.EnableDebugLogs)
                Debug.Log($"[BonbonTreeBoost] Applied {(isWild ? "wild" : "domestic")} production multiplier {productionMultiplier} with balance mode {balanceMode} (advantage: {advantageMultiplier}x) to OptimalProductionDuration: {originalResult} -> {__result}");
        }
    }

    [HarmonyPatch(typeof(Growing.StatesInstance), MethodType.Constructor, new System.Type[] { typeof(Growing) })]
    public static class Growing_StatesInstance_Constructor_Patch
    {
        public static void Postfix(Growing.StatesInstance __instance, Growing master)
        {
            if (master == null || master.gameObject == null)
                return;

            var prefabId = master.gameObject.GetComponent<KPrefabID>();
            bool isSpaceTreeTrunk = false;

            if (prefabId != null)
            {
                string prefabName = prefabId.PrefabID().ToString();
                isSpaceTreeTrunk = (prefabName == "SpaceTree");
            }

            if (!isSpaceTreeTrunk)
            {
                var spaceTreeDef = master.gameObject.GetDef<SpaceTreePlant.Def>();
                isSpaceTreeTrunk = (spaceTreeDef != null);
            }

            if (!isSpaceTreeTrunk)
                return;

            BonbonTreeBoostOptions options;
            try
            {
                options = POptions.ReadSettings<BonbonTreeBoostOptions>() ?? new BonbonTreeBoostOptions();
            }
            catch (System.Exception ex)
            {
                PUtil.LogWarning($"[BonbonTreeBoost] Failed to read config file, using defaults: {ex.Message}");
                options = new BonbonTreeBoostOptions();
            }

            float wildMultiplier = options.WildTrunkGrowthRate;
            float domesticMultiplier = options.DomesticTrunkGrowthRate;

            if (DebugFlags.EnableDebugLogs)
                Debug.Log($"[BonbonTreeBoost] Applying trunk multipliers to SpaceTree: Wild={wildMultiplier}, Domestic={domesticMultiplier}");

            if (Mathf.Approximately(wildMultiplier, 1.0f) && Mathf.Approximately(domesticMultiplier, 1.0f))
                return;

            var baseGrowingRateField = typeof(Growing.StatesInstance).GetField("baseGrowingRate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var wildGrowingRateField = typeof(Growing.StatesInstance).GetField("wildGrowingRate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (baseGrowingRateField != null && !Mathf.Approximately(domesticMultiplier, 1.0f))
            {
                var baseModifier = baseGrowingRateField.GetValue(__instance) as AttributeModifier;
                if (baseModifier != null)
                {
                    float currentValue = baseModifier.Value;
                    baseModifier.SetValue(currentValue * domesticMultiplier);
                        if (DebugFlags.EnableDebugLogs)
                            Debug.Log($"[BonbonTreeBoost] Applied domestic multiplier {domesticMultiplier} to baseGrowingRate: {currentValue} -> {baseModifier.Value}");
                }
            }

            if (wildGrowingRateField != null && !Mathf.Approximately(wildMultiplier, 1.0f))
            {
                var wildModifier = wildGrowingRateField.GetValue(__instance) as AttributeModifier;
                if (wildModifier != null)
                {
                    float currentValue = wildModifier.Value;
                    wildModifier.SetValue(currentValue * wildMultiplier);
                        if (DebugFlags.EnableDebugLogs)
                            Debug.Log($"[BonbonTreeBoost] Applied wild multiplier {wildMultiplier} to wildGrowingRate: {currentValue} -> {wildModifier.Value}");
                }
            }
        }
    }
}

