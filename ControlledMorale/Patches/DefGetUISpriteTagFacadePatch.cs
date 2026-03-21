using HarmonyLib;
using UnityEngine;

namespace ControlledMorale
{
    // Element recipe results have no prefab; use GetUISprite(object) to avoid NRE on side screen.
    [HarmonyPatch(typeof(Def), nameof(Def.GetUISprite), new[] { typeof(Tag), typeof(string) })]
    public static class DefGetUISpriteTagFacadePatch
    {
        public static bool Prefix(Tag prefabID, string facadeID, ref global::Tuple<Sprite, Color> __result)
        {
            GameObject prefab = Assets.GetPrefab(prefabID);
            if (prefab != null
                && prefab.GetComponent<Equippable>() != null
                && !string.IsNullOrWhiteSpace(facadeID))
            {
                __result = Db.GetEquippableFacades().Get(facadeID).GetUISprite();
                return false;
            }

            __result = Def.GetUISprite(prefabID, "ui", false);
            return false;
        }
    }
}
