using System;
using System.IO;
using UnityEngine;

// One-time migration: renames old "{AssemblyName}.dll" folders to the correct name
// (e.g. after adding staticID to mod.yaml). Runs in both config and mods folders.
// Safe to remove once all users have migrated (a few releases after the fix).
internal static class ConfigMigrationHelper
{
    /// <summary>Runs config-folder and mods-folder migration. Call from OnLoad.</summary>
    internal static void Migrate(string oldFolderName, string newFolderName)
    {
        MigrateInDirectory(Path.Combine(KMod.Manager.GetDirectory(), "config"), oldFolderName, newFolderName, isConfig: true);
        MigrateModsFolder(oldFolderName, newFolderName);
    }

    /// <summary>Migrate in mods folder: try game directory and user Documents so local mods are fixed.</summary>
    private static void MigrateModsFolder(string oldFolderName, string newFolderName)
    {
        string gameRoot = KMod.Manager.GetDirectory();
        string[] modsRoots = new[]
        {
            Path.Combine(gameRoot, "mods", "local"),
            Path.Combine(gameRoot, "mods"),
            gameRoot,
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Klei", "OxygenNotIncluded", "mods", "local")
        };
        foreach (string root in modsRoots)
        {
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
                continue;
            if (MigrateInDirectory(root, oldFolderName, newFolderName, isConfig: false))
                break; // only migrate in the first existing location that had the old folder
        }
    }

    /// <returns>True if the old folder existed and was handled (renamed or merged and removed).</returns>
    private static bool MigrateInDirectory(string parentDir, string oldFolderName, string newFolderName, bool isConfig)
    {
        try
        {
            if (!Directory.Exists(parentDir))
                return false;

            string oldDir = Path.Combine(parentDir, oldFolderName);
            string newDir = Path.Combine(parentDir, newFolderName);
            bool oldExists = Directory.Exists(oldDir);
            bool newExists = Directory.Exists(newDir);

            if (!oldExists)
                return false;

            string label = isConfig ? "config" : "mods";

            if (!newExists)
            {
                Directory.Move(oldDir, newDir);
                Debug.Log($"[ConfigMigration] Renamed {label} folder '{oldFolderName}' -> '{newFolderName}' at {parentDir}");
                return true;
            }

            // Both exist — for config, merge config.json; for mods, copy any files from old into new then remove old
            if (isConfig)
            {
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
            }
            else
            {
                foreach (string file in Directory.GetFiles(oldDir))
                {
                    string fileName = Path.GetFileName(file);
                    string dest = Path.Combine(newDir, fileName);
                    if (!File.Exists(dest) || File.GetLastWriteTimeUtc(file) > File.GetLastWriteTimeUtc(dest))
                        File.Copy(file, dest, true);
                }
                foreach (string subDir in Directory.GetDirectories(oldDir))
                {
                    string subName = Path.GetFileName(subDir);
                    string destSub = Path.Combine(newDir, subName);
                    if (!Directory.Exists(destSub))
                        CopyDirectory(subDir, destSub);
                }
            }

            try
            {
                Directory.Delete(oldDir, true);
                Debug.Log($"[ConfigMigration] Removed old {label} folder '{oldFolderName}'");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ConfigMigration] Could not remove old folder '{oldFolderName}': {ex.Message}");
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ConfigMigration] Migration failed in {parentDir}: {ex.Message}");
            return false;
        }
    }

    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);
        foreach (string file in Directory.GetFiles(sourceDir))
            File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)));
        foreach (string subDir in Directory.GetDirectories(sourceDir))
            CopyDirectory(subDir, Path.Combine(targetDir, Path.GetFileName(subDir)));
    }
}

