using ControlledPower.Buildings;
using HarmonyLib;
using STRINGS;

namespace ControlledPower.Patches
{
    [HarmonyPatch]
    public static class PowerDiodePatches
    {
        private static bool _buildingAddedToPlanScreen;

        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        [HarmonyPrefix]
        public static void LoadGeneratedBuildings_Prefix()
        {
            Strings.Add("STRINGS.BUILDINGS.PREFABS.POWERDIODE.NAME", STRINGS.UI.FormatAsLink("Power Diode", PowerDiodeConfig.ID));
            Strings.Add("STRINGS.BUILDINGS.PREFABS.POWERDIODE.DESC", "Allows power to flow in one direction only. Same behaviour as a large transformer.");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.POWERDIODE.EFFECT", "One-way power flow.");

            if (!_buildingAddedToPlanScreen)
            {
                ModUtil.AddBuildingToPlanScreen("Power", PowerDiodeConfig.ID, "PowerTransformerSmall", "PowerTransformerSmall");
                _buildingAddedToPlanScreen = true;
            }
        }

        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        [HarmonyPostfix]
        public static void Db_Initialize_Postfix()
        {
            var tech = Db.Get().Techs.Get("AdvancedPowerRegulation");
            if (tech == null || tech.unlockedItemIDs.Contains(PowerDiodeConfig.ID))
                return;
            tech.AddUnlockedItemIDs(new[] { PowerDiodeConfig.ID });
        }
    }
}
