using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using ControlledMods.ModDetection;
using ControlledMods.Options;
using PeterHan.PLib.Options;

namespace ControlledMods.Patches.SaveFileFixes
{
    /// <summary>Runs migration and applies path-redirect patches for Save File Fixes. Call from OnAllModsLoaded.</summary>
    public static class SaveFileFixApplier
    {
        private static readonly Dictionary<Assembly, string> ModAssemblyToModId = new Dictionary<Assembly, string>();
        /// <summary>Workshop path (normalized) -> shared path. Used by WriteSettings_Prefix to redirect saves.</summary>
        private static readonly List<(string normalizedWorkshop, string shared)> WorkshopToSharedPaths = new List<(string, string)>();

        public static void Apply(Harmony harmony)
        {
            if (harmony == null) return;

            var opts = POptions.ReadSettings<ControlledModsSaveFileFixOptions>() ?? new ControlledModsSaveFileFixOptions();
            var plibModIds = new[]
            {
                SaveFileFixPaths.ModIds.AdvancedCoolers,
                SaveFileFixPaths.ModIds.AiImprovements,
                SaveFileFixPaths.ModIds.CustomizableSpeed,
                SaveFileFixPaths.ModIds.DefaultBuildingSettings,
                SaveFileFixPaths.ModIds.Drains,
                SaveFileFixPaths.ModIds.ToastControl,
                SaveFileFixPaths.ModIds.WallPumps
            };

            ModAssemblyToModId.Clear();
            WorkshopToSharedPaths.Clear();

            foreach (string modId in plibModIds)
            {
                if (!IsEnabledFor(opts, modId)) continue;
                if (!SaveFileFixModDetection.IsLoaded(modId)) continue;

                Assembly modAssembly = SaveFileFixModDetection.GetAssemblyForModId(modId);
                if (modAssembly == null) continue;

                ModAssemblyToModId[modAssembly] = modId;

                // Migration: workshop-only -> shared; both = no-op
                string modDir = SaveFileFixModDetection.GetModDirectory(modId);
                if (!string.IsNullOrEmpty(modDir))
                {
                    string fileName = SaveFileFixPaths.GetConfigFileName(modId);
                    string workshopPath = Path.Combine(modDir, fileName);
                    string sharedPath = SaveFileFixPaths.GetSharedConfigPath(modId, fileName);
                    SaveFileFixMigration.TryMigrateToShared(workshopPath, sharedPath);
                    string norm = NormalizePath(workshopPath);
                    if (!string.IsNullOrEmpty(norm))
                        WorkshopToSharedPaths.Add((norm, sharedPath));
                }
            }

            if (ModAssemblyToModId.Count == 0) return;

            // Patch GetConfigPath in every assembly that contains POptions (except our own).
            // The options dialog may use a different PLib copy when saving (e.g. first-loaded mod's),
            // so we must patch all copies; our postfix only changes the path when modAssembly is one of our targets.
            Assembly ourAssembly = typeof(SaveFileFixApplier).Assembly;
            var assembliesToPatch = new Dictionary<Assembly, object>();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm == ourAssembly) continue; // don't patch our own POptions
                try
                {
                    Type poptions = asm.GetType("PeterHan.PLib.Options.POptions", throwOnError: false);
                    if (poptions == null) continue;
                    MethodInfo getConfigPath = AccessTools.Method(poptions, "GetConfigPath",
                        new[] { typeof(ConfigFileAttribute), typeof(Assembly) });
                    if (getConfigPath == null) continue;
                    assembliesToPatch[asm] = null;
                }
                catch { /* ignore */ }
            }

            foreach (Assembly asm in assembliesToPatch.Keys)
            {
                try
                {
                    Type poptions = asm.GetType("PeterHan.PLib.Options.POptions", throwOnError: false);
                    if (poptions == null) continue;
                    MethodInfo getConfigPath = AccessTools.Method(poptions, "GetConfigPath",
                        new[] { typeof(ConfigFileAttribute), typeof(Assembly) });
                    if (getConfigPath != null)
                    {
                        harmony.Patch(getConfigPath, postfix: new HarmonyMethod(typeof(SaveFileFixApplier), nameof(GetConfigPath_Postfix)));
                        ControlledModsMod.Log("Save File Fixes patches applied");
                    }
                    // Also patch WriteSettings(object, string path, bool) so saves go to shared even if path was cached
                    MethodInfo writeSettings = AccessTools.Method(poptions, "WriteSettings",
                        new[] { typeof(object), typeof(string), typeof(bool) });
                    if (writeSettings != null)
                    {
                        harmony.Patch(writeSettings, prefix: new HarmonyMethod(typeof(SaveFileFixApplier), nameof(WriteSettings_Prefix)));
                        ControlledModsMod.Log("Save File Fixes patches applied");
                    }
                }
                catch (Exception ex)
                {
                    ControlledModsMod.LogWarning($"[SaveFileFixes] Failed to patch {asm.GetName().Name}: {ex.Message}");
                }
            }
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            try
            {
                return Path.GetFullPath(new Uri(path, UriKind.RelativeOrAbsolute).LocalPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            catch
            {
                return path?.Replace('/', Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
        }

        /// <summary>Redirect write path to shared when the path is a known workshop config path.</summary>
        [HarmonyPrefix]
        public static void WriteSettings_Prefix([HarmonyArgument(1)] ref string path)
        {
            if (string.IsNullOrEmpty(path) || WorkshopToSharedPaths.Count == 0) return;
            string normalized = NormalizePath(path);
            foreach (var (workshop, shared) in WorkshopToSharedPaths)
            {
                if (string.Equals(normalized, workshop, StringComparison.OrdinalIgnoreCase))
                {
                    path = shared;
                    return;
                }
            }
        }

        private static bool IsEnabledFor(ControlledModsSaveFileFixOptions opts, string modId)
        {
            if (opts == null) return false;
            switch (modId)
            {
                case SaveFileFixPaths.ModIds.AdvancedCoolers: return opts.EnableSaveFileFix_AdvancedCoolers;
                case SaveFileFixPaths.ModIds.AiImprovements: return opts.EnableSaveFileFix_AiImprovements;
                case SaveFileFixPaths.ModIds.CustomizableSpeed: return opts.EnableSaveFileFix_CustomizableSpeed;
                case SaveFileFixPaths.ModIds.DefaultBuildingSettings: return opts.EnableSaveFileFix_DefaultBuildingSettings;
                case SaveFileFixPaths.ModIds.DragAreaMoreVisible: return opts.EnableSaveFileFix_DragAreaMoreVisible;
                case SaveFileFixPaths.ModIds.Drains: return opts.EnableSaveFileFix_Drains;
                case SaveFileFixPaths.ModIds.NotificationsPause: return opts.EnableSaveFileFix_NotificationsPause;
                case SaveFileFixPaths.ModIds.ToastControl: return opts.EnableSaveFileFix_ToastControl;
                case SaveFileFixPaths.ModIds.WallPumps: return opts.EnableSaveFileFix_WallPumps;
                default: return false;
            }
        }

        [HarmonyPostfix]
        public static void GetConfigPath_Postfix(ConfigFileAttribute attr, Assembly modAssembly, ref string __result)
        {
            if (__result == null || modAssembly == null) return;
            if (!ModAssemblyToModId.TryGetValue(modAssembly, out string modId)) return;

            string fileName = attr?.ConfigFileName ?? "config.json";
            __result = SaveFileFixPaths.GetSharedConfigPath(modId, fileName);
        }
    }
}
