using HarmonyLib;
using STRINGS;
using ControlledSublimation.Buildings;

namespace ControlledSublimation.Patches
{
	public static class BleachStoneSconceRegistration
	{
		private static bool _planScreenAdded;

		private static void RegStrings()
		{
			string id = BleachStoneSconceConfig.ID;
			string idUpper = id.ToUpperInvariant();
			Strings.Add(new[] { "STRINGS.BUILDINGS.PREFABS." + idUpper + ".NAME", "Bleach Stone Sconce" });
			Strings.Add(new[] { "STRINGS.BUILDINGS.PREFABS." + idUpper + ".DESC", "Stores Bleach Stone so it gradually releases Chlorine into the environment. Placed like the Oxylite Sconce but for chlorine." });
			Strings.Add(new[] { "STRINGS.BUILDINGS.PREFABS." + idUpper + ".EFFECT", "Stores up to 240 kg Bleach Stone. The stored Bleach Stone sublimates into Chlorine. Supply errand when low." });
		}

		[HarmonyPatch(typeof(GeneratedBuildings))]
		[HarmonyPatch(nameof(GeneratedBuildings.LoadGeneratedBuildings))]
		public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
		{
			public static void Prefix()
			{
				RegStrings();
				if (!_planScreenAdded)
				{
					ModUtil.AddBuildingToPlanScreen("Oxygen", BleachStoneSconceConfig.ID);
					_planScreenAdded = true;
				}
			}
		}

		[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
		public static class Db_Initialize_Patch
		{
			public static void Postfix()
			{
				// PortableGases = Oxylite Sconce tech (may be DLC). Fallback to vanilla gas tech if missing.
				var tech = Db.Get().Techs.TryGet("PortableGases") ?? Db.Get().Techs.TryGet("ImprovedGasPiping");
				if (tech != null && !tech.unlockedItemIDs.Contains(BleachStoneSconceConfig.ID))
					tech.unlockedItemIDs.Add(BleachStoneSconceConfig.ID);
			}
		}
	}
}
