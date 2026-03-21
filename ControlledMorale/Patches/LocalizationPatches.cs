using HarmonyLib;
using Klei;

namespace ControlledMorale
{
    [HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
    public static class LocalizationPatches
    {
        public static void Postfix()
        {
            LocString.CreateLocStringKeys(typeof(ControlledMoraleStrings.ELEMENTS));
            LocString.CreateLocStringKeys(typeof(ControlledMoraleStrings.BUILDINGS));
            LocString.CreateLocStringKeys(typeof(ControlledMoraleStrings.DUPLICANTS));
            LocString.CreateLocStringKeys(typeof(ControlledMoraleStrings.BUILDING));

            Strings.Add("STRINGS.ELEMENTS.CONTROLLEDMORALE.BEER.NAME", ControlledMoraleStrings.ELEMENTS.CONTROLLEDMORALE.BEER.NAME);
            Strings.Add("STRINGS.ELEMENTS.CONTROLLEDMORALE.BEER.DESC", ControlledMoraleStrings.ELEMENTS.CONTROLLEDMORALE.BEER.DESC);
            Strings.Add("STRINGS.ELEMENTS.CONTROLLEDMORALE.WINE.NAME", ControlledMoraleStrings.ELEMENTS.CONTROLLEDMORALE.WINE.NAME);
            Strings.Add("STRINGS.ELEMENTS.CONTROLLEDMORALE.WINE.DESC", ControlledMoraleStrings.ELEMENTS.CONTROLLEDMORALE.WINE.DESC);
            Strings.Add("STRINGS.ELEMENTS.CONTROLLEDMORALE.SPIRITS.NAME", ControlledMoraleStrings.ELEMENTS.CONTROLLEDMORALE.SPIRITS.NAME);
            Strings.Add("STRINGS.ELEMENTS.CONTROLLEDMORALE.SPIRITS.DESC", ControlledMoraleStrings.ELEMENTS.CONTROLLEDMORALE.SPIRITS.DESC);
        }
    }
}
