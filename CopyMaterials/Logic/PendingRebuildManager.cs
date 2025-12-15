using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CopyMaterialsTool
{
    internal static class PendingRebuildManager
    {
        internal static bool DEBUG_LOGS = false;

        internal struct Pending
        {
            public string prefabId;
            public int cell;
            public Tag[] materialTags;
            public object orientationEnum;
        }

        private static readonly Dictionary<int, Pending> pendingByInstanceId = new Dictionary<int, Pending>();

        internal static void Queue(int instanceId, Pending p)
        {
            pendingByInstanceId[instanceId] = p;
            Log("Queued pending rebuild instanceId=" + instanceId + " prefabId=" + p.prefabId + " cell=" + p.cell);
        }

        private static bool TryConsume(int instanceId, out Pending p)
        {
            return pendingByInstanceId.TryGetValue(instanceId, out p) && pendingByInstanceId.Remove(instanceId);
        }

        private static void Log(string msg)
        {
            if (DEBUG_LOGS)
                Debug.Log("[CopyMaterialsTool] " + msg);
        }

        [HarmonyPatch(typeof(BuildingComplete), "OnCleanUp")]
        private static class BuildingComplete_OnCleanUp_PlacePending_Patch
        {
            private static void Postfix(BuildingComplete __instance)
            {
                if (__instance == null)
                    return;

                int id = __instance.gameObject.GetInstanceID();

                Pending pending;
                if (!TryConsume(id, out pending))
                    return;

                bool placed = RebuildApply.TryPlaceReplacementPlan(
                    pending.prefabId,
                    pending.cell,
                    pending.materialTags,
                    pending.orientationEnum,
                    DEBUG_LOGS
                );

                if (DEBUG_LOGS)
                    Debug.Log("[CopyMaterialsTool] OnCleanUp placed=" + placed + " prefabId=" + pending.prefabId + " cell=" + pending.cell);
            }
        }
    }
}
