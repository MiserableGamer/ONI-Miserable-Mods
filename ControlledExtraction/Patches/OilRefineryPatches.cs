using HarmonyLib;
using ControlledExtraction.Components;
using ControlledExtraction.Options;
using UnityEngine;

namespace ControlledExtraction.Patches
{
    // Oil Refinery - Methane gas output port
    [HarmonyPatch(typeof(OilRefineryConfig), "ConfigureBuildingTemplate")]
    public static class OilRefineryConfig_ConfigureBuildingTemplate_Patch
    {
        public static void Postfix(GameObject go)
        {
            if (!ControlledExtractionOptions.Instance.OilRefineryMethanePort) return;

            RefineryPatchHelpers.SetOutputToStorage(go, SimHashes.Methane);

            // Secondary output since liquid is primary
            var secondaryOutput = go.AddOrGet<ConduitSecondaryOutput>();
            secondaryOutput.portInfo = new ConduitPortInfo(ConduitType.Gas, new CellOffset(-1, 3));
            go.AddOrGet<SecondaryGasOutputController>().gasElement = SimHashes.Methane;

            // Bypass vanilla "no liquid output" restriction and emit petroleum
            // to the world when no liquid pipe is connected
            var fallback = go.AddOrGet<PrimaryOutputFallbackEmitter>();
            fallback.conduitType = ConduitType.Liquid;
            fallback.element = SimHashes.Petroleum;
        }
    }
}
