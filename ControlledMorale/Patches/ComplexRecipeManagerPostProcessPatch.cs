using HarmonyLib;

namespace ControlledMorale.Patches
{
    // Recipes must exist in preProcessRecipes before PostProcess() runs (Assets.CreatePrefabs).
    // Db.Initialize postfix also calls RegisterIfNeeded — this prefix covers elements not ready at Db time.
    [HarmonyPatch(typeof(ComplexRecipeManager), nameof(ComplexRecipeManager.PostProcess))]
    public static class ComplexRecipeManagerPostProcessPatch
    {
        public static void Prefix()
        {
            AlcoholBreweryRecipes.RegisterIfNeeded();
        }
    }
}
