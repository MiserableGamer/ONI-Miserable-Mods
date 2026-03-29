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
                float beerDuration    = 600f;
                float wineDuration    = 720f;
                float spiritsDuration = 900f;

                var beer = new Effect(
                    ControlledMoraleEffects.Beer,
                    ControlledMoraleStrings.DUPLICANTS.MODIFIERS.BEERBUZZ.NAME,
                    ControlledMoraleStrings.DUPLICANTS.MODIFIERS.BEERBUZZ.DESCRIPTION,
                    beerDuration, true, true, false, null, -1f, 0f, null, "");
                db.effects.Add(beer);

                var wine = new Effect(
                    ControlledMoraleEffects.Wine,
                    ControlledMoraleStrings.DUPLICANTS.MODIFIERS.WINEMOOD.NAME,
                    ControlledMoraleStrings.DUPLICANTS.MODIFIERS.WINEMOOD.DESCRIPTION,
                    wineDuration, true, true, false, null, -1f, 0f, null, "");
                db.effects.Add(wine);

                var spirits = new Effect(
                    ControlledMoraleEffects.Spirits,
                    ControlledMoraleStrings.DUPLICANTS.MODIFIERS.SPIRITWARMTH.NAME,
                    ControlledMoraleStrings.DUPLICANTS.MODIFIERS.SPIRITWARMTH.DESCRIPTION,
                    spiritsDuration, true, true, false, null, -1f, 0f, null, "");
                db.effects.Add(spirits);

                ApplyBeverageModifiers(db, beer, wine, spirits);
            }

            internal static void ReapplyBeverageModifiers()
            {
                if (!effectsRegistered) return;
                var db      = Db.Get();
                var beer    = db.effects.TryGet(ControlledMoraleEffects.Beer);
                var wine    = db.effects.TryGet(ControlledMoraleEffects.Wine);
                var spirits = db.effects.TryGet(ControlledMoraleEffects.Spirits);
                if (beer == null || wine == null || spirits == null) return;
                beer.SelfModifiers.Clear();
                wine.SelfModifiers.Clear();
                spirits.SelfModifiers.Clear();
                ApplyBeverageModifiers(db, beer, wine, spirits);
            }

            private static void ApplyBeverageModifiers(Db db, Effect beer, Effect wine, Effect spirits)
            {
                var opts = ControlledMoraleOptions.Instance;
                ApplyToEffect(db, beer,    opts.Beer,    ControlledMoraleEffects.Beer,    defaultQoL: 2f, defaultAthletics: -2f);
                ApplyToEffect(db, wine,    opts.Wine,    ControlledMoraleEffects.Wine,    defaultQoL: 4f, defaultAthletics: -4f);
                ApplyToEffect(db, spirits, opts.Spirits, ControlledMoraleEffects.Spirits, defaultQoL: 6f, defaultAthletics: -6f);
            }

            private static void ApplyToEffect(Db db, Effect effect, BeverageModifiers mods, string effectId,
                float defaultQoL, float defaultAthletics)
            {
                // QoL and Athletics always apply — either the configured value or the beverage's baseline.
                // Other attributes only apply when explicitly enabled, with no fallback.
                float qolVal = mods.QualityOfLifeEnabled ? (float)mods.QualityOfLifeValue : defaultQoL;
                if (qolVal != 0f)
                    effect.Add(new AttributeModifier(db.Attributes.QualityOfLife.Id, qolVal, effectId, false, false));

                float athVal = mods.AthleticsEnabled ? (float)mods.AthleticsValue : defaultAthletics;
                if (athVal != 0f)
                    effect.Add(new AttributeModifier(db.Attributes.Athletics.Id, athVal, effectId, false, false));

                void AddIfEnabled(bool enabled, int value, string attrId, float scale = 1f)
                {
                    if (!enabled || value == 0) return;
                    effect.Add(new AttributeModifier(attrId, value * scale, effectId, false, false));
                }

                AddIfEnabled(mods.ConstructionEnabled,     mods.ConstructionValue,     db.Attributes.Construction.Id);
                AddIfEnabled(mods.DiggingEnabled,          mods.DiggingValue,          db.Attributes.Digging.Id);
                AddIfEnabled(mods.MachineryEnabled,        mods.MachineryValue,        db.Attributes.Machinery.Id);
                AddIfEnabled(mods.LearningEnabled,         mods.LearningValue,         db.Attributes.Learning.Id);
                AddIfEnabled(mods.CookingEnabled,          mods.CookingValue,          db.Attributes.Cooking.Id);
                AddIfEnabled(mods.CaringEnabled,           mods.CaringValue,           db.Attributes.Caring.Id);
                AddIfEnabled(mods.StrengthEnabled,         mods.StrengthValue,         db.Attributes.Strength.Id);
                AddIfEnabled(mods.ArtEnabled,              mods.ArtValue,              db.Attributes.Art.Id);
                AddIfEnabled(mods.BotanistEnabled,         mods.BotanistValue,         db.Attributes.Botanist.Id);
                AddIfEnabled(mods.RanchingEnabled,         mods.RanchingValue,         db.Attributes.Ranching.Id);
                AddIfEnabled(mods.SpaceNavigationEnabled,  mods.SpaceNavigationValue,  db.Attributes.SpaceNavigation.Id);
                AddIfEnabled(mods.GermResistanceEnabled,   mods.GermResistanceValue,   db.Attributes.GermResistance.Id);
                AddIfEnabled(mods.CarryAmountEnabled,      mods.CarryAmountValue,      db.Attributes.CarryAmount.Id,      scale: 18f);   // 18 kg/step (~10% of 180 kg base)
                AddIfEnabled(mods.SneezynessEnabled,       mods.SneezynessValue,       db.Attributes.Sneezyness.Id);
                AddIfEnabled(mods.DiseaseCureSpeedEnabled, mods.DiseaseCureSpeedValue, db.Attributes.DiseaseCureSpeed.Id, scale: 0.05f); // base=1.0; formatter ×100 → 5%/step
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
