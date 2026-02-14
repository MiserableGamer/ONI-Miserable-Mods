using System;
using System.IO;
using UnityEngine;

// One-time migration: renames old "{ModName}.dll" config/mod folders to the correct name.
// Also runs MigrateConfigFromFilePath so we merge even when config is under Documents.
// Safe to remove once all users have migrated (a few releases after the fix).
internal static class ConfigMigrationHelper
{
    internal const string OldConfigFolderName = "AdvancedWattageSensor.dll";
    internal const string NewConfigFolderName = "AdvancedWattageSensor";
    internal const string ConfigFileName = "config.json";

    internal static string GetCanonicalConfigPath(string configFilePath)
    {
        if (string.IsNullOrEmpty(configFilePath))
            return configFilePath;
        return configFilePath.Replace(OldConfigFolderName, NewConfigFolderName);
    }

    internal static void MigrateConfigFromFilePath(string configFilePath)
    {
        if (string.IsNullOrEmpty(configFilePath))
            return;
        string configFolder = Path.GetDirectoryName(configFilePath);
        if (string.IsNullOrEmpty(configFolder))
            return;
        string parentDir = Path.GetDirectoryName(configFolder);
        if (string.IsNullOrEmpty(parentDir))
            return;
        MigrateInDirectory(parentDir, OldConfigFolderName, NewConfigFolderName, isConfig: true);
    }

    internal static void Migrate(string oldFolderName, string newFolderName)
    {
        MigrateInDirectory(Path.Combine(KMod.Manager.GetDirectory(), "config"), oldFolderName, newFolderName, isConfig: true);
        MigrateModsFolder(oldFolderName, newFolderName);
    }

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
                break;
        }
    }

    private static bool MigrateInDirectory(string parentDir, string oldFolderName, string newFolderName, bool isConfig)
    {
        try
        {
            if (!Directory.Exists(parentDir))
                return false;
            string oldDir = Path.Combine(parentDir, oldFolderName);
            string newDir = Path.Combine(parentDir, newFolderName);
            if (!Directory.Exists(oldDir))
                return false;
            string label = isConfig ? "config" : "mods";
            if (!Directory.Exists(newDir))
            {
                Directory.Move(oldDir, newDir);
                Debug.Log($"[ConfigMigration] Renamed {label} folder '{oldFolderName}' -> '{newFolderName}' at {parentDir}");
                return true;
            }
            if (isConfig)
            {
                string oldConfig = Path.Combine(oldDir, ConfigFileName);
                string newConfig = Path.Combine(newDir, ConfigFileName);
                if (File.Exists(oldConfig))
                {
                    if (!File.Exists(newConfig) || File.GetLastWriteTimeUtc(oldConfig) > File.GetLastWriteTimeUtc(newConfig))
                    {
                        if (!Directory.Exists(newDir)) Directory.CreateDirectory(newDir);
                        File.Copy(oldConfig, newConfig, true);
                        Debug.Log($"[ConfigMigration] Copied config from '{oldFolderName}' -> '{newFolderName}'");
                    }
                }
            }
            else
            {
                foreach (string file in Directory.GetFiles(oldDir))
                {
                    string dest = Path.Combine(newDir, Path.GetFileName(file));
                    if (!File.Exists(dest) || File.GetLastWriteTimeUtc(file) > File.GetLastWriteTimeUtc(dest))
                        File.Copy(file, dest, true);
                }
                foreach (string subDir in Directory.GetDirectories(oldDir))
                {
                    string destSub = Path.Combine(newDir, Path.GetFileName(subDir));
                    if (!Directory.Exists(destSub)) CopyDirectory(subDir, destSub);
                }
            }
            try { Directory.Delete(oldDir, true); Debug.Log($"[ConfigMigration] Removed old {label} folder '{oldFolderName}'"); }
            catch (Exception ex) { Debug.LogWarning($"[ConfigMigration] Could not remove old folder '{oldFolderName}': {ex.Message}"); }
            return true;
        }
        catch (Exception ex) { Debug.LogWarning($"[ConfigMigration] Migration failed in {parentDir}: {ex.Message}"); return false; }
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
