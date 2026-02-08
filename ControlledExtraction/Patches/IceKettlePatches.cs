using HarmonyLib;
using ControlledExtraction.Components;
using ControlledExtraction.Options;
using ControlledExtraction.UI;
using PeterHan.PLib.UI;
using UnityEngine;

namespace ControlledExtraction.Patches
{
    // No changes to CreateBuildingDef - we use a secondary output port,
    // not OutputConduitType, to avoid RequireOutputs operational issues.

    [HarmonyPatch(typeof(IceKettleConfig), "ConfigureBuildingTemplate")]
    public static class IceKettleConfig_ConfigureBuildingTemplate_Patch
    {
        public static void Postfix(GameObject go)
        {
            var opts = ControlledExtractionOptions.Instance;

            if (opts.IceKettleCO2Port)
            {
                var gasOutput = go.AddComponent<ConduitSecondaryOutput>();
                gasOutput.portInfo = new ConduitPortInfo(ConduitType.Gas, new CellOffset(1, 1));

                // Gas output controller handles piping and environment fallback.
                // Uses fuelStorage (first storage) where CO2 gets stored by our
                // MeltNextBatch prefix. CO2 amounts are tiny (~0.4 kg/cycle).
                go.AddOrGet<SecondaryGasOutputController>().gasElement = SimHashes.CarbonDioxide;
            }

            if (opts.IceKettleLiquidPort)
            {
                var liquidOutput = go.AddComponent<ConduitSecondaryOutput>();
                liquidOutput.portInfo = new ConduitPortInfo(ConduitType.Liquid, new CellOffset(1, 0));

                // outputStorage is index 2; accept any liquid, not just a specific one
                var liquidController = go.AddOrGet<SecondaryLiquidOutputController>();
                liquidController.storageIndex = 2;
                liquidController.filterByElement = false;
            }

            // Always add the controller - other mods (Ronivan's Legacy, etc.) may add
            // IceOre elements at runtime that we auto-discover in BuildEnabledList.
            // The sidescreen only shows if multiple elements are available (HasMultipleOptions).
            var meltables = opts.IceKettleMeltables;
            if (meltables != null && HasAnyMeltableEnabled(meltables))
            {
                go.AddOrGet<IceKettleController>();
                go.AddOrGet<CopyBuildingSettings>();
            }
        }

        // Returns true if any meltable type is enabled in global options.
        // Checks both individual vanilla toggles and the modded toggle.
        // If no toggles are set, defaults apply (Ice = true).
        internal static bool HasAnyMeltableEnabled(IceKettleMeltableOptions m)
        {
            if (m == null) return true;
            if (m.EnableModdedMeltables) return true;
            if (m.ElementToggles != null && m.ElementToggles.Count > 0)
            {
                foreach (var kvp in m.ElementToggles)
                    if (kvp.Value) return true;
                return false;
            }
            // Empty dictionary means defaults apply (Ice = true)
            return true;
        }
    }

