using HarmonyLib;
using ControlledExtraction.Components;
using ControlledExtraction.Options;
using UnityEngine;

namespace ControlledExtraction.Patches
{
    // ========== Coal Generator (Generator) ==========
    // Has ConfigureBuildingTemplate
    
    [HarmonyPatch(typeof(GeneratorConfig), "CreateBuildingDef")]
    public static class GeneratorConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(BuildingDef __result)
        {
            if (ControlledExtractionOptions.Instance.CoalGenCO2Port)
            {
                __result.OutputConduitType = ConduitType.Gas;
                __result.UtilityOutputOffset = new CellOffset(1, 1);
            }
        }
    }

    [HarmonyPatch(typeof(GeneratorConfig), "ConfigureBuildingTemplate")]
    public static class GeneratorConfig_ConfigureBuildingTemplate_Patch
    {
        public static void Postfix(GameObject go)
        {
            if (ControlledExtractionOptions.Instance.CoalGenCO2Port)
            {
                // Set CO2 to store so it can be dispensed
                var generator = go.GetComponent<EnergyGenerator>();
                if (generator != null)
                {
                    GeneratorPatchHelpers.SetOutputToStorage(generator, SimHashes.CarbonDioxide, true);
                }

                var dispenser = go.AddOrGet<ConduitDispenser>();
                dispenser.conduitType = ConduitType.Gas;
                dispenser.alwaysDispense = true;
                dispenser.elementFilter = new SimHashes[] { SimHashes.CarbonDioxide };
            }
        }
    }

    // ========== Wood Burner (WoodGasGenerator) ==========
    // Only has DoPostConfigureComplete
    
    [HarmonyPatch(typeof(WoodGasGeneratorConfig), "CreateBuildingDef")]
    public static class WoodGasGeneratorConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(BuildingDef __result)
        {
            if (ControlledExtractionOptions.Instance.WoodGenCO2Port)
            {
                __result.OutputConduitType = ConduitType.Gas;
                __result.UtilityOutputOffset = new CellOffset(0, 1);
            }
        }
    }

    [HarmonyPatch(typeof(WoodGasGeneratorConfig), "DoPostConfigureComplete")]
    public static class WoodGasGeneratorConfig_DoPostConfigureComplete_Patch
    {
        public static void Postfix(GameObject go)
        {
            if (ControlledExtractionOptions.Instance.WoodGenCO2Port)
            {
                // Set CO2 to store so it can be dispensed
                var generator = go.GetComponent<EnergyGenerator>();
                if (generator != null)
                {
                    GeneratorPatchHelpers.SetOutputToStorage(generator, SimHashes.CarbonDioxide, true);
                }

                var dispenser = go.AddOrGet<ConduitDispenser>();
                dispenser.conduitType = ConduitType.Gas;
                dispenser.alwaysDispense = true;
                dispenser.elementFilter = new SimHashes[] { SimHashes.CarbonDioxide };
            }
        }
    }

    // ========== Petroleum Generator ==========
    // Only has DoPostConfigureComplete

    [HarmonyPatch(typeof(PetroleumGeneratorConfig), "CreateBuildingDef")]
    public static class PetroleumGeneratorConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(BuildingDef __result)
        {
            var opts = ControlledExtractionOptions.Instance;
            
            // Primary output - prefer liquid if enabled, otherwise gas
            if (opts.PetrolGenPWaterPort)
            {
                __result.OutputConduitType = ConduitType.Liquid;
                __result.UtilityOutputOffset = new CellOffset(1, 1);
            }
            else if (opts.PetrolGenCO2Port)
            {
                __result.OutputConduitType = ConduitType.Gas;
                __result.UtilityOutputOffset = new CellOffset(0, 1);
            }
        }
    }

    [HarmonyPatch(typeof(PetroleumGeneratorConfig), "DoPostConfigureComplete")]
    public static class PetroleumGeneratorConfig_DoPostConfigureComplete_Patch
    {
        public static void Postfix(GameObject go)
        {
            var opts = ControlledExtractionOptions.Instance;
            var generator = go.GetComponent<EnergyGenerator>();

            // Polluted Water output (primary if enabled)
            if (opts.PetrolGenPWaterPort)
            {
                // Set Polluted Water to store so it can be dispensed
                if (generator != null)
                {
                    GeneratorPatchHelpers.SetOutputToStorage(generator, SimHashes.DirtyWater, true);
                }

                var liquidDispenser = go.AddOrGet<ConduitDispenser>();
                liquidDispenser.conduitType = ConduitType.Liquid;
                liquidDispenser.alwaysDispense = true;
                liquidDispenser.elementFilter = new SimHashes[] { SimHashes.DirtyWater };
            }

            // CO2 output (secondary if liquid also enabled, otherwise primary)
            if (opts.PetrolGenCO2Port)
            {
                // Set CO2 to store so it can be dispensed
                if (generator != null)
                {
                    GeneratorPatchHelpers.SetOutputToStorage(generator, SimHashes.CarbonDioxide, true);
                }

                if (opts.PetrolGenPWaterPort)
                {
                    // Use secondary output for gas
                    var secondaryOutput = go.AddOrGet<ConduitSecondaryOutput>();
                    secondaryOutput.portInfo = new ConduitPortInfo(ConduitType.Gas, new CellOffset(0, 1));
                    go.AddOrGet<SecondaryGasOutputController>().gasElement = SimHashes.CarbonDioxide;
                }
                else
                {
                    // Gas is primary output
                    var gasDispenser = go.AddOrGet<ConduitDispenser>();
                    gasDispenser.conduitType = ConduitType.Gas;
                    gasDispenser.alwaysDispense = true;
                    gasDispenser.elementFilter = new SimHashes[] { SimHashes.CarbonDioxide };
                }
            }
        }
    }

    // ========== Natural Gas Generator (MethaneGenerator) ==========
    // Note: Already has a gas output port at (2,2) for CO2 in vanilla!
    // CO2 already has store = true in vanilla
    // Only has DoPostConfigureComplete

    [HarmonyPatch(typeof(MethaneGeneratorConfig), "CreateBuildingDef")]
    public static class MethaneGeneratorConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(BuildingDef __result)
        {
            var opts = ControlledExtractionOptions.Instance;
            
            // Vanilla already has gas output at (2,2) - we only need to add liquid if enabled
            if (opts.NatGasGenPWaterPort)
            {
                // Need to use secondary for liquid since gas is primary
                // We'll handle this in DoPostConfigureComplete
            }
        }
    }

    [HarmonyPatch(typeof(MethaneGeneratorConfig), "DoPostConfigureComplete")]
    public static class MethaneGeneratorConfig_DoPostConfigureComplete_Patch
    {
        public static void Postfix(GameObject go)
        {
            var opts = ControlledExtractionOptions.Instance;
            var generator = go.GetComponent<EnergyGenerator>();

            // Polluted Water output - needs secondary since vanilla already has gas output
            if (opts.NatGasGenPWaterPort)
            {
                // Set Polluted Water to store so it can be dispensed
                if (generator != null)
                {
                    GeneratorPatchHelpers.SetOutputToStorage(generator, SimHashes.DirtyWater, true);
                }

                var secondaryOutput = go.AddOrGet<ConduitSecondaryOutput>();
                secondaryOutput.portInfo = new ConduitPortInfo(ConduitType.Liquid, new CellOffset(1, 1));
                go.AddOrGet<SecondaryLiquidOutputController>().liquidElement = SimHashes.DirtyWater;
            }
        }
    }

    // ========== Helper Class ==========

    public static class GeneratorPatchHelpers
    {
        public static void SetOutputToStorage(EnergyGenerator generator, SimHashes element, bool store)
        {
            if (generator == null || generator.formula.outputs == null) return;

            for (int i = 0; i < generator.formula.outputs.Length; i++)
            {
                if (generator.formula.outputs[i].element == element)
                {
                    generator.formula.outputs[i].store = store;
                    break;
                }
            }
        }
    }
}
