using HarmonyLib;
using UnityEngine;
using CopyMaterials.Logic;
using PeterHan.PLib.Core;

namespace CopyMaterials.Patches
{
    /// <summary>
    /// Patch BuildingComplete.OnSpawn to restore connections for conduits
    /// This catches all conduit types regardless of their specific class
    /// </summary>
    [HarmonyPatch(typeof(BuildingComplete), "OnSpawn")]
    public static class BuildingCompleteOnSpawnPatch
    {
        [HarmonyPostfix]
        static void Postfix(BuildingComplete __instance)
        {
            // Check if this building has IHaveUtilityNetworkMgr (conduits have this)
            var networkItem = __instance.GetComponent<IHaveUtilityNetworkMgr>();
            if (networkItem == null) return;

            int cell = Grid.PosToCell(__instance.transform.GetPosition());
            int layer = (int)__instance.Def.ObjectLayer;
            
            // Get stored connections from the manager's dictionary
            UtilityConnections storedConnections = CopyMaterialsManager.GetStoredConnections(cell, layer);
            
            if (storedConnections != (UtilityConnections)0)
            {
                string typeName = __instance.GetType().Name;
                CopyMaterialsManager.Log($"BuildingCompleteOnSpawnPatch: {typeName} at cell {cell}, layer {layer} - Restoring connections {storedConnections}");

                // Wait a bit for the network system to finish processing
                GameScheduler.Instance.Schedule($"CopyMaterialsBuildingCompleteConnectionRestore_{cell}", 0.25f, (obj) =>
                {
                    RestoreConnections(__instance, cell, layer, storedConnections);
                });
            }
        }

        private static void RestoreConnections(BuildingComplete building, int cell, int layer, UtilityConnections storedConnections)
        {
            var networkItem = building.GetComponent<IHaveUtilityNetworkMgr>();
            if (networkItem == null) return;

            var mgr = networkItem.GetNetworkManager();
            if (mgr == null) return;

            CopyMaterialsManager.Log($"BuildingCompleteOnSpawnPatch: Restoring connections {storedConnections} at cell {cell}");

            // Set connections
            mgr.SetConnections(storedConnections, cell, false);

            // Update neighbors
            UpdateNeighborConnections(mgr, cell, storedConnections);

            // Refresh neighbors
            RefreshNeighborConduits(cell, layer);

            // Force another rebuild to apply the connections
            var forceRebuildMethod = mgr.GetType().GetMethod("ForceRebuildNetworks", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (forceRebuildMethod != null)
            {
                try
                {
                    forceRebuildMethod.Invoke(mgr, null);
                    CopyMaterialsManager.Log("BuildingCompleteOnSpawnPatch: Called ForceRebuildNetworks()");
                }
                catch (System.Exception e)
                {
                    CopyMaterialsManager.Warn($"BuildingCompleteOnSpawnPatch: Error calling ForceRebuildNetworks(): {e.Message}");
                }
            }

            // Update visualizer
            if (building.TryGetComponent<KAnimGraphTileVisualizer>(out var viz))
            {
                UtilityConnections currentConnections = mgr.GetConnections(cell, false);
                var updateMethod = viz.GetType().GetMethod("UpdateConnections", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (updateMethod != null)
                {
                    updateMethod.Invoke(viz, new object[] { currentConnections });
                }
                viz.Refresh();
            }

            // Verify and clear stored connections
            UtilityConnections actualConnections = mgr.GetConnections(cell, false);
            CopyMaterialsManager.Log($"BuildingCompleteOnSpawnPatch: Final connections at cell {cell}: {actualConnections} (expected: {storedConnections})");
            
            // Clear the stored connections after restoration
            CopyMaterialsManager.ClearStoredConnections(cell, layer);
        }

        private static void UpdateNeighborConnections(IUtilityNetworkMgr mgr, int cell, UtilityConnections connections)
        {
            if ((connections & UtilityConnections.Left) != 0)
            {
                int neighborCell = Grid.OffsetCell(cell, new CellOffset(-1, 0));
                if (Grid.IsValidCell(neighborCell))
                {
                    UtilityConnections current = mgr.GetConnections(neighborCell, false);
                    UtilityConnections updated = current | UtilityConnections.Right;
                    if (updated != current)
                    {
                        mgr.SetConnections(updated, neighborCell, false);
                    }
                }
            }

            if ((connections & UtilityConnections.Right) != 0)
            {
                int neighborCell = Grid.OffsetCell(cell, new CellOffset(1, 0));
                if (Grid.IsValidCell(neighborCell))
                {
                    UtilityConnections current = mgr.GetConnections(neighborCell, false);
                    UtilityConnections updated = current | UtilityConnections.Left;
                    if (updated != current)
                    {
                        mgr.SetConnections(updated, neighborCell, false);
                    }
                }
            }

            if ((connections & UtilityConnections.Up) != 0)
            {
                int neighborCell = Grid.OffsetCell(cell, new CellOffset(0, 1));
                if (Grid.IsValidCell(neighborCell))
                {
                    UtilityConnections current = mgr.GetConnections(neighborCell, false);
                    UtilityConnections updated = current | UtilityConnections.Down;
                    if (updated != current)
                    {
                        mgr.SetConnections(updated, neighborCell, false);
                    }
                }
            }

            if ((connections & UtilityConnections.Down) != 0)
            {
                int neighborCell = Grid.OffsetCell(cell, new CellOffset(0, -1));
                if (Grid.IsValidCell(neighborCell))
                {
                    UtilityConnections current = mgr.GetConnections(neighborCell, false);
                    UtilityConnections updated = current | UtilityConnections.Up;
                    if (updated != current)
                    {
                        mgr.SetConnections(updated, neighborCell, false);
                    }
                }
            }
        }

        private static void RefreshNeighborConduits(int cell, int layer)
        {
            CellOffset[] offsets = new CellOffset[]
            {
                new CellOffset(-1, 0), // Left
                new CellOffset(1, 0),  // Right
                new CellOffset(0, 1),   // Up
                new CellOffset(0, -1)   // Down
            };

            foreach (CellOffset offset in offsets)
            {
                int neighborCell = Grid.OffsetCell(cell, offset);
                if (Grid.IsValidCell(neighborCell))
                {
                    GameObject neighborGO = Grid.Objects[neighborCell, layer];
                    if (neighborGO != null)
                    {
                        // Call Connect() on neighbor to recalculate connections
                        var neighborConnectMethod = neighborGO.GetType().GetMethod("Connect", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                        if (neighborConnectMethod != null)
                        {
                            try
                            {
                                neighborConnectMethod.Invoke(neighborGO, null);
                            }
                            catch (System.Exception e)
                            {
                                CopyMaterialsManager.Warn($"BuildingCompleteOnSpawnPatch: Error calling Connect() on neighbor: {e.Message}");
                            }
                        }

                        // Update visualizer for neighbor
                        var neighborNetworkItem = neighborGO.GetComponent<IHaveUtilityNetworkMgr>();
                        var neighborMgr = neighborNetworkItem?.GetNetworkManager();
                        if (neighborMgr != null && neighborGO.TryGetComponent<KAnimGraphTileVisualizer>(out var neighborViz))
                        {
                            UtilityConnections neighborConnections = neighborMgr.GetConnections(neighborCell, false);
                            var updateMethod = neighborViz.GetType().GetMethod("UpdateConnections", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                            if (updateMethod != null)
                            {
                                updateMethod.Invoke(neighborViz, new object[] { neighborConnections });
                            }
                            neighborViz.Refresh();
                        }
                    }
                }
            }
        }
    }
}

