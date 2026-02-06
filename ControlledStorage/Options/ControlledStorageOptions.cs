using System;
using System.IO;
using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ControlledStorage
{
    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile(POptions.CONFIG_FILE_NAME, false, true)]
    [RestartRequired]
    public sealed class ControlledStorageOptions
    {
        private static ControlledStorageOptions _instance;
        public static ControlledStorageOptions Instance
        {
            get
            {
                if (_instance == null)
                    _instance = SafeReadSettings() ?? new ControlledStorageOptions();
                return _instance;
            }
        }
        
        // Reload settings (useful when options change)
        public static void Reload()
        {
            _instance = SafeReadSettings() ?? new ControlledStorageOptions();
        }

        // PLib ReadSettings throws when config path doesn't exist (first run) - catch and use defaults
        private static ControlledStorageOptions SafeReadSettings()
        {
            try
            {
                return POptions.ReadSettings<ControlledStorageOptions>();
            }
            catch (DirectoryNotFoundException) { return null; }
            catch (FileNotFoundException) { return null; }
        }

        #region Empty Storage Options
        
        [Option("Immediate Emptying", 
            "If enabled, storage contents are dropped immediately. If disabled, a duplicant task is created.", 
            "Empty Storage")]
        [JsonProperty]
        public bool ImmediateEmptying { get; set; } = true;

        [Option("Require Skills", 
            "Duplicants need the Tidy skill to empty solid storage. Only applies when Immediate Emptying is disabled.", 
            "Empty Storage")]
        [JsonProperty]
        public bool RequireSkills { get; set; } = true;

        [Option("Use Work Time", 
            "Emptying takes time based on mass stored. Only applies when Immediate Emptying is disabled.", 
            "Empty Storage")]
        [JsonProperty]
        public bool UseWorkTime { get; set; } = true;

        [Option("Work Time per 100kg (seconds)", 
            "Time to empty 100kg of material. Range: 0.1 to 10 seconds.", 
            "Empty Storage")]
        [Limit(0.1, 10.0)]
        [JsonProperty]
        public float WorkTimePer100kg { get; set; } = 1.0f;

        #endregion

        #region Filtering Options
        
        [Option("Clothing is Non-Standard", 
            "When enabled, Clothing appears in the Non-Standard section of storage filters.", 
            "Storage Filtering")]
        [JsonProperty]
        public bool ClothingIsNonStandard { get; set; } = true;

        [Option("Critter Eggs are Non-Standard", 
            "When enabled, Critter Eggs appear in the Non-Standard section of storage filters.", 
            "Storage Filtering")]
        [JsonProperty]
        public bool EggsAreNonStandard { get; set; } = true;

        [Option("Sublimating Items are Non-Standard", 
            "When enabled, Sublimating items (Bleach Stone, Oxylite) appear in Non-Standard section.", 
            "Storage Filtering")]
        [JsonProperty]
        public bool SublimatingIsNonStandard { get; set; } = true;

        #endregion

        #region Capacity Control Options
        
        [Option("Additional Input Characters", 
            "Extra characters beyond vanilla 6-character limit for capacity input. 2 = 8 total (up to 9,999,999 kg).", 
            "Capacity Control")]
        [Limit(1, 10)]
        [JsonProperty]
        public int AdditionalCharacters { get; set; } = 2;

        // Computed property
        public int TotalCharacterLimit => 6 + AdditionalCharacters;

        #endregion

        #region Delivery Control Options (Phase 3)
        
        [Option("Enable for Storage Bins", 
            "Add delivery control sidescreen to storage bins and tiles (StorageLocker, StorageLockerSmart).", 
            "Delivery Control")]
        [JsonProperty]
        public bool EnableDeliveryControlStorage { get; set; } = true;

        [Option("Enable for Fridges", 
            "Add delivery control sidescreen to refrigerators and ration boxes (Refrigerator, RationBox).", 
            "Delivery Control")]
        [JsonProperty]
        public bool EnableDeliveryControlFridges { get; set; } = true;

        public bool EnableDeliveryControl => EnableDeliveryControlStorage || EnableDeliveryControlFridges;

        [Option("Delivery Control Debug Logs",
            "Log sweeper loop and fetch target details to Player.log. Set to true to diagnose the same-bin loop issue.",
            "Delivery Control")]
        [JsonProperty]
        public bool EnableDeliveryControlDebugLogs { get; set; } = false;

        #endregion

        #region No-Sweep Zone Options (Phase 4)
        
        [Option("Enable No-Sweep Zones", 
            "Adds a tool to mark zones where dupes will not sweep items from.", 
            "No-Sweep Zones")]
        [JsonProperty]
        public bool EnableNoSweepZones { get; set; } = true;

        #endregion
    }
}
