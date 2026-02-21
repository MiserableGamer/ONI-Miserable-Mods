using HarmonyLib;
using UnityEngine;

namespace ControlledBuildings.Patches
{
    // GameUtil.DestroyCell only clears a hardcoded set of object layers and does not include PlasticTile.
    // We put transit tubes on PlasticTile, so the debug "Destroy cell" tool never removed them.
    // Postfix: also destroy any object on PlasticTile in the cell so debug cell delete removes transit tubes.
    [HarmonyPatch(typeof(GameUtil), nameof(GameUtil.DestroyCell))]
    public static class GameUtil_DestroyCell_Patch
    {
        public static void Postfix(int cell)
        {
            GameObject go = Grid.Objects[cell, (int)ObjectLayer.PlasticTile];
            if (go != null)
            {
                Util.KDestroyGameObject(go);
            }
        }
    }
}
