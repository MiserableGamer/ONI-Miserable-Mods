using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ControlledMorale
{
    // Ensure YAML elements get substance anims before ore generation (AddLiquid needs prefab).
    [HarmonyPatch(typeof(ElementLoader), nameof(ElementLoader.Load))]
    public static class ElementLoader_Load_ConfigureSubstances
    {
        public static void Postfix()
        {
            var waterSubstance = ElementLoader.FindElementByHash(SimHashes.Water)?.substance;
            if (waterSubstance == null)
                return;

            var liquidAnim = Assets.GetAnim("liquid_tank_kanim");
            if (liquidAnim == null)
                return;

            var baseMaterial = waterSubstance.material;

            ConfigureSubstance(ControlledMoraleElements.BeerHash, liquidAnim, baseMaterial,
                new Color(0.82f, 0.67f, 0.20f, 1f));
            ConfigureSubstance(ControlledMoraleElements.WineHash, liquidAnim, baseMaterial,
                new Color(0.55f, 0.08f, 0.24f, 1f));
            ConfigureSubstance(ControlledMoraleElements.SpiritsHash, liquidAnim, baseMaterial,
                new Color(0.86f, 0.86f, 0.94f, 1f));
        }

        private static void ConfigureSubstance(SimHashes hash, KAnimFile anim, Material baseMaterial, Color colour)
        {
            var element = ElementLoader.FindElementByHash(hash);
            if (element?.substance == null)
                return;

            element.substance.anim = anim;
            element.substance.material = new Material(baseMaterial);
            element.substance.colour = colour;
            element.substance.uiColour = colour;
            element.substance.conduitColour = colour;
        }
    }
}
