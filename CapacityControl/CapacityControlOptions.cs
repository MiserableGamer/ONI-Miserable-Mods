using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace CapacityControl
{
	[JsonObject(MemberSerialization.OptIn)]
	[ConfigFile(POptions.CONFIG_FILE_NAME, false, true)]
	[RestartRequired]
	public sealed class CapacityControlOptions
	{
		// Singleton instance for easy access
		private static CapacityControlOptions _instance;
		public static CapacityControlOptions Instance
		{
			get
			{
				if (_instance == null)
					_instance = POptions.ReadSettings<CapacityControlOptions>() ?? new CapacityControlOptions();
				return _instance;
			}
		}

		[Option("Additional Characters", "Number of additional characters to add beyond the vanilla 6 character limit. For example, 2 gives you 8 characters total (up to 9,999,999 kg).")]
		[Limit(1, 10)]
		[JsonProperty]
		public int AdditionalCharacters { get; set; } = 2;

		// Computed property for total character limit
		public int TotalCharacterLimit => 6 + AdditionalCharacters;

		public CapacityControlOptions()
		{
			// Default values are set above
		}
	}
}

