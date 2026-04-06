using System.IO;
using System.Reflection;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using System.Collections.Generic;
using UnityEngine;
using ControlledMods.ModDetection;
using ControlledMods.Options;
using ControlledMods.Patches;
using ControlledMods.Patches.FreeResourceBuildings;
using ControlledMods.Patches.CustomizablePlants;
using ControlledMods.Patches.ResourceSensor;
using ControlledMods.Patches.SaveFileFixes;
using ControlledMods.Patches.UndergroundConduit;
using ControlledMods.Patches.DuplicantRoomSensor;
using ControlledMods.Patches.DarknessNotExcludedRelit;
using ControlledMods.Patches.SignsTagsAndRibbons;
using ControlledMods.Patches.DeliveryTemperatureLimit;

namespace ControlledMods
{
    public class ControlledModsMod : UserMod2
    {
        // Set to true to enable debug logging, false for production
        public const bool EnableDebugLogs = true;

        // Config save location fix for several mods — set to true to re-enable (registers Save File Fixes options and applies patches)
        public const bool EnableSaveFileFixes = false;

        public static ControlledModsMod Instance { get; private set; }
        public static Harmony HarmonyInstance { get; private set; }

        // Centralized debug logging - only logs if EnableDebugLogs is true
        public static void Log(string message)
        {
            if (EnableDebugLogs)
                Debug.Log($"[ControlledMods] {message}");
        }

        public static void LogWarning(string message)
        {
            if (EnableDebugLogs)
                Debug.LogWarning($"[ControlledMods] {message}");
        }

        /// <summary>Read version from mod_info.yaml next to our DLL (single source of truth; not AssemblyVersion).</summary>
        private static string GetModVersionFromModInfo()
        {
            try
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(dir)) return "?.?.?.?";
                string path = Path.Combine(dir, "mod_info.yaml");
                if (!File.Exists(path)) return "?.?.?.?";
                foreach (string line in File.ReadAllLines(path))
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("version:", System.StringComparison.OrdinalIgnoreCase))
                    {
                        string val = trimmed.Substring(8).Trim();
                        if (!string.IsNullOrEmpty(val)) return val;
                        break;
                    }
                }
                return "?.?.?.?";
            }
            catch
            {
                return "?.?.?.?";
            }
        }

        public override void OnLoad(Harmony harmony)
        {
            Instance = this;
            HarmonyInstance = harmony;

            base.OnLoad(harmony);

            // Initialize PLib
            PUtil.InitLibrary();
            ConfigMigrationHelper.Migrate(ConfigMigrationHelper.OldConfigFolderName, ConfigMigrationHelper.NewConfigFolderName);

            // Register options - must be done in OnLoad for PLib UI to work
            new POptions().RegisterOptions(this, typeof(ControlledModsOptions));
            // Save File Fixes section only when the feature is enabled (so it's hidden when disabled)
            if (EnableSaveFileFixes)
                new POptions().RegisterOptions(this, typeof(ControlledModsSaveFileFixOptions));

            // Apply patches that don't depend on other mods
            OptionsDialogPatch.ApplyPatch(harmony);
            MainMenuPatches.ApplyPatch(harmony);
            DebugPaintElementScreenPatches.ApplyPatches(harmony);

            string displayVersion = GetModVersionFromModInfo();
            Log($"Mod loaded version {displayVersion} - waiting for OnAllModsLoaded to detect target mods");
        }

        public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods)
        {
            base.OnAllModsLoaded(harmony, mods);

            // Detect which target mods are loaded
            ModDetector.DetectMods(mods);

            if (UndergroundConduitDetection.Loaded)
                UndergroundConduitPatches.ApplyPatches(harmony);

            if (ResourceSensorDetection.Loaded && ControlledModsOptions.Instance.EnableResourceSensor)
                ResourceSensorPatches.ApplyPatches(harmony);

            if (FreeResourceBuildingsDetection.Loaded)
            {
                FreeEnergyGeneratorPatches.ApplyPatches(harmony);

                if (ControlledModsOptions.Instance.AddPowerSinkBuilding)
                    PowerSinkRegistrationPatches.ApplyPatches(harmony);
            }

            if (CustomizablePlantsDetection.Loaded && ControlledModsOptions.Instance.EnableCustomizablePlantsVineBranchMaxAge)
                VineBranchMaxAgePatches.ApplyPatches(harmony);

            if (DuplicantRoomSensorDetection.Loaded && ControlledModsOptions.Instance.EnableDuplicantRoomSensorRangeCompatibility)
                DuplicantRoomSensorPatches.ApplyPatches(harmony);

            if (DarknessNotExcludedRelitDetection.Loaded && ControlledModsOptions.Instance.EnableDarknessImpliedLightOcclusionFix)
                DarknessNotExcludedRelitPatches.ApplyPatches(harmony);

            if (SignsTagsAndRibbonsDetection.Loaded)
                SignsTagsAndRibbonsPatches.ApplyPatches(harmony);

            if (DeliveryTemperatureLimitDetection.Loaded && ControlledModsOptions.Instance.FixDeliveryTemperatureLimitSideScreen)
                DeliveryTemperatureLimitPatches.ApplyPatches(harmony);

            if (EnableSaveFileFixes)
            {
                CustomModPathPatches.Apply(harmony);
                SaveFileFixApplier.Apply(harmony);
            }

            Log("All conditional patches applied");
        }
    }
}
