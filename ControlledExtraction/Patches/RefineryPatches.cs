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
        }
    }

    // Ethanol Distillery - CO2, solid input/output ports
    [HarmonyPatch(typeof(EthanolDistilleryConfig), "CreateBuildingDef")]
    public static class EthanolDistilleryConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(BuildingDef __result)
        {
            if (ControlledExtractionOptions.Instance.EthanolSolidInput)
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

            if (opts.EthanolCO2Port)
            {
                RefineryPatchHelpers.SetOutputToStorage(go, SimHashes.CarbonDioxide);

                // Secondary output since liquid is primary
                var secondaryGasOutput = go.AddOrGet<ConduitSecondaryOutput>();
                secondaryGasOutput.portInfo = new ConduitPortInfo(ConduitType.Gas, new CellOffset(2, 2));
                go.AddOrGet<SecondaryGasOutputController>().gasElement = SimHashes.CarbonDioxide;
            }

            if (opts.EthanolSolidInput)
            {
                var solidConsumer = go.AddOrGet<SolidConduitConsumer>();
                solidConsumer.capacityTag = GameTags.BuildingWood;
                solidConsumer.capacityKG = 600f;
            }

            if (opts.EthanolSolidOutput)
            {
                var solidDispenser = go.AddOrGet<SolidConduitDispenser>();
                solidDispenser.solidOnly = true;
                solidDispenser.elementFilter = new SimHashes[] { SimHashes.ToxicSand };

                go.AddOrGet<SecondarySolidOutputController>().Initialize(new CellOffset(0, 0), SimHashes.ToxicSand);
            }
        }
    }

    public static class RefineryPatchHelpers
    {
        public static void SetOutputToStorage(GameObject go, SimHashes element)
        {
            var converter = go.GetComponent<ElementConverter>();
            if (converter?.outputElements == null) return;

            for (int i = 0; i < converter.outputElements.Length; i++)
            {
                if (converter.outputElements[i].elementHash == element)
                {
                    var output = converter.outputElements[i];
                    output.storeOutput = true;
                    converter.outputElements[i] = output;
                    break;
                }
            }
        }
    }
}
