using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ControlledStorage.Patches
{
    /// <summary>
    /// Patches CopySettingsTool to support checkbox-only copy from the Delivery Control sidescreen.
    /// Reuses the vanilla Copy Settings tool - sets source and activates it; on drag, copies only StorageDeliveryControl.
    /// </summary>
    public static class DeliveryControlCopyPatches
    {
        public static bool IsDeliveryControlCopyMode { get; private set; }

        private static readonly HashSet<Building> ProcessedBuildings = new HashSet<Building>();
        private static readonly FieldInfo SourceField = AccessTools.Field(typeof(CopySettingsTool), "sourceGameObject");

        public static void StartCopyDeliveryControl(GameObject sourceBuilding)
        {
            if (CopySettingsTool.Instance == null || sourceBuilding == null) return;
            SourceField?.SetValue(CopySettingsTool.Instance, sourceBuilding);
            IsDeliveryControlCopyMode = true;
        }

        [HarmonyPatch(typeof(CopySettingsTool), "OnDragTool")]
        public static class CopySettingsTool_OnDragTool_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableDeliveryControl;

            internal static bool Prefix()
            {
                if (IsDeliveryControlCopyMode)
                    return false;
                return true;
            }

            internal static void Postfix(CopySettingsTool __instance, int cell, int distFromOrigin)
            {
                if (!IsDeliveryControlCopyMode) return;

                var sourceGO = (GameObject)SourceField?.GetValue(__instance);
                if (sourceGO == null) return;

                var sourceCtrl = sourceGO.GetComponent<StorageDeliveryControl>();
                if (sourceCtrl == null) return;

                var sourceBuilding = sourceGO.GetComponent<Building>();
                if (sourceBuilding == null) return;

                int layer = (int)sourceBuilding.Def.ObjectLayer;
                var obj = Grid.Objects[cell, layer];
                if (obj == null) return;

                var targetBuilding = obj.GetComponent<Building>();
                if (targetBuilding == null) return;

                if (targetBuilding.gameObject == sourceGO) return;
                if (targetBuilding.Def.PrefabID != sourceBuilding.Def.PrefabID) return;

                if (ProcessedBuildings.Contains(targetBuilding)) return;
                ProcessedBuildings.Add(targetBuilding);

                var targetCtrl = obj.GetComponent<StorageDeliveryControl>();
                if (targetCtrl != null)
                {
                    StorageDeliveryControl.CopyDeliveryControlOnly(sourceCtrl, targetCtrl);
                    var pos = Grid.CellToPosCCC(cell, Grid.SceneLayer.Building);
                    PopFXManager.Instance.SpawnFX(
                        PopFXManager.Instance.sprite_Plus,
                        "Delivery settings applied",
                        null,
                        pos,
                        2f
                    );
                }
            }
        }

        [HarmonyPatch(typeof(CopySettingsTool), "OnDeactivateTool")]
        public static class CopySettingsTool_OnDeactivateTool_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableDeliveryControl;

            internal static void Postfix(CopySettingsTool __instance)
            {
                ProcessedBuildings.Clear();
                IsDeliveryControlCopyMode = false;
                SourceField?.SetValue(__instance, null);
            }
        }
    }
}
