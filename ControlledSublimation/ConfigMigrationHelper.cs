using System;
using System.IO;
using UnityEngine;

// One-time config folder migration: renames old "{AssemblyName}.dll" folders
// to the correct name after adding staticID to mod.yaml.
// Safe to remove once all users have migrated (a few releases after the fix).
internal static class ConfigMigrationHelper
{
    internal static void Migrate(string oldFolderName, string newFolderName)
    {
        try
        {
            string configRoot = Path.Combine(KMod.Manager.GetDirectory(), "config");
            if (!Directory.Exists(configRoot))
                return;

            string oldDir = Path.Combine(configRoot, oldFolderName);
            string newDir = Path.Combine(configRoot, newFolderName);
            bool oldExists = Directory.Exists(oldDir);
            bool newExists = Directory.Exists(newDir);

            if (!oldExists)
                return;

            if (!newExists)
            {
                // Old exists, new doesn't — just rename
                Directory.Move(oldDir, newDir);
                Debug.Log($"[ConfigMigration] Renamed config folder '{oldFolderName}' -> '{newFolderName}'");
                return;
            }

            // Both exist — keep whichever config.json is newer
            string oldConfig = Path.Combine(oldDir, "config.json");
            string newConfig = Path.Combine(newDir, "config.json");

            if (File.Exists(oldConfig))
            {
                if (!File.Exists(newConfig) ||
                    File.GetLastWriteTimeUtc(oldConfig) > File.GetLastWriteTimeUtc(newConfig))
                {
                    File.Copy(oldConfig, newConfig, true);
                    Debug.Log($"[ConfigMigration] Copied newer config from '{oldFolderName}' -> '{newFolderName}'");
                }
            }

            // Clean up old folder
            try
            {
                Directory.Delete(oldDir, true);
                Debug.Log($"[ConfigMigration] Removed old config folder '{oldFolderName}'");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ConfigMigration] Could not remove old folder '{oldFolderName}': {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ConfigMigration] Migration failed: {ex.Message}");
        }
    }
}

