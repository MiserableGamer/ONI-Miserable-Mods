using HarmonyLib;
using ControlledExtraction.Components;
using ControlledExtraction.Options;
using UnityEngine;

namespace ControlledExtraction.Patches
{
    // Coal Generator - CO2 output port + solid input port
    [HarmonyPatch(typeof(GeneratorConfig), "CreateBuildingDef")]
    public static class GeneratorConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(BuildingDef __result)
        {
            var opts = ControlledExtractionOptions.Instance;

            if (opts.CoalGenCO2Port)
            {
                __result.OutputConduitType = ConduitType.Gas;
                __result.UtilityOutputOffset = new CellOffset(1, 1);
            }

            if (opts.CoalGenSolidInput)
            {
                __result.InputConduitType = ConduitType.Solid;
                __result.UtilityInputOffset = new CellOffset(1, 0);
            }
        }
    }

    [HarmonyPatch(typeof(GeneratorConfig), "ConfigureBuildingTemplate")]
    public static class GeneratorConfig_ConfigureBuildingTemplate_Patch
    {
        public static void Postfix(GameObject go)
        {
            var opts = ControlledExtractionOptions.Instance;

            if (opts.CoalGenCO2Port)
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

            if (opts.CoalGenSolidInput)
            {
                var solidConsumer = go.AddOrGet<SolidConduitConsumer>();
                solidConsumer.capacityTag = new Tag("Coal");
                solidConsumer.capacityKG = 600f;
                solidConsumer.alwaysConsume = true;
            }
        }
    }
}
