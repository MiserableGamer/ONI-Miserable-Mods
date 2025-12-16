using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace CopyMaterials.Patches
{
    [HarmonyPatch(typeof(CopySettingsTool), "OnDragTool")]
    public static class CopySettingsTool_OnDragTool_Patch
    {
        private static HashSet<int> processedThisDrag = new HashSet<int>();

        public static void Postfix(int cell, int distFromOrigin, CopySettingsTool __instance)
        {
            if (__instance == null) return;

            GameObject targetObj = Grid.Objects[cell, (int)ObjectLayer.Building];
            if (targetObj == null) return;

            Building targetBuilding = targetObj.GetComponent<Building>();
            if (targetBuilding == null) return;

            var sourceBuilding = CopyMaterials.Logic.CopyMaterialsManager.GetSourceBuilding();
            if (sourceBuilding == null) return;

            // Skip source building
            if (targetBuilding == sourceBuilding)
            {
                Debug.Log($"[CopyMaterials] Skipped source building {targetBuilding.name}");
                return;
            }

            // Skip if prefab mismatch
            if (targetBuilding.Def.PrefabID != sourceBuilding.Def.PrefabID)
            {
                Debug.Log($"[CopyMaterials] Skipped {targetBuilding.name} (prefab mismatch: {targetBuilding.Def.PrefabID} vs {sourceBuilding.Def.PrefabID})");
                return;
            }

            // Only process once per building per drag
            int id = targetBuilding.GetInstanceID();
            if (!processedThisDrag.Add(id))
            {
                Debug.Log($"[CopyMaterials] Skipped {targetBuilding.name} (already processed this drag)");
                return;
            }

            // Apply materials once (ApplyTo handles popup)
            Debug.Log($"[CopyMaterials] Applying to {targetBuilding.name} (prefab {targetBuilding.Def.PrefabID})");
            CopyMaterials.Logic.CopyMaterialsManager.ApplyTo(targetBuilding);
        }

        [HarmonyPatch(typeof(CopySettingsTool), "OnActivateTool")]
        public static class CopySettingsTool_OnActivateTool_Patch
        {
            public static void Postfix()
            {
                processedThisDrag.Clear();
                Debug.Log("[CopyMaterials] Drag session started, cleared processed set");
            }
        }

        [HarmonyPatch(typeof(CopySettingsTool), "OnDeactivateTool")]
        public static class CopySettingsTool_OnDeactivateTool_Patch
        {
            public static void Postfix()
            {
                processedThisDrag.Clear();
                Debug.Log("[CopyMaterials] Drag session ended, cleared processed set");
            }
        }
    }
}