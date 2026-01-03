using UnityEngine;
using System.Collections.Generic;

namespace CopyTileTool.Logic
{
    // Watches for tile deconstruction and queues reconstruction with new type/material
    public class TileReplacementWatcher : KMonoBehaviour
    {
        private static Dictionary<int, TileReplacementWatcher> activeWatchers = new Dictionary<int, TileReplacementWatcher>();

        private int cell;
        private BuildingDef destinationDef;
        private SimHashes destinationMaterial;
        private Orientation orientation;
        private PrioritySetting priority;
        private float timeoutTimer;
        private const float TIMEOUT_SECONDS = 30f;

        public static void Attach(
            Building originalTile,
            int tileCell,
            BuildingDef destDef,
            SimHashes destMaterial,
            Orientation tileOrientation,
            PrioritySetting tilePriority)
        {
            if (originalTile == null || destDef == null) return;

            // Don't create duplicate watchers for the same cell
            if (activeWatchers.ContainsKey(tileCell))
            {
                CopyTileManager.Log($"Watcher already exists for cell {tileCell}, skipping");
                return;
            }

            var watcher = originalTile.gameObject.AddComponent<TileReplacementWatcher>();
            watcher.cell = tileCell;
            watcher.destinationDef = destDef;
            watcher.destinationMaterial = destMaterial;
            watcher.orientation = tileOrientation;
            watcher.priority = tilePriority;
            watcher.timeoutTimer = TIMEOUT_SECONDS;

            activeWatchers[tileCell] = watcher;

            CopyTileManager.Log($"Watcher attached for cell {tileCell}: {destDef.PrefabID} with {destMaterial}");
        }

        protected override void OnCleanUp()
        {
            base.OnCleanUp();

            // Remove from active watchers
            activeWatchers.Remove(cell);

            // Queue construction of the new tile
            QueueConstruction();
        }

        private void Update()
        {
            // Timeout check - in case the tile never gets deconstructed
            timeoutTimer -= Time.deltaTime;
            if (timeoutTimer <= 0)
            {
                CopyTileManager.Warn($"Watcher for cell {cell} timed out");
                activeWatchers.Remove(cell);
                Destroy(this);
            }
        }

        private void QueueConstruction()
        {
            if (destinationDef == null)
            {
                CopyTileManager.Warn($"Cannot queue construction - destinationDef is null");
                return;
            }

            CopyTileManager.Log($"Queueing construction at cell {cell}: {destinationDef.PrefabID} with {destinationMaterial}");

            try
            {
                // Get the position from the cell
                Vector3 position = Grid.CellToPosCBC(cell, destinationDef.SceneLayer);

                // Create the build order
                var selectedElements = new Tag[] { ElementLoader.FindElementByHash(destinationMaterial).tag };

                // Use the game's building placement system
                if (destinationDef.IsValidPlaceLocation(null, cell, orientation, out string _))
                {
                    // Queue the build order
                    var buildOrder = destinationDef.TryPlace(
                        null,
                        position,
                        orientation,
                        selectedElements,
                        0  // facadeIndex
                    );

                    if (buildOrder != null)
                    {
                        // Apply priority
                        var prioritizable = buildOrder.GetComponent<Prioritizable>();
                        if (prioritizable != null)
                        {
                            prioritizable.SetMasterPriority(priority);
                        }

                        CopyTileManager.Log($"Build order created successfully at cell {cell}");
                    }
                    else
                    {
                        CopyTileManager.Warn($"TryPlace returned null for cell {cell}");
                    }
                }
                else
                {
                    CopyTileManager.Warn($"Invalid placement location for cell {cell}");
                }
            }
            catch (System.Exception e)
            {
                CopyTileManager.Warn($"Error queueing construction: {e.Message}\n{e.StackTrace}");
            }
        }

        public static void ClearAllWatchers()
        {
            foreach (var watcher in activeWatchers.Values)
            {
                if (watcher != null)
                {
                    Destroy(watcher);
                }
            }
            activeWatchers.Clear();
        }

        public static int GetActiveWatcherCount() => activeWatchers.Count;
    }
}

