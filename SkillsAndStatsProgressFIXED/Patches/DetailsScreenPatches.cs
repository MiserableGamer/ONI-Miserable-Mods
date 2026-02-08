using HarmonyLib;
using UnityEngine;

namespace SkillsAndStatsProgressFIXED
{
    [HarmonyPatch(typeof(DetailsScreen), "Refresh")]
    public class DetailsScreenPatches
    {
        public static void Prefix(GameObject go)
        {
            if (!Config.Cfg.AlterSortOrder)
                return;

            if (go.GetComponent<MinionIdentity>() != null && DetailsScreen.Instance.previouslyActiveTab < 0)
            {
                DetailsScreen.Instance.previouslyActiveTab = 2;
            }
        }
    }
}
