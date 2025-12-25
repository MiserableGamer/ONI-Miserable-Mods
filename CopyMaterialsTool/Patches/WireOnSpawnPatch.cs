using HarmonyLib;
using System.Reflection;
using System.Collections;
using UnityEngine;
using CopyMaterials.Logic;
using PeterHan.PLib.Core;

namespace CopyMaterials.Patches
{
    /// <summary>
    /// Generic patch for all conduit types (Wire, LogicWire, LiquidConduit, GasConduit) to restore connections
    /// </summary>
    public static class ConduitOnSpawnPatch
    {
        /// <summary>
        /// Common method to restore connections for any conduit type
        /// </summary>
        private static void RestoreConduitConnections(KMonoBehaviour conduit, ConnectionStorage storage)
        {
            int cell = Grid.PosToCell(conduit.transform.GetPosition());
            var networkItem = conduit.GetComponent<IHaveUtilityNetworkMgr>();
            if (networkItem == null)
            {
                CopyMaterialsManager.Warn($"ConduitOnSpawnPatch: No IHaveUtilityNetworkMgr found");
                return;
            }

            var mgr = networkItem.GetNetworkManager();
            if (mgr == null)
            {
                CopyMaterialsManager.Warn("ConduitOnSpawnPatch: No network manager");
                return;
            }

            CopyMaterialsManager.Log($"ConduitOnSpawnPatch: Restoring connections {storage.StoredConnections} at cell {cell}");

            // Set connections
            mgr.SetConnections(storage.StoredConnections, cell, false);

            // Update neighbors
            UpdateNeighborConnections(mgr, cell, storage.StoredConnections);

            // Force another rebuild to apply the connections
            var forceRebuildMethod = mgr.GetType().GetMethod("ForceRebuildNetworks", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (forceRebuildMethod != null)
            {
                try
                {
                    forceRebuildMethod.Invoke(mgr, null);
                    CopyMaterialsManager.Log("ConduitOnSpawnPatch: Called ForceRebuildNetworks()");
                }
                catch (System.Exception e)
                {
                    CopyMaterialsManager.Warn($"ConduitOnSpawnPatch: Error calling ForceRebuildNetworks(): {e.Message}");
                }
            }

            // Update visualizer
            if (conduit.TryGetComponent<KAnimGraphTileVisualizer>(out var viz))
            {
                UtilityConnections currentConnections = mgr.GetConnections(cell, false);
                var updateMethod = viz.GetType().GetMethod("UpdateConnections", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (updateMethod != null)
                {
                    updateMethod.Invoke(viz, new object[] { currentConnections });
                }
                viz.Refresh();
            }

            // Verify
            UtilityConnections actualConnections = mgr.GetConnections(cell, false);
            CopyMaterialsManager.Log($"ConduitOnSpawnPatch: Final connections at cell {cell}: {actualConnections} (expected: {storage.StoredConnections})");
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
                        CopyMaterialsManager.Log($"ConduitOnSpawnPatch: Updated left neighbor at cell {neighborCell} to connect right");
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
                        CopyMaterialsManager.Log($"ConduitOnSpawnPatch: Updated right neighbor at cell {neighborCell} to connect left");
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
                        CopyMaterialsManager.Log($"ConduitOnSpawnPatch: Updated up neighbor at cell {neighborCell} to connect down");
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
                        CopyMaterialsManager.Log($"ConduitOnSpawnPatch: Updated down neighbor at cell {neighborCell} to connect up");
                    }
                }
            }
        }

