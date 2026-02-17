using HarmonyLib;
using ControlledMods.Buildings;

namespace ControlledMods.Patches.FreeResourceBuildings
{
    /// <summary>
    /// Conditional patches that register the Power Sink building (strings, plan screen, tech tree).
    /// Only applied when Free Resource Buildings mod is detected and the option is enabled.
    /// </summary>
    public static class PowerSinkRegistrationPatches
    {
        public static void ApplyPatches(Harmony harmony)
        {
            // String registration
            harmony.Patch(
                AccessTools.Method(typeof(Localization), nameof(Localization.Initialize)),
                postfix: new HarmonyMethod(typeof(PowerSinkRegistrationPatches),
                    nameof(Localization_Initialize_Postfix)));

            // Plan screen + tech tree registration
            harmony.Patch(
                AccessTools.Method(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings)),
                postfix: new HarmonyMethod(typeof(PowerSinkRegistrationPatches),
                    nameof(GeneratedBuildings_LoadGeneratedBuildings_Postfix)));

            harmony.Patch(
                AccessTools.Method(typeof(Db), nameof(Db.Initialize)),
                postfix: new HarmonyMethod(typeof(PowerSinkRegistrationPatches),
                    nameof(Db_Initialize_Postfix)));

            ControlledModsMod.Log("Power Sink registration patches applied");
        }

        private static void Localization_Initialize_Postfix()
        {
            Strings.Add("STRINGS.BUILDINGS.PREFABS.CONTROLLEDMODS_POWERSINK.NAME",
                STRINGS.UI.FormatAsLink("Power Sink", PowerSinkConfig.ID));
            Strings.Add("STRINGS.BUILDINGS.PREFABS.CONTROLLEDMODS_POWERSINK.DESC",
                "Consumes power from the electrical grid at a configurable rate. Useful for testing power systems.");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.CONTROLLEDMODS_POWERSINK.EFFECT",
                "Drains power from the connected circuit. Use the slider to set how many watts to consume.");

            Strings.Add("STRINGS.UI.UISIDESCREENS.CONTROLLEDMODS_POWERSINK.TITLE",
                "Power Consumption");
            Strings.Add("STRINGS.UI.UISIDESCREENS.CONTROLLEDMODS_POWERSINK.TOOLTIP",
                "Adjust how much power this building consumes");

            ControlledModsMod.Log("Power Sink strings registered");
        }

        private static void GeneratedBuildings_LoadGeneratedBuildings_Postfix()
        {
            ModUtil.AddBuildingToPlanScreen("Power", PowerSinkConfig.ID);
            ControlledModsMod.Log("Power Sink added to Power build menu");
        }

        private static void Db_Initialize_Postfix()
        {
            var tech = Db.Get().Techs.TryGet("PowerRegulation");
            if (tech != null)
            {
                if (!tech.unlockedItemIDs.Contains(PowerSinkConfig.ID))
                {
                    tech.unlockedItemIDs.Add(PowerSinkConfig.ID);
                    ControlledModsMod.Log("Power Sink added to Power Regulation tech");
                }
            }
            else
            {
                ControlledModsMod.LogWarning("Could not find PowerRegulation tech for Power Sink");
            }
        }
    }
}
