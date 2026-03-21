using System.Reflection;
using HarmonyLib;
using Klei;
using UnityEngine;

namespace ControlledMorale.Patches
{
    // Clear stale empty recipe cache when Game was null on first GetRecipes (DLC filter dropped all).
    [HarmonyPatch(typeof(ComplexFabricator), nameof(ComplexFabricator.GetRecipes))]
    public static class AlcoholBreweryRecipeCachePatch
    {
        private static readonly FieldInfo RecipeListField =
            typeof(ComplexFabricator).GetField("recipe_list", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Prefix(ComplexFabricator __instance)
        {
            if (!(__instance is AlcoholBrewery))
                return;
            if (Game.Instance == null || RecipeListField == null)
                return;
            var cached = RecipeListField.GetValue(__instance) as ComplexRecipe[];
            if (cached == null || cached.Length != 0)
                return;
            if (!FabricatorHasRecipesInManager(__instance))
                return;
            RecipeListField.SetValue(__instance, null);
        }

        private static bool FabricatorHasRecipesInManager(ComplexFabricator fab)
        {
            var prefabTag = fab.GetComponent<KPrefabID>().PrefabTag;
            foreach (ComplexRecipe recipe in ComplexRecipeManager.Get().recipes)
            {
                if (recipe.fabricators == null)
                    continue;
                foreach (Tag t in recipe.fabricators)
                {
                    if (t == prefabTag)
                        return true;
                }
            }
            return false;
        }
    }
}
