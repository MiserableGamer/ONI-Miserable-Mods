using HarmonyLib;

namespace ControlledMorale
{
    // Prefix: eject mismatched output before RefreshQueue (pairs with hold-output patch).
    [HarmonyPatch(typeof(ComplexFabricator), nameof(ComplexFabricator.SetRecipeQueueCount))]
    public static class ComplexFabricator_SetRecipeQueueCount_AlcoholBreweryEjectIncompatibleOutput
    {
        public static void Prefix(ComplexFabricator __instance, ComplexRecipe recipe, int count)
        {
            if (!(__instance is AlcoholBrewery ab))
                return;
            if (count <= 0 && count != ComplexFabricator.QUEUE_INFINITE)
                return;
            ab.EjectOutputIfIncompatibleWithRecipe(recipe);
        }
    }

    [HarmonyPatch(typeof(ComplexFabricator), nameof(ComplexFabricator.IncrementRecipeQueueCount))]
    public static class ComplexFabricator_IncrementRecipeQueueCount_AlcoholBreweryEjectIncompatibleOutput
    {
        public static void Prefix(ComplexFabricator __instance, ComplexRecipe recipe)
        {
            if (__instance is AlcoholBrewery ab)
                ab.EjectOutputIfIncompatibleWithRecipe(recipe);
        }
    }
}
