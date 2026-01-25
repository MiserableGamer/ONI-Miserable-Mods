using HarmonyLib;
using ControlledAutomation.Buildings;

namespace ControlledAutomation.Patches
{
    public static class BuildingPatches
    {
        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                string id = TemperatureRangeSensorConfig.ID.ToUpperInvariant();
                Strings.Add($"STRINGS.BUILDINGS.PREFABS.{id}.NAME",
                    STRINGS.CONTROLLEDAUTOMATION.BUILDINGS.PREFABS.TEMPERATURERANGESENSOR.NAME);
                Strings.Add($"STRINGS.BUILDINGS.PREFABS.{id}.DESC",
                    STRINGS.CONTROLLEDAUTOMATION.BUILDINGS.PREFABS.TEMPERATURERANGESENSOR.DESC);
                Strings.Add($"STRINGS.BUILDINGS.PREFABS.{id}.EFFECT",
                    STRINGS.CONTROLLEDAUTOMATION.BUILDINGS.PREFABS.TEMPERATURERANGESENSOR.EFFECT);

                ModUtil.AddBuildingToPlanScreen("Automation", TemperatureRangeSensorConfig.ID, "sensors", LogicTemperatureSensorConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_Patch
        {
            public static void Postfix()
            {
                Db.Get().Techs.TryGet("GenericSensors")?.unlockedItemIDs.Add(TemperatureRangeSensorConfig.ID);
            }
        }
    }
}
