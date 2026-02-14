using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace ControlledFiltering
{
	public sealed class ControlledFilteringMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			PUtil.InitLibrary();

			var options = new POptions();
			options.RegisterOptions(this, typeof(ControlledFilteringOptions));

			harmony.PatchAll();
		}
	}
}

