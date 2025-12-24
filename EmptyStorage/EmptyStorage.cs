using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace EmptyStorage
{
	public sealed class EmptyStorage : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);

			// Initialize PLib
			PUtil.InitLibrary();

			// Register options with shared config location
			// The [ConfigFile] attribute should use shared config location automatically
			var options = new POptions();
			options.RegisterOptions(this, typeof(EmptyStorageOptions));

			// Apply Harmony patches
			harmony.PatchAll();
		}
	}
}

