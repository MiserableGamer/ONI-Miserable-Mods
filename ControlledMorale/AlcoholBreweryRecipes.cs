using System.Collections.Generic;

namespace ControlledMorale
{
    public static class AlcoholBreweryRecipes
    {
        private static bool registered;

        public static void RegisterIfNeeded()
        {
            if (registered)
                return;
            var beer = ElementLoader.FindElementByHash(ControlledMoraleElements.BeerHash);
            var wine = ElementLoader.FindElementByHash(ControlledMoraleElements.WineHash);
            var spirits = ElementLoader.FindElementByHash(ControlledMoraleElements.SpiritsHash);
            if (beer == null || wine == null || spirits == null)
                return;

            Tag beerTag = beer.tag;
            Tag wineTag = wine.tag;
            Tag spiritsTag = spirits.tag;
            Tag waterTag = SimHashes.Water.CreateTag();
            Tag ethanolTag = ElementLoader.FindElementByHash(SimHashes.Ethanol).tag;
            Tag sucroseTag = ElementLoader.FindElementByHash(SimHashes.Sucrose).tag;
            // Match vanilla fabricator lists (e.g. MicrobeMusher, MilkPress) — same Tag as building PrefabTag.
            var fabricators = new List<Tag> { AlcoholBreweryConfig.ID };

            ComplexRecipe.RecipeElement[] beerIn =
            {
                new ComplexRecipe.RecipeElement("ColdWheatSeed", 2f),
                new ComplexRecipe.RecipeElement(sucroseTag, 1f),
                new ComplexRecipe.RecipeElement(waterTag, 15f),
                new ComplexRecipe.RecipeElement(ethanolTag, 3f)
            };
            ComplexRecipe.RecipeElement[] beerOut =
            {
                new ComplexRecipe.RecipeElement(beerTag, 10f, ComplexRecipe.RecipeElement.TemperatureOperation.Melted, false)
            };
            _ = new ComplexRecipe(
                    ComplexRecipeManager.MakeRecipeID(AlcoholBreweryConfig.ID, beerIn, beerOut),
                    beerIn,
                    beerOut)
            {
                time = 80f,
                description = "Ferment sleet wheat into draft beer for the Water Cooler.",
                nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
                fabricators = fabricators,
                sortOrder = 1
            };

            bool hasOvagro = DlcManager.IsContentSubscribed("DLC4_ID");
            Tag wineFruitTag = hasOvagro
                ? VineFruitConfig.ID.ToTag()
                : PrickleFruitConfig.ID.ToTag();

            ComplexRecipe.RecipeElement[] wineIn =
            {
                new ComplexRecipe.RecipeElement(wineFruitTag, 2f),
                new ComplexRecipe.RecipeElement(sucroseTag, 1f),
                new ComplexRecipe.RecipeElement(waterTag, 15f),
                new ComplexRecipe.RecipeElement(ethanolTag, 6f)
            };
            ComplexRecipe.RecipeElement[] wineOut =
            {
                new ComplexRecipe.RecipeElement(wineTag, 10f, ComplexRecipe.RecipeElement.TemperatureOperation.Melted, false)
            };
            _ = new ComplexRecipe(
                    ComplexRecipeManager.MakeRecipeID(AlcoholBreweryConfig.ID, wineIn, wineOut),
                    wineIn,
                    wineOut)
            {
                time = 100f,
                description = hasOvagro
                    ? "Ferment ovagro figs into wine for the Water Cooler."
                    : "Ferment bristle berries into wine for the Water Cooler.",
                nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
                fabricators = fabricators,
                sortOrder = 2
            };

            ComplexRecipe.RecipeElement[] spiritsIn =
            {
                new ComplexRecipe.RecipeElement("BeanPlantSeed", 2f),
                new ComplexRecipe.RecipeElement(sucroseTag, 1f),
                new ComplexRecipe.RecipeElement(waterTag, 12f),
                new ComplexRecipe.RecipeElement(ethanolTag, 18f)
            };
            ComplexRecipe.RecipeElement[] spiritsOut =
            {
                new ComplexRecipe.RecipeElement(spiritsTag, 10f, ComplexRecipe.RecipeElement.TemperatureOperation.Melted, false)
            };
            _ = new ComplexRecipe(
                    ComplexRecipeManager.MakeRecipeID(AlcoholBreweryConfig.ID, spiritsIn, spiritsOut),
                    spiritsIn,
                    spiritsOut)
            {
                time = 130f,
                description = "Distill nosh beans into spirits for the Water Cooler.",
                nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
                fabricators = fabricators,
                sortOrder = 3
            };

            // Only after constructors ran: ComplexRecipe adds to preProcessRecipes; PostProcess will promote to recipes.
            registered = true;
        }
    }
}
