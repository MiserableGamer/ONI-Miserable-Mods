using HarmonyLib;
using ControlledExtraction.Components;
using ControlledExtraction.Options;
using UnityEngine;

namespace ControlledExtraction.Patches
{
    // Petroleum Generator - CO2 and Polluted Water output ports
    [HarmonyPatch(typeof(PetroleumGeneratorConfig), "CreateBuildingDef")]
    public static class PetroleumGeneratorConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(BuildingDef __result)
        {
            var opts = ControlledExtractionOptions.Instance;

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

                var fallback = go.AddOrGet<PrimaryOutputFallbackEmitter>();
                fallback.conduitType = ConduitType.Liquid;
                fallback.element = SimHashes.DirtyWater;
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

                    var fallback = go.AddOrGet<PrimaryOutputFallbackEmitter>();
                    fallback.conduitType = ConduitType.Gas;
                    fallback.element = SimHashes.CarbonDioxide;
                }
            }
        }
    }
}
