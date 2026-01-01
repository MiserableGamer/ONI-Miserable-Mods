using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace ControlledSublimation
{
	public sealed class ControlledSublimationMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			PUtil.InitLibrary();

			// Register options with shared config location
			var options = new POptions();
			options.RegisterOptions(this, typeof(ControlledSublimationOptions));

			harmony.PatchAll();
		}
	}
}

