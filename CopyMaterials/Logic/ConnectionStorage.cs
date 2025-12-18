using UnityEngine;
using PeterHan.PLib.Core;

namespace CopyMaterials.Logic
{
    /// <summary>
    /// Component to store connection information that persists through deconstruction/rebuild
    /// </summary>
    public class ConnectionStorage : KMonoBehaviour
    {
        public UtilityConnections StoredConnections { get; set; }
        public int Cell { get; set; }
        public ObjectLayer Layer { get; set; }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            
            if (StoredConnections != (UtilityConnections)0)
            {
                CopyMaterialsManager.Log($"ConnectionStorage OnSpawn: Stored connections {StoredConnections} at cell {Cell}");
                
                // Try to set connections immediately - don't wait for BuildingComplete
                // The conduit patches will handle restoration when the conduit actually spawns
                // But we can also try to set connections on the Constructable if it has network access
                TrySetConnectionsOnConstructable();
            }
        }

        private void TrySetConnectionsOnConstructable()
        {
            // Check if this is a Constructable with network access
            var networkItem = GetComponent<IHaveUtilityNetworkMgr>();
            if (networkItem != null)
            {
                var mgr = networkItem.GetNetworkManager();
                if (mgr != null)
                {
                    CopyMaterialsManager.Log($"ConnectionStorage: Setting connections on Constructable at cell {Cell}");
                    mgr.SetConnections(StoredConnections, Cell, false);
                    UpdateNeighborConnections(mgr, Cell, StoredConnections);
                }
            }
        }


        private void RestoreConnections()
        {
            var networkItem = GetComponent<IHaveUtilityNetworkMgr>();
            if (networkItem == null)
            {
                CopyMaterialsManager.Warn("ConnectionStorage: No IHaveUtilityNetworkMgr found");
                return;
            }

            var mgr = networkItem.GetNetworkManager();
            if (mgr == null)
            {
                CopyMaterialsManager.Warn("ConnectionStorage: No network manager found");
                return;
            }

            CopyMaterialsManager.Log($"ConnectionStorage: Setting connections {StoredConnections} via network manager at cell {Cell}");
            
            // Set connections on this cell
            mgr.SetConnections(StoredConnections, Cell, false);
            
            // Also update neighbors - connections are bidirectional
            UpdateNeighborConnections(mgr, Cell, StoredConnections);
            
            // Force network rebuild to apply the connections
            // This is similar to what Wire.Connect() does
            var forceRebuildMethod = mgr.GetType().GetMethod("ForceRebuildNetworks", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (forceRebuildMethod != null)
            {
                try
                {
                    forceRebuildMethod.Invoke(mgr, null);
                    CopyMaterialsManager.Log("ConnectionStorage: Called ForceRebuildNetworks()");
                }
                catch (System.Exception e)
                {
                    CopyMaterialsManager.Warn($"ConnectionStorage: Error calling ForceRebuildNetworks(): {e.Message}");
                }
            }

            // Call Connect() on the building component if it exists
            var building = gameObject.GetComponent<Building>();
            if (building != null)
            {
                var buildingType = building.GetType();
                var connectMethod = buildingType.GetMethod("Connect", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (connectMethod != null)
                {
                    try
                    {
                        connectMethod.Invoke(building, null);
                        CopyMaterialsManager.Log("ConnectionStorage: Called Connect() method on building");
                    }
                    catch (System.Exception e)
                    {
                        CopyMaterialsManager.Warn($"ConnectionStorage: Error calling Connect(): {e.Message}");
                    }
                }
            }

            // Update visualizer
            if (TryGetComponent<KAnimGraphTileVisualizer>(out var viz))
            {
                UtilityConnections currentConnections = mgr.GetConnections(Cell, false);
                var updateMethod = viz.GetType().GetMethod("UpdateConnections", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (updateMethod != null)
                {
                    updateMethod.Invoke(viz, new object[] { currentConnections });
                }
                viz.Refresh();
            }

            // Schedule a delayed refresh to ensure neighbors are updated
            GameScheduler.Instance.Schedule("CopyMaterialsConnectionRefresh", 0.1f, (obj) =>
            {
                RefreshNeighborsAndVerify();
            });
        }

        private void RefreshNeighborsAndVerify()
        {
            var networkItem = GetComponent<IHaveUtilityNetworkMgr>();
            var mgr = networkItem?.GetNetworkManager();
            
            if (mgr != null)
            {
                UtilityConnections actualConnections = mgr.GetConnections(Cell, false);
                CopyMaterialsManager.Log($"ConnectionStorage: Final connections at cell {Cell}: {actualConnections} (expected: {StoredConnections})");
                
                if (actualConnections != StoredConnections && StoredConnections != (UtilityConnections)0)
                {
                    CopyMaterialsManager.Warn($"ConnectionStorage: Connections mismatch! Retrying...");
                    mgr.SetConnections(StoredConnections, Cell, false);
                    
                    var building = gameObject.GetComponent<Building>();
                    if (building != null)
                    {
                        var buildingType = building.GetType();
                        var connectMethod = buildingType.GetMethod("Connect", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                        connectMethod?.Invoke(building, null);
                    }
                }
            }

            // Refresh neighbors
            ConstructableCleanup.RefreshNeighbors(Cell, (int)Layer);
        }

        private void UpdateNeighborConnections(IUtilityNetworkMgr mgr, int cell, UtilityConnections connections)
        {
            // Update neighbors based on our connections
            // If we connect left, the left neighbor should connect right, etc.
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
                        CopyMaterialsManager.Log($"ConnectionStorage: Updated left neighbor at cell {neighborCell}");
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
                        CopyMaterialsManager.Log($"ConnectionStorage: Updated right neighbor at cell {neighborCell}");
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
                        CopyMaterialsManager.Log($"ConnectionStorage: Updated up neighbor at cell {neighborCell}");
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
                        CopyMaterialsManager.Log($"ConnectionStorage: Updated down neighbor at cell {neighborCell}");
                    }
                }
            }
        }
    }
}

