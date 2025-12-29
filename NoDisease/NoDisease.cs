using HarmonyLib;
using KMod;
using NoDisease.Patches;
using PeterHan.PLib.Core;

namespace NoDisease
{
	public sealed class NoDiseaseMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);

			// Initialize PLib
			PUtil.InitLibrary();

			// Apply the disease-disabling patches manually
			// These are applied manually rather than with attributes because they need
			// to be applied to many methods with the same prefix/postfix
			NoDiseasePatches.Apply(harmony);

			// Apply any additional attribute-based patches
			harmony.PatchAll();
		}
	}
}

