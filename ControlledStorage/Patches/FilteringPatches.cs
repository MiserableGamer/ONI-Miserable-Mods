using HarmonyLib;
using TUNING;

namespace ControlledStorage.Patches
{
    // SPECIAL_STORAGE must be modified before storage screens are created.
    [HarmonyPatch(typeof(Game), nameof(Game.OnPrefabInit))]
    public static class Game_OnPrefabInit_Patch
    {
        public static void Postfix()
        {
            var options = ControlledStorageOptions.Instance;

            // When removed from SPECIAL_STORAGE, they appear as standard and are included in "Select All".
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
