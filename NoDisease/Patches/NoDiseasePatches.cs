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
using Klei.AI;
using PeterHan.PLib.Core;
using UnityEngine;

namespace NoDisease.Patches
{
    // Disables the disease/germ system in ONI by patching relevant game systems.
    public static class NoDiseasePatches
    {
        // Cached FieldInfo for performance - avoids reflection lookup on each call
        private static FieldInfo primaryElementDiseaseCount;
        private static FieldInfo primaryElementDiseaseID;
        private static FieldInfo stateMachineSerializable;
        private static FieldInfo disinfectableIsMarked;
        private static FieldInfo overlayMenuToggleInfos;

        // Registers all patches to disable the disease system.
        public static void Apply(Harmony harmony)
        {
            // Cache FieldInfo objects once at load time for optimal performance
            CacheFieldInfo();

            // Apply patches in logical groups
            PatchGermSystems(harmony);
            PatchDuplicantHealth(harmony);
            PatchConduitSystems(harmony);
            PatchUIAndTools(harmony);
            PatchWorldGeneration(harmony);
            DeprecateMedicalBuildings(harmony);

            PUtil.LogDebug("NoDisease patches applied successfully");
        }

        // Cache all FieldInfo objects at mod load for fast access during callbacks
        private static void CacheFieldInfo()
        {
            primaryElementDiseaseCount = AccessTools.Field(typeof(PrimaryElement), "diseaseCount");
            primaryElementDiseaseID = AccessTools.Field(typeof(PrimaryElement), "diseaseID");
            stateMachineSerializable = AccessTools.Field(typeof(StateMachine), "serializable");
            disinfectableIsMarked = AccessTools.Field(typeof(Disinfectable), "isMarkedForDisinfect");
            overlayMenuToggleInfos = AccessTools.Field(typeof(OverlayMenu), "overlayToggleInfos");
        }

        #region Germ System Patches

        private static void PatchGermSystems(Harmony harmony)
        {
            var skipMethod = new HarmonyMethod(typeof(NoDiseasePatches), nameof(SkipOriginalMethod));

            // Disable germ containers that track germs on objects
            SafePatch(harmony, typeof(DiseaseContainers), "AddDisease", skipMethod);
            SafePatch(harmony, typeof(DiseaseContainers), "ModifyDiseaseCount", skipMethod);
            SafePatch(harmony, typeof(DiseaseContainers), "UpdateOverlayColours", skipMethod);
            SafePatch(harmony, typeof(DiseaseContainers), "Sim200ms", skipMethod);

            // Disable germ emitters (private methods)
            SafePatch(harmony, typeof(DiseaseEmitter), "SimRegister", skipMethod);
            SafePatch(harmony, typeof(DiseaseEmitter), "SimUnregister", skipMethod);
            SafePatch(harmony, typeof(DiseaseDropper.Instance), "DropDisease", skipMethod);

            // Prevent germs from being added to elements
            SafePatch(harmony, typeof(PrimaryElement), "AddDisease", skipMethod);
            SafePatch(harmony, typeof(PrimaryElement), "ModifyDiseaseCount", skipMethod);
            SafePatch(harmony, typeof(PrimaryElement), "ForcePermanentDiseaseContainer", skipMethod);

            // Clear germs when elements are loaded from save
            SafePatch(harmony, typeof(PrimaryElement), "OnDeserialized", 
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(ClearElementGerms)));

            // Disable germ simulation messages
            SafePatch(harmony, typeof(SimMessages), "ModifyDiseaseOnCell", skipMethod);
            SafePatch(harmony, typeof(SimMessages), "ModifyCell",
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(StripGermsFromCellModification)));

            // Note: Storage.ConsumeAndGetDisease has multiple overloads - we skip patching it
            // since disease is already disabled at the PrimaryElement and DiseaseContainers level

