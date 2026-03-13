using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ControlledMods.ModDetection
{
    // Overarching mod detection: shared helpers and orchestration. Per-mod logic lives in separate *Detection.cs files.
    public static class ModDetector
    {
        // Call from OnAllModsLoaded to detect all supported mods
        public static void DetectMods(IReadOnlyList<KMod.Mod> mods)
        {
            UndergroundConduitDetection.Detect();
            ResourceSensorDetection.Detect();
            FreeResourceBuildingsDetection.Detect();
            CustomizablePlantsDetection.Detect();
            DuplicantRoomSensorDetection.Detect();
            ShowRangeDetection.Detect();
            DarknessNotExcludedRelitDetection.Detect();
            SignsTagsAndRibbonsDetection.Detect();

            LogDetectionResults();
        }

        // --- Shared helpers for use by per-mod detection classes ---

        public static bool DetectByType(string fullTypeName)
        {
            try
            {
                Type type = AccessTools.TypeByName(fullTypeName);
                return type != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool DetectByAssembly(string assemblyName)
        {
            try
            {
                return AppDomain.CurrentDomain.GetAssemblies()
                    .Any(a => a.GetName().Name == assemblyName);
            }
            catch
            {
                return false;
            }
        }

        public static bool DetectByModId(IReadOnlyList<KMod.Mod> mods, string modId)
        {
            return mods.Any(m => m.staticID == modId);
        }

        private static void LogDetectionResults()
        {
            ControlledModsMod.Log("Mod Detection Results:");
            ControlledModsMod.Log($"  - {UndergroundConduitDetection.DisplayName}: {(UndergroundConduitDetection.Loaded ? "DETECTED" : "not found")}");
            ControlledModsMod.Log($"  - {ResourceSensorDetection.DisplayName}: {(ResourceSensorDetection.Loaded ? "DETECTED" : "not found")}");
            ControlledModsMod.Log($"  - {FreeResourceBuildingsDetection.DisplayName}: {(FreeResourceBuildingsDetection.Loaded ? "DETECTED" : "not found")}");
            ControlledModsMod.Log($"  - {CustomizablePlantsDetection.DisplayName}: {(CustomizablePlantsDetection.Loaded ? "DETECTED" : "not found")}");
            ControlledModsMod.Log($"  - {DuplicantRoomSensorDetection.DisplayName}: {(DuplicantRoomSensorDetection.Loaded ? "DETECTED" : "not found")}");
            ControlledModsMod.Log($"  - {ShowRangeDetection.DisplayName}: {(ShowRangeDetection.Loaded ? "DETECTED" : "not found")}");
            ControlledModsMod.Log($"  - {DarknessNotExcludedRelitDetection.DisplayName}: {(DarknessNotExcludedRelitDetection.Loaded ? "DETECTED" : "not found")}");
            ControlledModsMod.Log($"  - {SignsTagsAndRibbonsDetection.DisplayName}: {(SignsTagsAndRibbonsDetection.Loaded ? "DETECTED" : "not found")}");
        }
    }
}
