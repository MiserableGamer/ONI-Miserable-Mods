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
            public int[] elements;
            public object orientationEnum;
        }

        private static readonly Dictionary<int, Pending> pendingByInstanceId = new Dictionary<int, Pending>();

        internal static void Queue(int instanceId, Pending p)
        {
            pendingByInstanceId[instanceId] = p;
            Log("Queued pending rebuild instanceId=" + instanceId + " prefabId=" + p.prefabId + " cell=" + p.cell);
        }

        internal static bool TryConsume(int instanceId, out Pending p)
        {
            if (pendingByInstanceId.TryGetValue(instanceId, out p))
            {
                pendingByInstanceId.Remove(instanceId);
                return true;
            }
            return false;
        }

        private static void Log(string msg)
        {
            if (DEBUG_LOGS)
                Debug.Log("[CopyMaterialsTool] " + msg);
        }
    }

    [HarmonyPatch(typeof(BuildingComplete), "OnCleanUp")]
    internal static class BuildingComplete_OnCleanUp_PlacePending_Patch
    {
        private static void Postfix(BuildingComplete __instance)
        {
            if (__instance == null) return;

            int id = __instance.gameObject.GetInstanceID();
            if (!PendingRebuildManager.TryConsume(id, out var pending))
                return;

            bool placed = RebuildApply.TryPlacePlanByPrefabId(
                pending.prefabId,
                pending.cell,
                pending.elements,
                pending.orientationEnum,
                PendingRebuildManager.DEBUG_LOGS
            );

            if (PendingRebuildManager.DEBUG_LOGS)
                Debug.Log("[CopyMaterialsTool] OnCleanUp placed=" + placed + " prefabId=" + pending.prefabId + " cell=" + pending.cell);
        }
    }
}
