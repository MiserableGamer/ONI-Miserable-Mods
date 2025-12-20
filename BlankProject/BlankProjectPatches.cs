using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace BlankProject
{
	public class BlankProjectPatches : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);

			// Initialize PLib
			PUtil.InitLibrary();

			// Register options with shared config location
			var options = new POptions();
			options.RegisterOptions(this, typeof(BlankProjectOptions));

			// Apply Harmony patches
			harmony.PatchAll();
		}

		// Add your Harmony patches here
		// Example:
		// [HarmonyPatch(typeof(SomeClass), "SomeMethod")]
		// public static class SomeClass_SomeMethod_Patch
		// {
		//     internal static void Postfix(SomeClass __instance)
		//     {
		//         // Your patch code here
		//     }
		// }
	}
}

