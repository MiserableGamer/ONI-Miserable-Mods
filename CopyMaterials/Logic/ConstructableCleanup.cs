using System;
using System.Reflection;
using UnityEngine;

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

        public static void Attach(
            GameObject placed,
            BuildingDef def,
            int originCell,
            Orientation orientation,
            GameObject visualizer,  // Ignored
            SimHashes materialToApply,
            UtilityConnections connections  // New
        )
        {
            var cleanup = placed.AddComponent<ConstructableCleanup>();
            cleanup.def = def;
            cleanup.originCell = originCell;
            cleanup.orientation = orientation;
            cleanup.objectLayer = def.ObjectLayer;
            cleanup.materialToApply = materialToApply;
            cleanup.storedConnections = connections;
            cleanup.DoCleanup();
        }

        private void DoCleanup()
        {
            GameObject completeGO = Grid.Objects[originCell, (int)objectLayer];
            if (completeGO == null) return;

            var bc = completeGO.GetComponent<BuildingComplete>();
            if (bc == null || bc.Def.PrefabID != def.PrefabID) return;

            // New: Restore connections if stored
            var networkItem = completeGO.GetComponent<IHaveUtilityNetworkMgr>();
            var mgr = networkItem?.GetNetworkManager();
            if (storedConnections != (UtilityConnections)0)
            {
                // Try to call SetConnections on the building component if exists
                var setConnectionsMethod = completeGO.GetType().GetMethod("SetConnections", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (setConnectionsMethod != null)
                {
                    setConnectionsMethod.Invoke(completeGO, new object[] { storedConnections });
                }
                else if (mgr != null)
                {
                    // Fallback to manager SetConnections
                    mgr.SetConnections(storedConnections, originCell, false);
                }
            }

            // New: Apply source settings
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

            // Get current connections for visual update
            UtilityConnections currentConnections = mgr != null ? mgr.GetConnections(originCell, false) : (UtilityConnections)0;

            // Refresh visuals for this building
            if (completeGO.TryGetComponent<KAnimGraphTileVisualizer>(out var viz))
            {
                var updateMethod = viz.GetType().GetMethod("UpdateConnections", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                updateMethod?.Invoke(viz, new object[] { currentConnections });
                viz.Refresh();
            }

            // Refresh neighbor connections and visuals
            RefreshNeighbors(originCell, (int)objectLayer);

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
                        neighborConnectMethod?.Invoke(neighborGO, null);

                        // Get neighbor connections for visual update
                        var neighborMgr = neighborGO.GetComponent<IHaveUtilityNetworkMgr>()?.GetNetworkManager();
                        UtilityConnections neighborConnections = neighborMgr != null ? neighborMgr.GetConnections(neighborCell, false) : (UtilityConnections)0;

                        if (neighborGO.TryGetComponent<KAnimGraphTileVisualizer>(out var neighborViz))
                        {
                            var updateMethod = neighborViz.GetType().GetMethod("UpdateConnections", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            updateMethod?.Invoke(neighborViz, new object[] { neighborConnections });
                            neighborViz.Refresh();
                        }
                    }
                }
            }
        }
    }
}