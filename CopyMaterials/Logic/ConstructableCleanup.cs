using System;
using System.Reflection;
using UnityEngine;
using PeterHan.PLib.Core;

namespace CopyMaterials.Logic
{
    public class ConstructableCleanup : KMonoBehaviour
    {
        private BuildingDef def;
        private int originCell;
        private Orientation orientation;
        private ObjectLayer objectLayer;

        private SimHashes materialToApply = SimHashes.Vacuum;
        private UtilityConnections storedConnections;
        private int? bridgeWidth = null; // Store bridge width for ExtendedBuildingWidth support
        private bool cleanupDone = false;

        public static void Attach(
            GameObject placed,
            BuildingDef def,
            int originCell,
            Orientation orientation,
            GameObject visualizer,  // Ignored
            SimHashes materialToApply,
            UtilityConnections connections,
            int? bridgeWidth = null  // Bridge width for ExtendedBuildingWidth support
        )
        {
            var cleanup = placed.AddComponent<ConstructableCleanup>();
            cleanup.def = def;
            cleanup.originCell = originCell;
            cleanup.orientation = orientation;
            cleanup.objectLayer = def.ObjectLayer;
            cleanup.materialToApply = materialToApply;
            cleanup.storedConnections = connections;
            cleanup.bridgeWidth = bridgeWidth;
            cleanup.cleanupDone = false;
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            // Try cleanup immediately on spawn (for buildings that spawn complete)
            DoCleanup();
        }

        private void Update()
        {
            if (!cleanupDone)
            {
                DoCleanup();
            }
        }

        private void DoCleanup()
        {
            GameObject completeGO = Grid.Objects[originCell, (int)objectLayer];
            if (completeGO == null) return;

            var bc = completeGO.GetComponent<BuildingComplete>();
            if (bc == null || bc.Def.PrefabID != def.PrefabID) return;

            // Mark as done to prevent multiple calls
            cleanupDone = true;
            
            CopyMaterialsManager.Log($"DoCleanup: Building {def.PrefabID} completed at cell {originCell}");

            // Apply material first
            if (materialToApply != SimHashes.Vacuum)
            {
                CopyMaterialsManager.TrySetPrimaryElement(completeGO, materialToApply);
            }

            // Apply bridge width if this is a bridge (for ExtendedBuildingWidth support)
            if (bridgeWidth.HasValue && def.PrefabID.Contains("Bridge"))
            {
                if (CopyMaterialsManager.ApplyBridgeWidth(completeGO, bridgeWidth))
                {
                    CopyMaterialsManager.Log($"Applied bridge width {bridgeWidth.Value} to completed building");
                }
                else
                {
                    CopyMaterialsManager.Warn($"Failed to apply bridge width {bridgeWidth.Value} to completed building");
                }
            }

            // Check if ConnectionStorage exists (it should have been attached to the blueprint)
            var connectionStorage = completeGO.GetComponent<ConnectionStorage>();
            if (connectionStorage == null && storedConnections != (UtilityConnections)0)
            {
                // ConnectionStorage wasn't preserved, create it now
                CopyMaterialsManager.Log($"DoCleanup: ConnectionStorage missing, creating it now");
                connectionStorage = completeGO.AddComponent<ConnectionStorage>();
                connectionStorage.StoredConnections = storedConnections;
                connectionStorage.Cell = originCell;
                connectionStorage.Layer = objectLayer;
            }

            // Restore connections if stored (for conduits)
            // Note: The conduit patches will handle this via the dictionary, but we'll also try here as backup
            if (storedConnections != (UtilityConnections)0)
            {
                RestoreConnections(completeGO);
            }

            // Apply source settings
            CopyMaterialsManager.ApplyPriorityToObject(completeGO, CopyMaterialsManager.sourcePriority);

            if (!string.IsNullOrEmpty(CopyMaterialsManager.sourceFacadeID))
            {
                var facadeComp = completeGO.GetComponent<BuildingFacade>();
                if (facadeComp != null)
                {
                    var field = typeof(BuildingFacade).GetField("currentFacade", BindingFlags.Instance | BindingFlags.NonPublic);
                    field?.SetValue(facadeComp, CopyMaterialsManager.sourceFacadeID);
                }
            }

            if (CopyMaterialsManager.sourceCopyGroupTag != Tag.Invalid)
            {
                var cbs = completeGO.GetComponent<CopyBuildingSettings>();
                if (cbs != null)
                {
                    var field = typeof(CopyBuildingSettings).GetField("copyGroupTag", BindingFlags.Instance | BindingFlags.NonPublic);
                    field?.SetValue(cbs, CopyMaterialsManager.sourceCopyGroupTag);
                }
            }

            // Refresh visuals and neighbors after a short delay to ensure connections are processed
            GameScheduler.Instance.Schedule("CopyMaterialsRefresh", 0.1f, (obj) =>
            {
                RefreshBuildingAndNeighbors(completeGO);
            });

            // Existing popup
            Vector3 pos = Grid.CellToPosCCC(originCell, Grid.SceneLayer.Building);
            PopFXManager.Instance.SpawnFX(
                PopFXManager.Instance.sprite_Plus,
                "Materials Applied",
                null,
                pos,
                2f
            );

            Destroy(this);
        }

