using System.Reflection;
using HarmonyLib;
using Klei;
using UnityEngine;

namespace ControlledMorale.Patches
{
    internal static class AlcoholBreweryFabricatorFields
    {
        internal static readonly FieldInfo RecipeListField =
            typeof(ComplexFabricator).GetField("recipe_list", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static readonly FieldInfo NextOrderIdxField =
            typeof(ComplexFabricator).GetField("nextOrderIdx", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static readonly FieldInfo NextOrderIsWorkableField =
            typeof(ComplexFabricator).GetField("nextOrderIsWorkable", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    // Clear stale empty recipe cache when Game was null on first GetRecipes (DLC filter dropped all).
    [HarmonyPatch(typeof(ComplexFabricator), nameof(ComplexFabricator.GetRecipes))]
    public static class AlcoholBreweryRecipeCachePatch
    {
        public static void Prefix(ComplexFabricator __instance)
        {
            if (!(__instance is AlcoholBrewery))
                return;
            if (Game.Instance == null || AlcoholBreweryFabricatorFields.RecipeListField == null)
                return;
            var cached = AlcoholBreweryFabricatorFields.RecipeListField.GetValue(__instance) as ComplexRecipe[];
            if (cached == null || cached.Length != 0)
                return;
            if (!FabricatorHasRecipesInManager(__instance))
                return;
            AlcoholBreweryFabricatorFields.RecipeListField.SetValue(__instance, null);
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

    // RefreshQueue runs ValidateNextOrder before UpdateOpenOrders (which calls GetRecipes). Rebuild recipe_list first.
    [HarmonyPatch(typeof(ComplexFabricator), "RefreshQueue")]
    public static class AlcoholBreweryRefreshQueuePatch
    {
        public static void Prefix(ComplexFabricator __instance)
        {
            if (!(__instance is AlcoholBrewery))
                return;
            var rf = AlcoholBreweryFabricatorFields.RecipeListField;
            var nf = AlcoholBreweryFabricatorFields.NextOrderIdxField;
            if (rf == null)
                return;

            var rl = rf.GetValue(__instance) as ComplexRecipe[];
            if (rl == null || rl.Length == 0)
            {
                rf.SetValue(__instance, null);
                __instance.GetRecipes();
                rl = rf.GetValue(__instance) as ComplexRecipe[];
            }

            if (rl != null && rl.Length > 0 && nf != null)
            {
                int idx = (int)nf.GetValue(__instance);
                if (idx < 0 || idx >= rl.Length)
                    nf.SetValue(__instance, 0);
            }
        }
    }

    // If recipe_list is still empty, skip ValidateNextOrder (avoids [0] and AdvanceNextOrder % 0).
    [HarmonyPatch(typeof(ComplexFabricator), "ValidateNextOrder")]
    public static class AlcoholBreweryValidateNextOrderPatch
    {
        public static bool Prefix(ComplexFabricator __instance)
        {
            if (!(__instance is AlcoholBrewery))
                return true;
            var rf = AlcoholBreweryFabricatorFields.RecipeListField;
            var wf = AlcoholBreweryFabricatorFields.NextOrderIsWorkableField;
            if (rf == null || wf == null)
                return true;
            var rl = rf.GetValue(__instance) as ComplexRecipe[];
            if (rl != null && rl.Length > 0)
                return true;
            wf.SetValue(__instance, false);
            return false;
        }
    }

    [HarmonyPatch(typeof(ComplexFabricator), "AdvanceNextOrder")]
    public static class AlcoholBreweryAdvanceNextOrderPatch
    {
        public static bool Prefix(ComplexFabricator __instance)
        {
            if (!(__instance is AlcoholBrewery))
                return true;
            var rf = AlcoholBreweryFabricatorFields.RecipeListField;
            if (rf == null)
                return true;
            var rl = rf.GetValue(__instance) as ComplexRecipe[];
            if (rl == null || rl.Length == 0)
                return false;
            return true;
        }
    }
}
