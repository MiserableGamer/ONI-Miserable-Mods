using HarmonyLib;

namespace ControlledMorale
{
    // Do not start next order while outStorage still holds output.
    [HarmonyPatch(typeof(ComplexFabricator), "RefreshAndStartNextOrder")]
    public static class ComplexFabricator_RefreshAndStartNextOrder_AlcoholBreweryHoldOutput
    {
        public static bool Prefix(ComplexFabricator __instance)
        {
            if (__instance is AlcoholBrewery ab && ab.outStorage != null && ab.outStorage.MassStored() > 0f)
                return false;
            return true;
        }
    }
}