        private static void RefreshNeighborConduits(int cell, int layer)
        {
            // Refresh neighbors to ensure they update their visualizers and recalculate connections
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
                        var neighborConnectMethod = neighborGO.GetType().GetMethod("Connect", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (neighborConnectMethod != null)
                        {
                            try
                            {
                                neighborConnectMethod.Invoke(neighborGO, null);
                            }
                            catch (System.Exception e)
                            {
                                CopyMaterialsManager.Warn($"ConduitOnSpawnPatch: Error calling Connect() on neighbor: {e.Message}");
                            }
                        }

                        // Update visualizer for neighbor
                        var neighborNetworkItem = neighborGO.GetComponent<IHaveUtilityNetworkMgr>();
                        var neighborMgr = neighborNetworkItem?.GetNetworkManager();
                        if (neighborMgr != null && neighborGO.TryGetComponent<KAnimGraphTileVisualizer>(out var neighborViz))
                        {
                            UtilityConnections neighborConnections = neighborMgr.GetConnections(neighborCell, false);
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

        /// <summary>
        /// Common postfix for all conduit OnSpawn methods
        /// </summary>
        public static void OnSpawnPostfix(KMonoBehaviour __instance, string typeName)
        {
            int cell = Grid.PosToCell(__instance.transform.GetPosition());
            var building = __instance.GetComponent<Building>();
            if (building == null)
            {
                CopyMaterialsManager.Log($"ConduitOnSpawnPatch: {typeName}.OnSpawn - No Building component found");
                return;
            }

            int layer = (int)building.Def.ObjectLayer;
            
            // Get stored connections from the manager's dictionary
            UtilityConnections storedConnections = CopyMaterialsManager.GetStoredConnections(cell, layer);
            
            CopyMaterialsManager.Log($"ConduitOnSpawnPatch: {typeName}.OnSpawn called at cell {cell}, layer {layer}, stored connections: {storedConnections}");
            
            if (storedConnections != (UtilityConnections)0)
            {
                CopyMaterialsManager.Log($"ConduitOnSpawnPatch: {typeName}.OnSpawn - Restoring connections {storedConnections} at cell {cell}");

                // Wait a bit for the network system to finish processing (OnSpawn calls AddToNetworks)
                // Then restore connections
                GameScheduler.Instance.Schedule($"CopyMaterials{typeName}ConnectionRestore", 0.25f, (obj) =>
                {
                    RestoreConduitConnectionsFromStorage(__instance, cell, layer, storedConnections);
                });
            }
            else
            {
                CopyMaterialsManager.Log($"ConduitOnSpawnPatch: {typeName}.OnSpawn - No stored connections for cell {cell}, layer {layer}");
            }
        }

        private static void RestoreConduitConnectionsFromStorage(KMonoBehaviour conduit, int cell, int layer, UtilityConnections storedConnections)
        {
            var networkItem = conduit.GetComponent<IHaveUtilityNetworkMgr>();
            if (networkItem == null)
            {
                CopyMaterialsManager.Warn($"ConduitOnSpawnPatch: No IHaveUtilityNetworkMgr found");
                return;
            }

            var mgr = networkItem.GetNetworkManager();
            if (mgr == null)
            {
                CopyMaterialsManager.Warn("ConduitOnSpawnPatch: No network manager");
                return;
            }

            CopyMaterialsManager.Log($"ConduitOnSpawnPatch: Restoring connections {storedConnections} at cell {cell}");

            // Set connections
            mgr.SetConnections(storedConnections, cell, false);

            // Update neighbors - this ensures neighbors that weren't rebuilt also get their connections updated
            UpdateNeighborConnections(mgr, cell, storedConnections);
            
            // Also refresh neighbors to ensure they recalculate their connections
            RefreshNeighborConduits(cell, layer);

            // Force another rebuild to apply the connections
            var forceRebuildMethod = mgr.GetType().GetMethod("ForceRebuildNetworks", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (forceRebuildMethod != null)
            {
                try
                {
                    forceRebuildMethod.Invoke(mgr, null);
                    CopyMaterialsManager.Log("ConduitOnSpawnPatch: Called ForceRebuildNetworks()");
                }
                catch (System.Exception e)
                {
                    CopyMaterialsManager.Warn($"ConduitOnSpawnPatch: Error calling ForceRebuildNetworks(): {e.Message}");
                }
            }

            // Update visualizer
            if (conduit.TryGetComponent<KAnimGraphTileVisualizer>(out var viz))
            {
                UtilityConnections currentConnections = mgr.GetConnections(cell, false);
                var updateMethod = viz.GetType().GetMethod("UpdateConnections", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (updateMethod != null)
                {
                    updateMethod.Invoke(viz, new object[] { currentConnections });
                }
                viz.Refresh();
            }

            // Verify and clear stored connections
            UtilityConnections actualConnections = mgr.GetConnections(cell, false);
            CopyMaterialsManager.Log($"ConduitOnSpawnPatch: Final connections at cell {cell}: {actualConnections} (expected: {storedConnections})");
            
            // Clear the stored connections after restoration
            CopyMaterialsManager.ClearStoredConnections(cell, layer);
        }

        /// <summary>
        /// Helper to find types in loaded assemblies
        /// </summary>
        public static System.Type FindTypeInAssemblies(string typeName)
        {
            // Try direct lookup first
            var type = System.Type.GetType(typeName);
            if (type != null) return type;

            type = System.Type.GetType($"{typeName}, Assembly-CSharp");
            if (type != null) return type;

            // Search all loaded assemblies
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    type = assembly.GetType(typeName);
                    if (type != null) return type;
                }
                catch
                {
                    // Ignore errors when searching assemblies
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Patch Wire.OnSpawn to restore connections after the network system has processed the wire
    /// </summary>
    [HarmonyPatch(typeof(Wire), "OnSpawn")]
    public static class WireOnSpawnPatch
    {
        [HarmonyPostfix]
        static void Postfix(Wire __instance)
        {
            ConduitOnSpawnPatch.OnSpawnPostfix(__instance, "Wire");
        }
    }

    /// <summary>
    /// Patch LogicWire.OnSpawn to restore connections
    /// </summary>
    [HarmonyPatch]
    public static class LogicWireOnSpawnPatch
    {
        static bool Prepare()
        {
            // Only apply this patch if LogicWire type exists
            var logicWireType = System.Type.GetType("LogicWire") ?? System.Type.GetType("LogicWire, Assembly-CSharp");
            return logicWireType != null;
        }

        [HarmonyTargetMethod]
        static System.Reflection.MethodBase TargetMethod()
        {
            // Try to find LogicWire type
            var logicWireType = System.Type.GetType("LogicWire") ?? System.Type.GetType("LogicWire, Assembly-CSharp");
            if (logicWireType == null)
            {
                return null; // Shouldn't happen if Prepare returned true
            }
            return logicWireType.GetMethod("OnSpawn", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [HarmonyPostfix]
        static void Postfix(KMonoBehaviour __instance)
        {
            ConduitOnSpawnPatch.OnSpawnPostfix(__instance, "LogicWire");
        }
    }

    /// <summary>
    /// Patch LiquidConduit.OnSpawn to restore connections
    /// </summary>
    [HarmonyPatch]
    public static class LiquidConduitOnSpawnPatch
    {
        static bool Prepare()
        {
            // Only apply this patch if LiquidConduit type exists
            var liquidConduitType = ConduitOnSpawnPatch.FindTypeInAssemblies("LiquidConduit");
            if (liquidConduitType != null)
            {
                CopyMaterialsManager.Log($"LiquidConduitOnSpawnPatch: Found LiquidConduit type");
            }
            else
            {
                CopyMaterialsManager.Warn("LiquidConduitOnSpawnPatch: LiquidConduit type not found");
            }
            return liquidConduitType != null;
        }

        [HarmonyTargetMethod]
        static System.Reflection.MethodBase TargetMethod()
        {
            var liquidConduitType = ConduitOnSpawnPatch.FindTypeInAssemblies("LiquidConduit");
            if (liquidConduitType == null)
            {
                return null; // Shouldn't happen if Prepare returned true
            }
            var method = liquidConduitType.GetMethod("OnSpawn", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                CopyMaterialsManager.Warn("LiquidConduitOnSpawnPatch: OnSpawn method not found");
            }
            return method;
        }

        [HarmonyPostfix]
        static void Postfix(KMonoBehaviour __instance)
        {
            ConduitOnSpawnPatch.OnSpawnPostfix(__instance, "LiquidConduit");
        }
    }

    /// <summary>
    /// Patch GasConduit.OnSpawn to restore connections
    /// </summary>
    [HarmonyPatch]
    public static class GasConduitOnSpawnPatch
    {
        static bool Prepare()
        {
            // Only apply this patch if GasConduit type exists
            var gasConduitType = ConduitOnSpawnPatch.FindTypeInAssemblies("GasConduit");
            if (gasConduitType != null)
            {
                CopyMaterialsManager.Log($"GasConduitOnSpawnPatch: Found GasConduit type");
            }
            else
            {
                CopyMaterialsManager.Warn("GasConduitOnSpawnPatch: GasConduit type not found");
            }
            return gasConduitType != null;
        }

        [HarmonyTargetMethod]
        static System.Reflection.MethodBase TargetMethod()
        {
            var gasConduitType = ConduitOnSpawnPatch.FindTypeInAssemblies("GasConduit");
            if (gasConduitType == null)
            {
                return null; // Shouldn't happen if Prepare returned true
            }
            var method = gasConduitType.GetMethod("OnSpawn", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                CopyMaterialsManager.Warn("GasConduitOnSpawnPatch: OnSpawn method not found");
            }
            return method;
        }

        [HarmonyPostfix]
        static void Postfix(KMonoBehaviour __instance)
        {
            ConduitOnSpawnPatch.OnSpawnPostfix(__instance, "GasConduit");
        }
    }
}

