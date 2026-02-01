using HarmonyLib;
using TUNING;

namespace ControlledStorage.Patches
{
    /// <summary>
    /// Modifies SPECIAL_STORAGE based on user options.
    /// Runs early during game initialization before storage screens are created.
    /// </summary>
    [HarmonyPatch(typeof(Game), nameof(Game.OnPrefabInit))]
    public static class Game_OnPrefabInit_Patch
    {
        public static void Postfix()
        {
            var options = ControlledStorageOptions.Instance;

            // Remove categories from SPECIAL_STORAGE based on user preferences.
            // When removed from SPECIAL_STORAGE, they appear as standard items
            // and are included in "Select All".

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
