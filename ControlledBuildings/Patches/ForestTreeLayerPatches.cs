using HarmonyLib;
using UnityEngine;

namespace ControlledBuildings.Patches
{
    // Fix vanilla bug: Arbor Tree (Forest Tree) trunk is not visible when planted in farm tiles
    // because the trunk uses Building (25) and branches use BuildingFront (27), while farm tiles
    // use TileMain (35). Higher layer value = drawn on top, so the tile draws over the trunk.
    // We set the trunk to TileFront (36) and branches to FXFront (37) so they render above the tile.
    [HarmonyPatch(typeof(ForestTreeConfig), nameof(ForestTreeConfig.CreatePrefab))]
    public static class ForestTreeConfig_CreatePrefab_Patch
    {
        public static void Postfix(GameObject __result)
        {
            if (__result == null) return;
            var kbac = __result.GetComponent<KBatchedAnimController>();
            if (kbac != null)
                kbac.sceneLayer = Grid.SceneLayer.TileFront;
        }
    }

    [HarmonyPatch(typeof(ForestTreeBranchConfig), nameof(ForestTreeBranchConfig.CreatePrefab))]
    public static class ForestTreeBranchConfig_CreatePrefab_Patch
    {
        public static void Postfix(GameObject __result)
        {
            if (__result == null) return;
            var kbac = __result.GetComponent<KBatchedAnimController>();
            if (kbac != null)
                kbac.sceneLayer = Grid.SceneLayer.FXFront;
        }
    }
}
