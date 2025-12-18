using System;
using System.Collections.Generic;
using UnityEngine;

namespace CopyMaterials.Logic
{
    public class CopyMaterialsWatcher : KMonoBehaviour
    {
        private int originalCell;
        private string prefabID;
        private SimHashes material;
        private bool blueprintCreated;
        private Orientation orientation = Orientation.Neutral;
        private ObjectLayer objectLayer;
        private UtilityConnections storedConnections;
        private int? bridgeWidth = null; // Store bridge width for ExtendedBuildingWidth support

        public static CopyMaterialsWatcher Attach(Building target, string prefabID, SimHashes material, Orientation orientation = Orientation.Neutral, UtilityConnections connections = default(UtilityConnections))
        {
            if (target == null) return null;

            GameObject root = Game.Instance?.gameObject ?? GameObject.Find("CopyMaterialsRoot");
            if (root == null)
            {
                root = new GameObject("CopyMaterialsRoot");
                UnityEngine.Object.DontDestroyOnLoad(root);
            }

            var watcher = root.AddComponent<CopyMaterialsWatcher>();
            watcher.originalCell = Grid.PosToCell(target);
            watcher.prefabID = prefabID;
            watcher.material = material;
            watcher.orientation = orientation;
            watcher.objectLayer = target.Def.ObjectLayer;
            watcher.storedConnections = connections;
            watcher.bridgeWidth = CopyMaterialsManager.sourceBridgeWidth; // Capture bridge width from source
            watcher.blueprintCreated = false;

            CopyMaterialsManager.Log($"Watcher attached for {prefabID} at cell {watcher.originalCell}, bridge width: {watcher.bridgeWidth}");
            return watcher;
        }

        private void Update()
        {
            if (blueprintCreated)
            {
                Destroy(this);
                return;
            }

            // Check if the cell is completely empty - no building, no blueprint, nothing
            if (!IsCellCompletelyEmpty())
            {
                // Cell is not empty, wait
                return;
            }

            // Cell is empty - try to place blueprint
            // If placement fails (e.g., port overlap), we'll retry next frame
            if (TryCreateBlueprint())
            {
                blueprintCreated = true;
            }
        }

        private bool IsCellCompletelyEmpty()
        {
            var def = Assets.GetBuildingDef(prefabID);
            if (def == null) return false;

            // For bridges (especially extended bridges), we need to check ALL cells the building occupies
            // not just the origin cell
            bool allCellsEmpty = true;
            HashSet<int> cellsToCheck = new HashSet<int>();

            // Get all cells this building occupies
            def.RunOnArea(originalCell, orientation, (int cell) =>
            {
                cellsToCheck.Add(cell);
            });

            // If RunOnArea didn't work or returned no cells, at least check the origin cell
            if (cellsToCheck.Count == 0)
            {
                cellsToCheck.Add(originalCell);
            }

            // Check each cell the building will occupy
            foreach (int cell in cellsToCheck)
            {
                if (!IsSingleCellEmpty(cell))
                {
                    allCellsEmpty = false;
                    break;
                }
            }

            return allCellsEmpty;
        }

        private bool IsSingleCellEmpty(int cell)
        {
            // Check if there's any object in the grid at this cell and layer
            GameObject obj = Grid.Objects[cell, (int)objectLayer];
            if (obj != null)
            {
                // Something exists at this cell
                var building = obj.GetComponent<Building>();
                if (building != null)
                {
                    // Check if it's the original building (same prefab ID)
                    if (building.Def.PrefabID == prefabID)
                    {
                        // Original building still exists - check if it's being deconstructed
                        var deconstructable = obj.GetComponent<Deconstructable>();
                        if (deconstructable != null)
                        {
                            // Building is being deconstructed but still exists - not empty yet
                            return false;
                        }
                        
                        // Building exists and not being deconstructed - not empty
                        return false;
                    }
                }
                
                // There's something else at this cell (different building) - not empty
                return false;
            }

            // Grid.Objects shows nothing, but check if there's a Constructable (blueprint) at this cell
            // We need to check the Constructable layer specifically
            int constructableLayer = (int)ObjectLayer.Building;
            GameObject constructableObj = Grid.Objects[cell, constructableLayer];
            if (constructableObj != null)
            {
                var constructable = constructableObj.GetComponent<Constructable>();
                if (constructable != null)
                {
                    var constructableBuilding = constructableObj.GetComponent<Building>();
                    if (constructableBuilding != null && constructableBuilding.Def.PrefabID == prefabID)
                    {
                        // There's already a blueprint of the same type at this cell
                        return false;
                    }
                }
            }

            // Cell appears to be completely empty
            return true;
        }

        private bool TryCreateBlueprint()
        {
            var def = Assets.GetBuildingDef(prefabID);
            if (def == null)
            {
                CopyMaterialsManager.Warn($"BuildingDef not found for {prefabID}");
                return true; // Don't retry if def not found
            }

            // Check if ALL cells the building will occupy are clear
            // This is especially important for extended bridges that span multiple cells
            HashSet<int> cellsToCheck = new HashSet<int>();
            def.RunOnArea(originalCell, orientation, (int cell) =>
            {
                cellsToCheck.Add(cell);
            });

            // If RunOnArea didn't work, at least check the origin cell
            if (cellsToCheck.Count == 0)
            {
                cellsToCheck.Add(originalCell);
            }

            // Check each cell
            foreach (int cell in cellsToCheck)
            {
                if (Grid.Objects[cell, (int)objectLayer] != null)
                {
                    return false; // At least one cell is not clear, retry later
                }
            }

            Vector3 worldPos = Grid.CellToPosCBC(originalCell, def.SceneLayer);
            IList<Tag> selectedElements = PlacementHelpers.BuildSelectedElementsFromMaterial(material);

            // Try to place the blueprint - if it fails (e.g., port overlap for bridges), we'll retry next frame
            var placed = def.TryPlace(null, worldPos, orientation, selectedElements, null, 0);
            if (placed != null)
            {
                CopyMaterialsManager.Log($"Blueprint successfully placed at cell {originalCell}");
                
                // Apply bridge width if this is a bridge (for ExtendedBuildingWidth support)
                if (bridgeWidth.HasValue && IsBridge(prefabID))
                {
                    // Try to apply width to the Constructable (blueprint)
                    if (CopyMaterialsManager.ApplyBridgeWidth(placed, bridgeWidth))
                    {
                        CopyMaterialsManager.Log($"Applied bridge width {bridgeWidth.Value} to blueprint");
                    }
                    else
                    {
                        // If we can't apply to Constructable, store it for later application in ConstructableCleanup
                        CopyMaterialsManager.Log($"Could not apply bridge width to Constructable, will try on BuildingComplete");
                    }
                }
                
                // Connections are stored in CopyMaterialsManager.storedConnectionsMap
                // No need to attach ConnectionStorage component

                ConstructableCleanup.Attach(
                    placed,
                    def,
                    originalCell,
                    orientation,
                    null,
                    material,
                    storedConnections,
                    bridgeWidth  // Pass bridge width to cleanup
                );
                CopyMaterialsManager.Log("Blueprint placed and cleanup attached");
                
                ConstructableCleanup.RefreshNeighbors(originalCell, (int)objectLayer);
                return true;
            }
            else
            {
                // TryPlace failed - might be port overlap (for bridges) or other issue
                // Will retry next frame until it succeeds
                return false;
            }
        }

        private static bool IsBridge(string prefabID)
        {
            // Check if the prefab ID contains "Bridge"
            return prefabID != null && prefabID.Contains("Bridge");
        }

    }
}