    [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
    public static class DetailsScreen_OnPrefabInit_Patch
    {
        private static DetailsScreen _lastDetailsScreen;

        internal static bool Prepare()
        {
            var meltables = ControlledExtractionOptions.Instance.IceKettleMeltables;
            return meltables != null &&
                IceKettleConfig_ConfigureBuildingTemplate_Patch.HasAnyMeltableEnabled(meltables);
        }

        internal static void Postfix(DetailsScreen __instance)
        {
            if (__instance != _lastDetailsScreen)
            {
                _lastDetailsScreen = __instance;
                PUIUtils.AddSideScreenContentWithOrdering<IceKettleSelectionSideScreen>(
                    typeof(TreeFilterableSideScreen).FullName, true, null);
            }
        }
    }

    // RESTORE POINT: Remove this entire patch class to revert to vanilla MeltNextBatch behavior.
    // This Prefix runs when CO2 port is enabled OR IceKettleController is active.
    // Two responsibilities:
    //   1. Use elementToMelt.tag (not def.targetElementTag) so switched elements work
    //   2. When CO2 port is on, store exhaust in fuelStorage for conduit output
    [HarmonyPatch(typeof(IceKettle.Instance), "MeltNextBatch")]
    public static class IceKettle_Instance_MeltNextBatch_Patch
    {
        private static bool hasCO2Port;
        private static bool hasMultipleMeltables;

        internal static bool Prepare()
        {
            var opts = ControlledExtractionOptions.Instance;
            hasCO2Port = opts.IceKettleCO2Port;
            hasMultipleMeltables = opts.IceKettleMeltables != null &&
                IceKettleConfig_ConfigureBuildingTemplate_Patch.HasAnyMeltableEnabled(opts.IceKettleMeltables);
            return hasCO2Port || hasMultipleMeltables;
        }

        public static bool Prefix(IceKettle.Instance __instance)
        {
            if (!__instance.HasAtLeastOneBatchOfSolidsWaitingToMelt)
                return false;

            var def = __instance.def;

            // When multi-select controller is present, dynamically determine which
            // element to melt based on what's actually in kettleStorage (most mass first).
            // Without controller, use the current elementToMelt (set by vanilla or single-select).
            if (hasMultipleMeltables)
            {
                var controller = __instance.gameObject.GetComponent<IceKettleController>();
                if (controller != null)
                {
                    var best = controller.FindBestElementToMelt();
                    if (best == null) return false;
                    __instance.elementToMelt = best;
                }
            }

            // Use elementToMelt.tag (not def.targetElementTag) to support
            // IceKettleController switching the ice type per-building.
            // Vanilla uses def.targetElementTag which is always Ice and would
            // cause a NullReferenceException when a different element is selected.
            var iceItem = __instance.kettleStorage.FindFirst(__instance.elementToMelt.tag);
            if (iceItem == null) return false;
            PrimaryElement icePE = iceItem.GetComponent<PrimaryElement>();
            float iceTemp = icePE.Temperature;
            byte diseaseIdx = icePE.DiseaseIdx;
            int diseaseCount = icePE.DiseaseCount;
            float iceMass = icePE.Mass;

            // Support partial batches: melt what we have, even if less than a full batch.
            // Happens when multiple ice types share the storage and no single type
            // has enough for a full KGToMeltPerBatch.
            float massToMelt = Mathf.Min(def.KGToMeltPerBatch, iceMass);

            // Scale disease proportionally if we're taking less than the full stack
            if (massToMelt < iceMass && iceMass > 0f)
                diseaseCount = (int)(diseaseCount * (massToMelt / iceMass));

            float fuelNeeded = __instance.GetUnitsOfFuelRequiredToMelt(
                __instance.elementToMelt, massToMelt, iceTemp);
            float fuelUsed = Mathf.Min(fuelNeeded, __instance.FuelUnitsAvailable);

            __instance.kettleStorage.ConsumeIgnoringDisease(
                __instance.elementToMelt.id.CreateTag(), massToMelt);

            __instance.outputStorage.AddElement(
                __instance.elementToMelt.highTempTransitionTarget, massToMelt,
                def.TargetTemperature, diseaseIdx, diseaseCount, false, true);

            // Fuel may have been consumed between the state check and this call
            var fuelItem = __instance.fuelStorage.FindFirst(def.fuelElementTag);
            if (fuelItem == null) return false;
            float fuelTemp = fuelItem.GetComponent<PrimaryElement>().Temperature;

            __instance.fuelStorage.ConsumeIgnoringDisease(def.fuelElementTag, fuelUsed);

            float exhaustMass = fuelUsed * def.ExhaustMassPerUnitOfLumber;
            if (exhaustMass > 0f)
            {
                if (hasCO2Port)
                {
                    // Store in fuelStorage for SecondaryGasOutputController to pipe
                    // out or emit to environment on the next conduit tick
                    __instance.fuelStorage.AddGasChunk(
                        def.exhaust_tag, exhaustMass, fuelTemp,
                        byte.MaxValue, 0, false);
                }
                else
                {
                    // Emit to world directly (same as vanilla)
                    var exhaust = ElementLoader.FindElementByHash(def.exhaust_tag);
                    SimMessages.AddRemoveSubstance(
                        Grid.PosToCell(__instance.gameObject), exhaust.id, null,
                        exhaustMass, fuelTemp, byte.MaxValue, 0, true, -1);
                }
            }

            return false; // skip original
        }
    }
}
