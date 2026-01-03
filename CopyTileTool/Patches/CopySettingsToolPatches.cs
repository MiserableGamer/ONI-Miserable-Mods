using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using CopyTileTool.Logic;

namespace CopyTileTool.Patches
{
    [HarmonyPatch(typeof(CopySettingsTool))]
    public static class CopySettingsToolPatches
    {
        private static HashSet<int> processedCells = new HashSet<int>();
        private static readonly FieldInfo sourceField = AccessTools.Field(typeof(CopySettingsTool), "sourceGameObject");

        public static bool isTileCopyMode = false;

        [HarmonyPrefix]
        [HarmonyPatch("OnDragTool")]
        public static bool Prefix(CopySettingsTool __instance, int cell)
        {
            if (!isTileCopyMode) return true;

            // Handle based on current state
            var state = CopyTileManager.CurrentState;

            if (state == ToolState.SelectingSource)
            {
                // User is clicking to select the source tile
                HandleSourceSelection(cell);
                return false;  // Don't run base OnDragTool
            }
            else if (state == ToolState.ReadyToDrag)
            {
                // User is dragging to apply replacements
                HandleDragReplacement(cell);
                return false;  // Don't run base OnDragTool
            }

            return true;
        }

        private static void HandleSourceSelection(int cell)
        {
            // Get tile at this cell - tiles use FoundationTile layer
            var obj = Grid.Objects[cell, (int)ObjectLayer.FoundationTile];
            if (obj == null)
            {
                CopyTileManager.Log($"No tile found at cell {cell} for source selection");
                return;
            }

            if (!CopyTileManager.IsTile(obj))
            {
                CopyTileManager.Log($"Object at cell {cell} is not a tile");
                return;
            }

            var building = obj.GetComponent<Building>();
            var pe = obj.GetComponent<PrimaryElement>();
            if (building == null || pe == null) return;

            // Reject if same tile type - use CopyMaterialsTool for that
            if (building.Def.PrefabID == CopyTileManager.GetDestinationDef()?.PrefabID)
            {
                // Clear everything and deactivate to prevent any further processing
                processedCells.Clear();
                sourceField.SetValue(CopySettingsTool.Instance, null);
                isTileCopyMode = false;
                CopyTileManager.ClearAll();
                
                Vector3 pos = obj.transform.position;
                CopyTileManager.ShowPopup(CopyTileStrings.UI.COPY_TILE.USE_COPY_MATERIALS, pos);
                
                // Deactivate the tool
                PlayerController.Instance.ActivateTool(SelectTool.Instance);
                return;
            }

            // Set this as the source
            CopyTileManager.SetSource(building, pe.ElementID);

            Vector3 pos2 = obj.transform.position;
            CopyTileManager.ShowPopup(CopyTileStrings.UI.COPY_TILE.SOURCE_SELECTED, pos2);
        }

        private static void HandleDragReplacement(int cell)
        {
            // Skip if already processed this cell
            if (!processedCells.Add(cell)) return;

            // Safety check - source must be set
            if (CopyTileManager.GetSourceDef() == null) return;

            // Safety check - source type must be different from destination type
            // (same type changes should use CopyMaterialsTool)
            if (CopyTileManager.GetSourceDef()?.PrefabID == CopyTileManager.GetDestinationDef()?.PrefabID)
            {
                return;
            }

            // Get tile at this cell
            var obj = Grid.Objects[cell, (int)ObjectLayer.FoundationTile];
            if (obj == null)
            {
                CopyTileManager.Log($"No tile found at cell {cell}");
                return;
            }

            var building = obj.GetComponent<Building>();
            var pe = obj.GetComponent<PrimaryElement>();
            if (building == null || pe == null) return;

            // Check if this tile matches our source criteria
            if (!CopyTileManager.MatchesSource(building, pe))
            {
                CopyTileManager.Log($"Tile at cell {cell} doesn't match source: " +
                    $"expected {CopyTileManager.GetSourceDef()?.PrefabID}/{CopyTileManager.GetSourceMaterial()}, " +
                    $"got {building.Def.PrefabID}/{pe.ElementID}");
                return;
            }

            // Check if destination and source are the same
            var destDef = CopyTileManager.GetDestinationDef();
            var destMaterial = CopyTileManager.GetDestinationMaterial();
            
            if (building.Def.PrefabID == destDef?.PrefabID && pe.ElementID == destMaterial)
            {
                CopyTileManager.Log($"Tile at cell {cell} already matches destination, skipping");
                return;
            }

            CopyTileManager.Log($"Processing tile at cell {cell}: {building.Def.PrefabID}/{pe.ElementID} -> {destDef?.PrefabID}/{destMaterial}");

            // Queue deconstruction
            var deconstructable = obj.GetComponent<Deconstructable>();
            if (deconstructable != null)
            {
                deconstructable.QueueDeconstruction(true);

                // Get orientation
                var orientation = obj.GetComponent<Rotatable>()?.GetOrientation() ?? Orientation.Neutral;

                // Attach watcher to queue reconstruction after deconstruction
                TileReplacementWatcher.Attach(
                    building,
                    cell,
                    destDef,
                    destMaterial,
                    orientation,
                    CopyTileManager.GetDestinationPriority()
                );

                Vector3 pos = obj.transform.position;
                CopyTileManager.ShowPopup(CopyTileStrings.UI.COPY_TILE.REPLACEMENT_QUEUED, pos);
            }
            else
            {
                CopyTileManager.Warn($"No Deconstructable component on tile at cell {cell}");
            }
        }

        [HarmonyPatch("OnDeactivateTool")]
        [HarmonyPostfix]
        public static void OnDeactivateTool_Postfix(CopySettingsTool __instance)
        {
            if (isTileCopyMode)
            {
                processedCells.Clear();
                sourceField.SetValue(__instance, null);
                isTileCopyMode = false;
                CopyTileManager.ClearAll();
            }
        }

    }
}
