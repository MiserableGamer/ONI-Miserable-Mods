using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ControlledExtraction.Options
{
    // Data class for which solid elements the Ice Kettle can melt.
    // Vanilla/DLC elements have per-element toggles (keyed by SimHashes name).
    // Modded elements (from other mods like Ronivan's Legacy) have a single toggle.
    //
    // Saved to its own file (not inside ControlledExtractionOptions) because
    // PLib's options dialog overwrites the main config when it closes, which
    // would clobber changes made by the meltable sub-dialog.
    [Serializable]
    public class IceKettleMeltableOptions
    {
        private const string FILENAME = "ControlledExtraction_Meltables.json";

        private static IceKettleMeltableOptions _cached;

        // Per-element toggles for vanilla/DLC elements.
        // Key = SimHashes enum name (e.g. "Ice", "DirtyIce", "SolidCarbonDioxide").
        // Elements not in the dictionary default to false, except "Ice" which defaults to true.
        [JsonProperty]
        public Dictionary<string, bool> ElementToggles { get; set; } = new Dictionary<string, bool>
        {
            { "Ice", true }
        };

        [JsonProperty]
        public bool EnableModdedMeltables { get; set; } = false;

        public bool IsElementEnabled(string key)
        {
            if (ElementToggles != null && ElementToggles.TryGetValue(key, out bool val))
                return val;
            // Only Ice is enabled by default when not explicitly set
            return key == "Ice";
        }

        public void SetElementEnabled(string key, bool enabled)
        {
            if (ElementToggles == null)
                ElementToggles = new Dictionary<string, bool>();
            ElementToggles[key] = enabled;
        }

        // Derive path from PLib's config location so both files live side by side. Use canonical path (non-.dll folder).
        private static string GetFilePath()
        {
            string mainConfigPath = ConfigMigrationHelper.GetCanonicalConfigPath(POptions.GetConfigFilePath(typeof(ControlledExtractionOptions)));
            string configDir = Path.GetDirectoryName(mainConfigPath);
            return Path.Combine(configDir, FILENAME);
        }

        public static IceKettleMeltableOptions Load()
        {
            if (_cached != null)
                return _cached;

            string path = GetFilePath();
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var result = JsonConvert.DeserializeObject<IceKettleMeltableOptions>(json);
                    if (result != null)
                    {
                        _cached = result;
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ControlledExtraction] Failed to load meltable options: " + e.Message);
            }

            _cached = new IceKettleMeltableOptions();
            return _cached;
        }

        public void Save()
        {
            string path = GetFilePath();
            try
            {
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(path, json);
                _cached = this;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ControlledExtraction] Failed to save meltable options: " + e.Message);
            }
        }

        // Force re-read from disk on next access
        public static void InvalidateCache()
        {
            _cached = null;
        }
    }
}
