using HarmonyLib;
using UnityEngine;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace BonbonTreeBoost
{
    internal static class DebugFlags
    {
        public static readonly bool EnableDebugLogs = true; // Set to false to disable debug logs
    }

    // Stores growth multipliers to avoid modifying base fields (which causes tooltip crashes)
    public class GrowthMultiplierData : KMonoBehaviour
    {
        public float WildTrunkMultiplier { get; set; } = 1.0f;
        public float WildBranchMultiplier { get; set; } = 1.0f;
        public float DomesticTrunkMultiplier { get; set; } = 1.0f;
        public float DomesticBranchMultiplier { get; set; } = 1.0f;
        public bool IsBranch { get; set; } = false;
    }

    [RestartRequired]
    [ConfigFile(SharedConfigLocation: true)]
    public class BonbonTreeBoostOptions
    {
        [Option("Nectar Production Multiplier", "Multiplier for wild Bonbon Tree Nectar production", "Wild Trees")]
        [Limit(1, 10.0)]
        public float WildProductionMultiplier { get; set; } = 2.0f;

        [Option("Trunk Growth Rate", "Multiplier for wild Bonbon Tree trunk growth rate", "Wild Trees")]
        [Limit(0.1, 10.0)]
        public float WildTrunkGrowthRate { get; set; } = 1.0f;

        [Option("Branch Growth Rate", "Multiplier for wild Bonbon Tree branch growth rate", "Wild Trees")]
        [Limit(0.1, 10.0)]
        public float WildBranchGrowthRate { get; set; } = 1.0f;

        [Option("Nectar Production Multiplier", "Multiplier for domesticated Bonbon Tree Nectar production", "Domesticated Trees")]
        [Limit(1, 10.0)]
        public float DomesticProductionMultiplier { get; set; } = 2.0f;

        [Option("Trunk Growth Rate", "Multiplier for domesticated Bonbon Tree trunk growth rate", "Domesticated Trees")]
        [Limit(0.1, 10.0)]
        public float DomesticTrunkGrowthRate { get; set; } = 1.0f;

        [Option("Branch Growth Rate", "Multiplier for domesticated Bonbon Tree branch growth rate", "Domesticated Trees")]
        [Limit(0.1, 10.0)]
        public float DomesticBranchGrowthRate { get; set; } = 1.0f;

        [Option("Wild vs Domestic Production Balance", "Controls the production relationship between wild and domesticated trees. 1 = Domestic Advantage (wild produces less), 2 = Equal Production (wild and domestic produce the same), 3 = Wild Advantage (wild produces more). Default: 3 (Wild Advantage).", "Production Balance")]
        [Limit(1, 3)]
        public int ProductionBalance { get; set; } = 3;

        [Option("Production Advantage Multiplier", "When balance is set to Domestic Advantage (1) or Wild Advantage (3), this controls how much more one produces than the other. For example, 2.0 means the advantaged type produces 2x more than the other. Default: 2.0", "Production Balance")]
        [Limit(0.5, 10.0)]
        public float ProductionAdvantageMultiplier { get; set; } = 2.0f;

        [Option("Allow Branch Harvesting", "If disabled, branches cannot be harvested for wood. Dupes will only harvest nectar from the trunk.", "Branch Settings")]
        public bool AllowBranchHarvesting { get; set; } = true;

                // FERTILIZER OPTION COMMENTED OUT - TO BE DEALT WITH LATER
                /*
                [Option("Fertilizer Consumption Rate", "Multiplier for Snow fertilizer consumption (domesticated trees only). Set to 0 to disable fertilizer consumption.", "Domesticated Trees")]
                [Limit(0.0, 10.0)]
                public float FertilizerConsumptionRate { get; set; } = 2.0f;
                */
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
