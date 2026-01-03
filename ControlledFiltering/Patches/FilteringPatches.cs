using HarmonyLib;
using TUNING;

namespace ControlledFiltering.Patches
{
	// Modify SPECIAL_STORAGE based on user options
	// This runs early during game initialization before storage screens are created
	[HarmonyPatch(typeof(Game), "OnPrefabInit")]
	public static class Game_OnPrefabInit_Patch
	{
		public static void Postfix()
		{
			var options = ControlledFilteringOptions.Instance;

			// Remove categories from SPECIAL_STORAGE based on user preferences
			// If the option is false (not non-standard), remove it from the special list
			// This makes it appear as a standard category

			if (!options.ClothingIsNonStandard)
			{
				STORAGEFILTERS.SPECIAL_STORAGE.Remove(GameTags.Clothes);
			}

			if (!options.EggsAreNonStandard)
			{
				STORAGEFILTERS.SPECIAL_STORAGE.Remove(GameTags.Egg);
			}

			if (!options.SublimatingIsNonStandard)
			{
				STORAGEFILTERS.SPECIAL_STORAGE.Remove(GameTags.Sublimating);
			}
		}
	}
}

