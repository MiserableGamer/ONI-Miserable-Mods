using HarmonyLib;
using ControlledExtraction.Components;
using ControlledExtraction.Options;
using UnityEngine;

namespace ControlledExtraction.Patches
{
    // Natural Gas Generator - Polluted Water output port (CO2 output is vanilla)
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

            // Vanilla NatGas Gen has OutputConduitType=Gas with CO2 store=true.
            // Add fallback so CO2 emits to world when no gas pipe connected,
            // and bypass the vanilla "no gas output" operational requirement.
            var fallback = go.AddOrGet<PrimaryOutputFallbackEmitter>();
            fallback.conduitType = ConduitType.Gas;
            fallback.element = SimHashes.CarbonDioxide;
        }
    }
}