            // Remove germs from element emissions
            SafePatch(harmony, typeof(ElementEmitter), "ForceEmit", 
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(StripGermsFromEmission)));
            SafePatch(harmony, typeof(ElementEmitter), "OnSimActivate",
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(StripGermsFromEmitterConfig)));
        }

        #endregion

        #region Duplicant Health Patches

        private static void PatchDuplicantHealth(Harmony harmony)
        {
            var skipMethod = new HarmonyMethod(typeof(NoDiseasePatches), nameof(SkipOriginalMethod));

            // Disable germ exposure monitoring
            SafePatch(harmony, typeof(GermExposureMonitor), "InitializeStates",
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(DisableGermExposureStateMachine)));

            // GermExposureMonitor.Instance methods
            SafePatch(harmony, typeof(GermExposureMonitor.Instance), "OnAirConsumed", skipMethod);
            SafePatch(harmony, typeof(GermExposureMonitor.Instance), "TryInjectDisease", skipMethod);
            SafePatch(harmony, typeof(GermExposureMonitor.Instance), "UpdateReports", skipMethod);

            // Prevent sickness from being applied
            SafePatch(harmony, typeof(Sicknesses), "CreateInstance", 
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(CreateEmptySicknessInstance)));
            SafePatch(harmony, typeof(Sicknesses), "Infect", skipMethod);
            SafePatch(harmony, typeof(SicknessInstance), "InitializeAndStart", skipMethod);
        }

        #endregion

        #region Conduit System Patches

        private static void PatchConduitSystems(Harmony harmony)
        {
            var skipMethod = new HarmonyMethod(typeof(NoDiseasePatches), nameof(SkipOriginalMethod));

            // Disable germ tracking in pipes
            SafePatch(harmony, typeof(ConduitDiseaseManager), "AddDisease", skipMethod);
            SafePatch(harmony, typeof(ConduitDiseaseManager), "ModifyDiseaseCount", skipMethod);
            SafePatch(harmony, typeof(ConduitDiseaseManager), "Sim200ms", skipMethod);
            // Note: SetData has multiple overloads - skip it since AddDisease/ModifyDiseaseCount cover disease logic
        }

        #endregion

        #region UI and Tools Patches

        private static void PatchUIAndTools(Harmony harmony)
        {
            var skipMethod = new HarmonyMethod(typeof(NoDiseasePatches), nameof(SkipOriginalMethod));

            // Disable auto-disinfect system
            SafePatch(harmony, typeof(AutoDisinfectableManager), "AddAutoDisinfectable", skipMethod);
            SafePatch(harmony, typeof(AutoDisinfectableManager), "RemoveAutoDisinfectable", skipMethod);

            // Prevent manual disinfection
            SafePatch(harmony, typeof(Disinfectable), "MarkForDisinfect",
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(PreventDisinfectMark)));

            // Hide germ info panels
            SafePatch(harmony, typeof(AdditionalDetailsPanel), "RefreshCurrentGermsPanel",
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(HideGermPanel)));
            SafePatch(harmony, typeof(AdditionalDetailsPanel), "RefreshDiseaseSourcePanel",
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(HideGermPanel)));

            // Remove disease overlay from menu
            SafePatch(harmony, typeof(OverlayMenu), "InitializeToggles", null,
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(RemoveDiseaseOverlayToggle)));

            // Remove disinfect tool
            SafePatch(harmony, typeof(ToolMenu), "CreateBasicTools", null,
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(RemoveDisinfectTool)));
        }

        #endregion

        #region World Generation Patches

        private static void PatchWorldGeneration(Harmony harmony)
        {
            // Clean germs from world on load - use reverse patch method
            SafePatch(harmony, typeof(Grid), "InitializeCells", null,
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(WipeWorldGerms)));

            // Prevent germs in newly generated worlds
            SafePatch(harmony, typeof(ProcGenGame.WorldGen), "RenderToMap", null,
                new HarmonyMethod(typeof(NoDiseasePatches), nameof(CleanGeneratedWorldGerms)));
        }

        #endregion

        #region Medical Buildings

        private static void DeprecateMedicalBuildings(Harmony harmony)
        {
            var hideBuilding = new HarmonyMethod(typeof(NoDiseasePatches), nameof(MarkAsDeprecated));

            // Hide doctor stations
            PatchBuildingConfig(harmony, typeof(DoctorStationConfig), hideBuilding);
            PatchBuildingConfig(harmony, typeof(AdvancedDoctorStationConfig), hideBuilding);
            PatchBuildingConfig(harmony, typeof(AdvancedApothecaryConfig), hideBuilding);

            // Keep basic Apothecary if radiation DLC is active (needed for rad pills)
            if (!DlcManager.FeatureRadiationEnabled())
            {
                PatchBuildingConfig(harmony, typeof(ApothecaryConfig), hideBuilding);
            }

            // Hide disease sensors
            PatchBuildingConfig(harmony, typeof(LogicDiseaseSensorConfig), hideBuilding);
            PatchBuildingConfig(harmony, typeof(GasConduitDiseaseSensorConfig), hideBuilding);
            PatchBuildingConfig(harmony, typeof(LiquidConduitDiseaseSensorConfig), hideBuilding);
            PatchBuildingConfig(harmony, typeof(SolidConduitDiseaseSensorConfig), hideBuilding);
        }

        private static void PatchBuildingConfig(Harmony harmony, Type configType, HarmonyMethod postfix)
        {
            var method = AccessTools.Method(configType, "CreateBuildingDef");
            if (method != null)
            {
                try
                {
                    harmony.Patch(method, postfix: postfix);
                }
                catch (Exception e)
                {
                    PUtil.LogDebug($"Could not patch {configType.Name}: {e.Message}");
                }
            }
        }

        #endregion

        #region Harmony Callbacks

        // Universal prefix that skips the original method.
        private static bool SkipOriginalMethod() => false;

        // Clears germ data from deserialized elements using cached reflection.
        private static void ClearElementGerms(PrimaryElement __instance)
        {
            if (primaryElementDiseaseCount != null)
                primaryElementDiseaseCount.SetValue(__instance, 0);
            if (primaryElementDiseaseID != null)
            {
                // diseaseID is HashedString, set HashValue to 0
                var diseaseID = (HashedString)primaryElementDiseaseID.GetValue(__instance);
                diseaseID.HashValue = 0;
                primaryElementDiseaseID.SetValue(__instance, diseaseID);
            }
        }

        // Removes germ data from cell modification calls.
        private static void StripGermsFromCellModification(ref byte disease_idx, ref int disease_count)
        {
            disease_idx = byte.MaxValue;
            disease_count = 0;
        }

        // Removes germs from element emission calls.
        private static void StripGermsFromEmission(ref byte disease_idx, ref int disease_count)
        {
            disease_idx = byte.MaxValue;
            disease_count = 0;
        }

        // Clears germ configuration from element emitters.
        private static void StripGermsFromEmitterConfig(ElementEmitter __instance)
        {
            // outputElement is a struct field, always exists
            __instance.outputElement.addedDiseaseIdx = byte.MaxValue;
            __instance.outputElement.addedDiseaseCount = 0;
        }

        // Disables the germ exposure state machine.
        private static void DisableGermExposureStateMachine(GermExposureMonitor __instance,
            ref StateMachine.BaseState default_state)
        {
            default_state = __instance.root;
            if (stateMachineSerializable != null)
                stateMachineSerializable.SetValue(__instance, StateMachine.SerializeType.Never);
        }

        // Creates an empty sickness instance without triggering infection logic.
        private static bool CreateEmptySicknessInstance(ref SicknessInstance __result,
            Sickness sickness, Sicknesses __instance)
        {
            __result = new SicknessInstance(__instance.gameObject, sickness);
            return false;
        }

        // Prevents objects from being marked for disinfection.
        private static bool PreventDisinfectMark(Disinfectable __instance)
        {
            if (disinfectableIsMarked != null)
                disinfectableIsMarked.SetValue(__instance, false);
            return false;
        }

        // Hides germ-related UI panels.
        private static bool HideGermPanel(CollapsibleDetailContentPanel targetPanel)
        {
            if (targetPanel != null)
                targetPanel.SetActive(false);
            return false;
        }

        // Removes the disease overlay option from the overlay menu.
        private static void RemoveDiseaseOverlayToggle(OverlayMenu __instance)
        {
            if (overlayMenuToggleInfos == null) return;

            var toggleInfos = overlayMenuToggleInfos.GetValue(__instance) as System.Collections.IList;
            if (toggleInfos == null) return;

            // Find and remove disease overlay
            for (int i = toggleInfos.Count - 1; i >= 0; i--)
            {
                var toggle = toggleInfos[i] as KIconToggleMenu.ToggleInfo;
                if (toggle != null && toggle.icon == "overlay_disease")
                {
                    toggleInfos.RemoveAt(i);
                }
            }
        }

        // Removes the disinfect tool from the tool menu.
        private static void RemoveDisinfectTool(ToolMenu __instance)
        {
            var basicTools = __instance.basicTools;
            if (basicTools == null) return;

            for (int i = basicTools.Count - 1; i >= 0; i--)
            {
                if (basicTools[i].icon == "icon_action_disinfect")
                {
                    basicTools.RemoveAt(i);
                }
            }
        }

        // Clears all germs from the world grid after loading using reverse patch.
        private static void WipeWorldGerms()
        {
            int n = Grid.WidthInCells * Grid.HeightInCells;
            byte invalidIdx = byte.MaxValue;
            
            for (int i = 0; i < n; i++)
            {
                int germs = Grid.DiseaseCount[i];
                byte disease = Grid.DiseaseIdx[i];
                if (disease != invalidIdx && germs > 0)
                {
                    // Use reverse patch to modify disease even though the original is disabled
                    DiseaseCellModifier.ModifyDiseaseOnCell(i, invalidIdx, -germs);
                }
            }
        }

        // Removes germs from world generation data.
        private static void CleanGeneratedWorldGerms(ref Sim.DiseaseCell[] dcs)
        {
            if (dcs == null) return;

            byte invalidIdx = byte.MaxValue;
            for (int i = 0; i < dcs.Length; i++)
            {
                dcs[i].diseaseIdx = invalidIdx;
                dcs[i].elementCount = 0;
            }
        }

        // Marks a building as deprecated (hidden from build menu).
        private static void MarkAsDeprecated(BuildingDef __result)
        {
            if (__result != null)
                __result.Deprecated = true;
        }

        #endregion

        #region Helper Methods

        // Safely patches a method using AccessTools to find it.
        private static void SafePatch(Harmony harmony, Type type, string methodName, HarmonyMethod prefix)
        {
            SafePatch(harmony, type, methodName, prefix, null);
        }

        // Safely patches a method with both prefix and postfix.
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
                    PUtil.LogWarning($"Failed to patch {type.Name}.{methodName}: {e.Message}");
                }
            }
            else
            {
                PUtil.LogDebug($"Method {type.Name}.{methodName} not found, skipping patch");
            }
        }

        #endregion

        #region Nested Patch Classes

        // Cures any existing diseases on duplicants when they spawn.
        [HarmonyPatch(typeof(MinionConfig), nameof(MinionConfig.OnSpawn))]
        public static class CureExistingDuplicantDiseases
        {
            public static void Postfix(GameObject go)
            {
                if (go == null) return;

                var modifiers = go.GetComponent<MinionModifiers>();
                if (modifiers?.sicknesses?.ModifierList == null) return;

                var diseases = modifiers.sicknesses.ModifierList;
                var toRemove = new List<SicknessInstance>(diseases);
                foreach (var disease in toRemove)
                {
                    modifiers.Trigger((int)GameHashes.SicknessCured, disease);
                }
                diseases.Clear();
            }
        }

        // Extracts the SimMessages.ModifyDiseaseOnCell method to modify diseases on load
        // even with the original method turned off.
        [HarmonyPatch(typeof(SimMessages), nameof(SimMessages.ModifyDiseaseOnCell))]
        public static class DiseaseCellModifier
        {
            [HarmonyReversePatch]
            [HarmonyPatch(nameof(SimMessages.ModifyDiseaseOnCell))]
            [MethodImpl(MethodImplOptions.NoInlining)]
            internal static void ModifyDiseaseOnCell(int gameCell, byte disease_idx, int disease_delta)
            {
                // Dummy code - this gets replaced by the reverse patch at runtime
                _ = gameCell;
                _ = disease_idx;
                _ = disease_delta;
                while (System.DateTime.Now.Ticks > 0L)
                    throw new NotImplementedException("Reverse patch stub");
            }
        }

        #endregion
    }
}
