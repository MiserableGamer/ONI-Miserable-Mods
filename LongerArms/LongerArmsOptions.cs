using System;
using System.IO;
using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace LongerArms
{
	[RestartRequired]
	[ConfigFile(POptions.CONFIG_FILE_NAME, false, true)]
	public sealed class LongerArmsOptions
	{
		private static LongerArmsOptions _instance;
		public static LongerArmsOptions Instance
		{
			get
			{
				if (_instance == null)
				{
					try
					{
						string plibPath = POptions.GetConfigFilePath(typeof(LongerArmsOptions));
						ConfigMigrationHelper.MigrateConfigFromFilePath(plibPath);
					}
					catch { /* ignore */ }
					_instance = SafeReadSettings() ?? new LongerArmsOptions();
				}
				return _instance;
			}
		}

		public static void Reload()
		{
			try
			{
				string plibPath = POptions.GetConfigFilePath(typeof(LongerArmsOptions));
				ConfigMigrationHelper.MigrateConfigFromFilePath(plibPath);
			}
			catch { /* ignore */ }
			_instance = SafeReadSettings() ?? new LongerArmsOptions();
		}

		// Read from canonical path first (non-.dll folder), fall back to PLib default
		private static LongerArmsOptions SafeReadSettings()
		{
			try
			{
				string path = ConfigMigrationHelper.GetCanonicalConfigPath(POptions.GetConfigFilePath(typeof(LongerArmsOptions)));
				if (!string.IsNullOrEmpty(path) && File.Exists(path))
				{
					string json = File.ReadAllText(path);
					if (!string.IsNullOrEmpty(json))
						return JsonConvert.DeserializeObject<LongerArmsOptions>(json);
				}
			}
			catch { /* fall through */ }

			try
			{
				return POptions.ReadSettings<LongerArmsOptions>();
			}
			catch { /* ignore */ }
			return null;
		}

		[Option("Vertical Reach (cells)", "Number of additional cells beyond vanilla reach that duplicants can reach vertically (up/down). Vanilla allows 4 cells up, so setting this to 1 allows reaching cell 5 (ceiling in 4-cell high rooms). Default: 1. Range: 0-10.")]
		[Limit(0, 10)]
		public int VerticalReach { get; set; } = 1;

		[Option("Horizontal Reach (cells)", "Number of additional cells beyond vanilla reach that duplicants can reach horizontally (left/right). This allows reaching over chasms and gaps. Safe mode (below) caps this at 2 to prevent reaching through solid tiles. Default: 1. Range: 0-10.")]
		[Limit(0, 10)]
		public int HorizontalReach { get; set; } = 1;

		[Option("Safe Mode (Prevent Reach-Through-Walls)", "When enabled, horizontal reach is capped at 2 additional cells to prevent duplicants from reaching through solid tiles. When disabled, you can use higher horizontal reach values, but this may allow reaching through walls.")]
		public bool SafeMode { get; set; } = true;
	}
}