        private void RestoreConnections(GameObject buildingGO)
        {
            if (buildingGO == null) return;

            var networkItem = buildingGO.GetComponent<IHaveUtilityNetworkMgr>();
            if (networkItem == null) return;

            var mgr = networkItem.GetNetworkManager();
            if (mgr == null) return;

            CopyMaterialsManager.Log($"Restoring connections {storedConnections} for building {def.PrefabID} at cell {originCell}");

            // First, ensure neighbors are connected (this helps conduits establish proper connections)
            RefreshNeighbors(originCell, (int)objectLayer);

            // For conduits, we need to set connections through the network manager
            // The manager's SetConnections method should handle the connection state
            mgr.SetConnections(storedConnections, originCell, false);

            // Also try calling Connect() on the building component itself if it exists
            // This is important for conduits to properly establish their connection state
            var connectMethod = buildingGO.GetType().GetMethod("Connect", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (connectMethod != null)
            {
                try
                {
                    connectMethod.Invoke(buildingGO, null);
                    CopyMaterialsManager.Log("Called Connect() on building component");
                }
                catch (Exception e)
                {
                    CopyMaterialsManager.Warn($"Error calling Connect(): {e.Message}");
                }
            }

            // Verify connections were set correctly
            UtilityConnections actualConnections = mgr.GetConnections(originCell, false);
            CopyMaterialsManager.Log($"Connections after restore: {actualConnections} (expected: {storedConnections})");

            // If connections don't match, try again after a short delay
            if (actualConnections != storedConnections && storedConnections != (UtilityConnections)0)
            {
                CopyMaterialsManager.Warn($"Connections mismatch! Retrying after delay...");
                GameScheduler.Instance.Schedule("CopyMaterialsRetryConnections", 0.2f, (obj) =>
                {
                    mgr.SetConnections(storedConnections, originCell, false);
                    var retryConnectMethod = buildingGO.GetType().GetMethod("Connect", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    retryConnectMethod?.Invoke(buildingGO, null);
                    
                    UtilityConnections retryConnections = mgr.GetConnections(originCell, false);
                    CopyMaterialsManager.Log($"Connections after retry: {retryConnections}");
                });
            }

            // For conduits specifically, we may need to update the visualizer immediately
            if (buildingGO.TryGetComponent<KAnimGraphTileVisualizer>(out var viz))
            {
                UtilityConnections currentConnections = mgr.GetConnections(originCell, false);
                var updateMethod = viz.GetType().GetMethod("UpdateConnections", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (updateMethod != null)
                {
                    updateMethod.Invoke(viz, new object[] { currentConnections });
                }
                viz.Refresh();
            }
        }

        private void RefreshBuildingAndNeighbors(GameObject buildingGO)
        {
            if (buildingGO == null) return;

            var networkItem = buildingGO.GetComponent<IHaveUtilityNetworkMgr>();
            var mgr = networkItem?.GetNetworkManager();

            // Refresh visuals for this building
            if (buildingGO.TryGetComponent<KAnimGraphTileVisualizer>(out var viz))
            {
                UtilityConnections currentConnections = mgr != null ? mgr.GetConnections(originCell, false) : (UtilityConnections)0;
                var updateMethod = viz.GetType().GetMethod("UpdateConnections", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (updateMethod != null)
                {
                    updateMethod.Invoke(viz, new object[] { currentConnections });
                }
                viz.Refresh();
            }

            // Refresh neighbor connections and visuals
            RefreshNeighbors(originCell, (int)objectLayer);
        }

        public static void RefreshNeighbors(int cell, int layer)
        {
            CellOffset[] offsets = new CellOffset[]
            {
                new CellOffset(-1, 0),
                new CellOffset(1, 0),
                new CellOffset(0, 1),
                new CellOffset(0, -1)
            };

            foreach (CellOffset offset in offsets)
            {
                int neighborCell = Grid.OffsetCell(cell, offset);
                if (Grid.IsValidCell(neighborCell))
                {
                    GameObject neighborGO = Grid.Objects[neighborCell, layer];
                    if (neighborGO != null)
                    {
                        // Call Connect on neighbor to recalculate
                        var neighborConnectMethod = neighborGO.GetType().GetMethod("Connect", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (neighborConnectMethod != null)
                        {
                            try
                            {
                                neighborConnectMethod.Invoke(neighborGO, null);
                            }
                            catch (Exception e)
                            {
                                CopyMaterialsManager.Warn($"Error calling Connect() on neighbor: {e.Message}");
                            }
                        }

                        // Get neighbor connections for visual update
                        var neighborMgr = neighborGO.GetComponent<IHaveUtilityNetworkMgr>()?.GetNetworkManager();
                        UtilityConnections neighborConnections = neighborMgr != null ? neighborMgr.GetConnections(neighborCell, false) : (UtilityConnections)0;

                        if (neighborGO.TryGetComponent<KAnimGraphTileVisualizer>(out var neighborViz))
                        {
                            var updateMethod = neighborViz.GetType().GetMethod("UpdateConnections", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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