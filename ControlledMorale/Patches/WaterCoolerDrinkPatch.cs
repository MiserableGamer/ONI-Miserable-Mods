using HarmonyLib;
using Klei.AI;
using UnityEngine;

namespace ControlledMorale
{
    [HarmonyPatch(typeof(WaterCooler.StatesInstance), nameof(WaterCooler.StatesInstance.Drink))]
    public static class WaterCoolerDrinkPatch
    {
        public static void Postfix(WaterCooler.StatesInstance __instance, GameObject druplicant)
        {
            if (druplicant == null)
                return;
            var tag = __instance.master.ChosenBeverage;
            string effectId = null;
            if (tag == ElementLoader.FindElementByHash(ControlledMoraleElements.BeerHash)?.tag)
                effectId = ControlledMoraleEffects.Beer;
            else if (tag == ElementLoader.FindElementByHash(ControlledMoraleElements.WineHash)?.tag)
                effectId = ControlledMoraleEffects.Wine;
            else if (tag == ElementLoader.FindElementByHash(ControlledMoraleElements.SpiritsHash)?.tag)
                effectId = ControlledMoraleEffects.Spirits;
            if (effectId == null)
                return;
            var effects = druplicant.GetComponent<Effects>();
            if (effects != null)
                effects.Add(effectId, true);
        }
    }

    // Reset invalid saved beverage tag to Water (missing element / partial mod load).
    [HarmonyPatch(typeof(WaterCooler), "OnSpawn")]
    public static class WaterCooler_OnSpawn_ValidateBeverage
    {
        public static void Prefix(WaterCooler __instance)
        {
            Tag chosen = __instance.ChosenBeverage;
            if (chosen == Tag.Invalid || ElementLoader.GetElement(chosen) == null)
            {
                Debug.LogWarning(
                    $"[ControlledMorale] WaterCooler at {__instance.transform.position} had invalid beverage tag '{chosen}' — resetting to Water.");
                __instance.ChosenBeverage = GameTags.Water;
            }
        }
    }
}
