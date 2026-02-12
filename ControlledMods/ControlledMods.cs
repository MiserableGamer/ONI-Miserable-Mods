using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using System.Collections.Generic;
using ControlledMods.ModDetection;
using ControlledMods.Options;
using ControlledMods.Patches;
using ControlledMods.Patches.ResourceSensor;
using ControlledMods.Patches.UndergroundConduit;

namespace ControlledMods
{
    public class ControlledModsMod : UserMod2
    {
        // Set to true to enable debug logging, false for production
        public const bool EnableDebugLogs = true;

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

            // Apply patches that don't depend on other mods
            OptionsDialogPatch.ApplyPatch(harmony);
            MainMenuPatches.ApplyPatch(harmony);

            Log("Mod loaded - waiting for OnAllModsLoaded to detect target mods");
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

            Log("All conditional patches applied");
        }
    }
}
