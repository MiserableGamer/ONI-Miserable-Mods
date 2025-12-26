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

		[Option("Horizontal Reach (cells)", "Number of additional cells beyond vanilla reach that duplicants can reach horizontally (left/right). This allows reaching over chasms and gaps. Safe mode (below) caps this at 2 to prevent reaching through solid tiles. Default: 1. Range: 0-10.")]
		[Limit(0, 10)]
		public int HorizontalReach { get; set; } = 1;

		[Option("Safe Mode (Prevent Reach-Through-Walls)", "When enabled, horizontal reach is capped at 2 additional cells to prevent duplicants from reaching through solid tiles. When disabled, you can use higher horizontal reach values, but this may allow reaching through walls.")]
		public bool SafeMode { get; set; } = true;
}
}

