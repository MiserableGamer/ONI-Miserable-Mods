using PeterHan.PLib.Options;

namespace EmptyStorage
{
	[ConfigFile(POptions.CONFIG_FILE_NAME, false, true)]
	public sealed class EmptyStorageOptions
	{
		[Option("Immediate Emptying", "If enabled, storage contents are dropped immediately. If disabled, a duplicant task is created to empty the storage. When enabled, all other options are disabled.")]
		public bool ImmediateEmptying { get; set; } = true;

		[Option("Require Skills", "If enabled, duplicants need the appropriate skill (Plumbing for gas/liquid storage, Tidy for solid storage) to empty storage. Only applies when Immediate Emptying is disabled.")]
		public bool RequireSkills { get; set; } = true;

		[Option("Use Work Time", "If enabled, emptying takes time based on the mass stored. If disabled, emptying is instant. Only applies when Immediate Emptying is disabled.")]
		public bool UseWorkTime { get; set; } = true;

		[Option("Work Time per 100kg (seconds)", "The amount of time (in seconds) it takes to empty 100kg of stored material. Range: 0.1 to 10 seconds. Only applies when Use Work Time is enabled.")]
		[Limit(0.1, 10.0)]
		public float WorkTimePer100kg { get; set; } = 1.0f;
	}
}

