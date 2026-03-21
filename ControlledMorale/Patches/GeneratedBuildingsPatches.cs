using HarmonyLib;
using PeterHan.PLib.Core;
using STRINGS;
using UnityEngine;

namespace ControlledMorale
{
    [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
    public static class GeneratedBuildingsPatches
    {
        private static bool registered;

        public static void Prefix()
        {
            Strings.Add(
                "STRINGS.BUILDINGS.PREFABS.ALCOHOLBREWERY.NAME",
                ControlledMoraleStrings.BUILDINGS.PREFABS.ALCOHOLBREWERY.NAME);
            Strings.Add(
                "STRINGS.BUILDINGS.PREFABS.ALCOHOLBREWERY.DESC",
                ControlledMoraleStrings.BUILDINGS.PREFABS.ALCOHOLBREWERY.DESC);
            Strings.Add(
                "STRINGS.BUILDINGS.PREFABS.ALCOHOLBREWERY.EFFECT",
                ControlledMoraleStrings.BUILDINGS.PREFABS.ALCOHOLBREWERY.EFFECT);
        }

        public static void Postfix()
        {
            if (registered)
                return;
            registered = true;

            ModUtil.AddBuildingToPlanScreen("Refining", AlcoholBreweryConfig.ID, "EthanolDistillery", "EthanolDistillery");

            var def = Assets.GetBuildingDef(AlcoholBreweryConfig.ID);
            if (def == null)
            {
                Debug.LogWarning("[ControlledMorale] Alcohol Brewery BuildingDef not found after load.");
                return;
            }

            var customAnim = Assets.GetAnim("alcohol_brewery_kanim");
            if (customAnim != null)
            {
                def.AnimFiles = new[] { customAnim };
                if (def.BuildingComplete != null)
                {
                    var kbac = def.BuildingComplete.GetComponent<KBatchedAnimController>();
                    if (kbac != null)
                        kbac.AnimFiles = new[] { customAnim };
                }
            }
        }
    }
}
