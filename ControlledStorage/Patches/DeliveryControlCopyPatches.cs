using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ControlledStorage.Patches
{
    public static class DeliveryControlCopyPatches
    {
        public static bool IsDeliveryControlCopyMode { get; private set; }

        private static GameObject _storedSource;
        private static readonly FieldInfo SourceField = AccessTools.Field(typeof(CopySettingsTool), "sourceGameObject");

        // Source and target must be same category: both storage bins, or both fridges
        private static bool AreCompatibleForDeliveryCopy(GameObject source, GameObject target)
        {
            if (source == null || target == null) return false;
            return (IsStorageBin(source) && IsStorageBin(target)) || (IsFridge(source) && IsFridge(target));
        }

        private static bool IsStorageBin(GameObject go) =>
            go.GetComponent<StorageLocker>() != null || go.GetComponent<StorageLockerSmart>() != null;

        private static bool IsFridge(GameObject go) =>
            go.GetComponent<Refrigerator>() != null || go.GetComponent<RationBox>() != null;

        public static void StartCopyDeliveryControl(GameObject sourceBuilding)
        {
            if (CopySettingsTool.Instance == null || sourceBuilding == null) return;
            _storedSource = sourceBuilding;
            if (SourceField != null)
                SourceField.SetValue(CopySettingsTool.Instance, sourceBuilding);
            IsDeliveryControlCopyMode = true;
        }

        // Skip vanilla OnDragTool, do our own copy of only the 4 delivery checkboxes (matches CopyMaterialsTool pattern)
        [HarmonyPatch(typeof(CopySettingsTool), "OnDragTool")]
        public static class CopySettingsTool_OnDragTool_Patch
        {
            private static readonly HashSet<Building> ProcessedBuildings = new HashSet<Building>();

            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableDeliveryControl;

            internal static bool Prefix() => !IsDeliveryControlCopyMode;

            internal static void ClearProcessed() => ProcessedBuildings.Clear();

            internal static void Postfix(CopySettingsTool __instance,
                [HarmonyArgument(0)] int cell,
                [HarmonyArgument(1)] int distFromOrigin)
            {
                if (!IsDeliveryControlCopyMode) return;
                var sourceGo = (GameObject)(SourceField != null ? SourceField.GetValue(__instance) : null);
                if (sourceGo == null) sourceGo = _storedSource;
                if (sourceGo == null) return;
                var src = sourceGo.GetComponent<StorageDeliveryControl>();
                if (src == null) return;
                var srcBuilding = sourceGo.GetComponent<Building>();
                if (srcBuilding == null) return;
                int layer = (int)srcBuilding.Def.ObjectLayer;
                var targetGo = Grid.Objects[cell, layer];
                if (targetGo == null) return;
                var targetBuilding = targetGo.GetComponent<Building>();
                if (targetBuilding == null || targetBuilding.gameObject == sourceGo) return;
                if (!AreCompatibleForDeliveryCopy(sourceGo, targetGo)) return;
                if (ProcessedBuildings.Contains(targetBuilding)) return;
                ProcessedBuildings.Add(targetBuilding);
                var dst = targetGo.GetComponent<StorageDeliveryControl>();
                if (dst == null && targetGo.GetComponent<Storage>() != null)
                    dst = targetGo.AddOrGet<StorageDeliveryControl>();
                if (dst != null)
                {
                    StorageDeliveryControl.CopyDeliveryControlOnly(src, dst);
                    var pos = Grid.CellToPosCCC(cell, Grid.SceneLayer.Building);
                    PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, "Delivery settings applied", null, pos, 2f, false, false);
                }
            }
        }

        [HarmonyPatch(typeof(CopySettingsTool), "OnDeactivateTool")]
        public static class CopySettingsTool_OnDeactivateTool_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableDeliveryControl;

            internal static void Postfix(CopySettingsTool __instance)
            {
                CopySettingsTool_OnDragTool_Patch.ClearProcessed();
                _storedSource = null;
                IsDeliveryControlCopyMode = false;
                if (SourceField != null)
                    SourceField.SetValue(__instance, null);
            }
        }

        // Block CopyBuildingSettings.ApplyCopy when in delivery-only mode - otherwise other mods
        // (e.g. CopyMaterialsTool) or game code may call it and copy filters/priorities too
        [HarmonyPatch(typeof(CopyBuildingSettings), nameof(CopyBuildingSettings.ApplyCopy))]
        public static class CopyBuildingSettings_ApplyCopy_Patch
        {
            internal static bool Prepare() => ControlledStorageOptions.Instance.EnableDeliveryControl;

            internal static bool Prefix()
            {
                if (IsDeliveryControlCopyMode)
                    return false;
                return true;
            }
        }
    }
}
