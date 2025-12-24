using PeterHan.PLib.Options;

namespace LongerArms
{
	[RestartRequired]
	[ConfigFile(POptions.CONFIG_FILE_NAME, false, true)]
	public sealed class LongerArmsOptions
	{
		[Option("Vertical Reach (cells)", "Number of additional cells beyond vanilla reach that duplicants can reach vertically (up/down). Vanilla allows 4 cells up, so setting this to 1 allows reaching cell 5 (ceiling in 4-cell high rooms). Default: 1. Range: 0-10.")]
		[Limit(0, 10)]
		public int VerticalReach { get; set; } = 1;

		[Option("Horizontal Reach (cells)", "Number of additional cells beyond vanilla reach that duplicants can reach horizontally (left/right). This allows reaching over chasms and gaps. Default: 1. Range: 0-10.")]
		[Limit(0, 10)]
		public int HorizontalReach { get; set; } = 1;
	}
}

