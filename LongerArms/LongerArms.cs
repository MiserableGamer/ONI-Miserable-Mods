using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace LongerArms
{
	public sealed class LongerArms : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			
			// Initialize PLib
			PUtil.InitLibrary();
			
			// Register options
			var options = new POptions();
			options.RegisterOptions(this, typeof(LongerArmsOptions));
			
			// Apply Harmony patches
			harmony.PatchAll();
		}
	}
}

