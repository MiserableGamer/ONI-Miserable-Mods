using System.IO;
using KMod;

namespace ControlledMods.Patches.SaveFileFixes
{
    /// <summary>Stable identifiers and paths for Save File Fixes (shared config location).</summary>
    public static class SaveFileFixPaths
    {
        public const string ConfigFolderName = "config";

        /// <summary>Stable mod IDs used as subfolder names under the game config folder.</summary>
        public static class ModIds
        {
            public const string AdvancedCoolers = "AdvancedCoolers";
            public const string AiImprovements = "AiImprovements";
            public const string CustomizableSpeed = "CustomizableSpeed";
            public const string DefaultBuildingSettings = "DefaultBuildingSettings";
            public const string DragAreaMoreVisible = "DragAreaMoreVisible";
            public const string Drains = "Drains";
            public const string NotificationsPause = "NotificationsPause";
            public const string ToastControl = "ToastControl";
            public const string WallPumps = "WallPumps";
        }

        /// <summary>Config file names per mod (most use config.json; NotificationsPause uses settings.json).</summary>
        public static string GetConfigFileName(string modId)
        {
            return modId == ModIds.NotificationsPause ? "settings.json" : "config.json";
        }

        /// <summary>Returns the shared config file path for a mod. Does not create the directory.</summary>
        public static string GetSharedConfigPath(string modId, string fileName)
        {
            string baseDir = Manager.GetDirectory();
            string configDir = Path.Combine(baseDir, ConfigFolderName, modId);
            return Path.Combine(configDir, fileName);
        }

        /// <summary>Returns the shared config file path for a mod (uses default file name for that mod).</summary>
        public static string GetSharedConfigPath(string modId)
        {
            return GetSharedConfigPath(modId, GetConfigFileName(modId));
        }
    }
}
