using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ControlledBuildings.Patches
{
    // Ensure drywall/backwall always draws behind Transit Tubes by fixing SceneLayer at registration.
    // Some setups (or mods) can leave drywall and tubes on the same or wrong layer.
    [HarmonyPatch(typeof(Assets), nameof(Assets.AddBuildingDef))]
    public static class Assets_AddBuildingDef_SceneLayer_Patch
    {
        // Farm/planter/crop buildings must never be forced to Backwall so the plant remains visible (drawn on its normal layer instead of behind everything).
        private static readonly HashSet<string> s_farmPlanterCropPrefabIds = new HashSet<string> { "PlanterBox", "FarmTile", "HydroponicFarm" };

        public static void Prefix(BuildingDef def)
        {
            if (def == null) return;

            // Transit Tubes: draw in front of backwall, allow build in/behind tiles, and don't block drywall in same cell.
            // SceneLayer = BuildingFront. ObjectLayer = PlasticTile (through walls, don't occupy Backwall).
            // BuildLocationRule = Anywhere for tube segment so it can be built in cells with tiles (e.g. behind drywall).
            switch (def.PrefabID)
            {
                case "TravelTube":
                    def.SceneLayer = Grid.SceneLayer.BuildingFront;
                    def.ObjectLayer = ObjectLayer.PlasticTile;
                    def.BuildLocationRule = BuildLocationRule.Anywhere;
                    return;
                case "TravelTubeWallBridge":
                case "TravelTubeEntrance":
                    def.SceneLayer = Grid.SceneLayer.BuildingFront;
                    def.ObjectLayer = ObjectLayer.PlasticTile;
                    return;
            }

            // Vanilla drywall and backwall-type: ObjectLayer.Backwall; use BuildingBack so they draw in front of
            // conduits/wires (GasConduits=6, Wires=12, etc.) but still behind transit tubes (BuildingFront=27).
            switch (def.PrefabID)
            {
                case "ExteriorWall":
                case "FacilityBackWallWindow":
                case "FacilityBackWallWindowHorizontal":
                case "ThermalBlock":
                case "PropGravitasLabWall":
                case "PropGravitasLabWindow":
                case "PropGravitasLabWindowHorizontal":
                case "PropGravitasWall":
                case "PropGravitasWallPurple":
                case "PropGravitasWallPurpleWhiteDiagonal":
                case "PixelPack":
                    def.ObjectLayer = ObjectLayer.Backwall;
                    def.SceneLayer = Grid.SceneLayer.BuildingBack;
                    return;
            }

            // Force all other backwall/drywall to draw on Backwall layer: by ObjectLayer (e.g. aki's Backwalls)
            // or by GameTags.Backwall (any mod that tags as backwall but uses a different ObjectLayer).
            // Exclude farm/planter/crop prefabs so they are never forced to Backwall and the plant remains visible.
            if (def.ObjectLayer == ObjectLayer.Backwall)
            {
                if (s_farmPlanterCropPrefabIds.Contains(def.PrefabID)) return;
                def.SceneLayer = Grid.SceneLayer.Backwall;
                return;
            }
            if (def.BuildingComplete != null && def.BuildingComplete.HasTag(GameTags.Backwall))
            {
                if (s_farmPlanterCropPrefabIds.Contains(def.PrefabID)) return;
                def.SceneLayer = Grid.SceneLayer.Backwall;
                return;
            }
        }
    }
}
