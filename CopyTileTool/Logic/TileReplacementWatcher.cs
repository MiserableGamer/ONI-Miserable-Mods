using UnityEngine;

namespace CopyTileTool.Logic
{
    // Watches a single cell for tile removal, then places the replacement blueprint
    public class TileReplacementWatcher : KMonoBehaviour
    {
        private int cell;
        private BuildingDef destinationDef;
        private SimHashes destinationMaterial;
        private Orientation orientation;
        private PrioritySetting priority;
        private bool blueprintCreated;

        public static TileReplacementWatcher Attach(int cell, BuildingDef destDef, SimHashes destMaterial, Orientation orientation, PrioritySetting priority)
        {
            // Create a persistent root object if needed
            GameObject root = Game.Instance?.gameObject ?? GameObject.Find("CopyTileRoot");
            if (root == null)
            {
                root = new GameObject("CopyTileRoot");
                Object.DontDestroyOnLoad(root);
            }

            var watcher = root.AddComponent<TileReplacementWatcher>();
            watcher.cell = cell;
            watcher.destinationDef = destDef;
            watcher.destinationMaterial = destMaterial;
            watcher.orientation = orientation;
            watcher.priority = priority;
            watcher.blueprintCreated = false;

            CopyTileManager.Log($"Watcher attached for cell {cell}: {destDef.PrefabID} with {destMaterial}");
            return watcher;
        }

        private void Update()
        {
            if (blueprintCreated)
            {
                Destroy(this);
                return;
            }

            // Check if the cell is empty (tile has been deconstructed)
            if (!IsCellEmpty())
            {
                return;
            }

            // Cell is empty - try to place blueprint
            if (TryCreateBlueprint())
            {
                blueprintCreated = true;
            }
        }

        private bool IsCellEmpty()
        {
            // Check the FoundationTile layer for tiles
            GameObject obj = Grid.Objects[cell, (int)ObjectLayer.FoundationTile];
            if (obj != null)
            {
                // Something exists at this cell
                var building = obj.GetComponent<Building>();
                if (building != null)
                {
                    // Check if it's a Constructable (blueprint) we created
                    var constructable = obj.GetComponent<Constructable>();
                    if (constructable != null)
                    {
                        // There's a blueprint - our work here is done
                        CopyTileManager.Log($"Blueprint already exists at cell {cell}");
                        blueprintCreated = true;
                        return false;
                    }

                    // Building still exists - not empty yet
                    return false;
                }
            }

            // Cell appears to be empty
            return true;
        }

        private bool TryCreateBlueprint()
        {
            if (destinationDef == null)
            {
                CopyTileManager.Warn($"Cannot create blueprint - destinationDef is null");
                return true; // Don't retry
            }

            var element = ElementLoader.FindElementByHash(destinationMaterial);
            if (element == null)
            {
                CopyTileManager.Warn($"Element not found for hash {destinationMaterial}");
                return true; // Don't retry
            }

            Vector3 worldPos = Grid.CellToPosCBC(cell, destinationDef.SceneLayer);
            var selectedElements = new Tag[] { element.tag };

            CopyTileManager.Log($"Attempting TryPlace at cell {cell}: {destinationDef.PrefabID} with {destinationMaterial}");

            var placed = destinationDef.TryPlace(null, worldPos, orientation, selectedElements, 0);
            if (placed != null)
            {
                // Apply priority
                var prioritizable = placed.GetComponent<Prioritizable>();
                if (prioritizable != null)
                {
                    prioritizable.SetMasterPriority(priority);
                }

                CopyTileManager.Log($"Blueprint successfully placed at cell {cell}");
                CopyTileManager.ShowPopup(CopyTileStrings.UI.COPY_TILE.REPLACEMENT_QUEUED, worldPos);
                return true;
            }
            else
            {
                // TryPlace failed - will retry next frame
                CopyTileManager.Log($"TryPlace failed for cell {cell}, will retry");
                return false;
            }
        }
    }
}
