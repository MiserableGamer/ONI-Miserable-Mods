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
            CopyTileManager.Log($"OnDragTool Prefix: isTileCopyMode={isTileCopyMode}, state={CopyTileManager.CurrentState}, cell={cell}");
            
            if (!isTileCopyMode) return true;

            // Handle based on current state
            var state = CopyTileManager.CurrentState;

            if (state == ToolState.SelectingSource)
            {
                // User is clicking to select the source tile
                CopyTileManager.Log("State is SelectingSource, calling HandleSourceSelection");
                HandleSourceSelection(cell);
                return false;  // Don't run base OnDragTool
            }
            else if (state == ToolState.ReadyToDrag)
            {
                // User is dragging to apply replacements
                CopyTileManager.Log("State is ReadyToDrag, calling HandleDragReplacement");
                HandleDragReplacement(cell);
                return false;  // Don't run base OnDragTool
            }

            CopyTileManager.Log($"State is {state}, falling through to base");
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

            // Queue build directly over existing tile - no deconstruction needed
            // ONI allows building tiles over existing tiles and handles the replacement
            QueueTileBuild(cell, destDef, destMaterial, obj);
        }

        private static void QueueTileBuild(int cell, BuildingDef destDef, SimHashes destMaterial, GameObject existingTile)
        {
            CopyTileManager.Log($"QueueTileBuild called: cell={cell}, destDef={destDef?.PrefabID}, destMaterial={destMaterial}");
            
            try
            {
                // Get orientation from existing tile
                var orientation = existingTile.GetComponent<Rotatable>()?.GetOrientation() ?? Orientation.Neutral;
                
                // Get priority from destination tile
                var priority = CopyTileManager.GetDestinationPriority();

                // Queue deconstruction of the existing tile
                var deconstructable = existingTile.GetComponent<Deconstructable>();
                if (deconstructable != null)
                {
                    // Mark for deconstruction (true = user triggered)
                    deconstructable.QueueDeconstruction(true);
                    
                    // Apply priority to deconstruction
                    var prioritizable = existingTile.GetComponent<Prioritizable>();
                    if (prioritizable != null)
                    {
                        prioritizable.SetMasterPriority(priority);
                    }
                    
                    // Attach a watcher that will place the blueprint once the tile is gone
                    TileReplacementWatcher.Attach(cell, destDef, destMaterial, orientation, priority);
                    
                    Vector3 pos = existingTile.transform.position;
                    CopyTileManager.ShowPopup(CopyTileStrings.UI.COPY_TILE.TILE_COPIED, pos);
                    CopyTileManager.Log($"Queued deconstruction at cell {cell}, watcher will rebuild as {destDef.PrefabID} with {destMaterial}");
                }
                else
                {
                    CopyTileManager.Warn($"No Deconstructable component on tile at cell {cell}");
                }
            }
            catch (System.Exception e)
            {
                CopyTileManager.Warn($"Error queueing tile replacement: {e.Message}\n{e.StackTrace}");
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
