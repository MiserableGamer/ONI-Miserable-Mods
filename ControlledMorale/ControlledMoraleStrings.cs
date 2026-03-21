using STRINGS;

namespace ControlledMorale
{
    public static class ControlledMoraleStrings
    {
        public static class ELEMENTS
        {
            public static class CONTROLLEDMORALE
            {
                public static class BEER
                {
                    public static LocString NAME = UI.FormatAsLink("Draft Beer", ControlledMoraleElements.BeerElementId);
                    public static LocString DESC = "Mild alcoholic beverage fermented from sleet wheat, sucrose, water, and ethanol.";
                }

                public static class WINE
                {
                    public static LocString NAME = UI.FormatAsLink("Ovagro Wine", ControlledMoraleElements.WineElementId);
                    public static LocString DESC = "Fortified wine made from figs, sucrose, water, and ethanol.";
                }

                public static class SPIRITS
                {
                    public static LocString NAME = UI.FormatAsLink("Nosh Spirit", ControlledMoraleElements.SpiritsElementId);
                    public static LocString DESC = "Strong spirit distilled with nosh beans, sucrose, water, and ethanol.";
                }
            }
        }

        public static class BUILDINGS
        {
            public static class PREFABS
            {
                public static class ALCOHOLBREWERY
                {
                    public static LocString NAME = UI.FormatAsLink("Alcohol Brewery", "ALCOHOLBREWERY");
                    public static LocString DESC = "Ferments ethanol, water, sucrose, and crops into beer, wine, or spirits for recreation.";
                    public static LocString EFFECT = "Outputs bottled beverages for Water Coolers or sends liquid to pipes.";
                }
            }
        }

        public static class DUPLICANTS
        {
            public static class MODIFIERS
            {
                public static class BEERBUZZ
                {
                    public static LocString NAME = "Beer Buzz";
                    public static LocString DESCRIPTION = "Relaxed and chatty after a drink.";
                }

                public static class WINEMOOD
                {
                    public static LocString NAME = "Wine Mood";
                    public static LocString DESCRIPTION = "Feeling cultured and mellow.";
                }

                public static class SPIRITWARMTH
                {
                    public static LocString NAME = "Spirit Warmth";
                    public static LocString DESCRIPTION = "A strong drink burns going down — nerves are steadier, legs less so.";
                }
            }
        }

        public static class FABRICATOR_INGREDIENT_UI
        {
            public static LocString IN_THIS_BUILDING = "In input storage: {0}";
            public static LocString IN_BUILD_BUFFER = "Reserved for active order (build buffer): {0}";
            public static LocString IN_COLONY = "In colony: {0}";
            public static LocString DELIVERY_HINT = "Enough exists colony-wide; dupes must move it into this building before a work order starts.";
        }

        public static class BUILDING
        {
            public static class WATERCOOLER
            {
                public static class OPTIONS
                {
                    public static LocString CONTROLLEDMORALEBEER = "Chilled draft beer.";
                    public static LocString CONTROLLEDMORALEWINE = "A glass of colony wine.";
                    public static LocString CONTROLLEDMORALESPIRITS = "A measure of spirits.";
                }
            }
        }
    }
}
