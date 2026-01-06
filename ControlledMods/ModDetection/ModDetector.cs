using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ControlledMods.ModDetection
{
    // Centralized mod detection for all supported mods
    public static class ModDetector
    {
        // Detected mod flags - set during OnAllModsLoaded
        public static bool RonivansLegacyLoaded { get; private set; }

        // Mod identification info
        private static class ModIds
        {
            // Ronivan's Legacy - the combined "All In One" mod
            public const string RonivansLegacyAssembly = "RonivansLegacy_AllInOne";
            public const string RonivansLegacyType = "RonivansLegacy_ChemicalProcessing.Mod";
        }

        // Call this from OnAllModsLoaded to detect all supported mods
        public static void DetectMods(IReadOnlyList<KMod.Mod> mods)
        {
            // Detect Ronivan's Legacy
            RonivansLegacyLoaded = DetectByType(ModIds.RonivansLegacyType);

            LogDetectionResults();
        }

        // Detect mod by checking if a specific type exists
        private static bool DetectByType(string fullTypeName)
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

        // Detect mod by checking loaded assemblies
        private static bool DetectByAssembly(string assemblyName)
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

        // Detect mod by checking the mod list for a specific staticID
        private static bool DetectByModId(IReadOnlyList<KMod.Mod> mods, string modId)
        {
            return mods.Any(m => m.staticID == modId);
        }

        private static void LogDetectionResults()
        {
            ControlledModsMod.Log("Mod Detection Results:");
            ControlledModsMod.Log($"  - Ronivan's Legacy: {(RonivansLegacyLoaded ? "DETECTED" : "not found")}");
        }
    }
}

