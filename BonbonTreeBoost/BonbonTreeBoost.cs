// BonbonTreeBoost for ONI U57 (U57-704096)
// Correct approach: modify SpaceTreePlant.Def, which is the authoritative source
// for Space Tree sugar water production in U57.
// This affects existing trees, new trees, and all saves.

using HarmonyLib;
using UnityEngine;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using System.Reflection;

namespace BonbonTreeBoost
{
    [RestartRequired]
    [ConfigFile]
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

    // Patch the prefab creation to modify the SpaceTreePlant.Def
    // This is the ONLY reliable way to affect production in U57
    [HarmonyPatch(typeof(SpaceTreeConfig), "CreatePrefab")]
    public static class SpaceTree_DefPatch
    {
        public static void Postfix(GameObject __result)
        {
            if (__result == null)
                return;

            var options = POptions.ReadSettings<BonbonTreeBoostOptions>() ?? new BonbonTreeBoostOptions();

            // --- Production tuning via Def ---
            var def = __result.GetDef<SpaceTreePlant.Def>();
            if (def != null)
            {
                // Production rate is controlled by duration, not amount
                // Lower duration = more production
                def.OptimalProductionDuration /= options.YieldMultiplier;

                // Adjust how quickly optimal production is reached
                def.OptimalAmountOfBranches = Mathf.Max(1,
                    Mathf.RoundToInt(def.OptimalAmountOfBranches * options.GrowthMultiplier));
            }

            // --- Fertilizer tuning (instance-based, still valid) ---
            var absorbers = __result.GetComponents<PlantElementAbsorber>();
            if (absorbers != null && absorbers.Length > 0)
            {
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
            Debug.Log("[BonbonTreeBoost] Loaded successfully (U57 Def-based Space Tree tuning)");
        }
    }
}