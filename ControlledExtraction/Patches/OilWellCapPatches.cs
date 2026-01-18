using HarmonyLib;
using ControlledExtraction.Components;
using ControlledExtraction.Options;
using UnityEngine;
using STRINGS;

namespace ControlledExtraction.Patches
{
    // Add output ports to building definition
    [HarmonyPatch(typeof(OilWellCapConfig), "CreateBuildingDef")]
    public static class OilWellCapConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(BuildingDef __result)
        {
            var opts = ControlledExtractionOptions.Instance;
            bool skipLiquidPort = ControlledExtractionMod.IsRonivansLegacyLoaded();

            // Liquid takes priority as primary output (skip if Ronivan's Legacy handles it)
            if (opts.AddLiquidOutputPort && !skipLiquidPort)
            {
                __result.OutputConduitType = ConduitType.Liquid;
                __result.UtilityOutputOffset = new CellOffset(2, 1);
            }
            else if (opts.AddGasOutputPort)
            {
                __result.OutputConduitType = ConduitType.Gas;
                __result.UtilityOutputOffset = new CellOffset(1, 1);
            }
        }
    }

    // Add components and dispensers
    [HarmonyPatch(typeof(OilWellCapConfig), "ConfigureBuildingTemplate")]
    public static class OilWellCapConfig_ConfigureBuildingTemplate_Patch
    {
        public static void Postfix(GameObject go)
        {
            go.AddOrGet<ExtractionRateController>();

            var opts = ControlledExtractionOptions.Instance;
            bool skipLiquidPort = ControlledExtractionMod.IsRonivansLegacyLoaded();
            bool addingOurLiquidPort = opts.AddLiquidOutputPort && !skipLiquidPort;

            if (addingOurLiquidPort)
            {
                // Store oil output instead of spawning in world
                var converter = go.GetComponent<ElementConverter>();
                if (converter?.outputElements?.Length > 0)
                {
                    var output = converter.outputElements[0];
                    output.storeOutput = true;
                    converter.outputElements[0] = output;
                }

                var liquidDispenser = go.AddOrGet<ConduitDispenser>();
                liquidDispenser.conduitType = ConduitType.Liquid;
                liquidDispenser.alwaysDispense = true;
                liquidDispenser.elementFilter = new SimHashes[] { SimHashes.CrudeOil };
            }

            if (opts.AddGasOutputPort)
            {
                if (addingOurLiquidPort)
                {
                    // Gas is secondary when liquid is primary
                    var secondaryOutput = go.AddOrGet<ConduitSecondaryOutput>();
                    secondaryOutput.portInfo = new ConduitPortInfo(ConduitType.Gas, new CellOffset(1, 1));
                    go.AddOrGet<GasOutputController>();
                }
                else
                {
                    // Gas is primary (no liquid port or Ronivan's handles liquid)
                    var gasDispenser = go.AddOrGet<ConduitDispenser>();
                    gasDispenser.conduitType = ConduitType.Gas;
                    gasDispenser.alwaysDispense = true;
                    gasDispenser.elementFilter = null;
                }
            }
        }
    }

    // Apply settings on spawn (works for existing saves too)
    [HarmonyPatch(typeof(OilWellCap), "OnSpawn")]
    public static class OilWellCap_OnSpawn_Patch
    {
        public static void Postfix(OilWellCap __instance)
        {
            __instance.gameObject.AddOrGet<ExtractionRateController>();
            __instance.maxGasPressure = ControlledExtractionOptions.Instance.MaxGasStorage;
            // Oil storage is scaled dynamically in ExtractionRateController.ApplyRates()
        }
    }

    // Slider patches - repurpose backpressure slider for extraction rate
    [HarmonyPatch(typeof(OilWellCap), "get_SliderTitleKey")]
    public static class OilWellCap_SliderTitleKey_Patch
    {
        public static bool Prefix(ref string __result)
        {
            __result = "STRINGS.UI.UISIDESCREENS.EXTRACTIONRATE.TITLE";
            return false;
        }
    }

    [HarmonyPatch(typeof(OilWellCap), "get_SliderUnits")]
    public static class OilWellCap_SliderUnits_Patch
    {
        public static bool Prefix(ref string __result)
        {
            __result = STRINGS.UI.UNITSUFFIXES.MASS.KILOGRAM + STRINGS.UI.UNITSUFFIXES.PERSECOND;
            return false;
        }
    }

    [HarmonyPatch(typeof(OilWellCap), nameof(OilWellCap.GetSliderMin))]
    public static class OilWellCap_GetSliderMin_Patch
    {
        public static bool Prefix(ref float __result)
        {
            __result = ControlledExtractionOptions.Instance.MinWaterRate;
            return false;
        }
    }

    [HarmonyPatch(typeof(OilWellCap), nameof(OilWellCap.GetSliderMax))]
    public static class OilWellCap_GetSliderMax_Patch
    {
        public static bool Prefix(ref float __result)
        {
            __result = ControlledExtractionOptions.Instance.MaxWaterRate;
            return false;
        }
    }

    [HarmonyPatch(typeof(OilWellCap), nameof(OilWellCap.SliderDecimalPlaces))]
    public static class OilWellCap_SliderDecimalPlaces_Patch
    {
        public static bool Prefix(ref int __result)
        {
            __result = 2;
            return false;
        }
    }

    [HarmonyPatch(typeof(OilWellCap), nameof(OilWellCap.GetSliderValue))]
    public static class OilWellCap_GetSliderValue_Patch
    {
        public static bool Prefix(OilWellCap __instance, ref float __result)
        {
            var controller = __instance.GetComponent<ExtractionRateController>();
            __result = controller != null ? controller.WaterInputRate : ControlledExtractionOptions.Instance.DefaultWaterRate;
            return false;
        }
    }

    [HarmonyPatch(typeof(OilWellCap), nameof(OilWellCap.SetSliderValue))]
    public static class OilWellCap_SetSliderValue_Patch
    {
        public static bool Prefix(OilWellCap __instance, float value)
        {
            var controller = __instance.GetComponent<ExtractionRateController>();
            if (controller != null)
                controller.WaterInputRate = value;
            return false;
        }
    }

    [HarmonyPatch(typeof(OilWellCap), "ISliderControl.GetSliderTooltip")]
    public static class OilWellCap_GetSliderTooltip_Patch
    {
        public static bool Prefix(OilWellCap __instance, ref string __result)
        {
            var controller = __instance.GetComponent<ExtractionRateController>();
            if (controller != null)
            {
                float water = controller.WaterInputRate;
                float oil = water * ExtractionRateController.VANILLA_OIL_RATIO;
                __result = $"Water: {water:F2} kg/s → Oil: {oil:F2} kg/s ({controller.ExtractionMultiplier * 100:F0}% of vanilla)";
            }
            else
            {
                __result = "Extraction rate controller initializing...";
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(OilWellCap), nameof(OilWellCap.GetSliderTooltipKey))]
    public static class OilWellCap_GetSliderTooltipKey_Patch
    {
        public static bool Prefix(ref string __result)
        {
            __result = "STRINGS.UI.UISIDESCREENS.EXTRACTIONRATE.TOOLTIP";
            return false;
        }
    }

    // Use global backpressure threshold
    [HarmonyPatch(typeof(OilWellCap), nameof(OilWellCap.NeedsDepressurizing))]
    public static class OilWellCap_NeedsDepressurizing_Patch
    {
        private static System.Reflection.FieldInfo smiField;

        public static bool Prefix(OilWellCap __instance, ref bool __result)
        {
            if (smiField == null)
                smiField = AccessTools.Field(typeof(OilWellCap), "smi");

            var smi = smiField.GetValue(__instance) as OilWellCap.StatesInstance;
            if (smi != null)
            {
                float threshold = ControlledExtractionOptions.Instance.BackpressureThreshold / 100f;
                __result = smi.GetPressurePercent() >= threshold;
                return false;
            }
            return true;
        }
    }

    // Register UI strings
    [HarmonyPatch(typeof(Localization), "Initialize")]
    public static class Localization_Initialize_Patch
    {
        public static void Postfix()
        {
            Strings.Add("STRINGS.UI.UISIDESCREENS.EXTRACTIONRATE.TITLE", "Water Input Rate");
            Strings.Add("STRINGS.UI.UISIDESCREENS.EXTRACTIONRATE.TOOLTIP", "Adjust water input to control extraction speed");
        }
    }
}
