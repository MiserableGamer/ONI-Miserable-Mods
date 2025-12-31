using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace CapacityControl
{
	public sealed class CapacityControlMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			PUtil.InitLibrary();

			// Register options with shared config location
			var options = new POptions();
			options.RegisterOptions(this, typeof(CapacityControlOptions));

			harmony.PatchAll();
		}
	}
}
