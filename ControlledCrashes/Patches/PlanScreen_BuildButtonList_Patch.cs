using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ControlledCrashes.Patches
{
    // PlanScreen.BuildButtonList() does pooledDictionary.Add(buildingDef.PrefabID, planInfo.category).
    // When a building appears in PLANORDER more than once (same category or different), the second Add throws.
    // Deduplicate PLANORDER so each PrefabID appears at most once (first occurrence wins).
    [HarmonyPatch(typeof(PlanScreen), "BuildButtonList")]
    public static class PlanScreen_BuildButtonList_Patch
    {
        public static void Prefix()
        {
            var planOrder = TUNING.BUILDINGS.PLANORDER;
            if (planOrder == null) return;

            var seenIds = new HashSet<string>();
            int totalRemoved = 0;
            for (int i = 0; i < planOrder.Count; i++)
            {
                var planInfo = planOrder[i];
                var data = planInfo.buildingAndSubcategoryData;
                if (data == null) continue;

                var deduped = new List<KeyValuePair<string, string>>(data.Count);
                foreach (var kvp in data)
                {
                    if (kvp.Key == null || seenIds.Contains(kvp.Key)) continue;
                    seenIds.Add(kvp.Key);
                    deduped.Add(kvp);
                }
                if (deduped.Count != data.Count)
                {
                    totalRemoved += data.Count - deduped.Count;
                    planInfo.buildingAndSubcategoryData = deduped;
                    planOrder[i] = planInfo;
                }
            }
            if (totalRemoved > 0)
            {
                Debug.LogWarning("[ControlledCrashes] Removed " + totalRemoved +
                    " duplicate building(s) from plan screen to prevent crash (same building in multiple categories or listed twice).");
            }
        }
    }
}
