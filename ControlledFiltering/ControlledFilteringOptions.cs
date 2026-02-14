using System;
using System.IO;
using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ControlledFiltering
{
	[JsonObject(MemberSerialization.OptIn)]
	[ConfigFile(POptions.CONFIG_FILE_NAME, false, true)]
	[RestartRequired]
	public sealed class ControlledFilteringOptions
	{
		private static ControlledFilteringOptions _instance;
		public static ControlledFilteringOptions Instance
		{
			get
			{
				if (_instance == null)
				{
					ConfigMigrationHelper.MigrateConfigFromFilePath(
						POptions.GetConfigFilePath(typeof(ControlledFilteringOptions)));
					_instance = SafeReadSettings() ?? new ControlledFilteringOptions();
				}
				return _instance;
			}
		}

		public static void Reload()
		{
			ConfigMigrationHelper.MigrateConfigFromFilePath(
				POptions.GetConfigFilePath(typeof(ControlledFilteringOptions)));
			_instance = SafeReadSettings() ?? new ControlledFilteringOptions();
		}

		private static ControlledFilteringOptions SafeReadSettings()
		{
			try
			{
				string canonical = ConfigMigrationHelper.GetCanonicalConfigPath(
					POptions.GetConfigFilePath(typeof(ControlledFilteringOptions)));
				if (!string.IsNullOrEmpty(canonical) && File.Exists(canonical))
				{
					string json = File.ReadAllText(canonical);
					return JsonConvert.DeserializeObject<ControlledFilteringOptions>(json);
				}
				return POptions.ReadSettings<ControlledFilteringOptions>();
			}
			catch (Exception) { return null; }
		}

		// By default, all are set to Non-Standard (true) to match vanilla behavior
		// Users can uncheck these to move categories to Standard

		[Option("Clothing is Non-Standard", "When enabled, Clothing appears in the Non-Standard section.\nWhen disabled, Clothing appears with Standard items and is included in 'Select All'.", "Category Settings")]
		[JsonProperty]
		public bool ClothingIsNonStandard { get; set; } = true;

		[Option("Critter Eggs are Non-Standard", "When enabled, Critter Eggs appear in the Non-Standard section.\nWhen disabled, Critter Eggs appear with Standard items and are included in 'Select All'.", "Category Settings")]
		[JsonProperty]
		public bool EggsAreNonStandard { get; set; } = true;

		[Option("Sublimating Items are Non-Standard", "When enabled, Sublimating items (Bleach Stone, Oxylite, etc.) appear in the Non-Standard section.\nWhen disabled, they appear with Standard items and are included in 'Select All'.", "Category Settings")]
		[JsonProperty]
		public bool SublimatingIsNonStandard { get; set; } = true;
	}
}

