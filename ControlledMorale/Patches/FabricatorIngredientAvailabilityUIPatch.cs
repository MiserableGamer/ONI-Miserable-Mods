using System.Reflection;
using HarmonyLib;
using STRINGS;
using TUNING;
using UnityEngine;

namespace ControlledMorale
{
    // Tooltip: show fabricator inStorage vs colony inventory for ingredients.
    [HarmonyPatch]
    public static class FabricatorIngredientAvailabilityUIPatch
    {
        public static MethodBase TargetMethod() =>
            AccessTools.Method(
                typeof(SelectedRecipeQueueScreen),
                "GetIngredientDescription",
                new[] { typeof(ComplexRecipe.RecipeElement), typeof(bool).MakeByRefType() });

        public static void Postfix(
            SelectedRecipeQueueScreen __instance,
            ComplexRecipe.RecipeElement ingredient,
            ref bool hasEnoughMaterial,
            ref string __result)
        {
            if (ingredient == null || !ingredient.material.IsValid)
                return;

            var target = Traverse.Create(__instance).Field<ComplexFabricator>("target").Value;
            if (target == null)
                return;

            Tag[] forbid = target.ForbiddenTags;
            float inInput = target.inStorage.GetAmountAvailable(ingredient.material, forbid);
            float inBuild = target.buildStorage.GetAmountAvailable(ingredient.material, forbid);
            float colony = target.GetMyWorld().worldInventory.GetAmountWithoutTag(ingredient.material, true, forbid);

            bool enoughInInput = ingredient.amount - inInput < PICKUPABLETUNING.MINIMUM_PICKABLE_AMOUNT;
            hasEnoughMaterial = enoughInInput;

            string formattedNeed = GameUtil.GetFormattedByTag(ingredient.material, ingredient.amount, GameUtil.TimeSlice.None);
            // Same storage HasIngredients uses for starting/processing orders (not buildStorage alone).
            string formattedInInput = GameUtil.GetFormattedByTag(ingredient.material, inInput, GameUtil.TimeSlice.None);
            string formattedColony = GameUtil.GetFormattedByTag(ingredient.material, colony, GameUtil.TimeSlice.None);

            GameObject prefab = Assets.GetPrefab(ingredient.material);
            if (prefab == null)
                return;

            string text = GameUtil.SafeStringFormat(UI.UISIDESCREENS.FABRICATORSIDESCREEN.RECIPE_REQUIREMENT, new object[]
            {
                prefab.GetProperName(),
                formattedNeed
            });
            text += "\n<size=12>";
            if (enoughInInput)
            {
                text += GameUtil.SafeStringFormat(ControlledMoraleStrings.FABRICATOR_INGREDIENT_UI.IN_THIS_BUILDING, new object[]
                {
                    formattedInInput
                });
            }
            else
            {
                text += "<color=#E68280>" + GameUtil.SafeStringFormat(ControlledMoraleStrings.FABRICATOR_INGREDIENT_UI.IN_THIS_BUILDING, new object[]
                {
                    formattedInInput
                }) + "</color>";
            }

            if (inBuild >= PICKUPABLETUNING.MINIMUM_PICKABLE_AMOUNT)
            {
                string formattedInBuild = GameUtil.GetFormattedByTag(ingredient.material, inBuild, GameUtil.TimeSlice.None);
                text += "\n<color=#AAAAAA>" + GameUtil.SafeStringFormat(ControlledMoraleStrings.FABRICATOR_INGREDIENT_UI.IN_BUILD_BUFFER, new object[]
                {
                    formattedInBuild
                }) + "</color>";
            }

            text += "\n<color=#AAAAAA>" + GameUtil.SafeStringFormat(ControlledMoraleStrings.FABRICATOR_INGREDIENT_UI.IN_COLONY, new object[]
            {
                formattedColony
            }) + "</color>";

            if (!enoughInInput && ingredient.amount - colony < PICKUPABLETUNING.MINIMUM_PICKABLE_AMOUNT)
            {
                text += "\n" + ControlledMoraleStrings.FABRICATOR_INGREDIENT_UI.DELIVERY_HINT;
            }

            text += "</size>";
            __result = text;
        }
    }
}
