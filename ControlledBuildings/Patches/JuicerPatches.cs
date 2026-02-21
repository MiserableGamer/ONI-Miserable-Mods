using HarmonyLib;

namespace ControlledBuildings.Patches
{
    // Reduce the Juicer's placement footprint from 3x4 to 3x3.
    // The kanim (visual) still renders at full 4-cell height, but the top row
    // is no longer occupied — ceiling trim and other buildings can be placed there.
    [HarmonyPatch(typeof(JuicerConfig), nameof(JuicerConfig.CreateBuildingDef))]
    public static class JuicerConfig_CreateBuildingDef_Patch
    {
        public static void Postfix(BuildingDef __result)
        {
            __result.HeightInCells = 3;
            __result.GenerateOffsets();
        }
    }
}
