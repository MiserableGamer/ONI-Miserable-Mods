using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ControlledBuildings.Patches
{
    // Vanilla drywall uses the building batch system at Grid.GetLayerZ(Backwall), so it ends up in front of
    // wires/conduits (higher SceneLayer = drawn on top). Backwalls mod draws with a custom renderer at Z = -1 or -16,
    // so wires correctly appear on top. Push vanilla drywall Z back to -16 so it matches and wires draw on top.
    [HarmonyPatch(typeof(BuildingComplete), nameof(BuildingComplete.OnSpawn))]
    public static class BuildingComplete_OnSpawn_DrywallZ_Patch
    {
        private static readonly HashSet<string> s_vanillaDrywallPrefabIds = new HashSet<string>
        {
            "ExteriorWall",
            "FacilityBackWallWindow",
            "FacilityBackWallWindowHorizontal",
            "ThermalBlock",
            "PropGravitasLabWall",
            "PropGravitasLabWindow",
            "PropGravitasLabWindowHorizontal",
            "PropGravitasWall",
            "PropGravitasWallPurple",
            "PropGravitasWallPurpleWhiteDiagonal",
            "PixelPack"
        };

        // Z used by Backwalls mod when drawing behind pipes; match it so wires/conduits draw on top of drywall.
        private const float DrywallZBehindWires = -16f;

        public static void Postfix(BuildingComplete __instance)
        {
            if (__instance?.Def == null) return;
            if (!s_vanillaDrywallPrefabIds.Contains(__instance.Def.PrefabID)) return;

            var t = __instance.transform;
            var p = t.position;
            t.position = new Vector3(p.x, p.y, DrywallZBehindWires);
        }
    }
}
