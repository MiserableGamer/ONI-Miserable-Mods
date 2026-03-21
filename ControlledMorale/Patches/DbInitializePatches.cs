using HarmonyLib;
using Klei.AI;

namespace ControlledMorale
{
    internal static class DbInitializePatches
    {
        private static bool effectsRegistered;
        private static bool waterCoolerBeveragesExtended;
        private static bool breweryUnlocked;

        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_EffectsAndTech
        {
            public static void Postfix(Db __instance)
            {
                AlcoholBreweryRecipes.RegisterIfNeeded();

                if (!effectsRegistered)
                {
                    RegisterEffects(__instance);
                    effectsRegistered = true;
                }
                if (!waterCoolerBeveragesExtended)
                {
                    ExtendWaterCoolerBeverages();
                    waterCoolerBeveragesExtended = true;
                }
                if (!breweryUnlocked)
                {
                    UnlockBuilding();
                    breweryUnlocked = true;
                }
            }

            private static void RegisterEffects(Db db)
            {
                float beerDuration = 600f;
                float wineDuration = 720f;
                float spiritsDuration = 900f;

                var beer = new Effect(
                    ControlledMoraleEffects.Beer,
                    ControlledMoraleStrings.DUPLICANTS.MODIFIERS.BEERBUZZ.NAME,
                    ControlledMoraleStrings.DUPLICANTS.MODIFIERS.BEERBUZZ.DESCRIPTION,
                    beerDuration,
                    true,
                    true,
                    false,
                    null,
                    -1f,
                    0f,
                    null,
                    "");
                beer.Add(new AttributeModifier(db.Attributes.QualityOfLife.Id, 2f, ControlledMoraleEffects.Beer, false, false));
                beer.Add(new AttributeModifier(db.Attributes.Athletics.Id, -1f, ControlledMoraleEffects.Beer, false, false));
                db.effects.Add(beer);

                var wine = new Effect(
                    ControlledMoraleEffects.Wine,
                    ControlledMoraleStrings.DUPLICANTS.MODIFIERS.WINEMOOD.NAME,
                    ControlledMoraleStrings.DUPLICANTS.MODIFIERS.WINEMOOD.DESCRIPTION,
                    wineDuration,
                    true,
                    true,
                    false,
                    null,
                    -1f,
                    0f,
                    null,
                    "");
                wine.Add(new AttributeModifier(db.Attributes.QualityOfLife.Id, 4f, ControlledMoraleEffects.Wine, false, false));
                wine.Add(new AttributeModifier(db.Attributes.Athletics.Id, -2f, ControlledMoraleEffects.Wine, false, false));
                db.effects.Add(wine);

                var spirits = new Effect(
                    ControlledMoraleEffects.Spirits,
                    ControlledMoraleStrings.DUPLICANTS.MODIFIERS.SPIRITWARMTH.NAME,
                    ControlledMoraleStrings.DUPLICANTS.MODIFIERS.SPIRITWARMTH.DESCRIPTION,
                    spiritsDuration,
                    true,
                    true,
                    false,
                    null,
                    -1f,
                    0f,
                    null,
                    "");
                spirits.Add(new AttributeModifier(db.Attributes.QualityOfLife.Id, 6f, ControlledMoraleEffects.Spirits, false, false));
                spirits.Add(new AttributeModifier(db.Attributes.Athletics.Id, -4f, ControlledMoraleEffects.Spirits, false, false));
                db.effects.Add(spirits);
            }

            private static void ExtendWaterCoolerBeverages()
            {
                var beer = ElementLoader.FindElementByHash(ControlledMoraleElements.BeerHash);
                var wine = ElementLoader.FindElementByHash(ControlledMoraleElements.WineHash);
                var spirits = ElementLoader.FindElementByHash(ControlledMoraleElements.SpiritsHash);
                if (beer == null || wine == null || spirits == null)
                    return;



                var orig = WaterCoolerConfig.BEVERAGE_CHOICE_OPTIONS;
                var extra = new[]
                {
                    new global::Tuple<Tag, string>(beer.tag, ControlledMoraleEffects.Beer),
                    new global::Tuple<Tag, string>(wine.tag, ControlledMoraleEffects.Wine),
                    new global::Tuple<Tag, string>(spirits.tag, ControlledMoraleEffects.Spirits)
                };
                var merged = new global::Tuple<Tag, string>[orig.Length + extra.Length];
                System.Array.Copy(orig, merged, orig.Length);
                System.Array.Copy(extra, 0, merged, orig.Length, extra.Length);
                WaterCoolerConfig.BEVERAGE_CHOICE_OPTIONS = merged;

                Strings.Add(
                    "STRINGS.BUILDINGS.PREFABS.WATERCOOLER.OPTION_TOOLTIPS.CONTROLLEDMORALEBEER",
                    ControlledMoraleStrings.BUILDING.WATERCOOLER.OPTIONS.CONTROLLEDMORALEBEER);
                Strings.Add(
                    "STRINGS.BUILDINGS.PREFABS.WATERCOOLER.OPTION_TOOLTIPS.CONTROLLEDMORALEWINE",
                    ControlledMoraleStrings.BUILDING.WATERCOOLER.OPTIONS.CONTROLLEDMORALEWINE);
                Strings.Add(
                    "STRINGS.BUILDINGS.PREFABS.WATERCOOLER.OPTION_TOOLTIPS.CONTROLLEDMORALESPIRITS",
                    ControlledMoraleStrings.BUILDING.WATERCOOLER.OPTIONS.CONTROLLEDMORALESPIRITS);
            }

            private static void UnlockBuilding()
            {
                var tech = Db.Get().Techs.Get("FoodRepurposing");
                tech?.AddUnlockedItemIDs(AlcoholBreweryConfig.ID);
            }
        }
    }
}
