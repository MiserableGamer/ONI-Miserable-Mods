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
			ConfigMigrationHelper.Migrate("EmptyStorage.dll", "EmptyStorage");

			// Register options with shared config location
			var options = new POptions();
			options.RegisterOptions(this, typeof(EmptyStorageOptions));

			// Apply Harmony patches
			harmony.PatchAll();
		}
	}
}

