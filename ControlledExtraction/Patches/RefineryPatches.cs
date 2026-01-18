using HarmonyLib;
using ControlledExtraction.Components;
using ControlledExtraction.Options;
using UnityEngine;

namespace ControlledExtraction.Patches
{
    // ========== Oil Refinery ==========
    // Size: 4 wide x 4 tall
    // Existing: Liquid input (0,0), Liquid output (1,1) for Petroleum
    // Produces: Petroleum (stored), Methane (to world)

    [HarmonyPatch(typeof(OilRefineryConfig), "CreateBuildingDef")]
    public static class OilRefineryConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(BuildingDef __result)
        {
            var opts = ControlledExtractionOptions.Instance;

            // Methane gas output - need secondary since liquid output is primary
            if (opts.OilRefineryMethanePort)
            {
                // We'll use secondary output since primary is already liquid
            }
        }
    }

    [HarmonyPatch(typeof(OilRefineryConfig), "ConfigureBuildingTemplate")]
    public static class OilRefineryConfig_ConfigureBuildingTemplate_Patch
    {
        public static void Postfix(GameObject go)
        {
            var opts = ControlledExtractionOptions.Instance;

            if (opts.OilRefineryMethanePort)
            {
                // Set Methane to store so it can be dispensed
                var converter = go.GetComponent<ElementConverter>();
                if (converter != null)
                {
                    RefineryPatchHelpers.SetOutputToStorage(converter, SimHashes.Methane, true);
                }

                // Add secondary gas output since primary is liquid
                var secondaryOutput = go.AddOrGet<ConduitSecondaryOutput>();
                secondaryOutput.portInfo = new ConduitPortInfo(ConduitType.Gas, new CellOffset(-1, 3));
                go.AddOrGet<SecondaryGasOutputController>().gasElement = SimHashes.Methane;
            }
        }
    }

    // ========== Ethanol Distillery ==========
    // Size: 4 wide x 3 tall
    // Existing: Power (2,0), Liquid output (-1,0) for Ethanol
    // Produces: Ethanol (stored), Polluted Dirt (stored, emitted via AlgaeDistillery), CO2 (to world)

    [HarmonyPatch(typeof(EthanolDistilleryConfig), "CreateBuildingDef")]
    public static class EthanolDistilleryConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(BuildingDef __result)
        {
            var opts = ControlledExtractionOptions.Instance;

            // Solid input for lumber
            if (opts.EthanolSolidInput)
            {
                __result.InputConduitType = ConduitType.Solid;
                __result.UtilityInputOffset = new CellOffset(2, 0);
            }
        }
    }

    [HarmonyPatch(typeof(EthanolDistilleryConfig), "ConfigureBuildingTemplate")]
    public static class EthanolDistilleryConfig_ConfigureBuildingTemplate_Patch
    {
        public static void Postfix(GameObject go)
        {
            var opts = ControlledExtractionOptions.Instance;
            var converter = go.GetComponent<ElementConverter>();

            // CO2 gas output - secondary since liquid is primary
            if (opts.EthanolCO2Port)
            {
                if (converter != null)
                {
                    RefineryPatchHelpers.SetOutputToStorage(converter, SimHashes.CarbonDioxide, true);
                }

                var secondaryGasOutput = go.AddOrGet<ConduitSecondaryOutput>();
                secondaryGasOutput.portInfo = new ConduitPortInfo(ConduitType.Gas, new CellOffset(2, 2));
                go.AddOrGet<SecondaryGasOutputController>().gasElement = SimHashes.CarbonDioxide;
            }

            // Solid input for lumber
            if (opts.EthanolSolidInput)
            {
                var solidConsumer = go.AddOrGet<SolidConduitConsumer>();
                solidConsumer.capacityTag = GameTags.BuildingWood;
                solidConsumer.capacityKG = 600f;
            }

            // Solid output for polluted dirt
            if (opts.EthanolSolidOutput)
            {
                // Polluted dirt is already stored, we just need to dispense it
                var solidDispenser = go.AddOrGet<SolidConduitDispenser>();
                solidDispenser.solidOnly = true;
                solidDispenser.elementFilter = new SimHashes[] { SimHashes.ToxicSand };
                
                // We need to add the solid output port info
                // Since we can't have two different secondary outputs easily,
                // we'll need a custom component for solid output
                go.AddOrGet<SecondarySolidOutputController>().Initialize(new CellOffset(0, 0), SimHashes.ToxicSand);
            }
        }
    }

    // ========== Helper Class ==========

    public static class RefineryPatchHelpers
    {
        public static void SetOutputToStorage(ElementConverter converter, SimHashes element, bool store)
        {
            if (converter == null || converter.outputElements == null) return;

            for (int i = 0; i < converter.outputElements.Length; i++)
            {
                if (converter.outputElements[i].elementHash == element)
                {
                    var output = converter.outputElements[i];
                    output.storeOutput = store;
                    converter.outputElements[i] = output;
                    break;
                }
            }
        }
    }
}
