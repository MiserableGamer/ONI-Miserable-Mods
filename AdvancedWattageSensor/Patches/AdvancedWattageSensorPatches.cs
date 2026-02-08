using AdvancedWattageSensor.Buildings;
using AdvancedWattageSensor.UI;
using HarmonyLib;
using STRINGS;

namespace AdvancedWattageSensor.Patches
{
    public class AdvancedWattageSensorPatches
    {
        private static bool buildingRegistered;
        private static bool sideScreenRegistered;

        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            private static void Prefix()
            {
                // Strings can be re-added safely but plan screen additions are cumulative
                Strings.Add(
                    "STRINGS.BUILDINGS.PREFABS.ADVANCEDWATTAGESENSOR.NAME",
                    STRINGS.UI.FormatAsLink("Advanced Wattage Sensor", AdvancedWattageSensorConfig.ID));
                Strings.Add(
                    "STRINGS.BUILDINGS.PREFABS.ADVANCEDWATTAGESENSOR.EFFECT",
                    "Wattage sensor with display of the current network power.");
                Strings.Add(
                    "STRINGS.BUILDINGS.PREFABS.ADVANCEDWATTAGESENSOR.DESC",
                    "Sends an automation signal based on the current wattage drawn by the electrical network, with a visual power meter.");

                if (!buildingRegistered)
                {
                    ModUtil.AddBuildingToPlanScreen("Automation", AdvancedWattageSensorConfig.ID, "sensors", LogicWattageSensorConfig.ID);
                    buildingRegistered = true;
                }
            }
        }

        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public class Db_Initialize_Patch
        {
            private static void Postfix()
            {
                Db.Get().Techs.Get("AdvancedPowerRegulation")
                    .AddUnlockedItemIDs(new[] { AdvancedWattageSensorConfig.ID });
            }
        }

        // Register the label sidescreen
        [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
        public class DetailsScreen_OnPrefabInit_Patch
        {
            private static void Postfix()
            {
                if (!sideScreenRegistered)
                {
                    SideScreenHelper.AddSideScreen<WattageSensorSideScreen>("WattageSensorLabelSideScreen");
                    sideScreenRegistered = true;
                }
            }
        }

        // Create the wattage monitor panel once the Resources panel exists
        // (same pattern as PinnedResourceListExtended reference mod)
        [HarmonyPatch(typeof(PinnedResourcesPanel), "OnSpawn")]
        public class PinnedResourcesPanel_OnSpawn_Patch
        {
            private static void Postfix()
            {
                WattageMonitorPanel.Create();
            }
        }

        // Clean up the panel when leaving a game session
        [HarmonyPatch(typeof(Game), "OnDestroy")]
        public class Game_OnDestroy_Patch
        {
            private static void Prefix()
            {
                WattageMonitorPanel.DestroyInstance();
            }
        }
    }
}
