using HarmonyLib;
using UnityEngine;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using System.Reflection;

namespace BonbonTreeBoost
{
    [RestartRequired]
    [ConfigFile(SharedConfigLocation: true)]
    public class BonbonTreeBoostOptions
    {
        [Option("Nectar Production Multiplier", "Multiplier for Space Tree sugar water production")]
        [Limit(0.1, 10.0)]
        public float YieldMultiplier { get; set; } = 2.0f;

        [Option("Growth / Branch Efficiency Multiplier", "Affects how quickly Space Trees reach optimal production")]
        [Limit(0.1, 5.0)]
        public float GrowthMultiplier { get; set; } = 1.0f;

        [Option("Fertilizer Consumption Multiplier", "Multiplier for Snow fertilizer consumption")]
        [Limit(0.0, 5.0)]
        public float FertilizerMultiplier { get; set; } = 1.0f;
    }

    [HarmonyPatch(typeof(SpaceTreeConfig), "CreatePrefab")]
    public static class SpaceTree_DefPatch
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

            var def = __result.GetDef<SpaceTreePlant.Def>();
            if (def != null)
            {
                // Lower duration means more frequent production cycles
                def.OptimalProductionDuration /= options.YieldMultiplier;
                def.OptimalAmountOfBranches = Mathf.Max(1,
                    Mathf.RoundToInt(def.OptimalAmountOfBranches * options.GrowthMultiplier));
            }
            var absorbers = __result.GetComponents<PlantElementAbsorber>();
            if (absorbers != null && absorbers.Length > 0)
            {
                // Use reflection to access private consumedElements field
                FieldInfo consumedField = typeof(PlantElementAbsorber)
                    .GetField("consumedElements", BindingFlags.NonPublic | BindingFlags.Instance);

                if (consumedField != null)
                {
                    foreach (var absorber in absorbers)
                    {
                        if (ReferenceEquals(absorber, null)) continue;

                        var consumed = (PlantElementAbsorber.ConsumeInfo[])consumedField.GetValue(absorber);
                        if (consumed == null) continue;

                        for (int i = 0; i < consumed.Length; i++)
                            consumed[i].massConsumptionRate *= options.FertilizerMultiplier;
                    }
                }
            }
        }
    }

    public sealed class BonbonTreeBoostMod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary();
            new POptions().RegisterOptions(this, typeof(BonbonTreeBoostOptions));
            Debug.Log("[BonbonTreeBoost] Loaded successfully");
        }
    }
}