using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using ControlledFramerate.Options;
using ControlledFramerate.Strings;

namespace ControlledFramerate
{
    public sealed class ControlledFramerateMod : UserMod2
    {
        public static ControlledFramerateMod Instance { get; private set; }

        public override void OnLoad(Harmony harmony)
        {
            Instance = this;
            base.OnLoad(harmony);

            PUtil.InitLibrary();
            InitLogFile();

            try
            {
                ConfigMigrationHelper.Migrate(ConfigMigrationHelper.OldConfigFolderName, ConfigMigrationHelper.NewConfigFolderName);
            }
            catch (Exception ex)
            {
                Log($"Config migration failed (non-fatal): {ex}");
            }

            try
            {
                var options = new POptions();
                options.RegisterOptions(this, typeof(ControlledFramerateOptions));
            }
            catch (Exception ex)
            {
                Log($"POptions registration failed (non-fatal): {ex}");
            }

            try
            {
                DetectSpeedMods();
            }
            catch (Exception ex)
            {
                Log($"Speed mod detection failed (non-fatal): {ex}");
            }

            harmony.PatchAll();
        }

        private void DetectSpeedMods()
        {
            string[] knownSpeedMods = new[]
            {
                "CustomizableSpeed.CustomizableSpeed",
                "CustomizableSpeed.SpeedControlPatchOnChanged"
            };

            foreach (string typeName in knownSpeedMods)
            {
                var type = AccessTools.TypeByName(typeName);
                if (type != null)
                {
                    Log(string.Format(ControlledFramerateStrings.SpeedModDetected, type.Assembly.GetName().Name));
                    break;
                }
            }
        }

        private const string LogFileName = "ControlledFramerate.log";
        private static string logFilePath;

        private static void InitLogFile()
        {
            try
            {
                string configPath = POptions.GetConfigFilePath(typeof(ControlledFramerateOptions));
                string configDir = Path.GetDirectoryName(
                    ConfigMigrationHelper.GetCanonicalConfigPath(configPath));

                if (!string.IsNullOrEmpty(configDir))
                {
                    if (!System.IO.Directory.Exists(configDir))
                        System.IO.Directory.CreateDirectory(configDir);

                    logFilePath = Path.Combine(configDir, LogFileName);

                    if (File.Exists(logFilePath))
                        File.Delete(logFilePath);

                    PUtil.LogDebug($"{ControlledFramerateStrings.ModPrefix}Logging to {logFilePath}");
                }
            }
            catch
            {
                logFilePath = null;
            }
        }

        public static void Log(string message)
        {
            if (!string.IsNullOrEmpty(logFilePath))
            {
                try
                {
                    File.AppendAllText(logFilePath,
                        $"[{System.DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
                }
                catch { }
            }
            else
            {
                PUtil.LogDebug(ControlledFramerateStrings.ModPrefix + message);
            }
        }

        private static readonly HashSet<string> loggedOnceKeys = new HashSet<string>();

        public static void LogOnce(string key, string message)
        {
            if (loggedOnceKeys.Contains(key)) return;
            loggedOnceKeys.Add(key);
            Log(message);
        }
    }
}
