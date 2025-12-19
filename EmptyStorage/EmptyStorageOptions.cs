using PeterHan.PLib.Options;

namespace EmptyStorage
{
	[ConfigFile(POptions.CONFIG_FILE_NAME, false, true)]
	public sealed class EmptyStorageOptions
	{
		[Option("Immediate Emptying", "If enabled, storage contents are dropped immediately. If disabled, a duplicant task is created to empty the storage.")]
		public bool ImmediateEmptying { get; set; } = true;
	}
}

