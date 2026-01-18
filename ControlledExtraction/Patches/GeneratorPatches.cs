using HarmonyLib;
using ControlledExtraction.Components;
using ControlledExtraction.Options;
using UnityEngine;

namespace ControlledExtraction.Patches
{
    // Coal Generator - CO2 output port
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
            if (!ControlledExtractionOptions.Instance.CoalGenCO2Port) return;

            GeneratorPatchHelpers.SetOutputToStorage(go, SimHashes.CarbonDioxide);

            var dispenser = go.AddOrGet<ConduitDispenser>();
            dispenser.conduitType = ConduitType.Gas;
            dispenser.alwaysDispense = true;
            dispenser.elementFilter = new SimHashes[] { SimHashes.CarbonDioxide };
        }
    }

    // Wood Burner - CO2 output port
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
            if (!ControlledExtractionOptions.Instance.WoodGenCO2Port) return;

            GeneratorPatchHelpers.SetOutputToStorage(go, SimHashes.CarbonDioxide);

            var dispenser = go.AddOrGet<ConduitDispenser>();
            dispenser.conduitType = ConduitType.Gas;
            dispenser.alwaysDispense = true;
            dispenser.elementFilter = new SimHashes[] { SimHashes.CarbonDioxide };
        }
    }

    // Petroleum Generator - CO2 and Polluted Water output ports
    [HarmonyPatch(typeof(PetroleumGeneratorConfig), "CreateBuildingDef")]
    public static class PetroleumGeneratorConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(BuildingDef __result)
        {
            var opts = ControlledExtractionOptions.Instance;

            // Liquid takes priority as primary output
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

            if (opts.PetrolGenPWaterPort)
            {
                GeneratorPatchHelpers.SetOutputToStorage(go, SimHashes.DirtyWater);

                var liquidDispenser = go.AddOrGet<ConduitDispenser>();
                liquidDispenser.conduitType = ConduitType.Liquid;
                liquidDispenser.alwaysDispense = true;
                liquidDispenser.elementFilter = new SimHashes[] { SimHashes.DirtyWater };
            }

            if (opts.PetrolGenCO2Port)
            {
                GeneratorPatchHelpers.SetOutputToStorage(go, SimHashes.CarbonDioxide);

                if (opts.PetrolGenPWaterPort)
                {
                    // Gas is secondary when liquid is primary
                    var secondaryOutput = go.AddOrGet<ConduitSecondaryOutput>();
                    secondaryOutput.portInfo = new ConduitPortInfo(ConduitType.Gas, new CellOffset(0, 1));
                    go.AddOrGet<SecondaryGasOutputController>().gasElement = SimHashes.CarbonDioxide;
                }
                else
                {
                    var gasDispenser = go.AddOrGet<ConduitDispenser>();
                    gasDispenser.conduitType = ConduitType.Gas;
                    gasDispenser.alwaysDispense = true;
                    gasDispenser.elementFilter = new SimHashes[] { SimHashes.CarbonDioxide };
                }
            }
        }
    }

    // Natural Gas Generator - Polluted Water output port (CO2 is vanilla)
    [HarmonyPatch(typeof(MethaneGeneratorConfig), "DoPostConfigureComplete")]
    public static class MethaneGeneratorConfig_DoPostConfigureComplete_Patch
    {
        public static void Postfix(GameObject go)
        {
            if (!ControlledExtractionOptions.Instance.NatGasGenPWaterPort) return;

            GeneratorPatchHelpers.SetOutputToStorage(go, SimHashes.DirtyWater);

            // Secondary output since vanilla already has gas
            var secondaryOutput = go.AddOrGet<ConduitSecondaryOutput>();
            secondaryOutput.portInfo = new ConduitPortInfo(ConduitType.Liquid, new CellOffset(1, 1));
            go.AddOrGet<SecondaryLiquidOutputController>().liquidElement = SimHashes.DirtyWater;
        }
    }

    public static class GeneratorPatchHelpers
    {
        public static void SetOutputToStorage(GameObject go, SimHashes element)
        {
            var generator = go.GetComponent<EnergyGenerator>();
            if (generator?.formula.outputs == null) return;

            for (int i = 0; i < generator.formula.outputs.Length; i++)
            {
                if (generator.formula.outputs[i].element == element)
                {
                    generator.formula.outputs[i].store = true;
                    break;
                }
            }
        }
    }
}
