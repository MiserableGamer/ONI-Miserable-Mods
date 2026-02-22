using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using ControlledMods.ModDetection;
using ControlledMods.Options;

namespace ControlledMods.Patches.SaveFileFixes
{
    /// <summary>Redirects config file path for custom mods (DragAreaMoreVisible, NotificationsPause) by patching File/StreamReader so they use the shared path.</summary>
    public static class CustomModPathPatches
    {
        private static readonly List<(string normalizedSource, string target)> Redirects = new List<(string, string)>();

        public static void Apply(Harmony harmony)
        {
            if (harmony == null) return;

            Redirects.Clear();
            var opts = PeterHan.PLib.Options.POptions.ReadSettings<ControlledModsSaveFileFixOptions>() ?? new ControlledModsSaveFileFixOptions();

            AddRedirectIfEnabled(SaveFileFixPaths.ModIds.DragAreaMoreVisible, opts.EnableSaveFileFix_DragAreaMoreVisible, SaveFileFixPaths.GetConfigFileName(SaveFileFixPaths.ModIds.DragAreaMoreVisible));
            AddRedirectIfEnabled(SaveFileFixPaths.ModIds.NotificationsPause, opts.EnableSaveFileFix_NotificationsPause, SaveFileFixPaths.GetConfigFileName(SaveFileFixPaths.ModIds.NotificationsPause));

            if (Redirects.Count == 0) return;

            try
            {
                MethodInfo fileOpenText = typeof(File).GetMethod(nameof(File.OpenText), new[] { typeof(string) });
                if (fileOpenText != null)
                    harmony.Patch(fileOpenText, prefix: new HarmonyMethod(typeof(CustomModPathPatches), nameof(FileOpenText_Prefix)));

                MethodInfo fileCreateText = typeof(File).GetMethod(nameof(File.CreateText), new[] { typeof(string) });
                if (fileCreateText != null)
                    harmony.Patch(fileCreateText, prefix: new HarmonyMethod(typeof(CustomModPathPatches), nameof(FileCreateText_Prefix)));

                ConstructorInfo streamReaderCtor = typeof(StreamReader).GetConstructor(new[] { typeof(string) });
                if (streamReaderCtor != null)
                    harmony.Patch(streamReaderCtor, new HarmonyMethod(typeof(CustomModPathPatches), nameof(StreamReaderCtor_Prefix)));

                ControlledModsMod.Log($"[SaveFileFixes] Custom mod path redirects applied: {Redirects.Count}");
            }
            catch (Exception ex)
            {
                ControlledModsMod.LogWarning($"[SaveFileFixes] Custom mod patches failed: {ex.Message}");
            }
        }

        private static void AddRedirectIfEnabled(string modId, bool enabled, string fileName)
        {
            if (!enabled || !SaveFileFixModDetection.IsLoaded(modId)) return;
            string modDir = SaveFileFixModDetection.GetModDirectory(modId);
            if (string.IsNullOrEmpty(modDir)) return;

            string workshopPath = Path.Combine(modDir, fileName);
            string sharedPath = SaveFileFixPaths.GetSharedConfigPath(modId, fileName);
            string normalized = NormalizePath(workshopPath);
            if (!string.IsNullOrEmpty(normalized))
                Redirects.Add((normalized, sharedPath));

            SaveFileFixMigration.TryMigrateToShared(workshopPath, sharedPath);
        }

        private static string NormalizePath(string path)
        {
            try
            {
                return Path.GetFullPath(new Uri(path, UriKind.RelativeOrAbsolute).LocalPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            catch
            {
                return path?.Replace('/', Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
        }

        private static bool TryRedirect(ref string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            string normalized = NormalizePath(path);
            foreach (var (source, target) in Redirects)
            {
                if (string.Equals(normalized, source, StringComparison.OrdinalIgnoreCase))
                {
                    path = target;
                    return true;
                }
            }
            return false;
        }

        public static void FileOpenText_Prefix(ref string path)
        {
            TryRedirect(ref path);
        }

        public static void FileCreateText_Prefix(ref string path)
        {
            TryRedirect(ref path);
        }

        public static void StreamReaderCtor_Prefix(ref string path)
        {
            TryRedirect(ref path);
        }
    }
}
