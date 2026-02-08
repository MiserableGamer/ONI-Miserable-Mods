using System;
using HarmonyLib;
using UnityEngine;

namespace SkillsAndStatsProgressFIXED
{
    [HarmonyPatch(typeof(PopFXManager), "SpawnFX", new Type[]
    {
        typeof(Sprite),
        typeof(string),
        typeof(Transform),
        typeof(Vector3),
        typeof(float),
        typeof(bool),
        typeof(bool)
    })]
    public static class PopFxPatches
    {
        public static void Postfix(PopFX __result)
        {
            if (__result != null)
            {
                __result.TextDisplay.fontSize = 24f;
                var iconColor = __result.IconDisplay.color;
                iconColor.a = 1f;
                __result.IconDisplay.color = iconColor;
            }
        }
    }
}
