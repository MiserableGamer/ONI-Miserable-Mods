/*
 * NoDisease Mod for Oxygen Not Included
 * 
 * Concept inspired by FastTrack mod by Peter Han (https://github.com/peterhaneve/ONIMods)
 * This implementation is independently written with a different architecture.
 * 
 * MIT License - See LICENSE.txt
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Klei;
using Klei.AI;
using PeterHan.PLib.Core;
using UnityEngine;

namespace NoDisease.Patches
{
    // Disables the disease/germ system in ONI by patching relevant game systems.
    public static class NoDiseasePatches
    {
        // Cached accessors for private members
        private static FieldInfo disinfectableMarked;
        private static FieldInfo primaryElementCount;
        private static FieldInfo primaryElementID;
        private static PropertyInfo stateMachineSerializable;
        private static FieldInfo overlayToggleInfos;

        // Registers all patches to disable the disease system.
        public static void Apply(Harmony harmony)
        {
            // Cache accessors for private members
            CacheAccessors();

            var skip = new HarmonyMethod(typeof(NoDiseasePatches), nameof(SkipMethod));

            // Auto-disinfection
            SafePatch(harmony, typeof(AutoDisinfectableManager), "AddAutoDisinfectable", skip);
            SafePatch(harmony, typeof(AutoDisinfectableManager), "RemoveAutoDisinfectable", skip);

            // Germ info panels
            var hidePanel = new HarmonyMethod(typeof(NoDiseasePatches), nameof(HideGermInfoPanel));
            SafePatch(harmony, typeof(AdditionalDetailsPanel), "RefreshCurrentGermsPanel", hidePanel);
            SafePatch(harmony, typeof(AdditionalDetailsPanel), "RefreshDiseaseSourcePanel", hidePanel);

            // Conduit disease manager
            SafePatch(harmony, typeof(ConduitDiseaseManager), "AddDisease", skip);
            SafePatch(harmony, typeof(ConduitDiseaseManager), "ModifyDiseaseCount", skip);
            SafePatch(harmony, typeof(ConduitDiseaseManager), "Sim200ms", skip);

            // Disease containers
            SafePatch(harmony, typeof(DiseaseContainers), "AddDisease", skip);
            SafePatch(harmony, typeof(DiseaseContainers), "UpdateOverlayColours", skip);
            SafePatch(harmony, typeof(DiseaseContainers), "ModifyDiseaseCount", skip);
            SafePatch(harmony, typeof(DiseaseContainers), "Sim200ms", skip);

            // Disease emitters and droppers
            SafePatch(harmony, typeof(DiseaseDropper.Instance), "DropDisease", skip);
            SafePatch(harmony, typeof(DiseaseEmitter), "SimRegister", skip);
            SafePatch(harmony, typeof(DiseaseEmitter), "SimUnregister", skip);

            // Disinfectable marking
            SafePatch(harmony, typeof(Disinfectable), "MarkForDisinfect",
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(BlockDisinfectMark)));

            // Element emitter germs
            SafePatch(harmony, typeof(ElementEmitter), "ForceEmit",
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(ClearEmissionGerms)));
            SafePatch(harmony, typeof(ElementEmitter), "OnSimActivate",
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(ClearEmitterConfig)));

            // Germ exposure monitoring
            SafePatch(harmony, typeof(GermExposureMonitor), "InitializeStates",
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(DisableExposureStateMachine)));
            SafePatch(harmony, typeof(GermExposureMonitor.Instance), "OnAirConsumed", skip);
            SafePatch(harmony, typeof(GermExposureMonitor.Instance), "TryInjectDisease", skip);
            SafePatch(harmony, typeof(GermExposureMonitor.Instance), "UpdateReports", skip);

            // World germ cleanup on load
            SafePatch(harmony, typeof(Grid), "InitializeCells", null,
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(WipeGridGerms)));

            // Disease overlay toggle
            SafePatch(harmony, typeof(OverlayMenu), "InitializeToggles", null,
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(RemoveDiseaseOverlay)));

            // Primary element germs
            SafePatch(harmony, typeof(PrimaryElement), "AddDisease", skip);
            SafePatch(harmony, typeof(PrimaryElement), "ForcePermanentDiseaseContainer", skip);
            SafePatch(harmony, typeof(PrimaryElement), "ModifyDiseaseCount", skip);
            SafePatch(harmony, typeof(PrimaryElement), "OnDeserialized",
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(ClearDeserializedGerms)));

            // Sickness prevention
            SafePatch(harmony, typeof(Sicknesses), "CreateInstance",
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(CreateDummySickness)));
            SafePatch(harmony, typeof(Sicknesses), "Infect", skip);
            SafePatch(harmony, typeof(SicknessInstance), "InitializeAndStart", skip);

            // Sim messages
            SafePatch(harmony, typeof(SimMessages), "ModifyDiseaseOnCell", skip);
            SafePatch(harmony, typeof(SimMessages), "ModifyCell",
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(StripCellGerms)));

            // Storage germ cleanup
            PatchStorageConsume(harmony);

            // Tool menu
            SafePatch(harmony, typeof(ToolMenu), "CreateBasicTools", null,
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(RemoveDisinfectFromTools)));

            // World generation
            SafePatch(harmony, typeof(ProcGenGame.WorldGen), "RenderToMap", null,
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(ClearGeneratedGerms)));

            // Deprecate medical buildings
            DeprecateMedicalBuildings(harmony);

            PUtil.LogDebug("NoDisease patches applied successfully");
        }

        private static void CacheAccessors()
        {
            disinfectableMarked = AccessTools.Field(typeof(Disinfectable), "isMarkedForDisinfect");
            primaryElementCount = AccessTools.Field(typeof(PrimaryElement), "diseaseCount");
            primaryElementID = AccessTools.Field(typeof(PrimaryElement), "diseaseID");
            stateMachineSerializable = AccessTools.Property(typeof(StateMachine), "serializable");
            overlayToggleInfos = AccessTools.Field(typeof(OverlayMenu), "overlayToggleInfos");
        }

        private static void SafePatch(Harmony harmony, Type type, string methodName, HarmonyMethod prefix)
        {
            SafePatch(harmony, type, methodName, prefix, null);
        }

        private static void SafePatch(Harmony harmony, Type type, string methodName,
            HarmonyMethod prefix, HarmonyMethod postfix)
        {
            var method = AccessTools.Method(type, methodName);
            if (method != null)
            {
                try
                {
                    harmony.Patch(method, prefix: prefix, postfix: postfix);
                }
                catch (Exception e)
                {
                    PUtil.LogDebug($"Could not patch {type.Name}.{methodName}: {e.Message}");
                }
            }
            else
            {
                PUtil.LogDebug($"Method {type.Name}.{methodName} not found, skipping");
            }
        }

        private static void PatchStorageConsume(Harmony harmony)
        {
            // ConsumeAndGetDisease has specific parameter signature
            var outFloat = typeof(float).MakeByRefType();
            var outDiseaseInfo = typeof(SimUtil.DiseaseInfo).MakeByRefType();

            var method = AccessTools.Method(typeof(Storage), "ConsumeAndGetDisease",
                new Type[] { typeof(Tag), typeof(float), outFloat, outDiseaseInfo, outFloat });

            if (method != null)
            {
                try
                {
                    harmony.Patch(method,
                        postfix: new HarmonyMethod(typeof(NoDiseasePatches), nameof(ClearConsumedGerms)));
                }
                catch (Exception e)
                {
                    PUtil.LogDebug($"Could not patch Storage.ConsumeAndGetDisease: {e.Message}");
                }
            }
        }

        private static void DeprecateMedicalBuildings(Harmony harmony)
        {
            var deprecate = new HarmonyMethod(typeof(NoDiseasePatches), nameof(HideBuilding));

            // Doctor stations
            PatchBuildingConfig(harmony, typeof(AdvancedApothecaryConfig), deprecate);
            PatchBuildingConfig(harmony, typeof(AdvancedDoctorStationConfig), deprecate);
            PatchBuildingConfig(harmony, typeof(DoctorStationConfig), deprecate);

            // Keep basic Apothecary if radiation DLC is active (needed for rad pills)
            if (!DlcManager.FeatureRadiationEnabled())
                PatchBuildingConfig(harmony, typeof(ApothecaryConfig), deprecate);

            // Disease sensors
            PatchBuildingConfig(harmony, typeof(LogicDiseaseSensorConfig), deprecate);
            PatchBuildingConfig(harmony, typeof(GasConduitDiseaseSensorConfig), deprecate);
            PatchBuildingConfig(harmony, typeof(LiquidConduitDiseaseSensorConfig), deprecate);
            PatchBuildingConfig(harmony, typeof(SolidConduitDiseaseSensorConfig), deprecate);
        }

        private static void PatchBuildingConfig(Harmony harmony, Type configType, HarmonyMethod postfix)
        {
            var method = AccessTools.Method(configType, "CreateBuildingDef");
            if (method != null)
            {
                try { harmony.Patch(method, postfix: postfix); }
                catch { /* Building config might not exist in this version */ }
            }
        }

        #region Harmony Callbacks

        // Prevents the original method from executing
        private static bool SkipMethod() => false;

        // Hides germ information panels
        private static bool HideGermInfoPanel(CollapsibleDetailContentPanel targetPanel)
        {
            if (targetPanel != null)
                targetPanel.SetActive(false);
            return false;
        }

        // Prevents objects from being marked for disinfection
        private static bool BlockDisinfectMark(Disinfectable __instance)
        {
            disinfectableMarked?.SetValue(__instance, false);
            return false;
        }

        // Removes germ data from element emissions
        private static void ClearEmissionGerms(ref byte disease_idx, ref int disease_count)
        {
            disease_idx = Sim.InvalidDiseaseIdx;
            disease_count = 0;
        }

        // Clears germ configuration from element emitters
        private static void ClearEmitterConfig(ElementEmitter __instance)
        {
            __instance.outputElement.addedDiseaseIdx = Sim.InvalidDiseaseIdx;
            __instance.outputElement.addedDiseaseCount = 0;
        }

        // Disables the germ exposure state machine
        private static void DisableExposureStateMachine(GermExposureMonitor __instance,
            ref StateMachine.BaseState default_state)
        {
            default_state = __instance.root;
            stateMachineSerializable?.SetValue(__instance, StateMachine.SerializeType.Never);
        }

        // Wipes germs from all grid cells after world load
        private static void WipeGridGerms()
        {
            int cellCount = Grid.WidthInCells * Grid.HeightInCells;
            byte invalidIdx = SimUtil.DiseaseInfo.Invalid.idx;

            for (int i = 0; i < cellCount; i++)
            {
                int germCount = Grid.DiseaseCount[i];
                byte diseaseIdx = Grid.DiseaseIdx[i];
                if (diseaseIdx != invalidIdx && germCount > 0)
                {
                    // Use reverse patch to modify disease since original is disabled
                    GermCellCleaner.ModifyDiseaseOnCell(i, invalidIdx, -germCount);
                }
            }
        }

        // Removes the disease overlay toggle from the overlay menu
        private static void RemoveDiseaseOverlay(OverlayMenu __instance)
        {
            var toggles = overlayToggleInfos?.GetValue(__instance) as System.Collections.IList;
            if (toggles == null) return;

            for (int i = toggles.Count - 1; i >= 0; i--)
            {
                var toggle = toggles[i] as KIconToggleMenu.ToggleInfo;
                if (toggle != null && toggle.icon == "overlay_disease")
                    toggles.RemoveAt(i);
            }
        }

        // Clears germs when elements are loaded from save
        private static void ClearDeserializedGerms(PrimaryElement __instance)
        {
            // Setting count to zero prevents disease container creation
            primaryElementCount?.SetValue(__instance, 0);

            if (primaryElementID != null)
            {
                var diseaseID = (HashedString)primaryElementID.GetValue(__instance);
                diseaseID.HashValue = 0;
                primaryElementID.SetValue(__instance, diseaseID);
            }
        }

        // Creates an empty sickness instance without triggering infection
        private static void CreateDummySickness(ref SicknessInstance __result,
            Sickness sickness, Sicknesses __instance)
        {
            // Create instance without report, event trigger, or adding to list
            __result = new SicknessInstance(__instance.gameObject, sickness);
        }

        // Removes germ data from cell modification calls
        private static void StripCellGerms(ref byte disease_idx, ref int disease_count)
        {
            disease_count = 0;
            disease_idx = byte.MaxValue;
        }

        // Clears germ data from consumed items
        private static void ClearConsumedGerms(ref SimUtil.DiseaseInfo disease_info)
        {
            disease_info.count = 0;
            disease_info.idx = SimUtil.DiseaseInfo.Invalid.idx;
        }

        // Removes the disinfect tool from the tool menu
        private static void RemoveDisinfectFromTools(ToolMenu __instance)
        {
            __instance.basicTools?.RemoveAll(tool => tool.icon == "icon_action_disinfect");
        }

        // Clears germs from world generation data
        private static void ClearGeneratedGerms(ref Sim.DiseaseCell[] dcs)
        {
            if (dcs == null) return;

            int count = dcs.Length;
            byte invalidIdx = SimUtil.DiseaseInfo.Invalid.idx;

            for (int i = 0; i < count; i++)
            {
                ref var cell = ref dcs[i];
                cell.diseaseIdx = invalidIdx;
                cell.elementCount = 0;
            }
        }

        // Marks a building as deprecated (hidden from build menu)
        private static void HideBuilding(BuildingDef __result)
        {
            if (__result != null)
                __result.Deprecated = true;
        }

        #endregion

        #region Nested Patch Classes

        // Cures any existing diseases on duplicants when they spawn
        [HarmonyPatch(typeof(MinionConfig), nameof(MinionConfig.OnSpawn))]
        public static class CureSpawnedDuplicants
        {
            public static void Postfix(GameObject go)
            {
                if (go.TryGetComponent(out MinionModifiers modifiers))
                {
                    var sicknessList = modifiers.sicknesses?.ModifierList;
                    if (sicknessList != null)
                    {
                        // Trigger cure event for each sickness before clearing
                        foreach (var sicknessInstance in sicknessList)
                            modifiers.Trigger((int)GameHashes.SicknessCured, sicknessInstance);
                        sicknessList.Clear();
                    }
                }
            }
        }

        // Reverse patch to access ModifyDiseaseOnCell even though we've disabled the original
        [HarmonyPatch(typeof(SimMessages), nameof(SimMessages.ModifyDiseaseOnCell))]
        public static class GermCellCleaner
        {
            [HarmonyReversePatch]
            [HarmonyPatch(nameof(SimMessages.ModifyDiseaseOnCell))]
            [MethodImpl(MethodImplOptions.NoInlining)]
            internal static void ModifyDiseaseOnCell(int gameCell, byte disease_idx, int disease_delta)
            {
                // Stub - replaced by reverse patch at runtime
                _ = gameCell;
                _ = disease_idx;
                _ = disease_delta;
                // Prevent inlining with infinite loop that never executes
                while (System.DateTime.Now.Ticks > 0L)
                    throw new NotImplementedException("Reverse patch stub");
            }
        }

        #endregion
    }
}
