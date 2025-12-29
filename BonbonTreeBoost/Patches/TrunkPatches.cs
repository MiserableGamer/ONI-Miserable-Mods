using HarmonyLib;
using UnityEngine;
using Klei.AI;
using Klei;
using PeterHan.PLib.Options;
using System.Reflection;

namespace BonbonTreeBoost
{
    // FERTILIZER CODE COMMENTED OUT - TO BE DEALT WITH LATER
    /*
    [HarmonyPatch(typeof(Assets), nameof(Assets.AddPrefab))]
    public static class Assets_AddPrefab_Patch
    {
        private const float BASE_FERTILIZER_RATE = 0.16666667f; // SpaceTreeConfig.SNOW_RATE

        public static void Postfix(KPrefabID prefab)
        {
            if (prefab == null)
                return;

            if (prefab.PrefabID().ToString() != "SpaceTree")
                return;

            BonbonTreeBoostOptions options;
            try
            {
                options = POptions.ReadSettings<BonbonTreeBoostOptions>() ?? new BonbonTreeBoostOptions();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[BonbonTreeBoost] Failed to read config file, using defaults: {ex.Message}");
                return;
            }

            var fertilizationMonitor = prefab.gameObject.GetComponent<FertilizationMonitor>();
            if (fertilizationMonitor == null)
                return;

            var defProperty = typeof(FertilizationMonitor).GetProperty("Def", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (defProperty == null)
                return;

            var def = defProperty.GetValue(fertilizationMonitor) as FertilizationMonitor.Def;
            if (def == null || def.consumedElements == null || def.consumedElements.Length == 0)
                return;

            for (int i = 0; i < def.consumedElements.Length; i++)
            {
                var consumeInfo = def.consumedElements[i];
                // Always restore to base rate first, then apply multiplier
                // This ensures changes are reverted when mod is disabled (multiplier = 1.0)
                consumeInfo.massConsumptionRate = BASE_FERTILIZER_RATE * options.FertilizerConsumptionRate;
                def.consumedElements[i] = consumeInfo;
            }

            if (DebugFlags.EnableDebugLogs)
                Debug.Log($"[BonbonTreeBoost] Applied fertilizer consumption multiplier {options.FertilizerConsumptionRate} to SpaceTree prefab via Assets.AddPrefab (base: {BASE_FERTILIZER_RATE}, result: {BASE_FERTILIZER_RATE * options.FertilizerConsumptionRate})");
        }
    }

    [HarmonyPatch(typeof(FertilizationMonitor.Instance), "StartAbsorbing")]
    public static class FertilizationMonitor_Instance_StartAbsorbing_Patch
    {
        public static void Prefix(FertilizationMonitor.Instance __instance, out bool __state)
        {
            __state = false;
            if (__instance == null || __instance.gameObject == null)
                return;

            var prefabId = __instance.gameObject.GetComponent<KPrefabID>();
            if (prefabId == null || prefabId.PrefabID().ToString() != "SpaceTree")
                return;

            // Check handle before
            var absorberHandleField = typeof(FertilizationMonitor.Instance).GetField("absorberHandle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (absorberHandleField != null)
            {
                var handle = absorberHandleField.GetValue(__instance);
                if (handle != null)
                {
                    var isValidMethod = handle.GetType().GetMethod("IsValid", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (isValidMethod != null)
                        __state = (bool)(isValidMethod.Invoke(handle, null) ?? false);
                }
            }
        }

        public static void Postfix(FertilizationMonitor.Instance __instance, bool __state)
        {
            if (__instance == null || __instance.gameObject == null)
                return;

            var prefabId = __instance.gameObject.GetComponent<KPrefabID>();
            if (prefabId == null || prefabId.PrefabID().ToString() != "SpaceTree")
                return;

            if (DebugFlags.EnableDebugLogs)
            {
                var absorberHandleField = typeof(FertilizationMonitor.Instance).GetField("absorberHandle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                bool isValidAfter = false;
                if (absorberHandleField != null)
                {
                    var handle = absorberHandleField.GetValue(__instance);
                    if (handle != null)
                    {
                        var isValidMethod = handle.GetType().GetMethod("IsValid", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (isValidMethod != null)
                            isValidAfter = (bool)(isValidMethod.Invoke(handle, null) ?? false);
                    }
                }

                // Check def and storage to see why it might have failed
                var defProperty = typeof(FertilizationMonitor.Instance).GetProperty("def", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                bool hasDef = defProperty != null && defProperty.GetValue(__instance) != null;
                
                var storageField = typeof(FertilizationMonitor.Instance).GetField("storage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                bool hasStorage = storageField != null && storageField.GetValue(__instance) != null;

                Debug.Log($"[BonbonTreeBoost] StartAbsorbing: before={__state}, after={isValidAfter}, hasDef={hasDef}, hasStorage={hasStorage}");
            }
        }
    }

    [HarmonyPatch(typeof(FertilizationMonitor.Instance), "UpdateFertilization")]
    public static class FertilizationMonitor_Instance_UpdateFertilization_Patch
    {
        public static void Postfix(FertilizationMonitor.Instance __instance, float dt)
        {
            if (__instance == null || __instance.gameObject == null)
                return;

            var prefabId = __instance.gameObject.GetComponent<KPrefabID>();
            if (prefabId == null || prefabId.PrefabID().ToString() != "SpaceTree")
                return;

            var spaceTreeSMI = __instance.gameObject.GetSMI<SpaceTreePlant.Instance>();
            if (spaceTreeSMI != null && spaceTreeSMI.IsInsideState(spaceTreeSMI.sm.production.producing))
            {
                // When tree is in production.producing state, ensure fertilizer absorption is active
                // Check if we need to transition to absorbing state
                var smField = typeof(FertilizationMonitor.Instance).GetField("sm", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (smField != null)
                {
                    var sm = smField.GetValue(__instance);
                    var hasCorrectFertilizerField = sm.GetType().GetField("hasCorrectFertilizer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (hasCorrectFertilizerField != null)
                    {
                        var hasCorrectFertilizerParam = hasCorrectFertilizerField.GetValue(sm);
                        var getValueMethod = hasCorrectFertilizerParam.GetType().GetMethod("Get", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        bool hasCorrectFertilizer = false;
                        if (getValueMethod != null)
                        {
                            hasCorrectFertilizer = (bool)(getValueMethod.Invoke(hasCorrectFertilizerParam, new object[] { __instance }) ?? false);
                        }

                        if (hasCorrectFertilizer)
                        {
                            // We have correct fertilizer and are in production state, ensure we're absorbing
                            var absorberHandleField = typeof(FertilizationMonitor.Instance).GetField("absorberHandle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (absorberHandleField != null)
                            {
                                var handle = absorberHandleField.GetValue(__instance);
                                if (handle != null)
                                {
                                    var isValidMethod = handle.GetType().GetMethod("IsValid", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    bool isValid = false;
                                    if (isValidMethod != null)
                                        isValid = (bool)(isValidMethod.Invoke(handle, null) ?? false);

                                    if (!isValid)
                                    {
                                        var startAbsorbingMethod = typeof(FertilizationMonitor.Instance).GetMethod("StartAbsorbing", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                        if (startAbsorbingMethod != null)
                                        {
                                            startAbsorbingMethod.Invoke(__instance, null);
                                            if (DebugFlags.EnableDebugLogs)
                                                Debug.Log($"[BonbonTreeBoost] Forced StartAbsorbing for SpaceTree in production state (hasCorrectFertilizer=true)");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
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
                Debug.LogWarning($"[BonbonTreeBoost] Failed to read config file, using defaults: {ex.Message}");
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
    */

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
                Debug.LogWarning($"[BonbonTreeBoost] Failed to read config file, using defaults: {ex.Message}");
                return;
            }

            bool isWild = __instance.IsWildPlanted;
            float productionMultiplier = isWild ? options.WildProductionMultiplier : options.DomesticProductionMultiplier;
            
            if (Mathf.Approximately(productionMultiplier, 1.0f))
                return;

            int balanceMode = Mathf.Clamp(options.ProductionBalance, 1, 3);
            float advantageMultiplier = options.ProductionAdvantageMultiplier;
            
            float originalResult = __result;
            if (isWild)
            {
                switch (balanceMode)
                {
                    case 1: // Domestic advantage
                        __result /= (productionMultiplier * 4f / advantageMultiplier);
                        break;
                    case 2: // Equal production
                        __result /= (productionMultiplier * 4f);
                        break;
                    case 3: // Wild advantage
                        __result /= (productionMultiplier * 4f * advantageMultiplier);
                        break;
                    default:
                        __result /= productionMultiplier;
                        break;
                }
            }
            else
            {
                __result /= productionMultiplier;
            }
            
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
                Debug.LogWarning($"[BonbonTreeBoost] Failed to read config file, using defaults: {ex.Message}");
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

