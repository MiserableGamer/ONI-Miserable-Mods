using HarmonyLib;
using ControlledExtraction.Components;
using ControlledExtraction.Options;
using UnityEngine;

namespace ControlledExtraction.Patches
{
    // Wood Burner - CO2 output port + solid input port
    [HarmonyPatch(typeof(WoodGasGeneratorConfig), "CreateBuildingDef")]
    public static class WoodGasGeneratorConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(BuildingDef __result)
        {
            var opts = ControlledExtractionOptions.Instance;

            if (opts.WoodGenCO2Port)
            {
                __result.OutputConduitType = ConduitType.Gas;
                __result.UtilityOutputOffset = new CellOffset(0, 1);
            }

            if (opts.WoodGenSolidInput)
            {
                __result.InputConduitType = ConduitType.Solid;
                __result.UtilityInputOffset = new CellOffset(0, 0);
            }
        }
    }

    [HarmonyPatch(typeof(WoodGasGeneratorConfig), "DoPostConfigureComplete")]
    public static class WoodGasGeneratorConfig_DoPostConfigureComplete_Patch
    {
        public static void Postfix(GameObject go)
        {
            var opts = ControlledExtractionOptions.Instance;

            if (opts.WoodGenCO2Port)
            {
                GeneratorPatchHelpers.SetOutputToStorage(go, SimHashes.CarbonDioxide);

                var dispenser = go.AddOrGet<ConduitDispenser>();
                dispenser.conduitType = ConduitType.Gas;
                dispenser.alwaysDispense = true;
                dispenser.elementFilter = new SimHashes[] { SimHashes.CarbonDioxide };

                var fallback = go.AddOrGet<PrimaryOutputFallbackEmitter>();
                fallback.conduitType = ConduitType.Gas;
                fallback.element = SimHashes.CarbonDioxide;
            }

            if (opts.WoodGenSolidInput)
            {
                var solidConsumer = go.AddOrGet<SolidConduitConsumer>();
                solidConsumer.capacityTag = GameTags.BuildingWood;
                solidConsumer.capacityKG = 720f;
                solidConsumer.alwaysConsume = true;
            }
        }
    }
}
