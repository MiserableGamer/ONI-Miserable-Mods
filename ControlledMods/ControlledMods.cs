using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using System.Collections.Generic;
using ControlledMods.ModDetection;
using ControlledMods.Options;
using ControlledMods.Patches;
using ControlledMods.Patches.RonivansLegacy;

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

            // Register options - must be done in OnLoad for PLib UI to work
            new POptions().RegisterOptions(this, typeof(ControlledModsOptions));

            // Apply patch to resize the options dialog
            OptionsDialogPatch.ApplyPatch(harmony);

            // Increase PrimaryElement.MAX_MASS to allow larger storage capacities
            // This is the global cap that prevents storing more than ~25,000 kg
            var opts = ControlledModsOptions.Instance;
            float maxConfiguredCapacity = System.Math.Max(
                System.Math.Max(
                    System.Math.Max(opts.MedGasReservoirCapacity, opts.MedLiquidReservoirCapacity),
                    System.Math.Max(opts.SmallGasReservoirCapacity, opts.SmallLiquidReservoirCapacity)),
                System.Math.Max(opts.WallGasTankCapacity, opts.WallLiquidTankCapacity));

            if (maxConfiguredCapacity > PrimaryElement.MAX_MASS)
            {
                PrimaryElement.MAX_MASS = maxConfiguredCapacity;
                Log($"Increased PrimaryElement.MAX_MASS to {maxConfiguredCapacity}");
            }

            Log("Mod loaded - waiting for OnAllModsLoaded to detect target mods");
        }

        public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods)
        {
            base.OnAllModsLoaded(harmony, mods);

            // Detect which target mods are loaded
            ModDetector.DetectMods(mods);

            // Apply conditional patches based on detected mods
            if (ModDetector.RonivansLegacyLoaded)
            {
                ReservoirPatches.ApplyPatches(harmony);
            }

            Log("All conditional patches applied");
        }
    }
}
