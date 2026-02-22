using System;
using System.IO;
using UnityEngine;

namespace ControlledMods.Patches.SaveFileFixes
{
    /// <summary>Migration for Save File Fixes: copy config from workshop folder to shared only when file exists in workshop only. If in both, do nothing.</summary>
    public static class SaveFileFixMigration
    {
        /// <summary>
        /// If the config file exists only in the workshop folder, copy it to the shared path.
        /// If it exists in both locations, do nothing (assume already migrated).
        /// If it exists only in shared, do nothing.
        /// </summary>
        /// <param name="workshopPath">Full path to the config file in the mod's folder (e.g. workshop).</param>
        /// <param name="sharedPath">Full path to the config file in the shared config location.</param>
        /// <returns>True if a copy was performed (workshop only → shared).</returns>
        public static bool TryMigrateToShared(string workshopPath, string sharedPath)
        {
            if (string.IsNullOrEmpty(workshopPath) || string.IsNullOrEmpty(sharedPath))
                return false;

            bool inWorkshop = File.Exists(workshopPath);
            bool inShared = File.Exists(sharedPath);

            if (inWorkshop && inShared)
                return false; // already migrated, do nothing

            if (!inWorkshop)
                return false; // nothing to migrate

            try
            {
                string sharedDir = Path.GetDirectoryName(sharedPath);
                if (!string.IsNullOrEmpty(sharedDir) && !Directory.Exists(sharedDir))
                    Directory.CreateDirectory(sharedDir);
                File.Copy(workshopPath, sharedPath, overwrite: false);
                ControlledModsMod.Log($"[SaveFileFixes] Migrated config to shared: {sharedPath}");
                return true;
            }
            catch (Exception ex)
            {
                ControlledModsMod.LogWarning($"[SaveFileFixes] Migration failed: {ex.Message}");
                return false;
            }
        }
    }
}
