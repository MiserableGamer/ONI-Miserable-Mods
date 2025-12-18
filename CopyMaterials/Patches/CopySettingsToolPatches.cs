using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using CopyMaterials.Logic;

namespace CopyMaterials.Patches
{
    [HarmonyPatch(typeof(CopySettingsTool))]
    public static class CopySettingsToolPatches
    {
        private static HashSet<int> processedCells = new HashSet<int>();
        private static HashSet<Building> processedBuildings = new HashSet<Building>(); // Track buildings, not just cells

        private static readonly FieldInfo sourceField = AccessTools.Field(typeof(CopySettingsTool), "sourceGameObject");

        public static bool isMaterialCopyMode = false;  // Changed to public

        [HarmonyPrefix]
        [HarmonyPatch("OnDragTool")]
        public static bool Prefix(CopySettingsTool __instance)
        {
            if (isMaterialCopyMode)
            {
                return false;  // Skip base OnDragTool (no settings apply, no "Settings Applied" popup)
            }
            return true;
        }

        [HarmonyPatch("OnDragTool")]
        [HarmonyPostfix]
        public static void OnDragTool_Postfix(CopySettingsTool __instance, int cell, int distFromOrigin)
        {
            GameObject sourceGO = (GameObject)sourceField.GetValue(__instance);
            if (sourceGO == null) return;

            Building source = sourceGO.GetComponent<Building>();
            if (source == null) return;

            int layer = (int)source.Def.ObjectLayer;  // Use source's layer (same as target's since prefabID matches)

            GameObject obj = Grid.Objects[cell, layer];
            if (obj == null) return;

            Building target = obj.GetComponent<Building>();
            if (target == null) return;

            if (target == source) return;

            // Log prefab IDs for debugging
            CopyMaterialsManager.Log($"Checking buildings: source={source.Def.PrefabID}, target={target.Def.PrefabID}");
            
            if (target.Def.PrefabID != source.Def.PrefabID)
            {
                CopyMaterialsManager.Log($"Prefab IDs don't match, skipping. Source: {source.Def.PrefabID}, Target: {target.Def.PrefabID}");
                return;
            }

            // Check if we've already processed this building (not just this cell)
            // This prevents processing the same building multiple times when it spans multiple cells
            if (processedBuildings.Contains(target))
            {
                CopyMaterialsManager.Log($"Building at cell {cell} already processed, skipping");
                return;
            }

            // Mark this building as processed
            processedBuildings.Add(target);
            
            if (processedCells.Add(cell))
            {
                CopyMaterialsManager.Log($"Processing cell {cell} for {target.Def.PrefabID}");
                
                var sourcePE = source.GetComponent<PrimaryElement>();
                var targetPE = target.GetComponent<PrimaryElement>();
                if (sourcePE == null || targetPE == null)
                {
                    CopyMaterialsManager.Warn($"Missing PrimaryElement: sourcePE={sourcePE != null}, targetPE={targetPE != null}");
                    return;
                }

                CopyMaterialsManager.Log($"Materials: source={sourcePE.ElementID}, target={targetPE.ElementID}");

                // For bridges, also check if widths match (for ExtendedBuildingWidth support)
                bool isBridge = target.Def.PrefabID.Contains("Bridge");
                bool widthsMatch = true;
                if (isBridge)
                {
                    int? sourceWidth = CopyMaterialsManager.GetBridgeWidth(source);
                    int? targetWidth = CopyMaterialsManager.GetBridgeWidth(target);
                    widthsMatch = sourceWidth.HasValue && targetWidth.HasValue && sourceWidth.Value == targetWidth.Value;
                    CopyMaterialsManager.Log($"Bridge width check: source={sourceWidth}, target={targetWidth}, match={widthsMatch}");
                    
                    // If widths don't match, we need to deconstruct and rebuild with the new width
                    if (!widthsMatch)
                    {
                        CopyMaterialsManager.Log($"Bridge widths don't match - will deconstruct and rebuild");
                    }
                }

                if (sourcePE.ElementID == targetPE.ElementID && widthsMatch)
                {
                    CopyMaterialsManager.Log($"Materials and widths match - applying settings only, skipping deconstruction");
                    
                    // Apply settings directly if materials and widths match
                    CopyMaterialsManager.ApplyPriorityToObject(obj, CopyMaterialsManager.sourcePriority);

                    if (!string.IsNullOrEmpty(CopyMaterialsManager.sourceFacadeID))
                    {
                        var facadeComp = obj.GetComponent<BuildingFacade>();
                        if (facadeComp != null)
                        {
                            var field = typeof(BuildingFacade).GetField("currentFacade", BindingFlags.Instance | BindingFlags.NonPublic);
                            field?.SetValue(facadeComp, CopyMaterialsManager.sourceFacadeID);
                        }
                    }

                    if (CopyMaterialsManager.sourceCopyGroupTag != Tag.Invalid)
                    {
                        var cbs = obj.GetComponent<CopyBuildingSettings>();
                        if (cbs != null)
                        {
                            var field = typeof(CopyBuildingSettings).GetField("copyGroupTag", BindingFlags.Instance | BindingFlags.NonPublic);
                            field?.SetValue(cbs, CopyMaterialsManager.sourceCopyGroupTag);
                        }
                    }

                    // Popup for settings applied (no material change)
                    Vector3 pos = obj.transform.position;
                    PopFXManager.Instance.SpawnFX(
                        PopFXManager.Instance.sprite_Plus,
                        "Settings Applied",
                        null,
                        pos,
                        2f
                    );

                    return;  // Skip deconstruct
                }

                // Store connections if network item (for conduits)
                UtilityConnections storedConnections = default(UtilityConnections);
                var networkItem = target.GetComponent<IHaveUtilityNetworkMgr>();
                if (networkItem != null)
                {
                    var mgr = networkItem.GetNetworkManager();
                    if (mgr != null)
                    {
                        // Get connections from the network manager
                        storedConnections = mgr.GetConnections(cell, false);
                        CopyMaterialsManager.Log($"Captured connections {storedConnections} for {target.Def.PrefabID} at cell {cell}");
                        
                        // Also try to get connections directly from the building component if it has a method
                        var getConnectionsMethod = target.GetType().GetMethod("GetConnections", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (getConnectionsMethod != null)
                        {
                            try
                            {
                                var directConnections = (UtilityConnections)getConnectionsMethod.Invoke(target, null);
                                if (directConnections != (UtilityConnections)0)
                                {
                                    storedConnections = directConnections;
                                    CopyMaterialsManager.Log($"Got connections directly from building component: {storedConnections}");
                                }
                            }
                            catch (System.Exception e)
                            {
                                CopyMaterialsManager.Warn($"Error getting connections from building: {e.Message}");
                            }
                        }
                    }
                }

                var deconstructable = target.GetComponent<Deconstructable>();
                if (deconstructable != null)
                {
                    CopyMaterialsManager.Log($"Queueing deconstruction for {target.Def.PrefabID} at cell {cell}");
                    deconstructable.QueueDeconstruction(true);
                    
                    // Verify deconstruction was queued
                    var isMarkedField = typeof(Deconstructable).GetField("isMarkedForDeconstruction", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (isMarkedField != null)
                    {
                        bool isMarked = (bool)isMarkedField.GetValue(deconstructable);
                        CopyMaterialsManager.Log($"Deconstruction queued: isMarked={isMarked}");
                    }
                }
                else
                {
                    CopyMaterialsManager.Warn($"No Deconstructable component found on {target.Def.PrefabID} at cell {cell}");
                }

                // Store connections in the manager's dictionary
                CopyMaterialsManager.StoreConnections(cell, layer, storedConnections);

                // Capture bridge width if this is a bridge (for ExtendedBuildingWidth support)
                int? bridgeWidth = CopyMaterialsManager.GetBridgeWidth(target);
                if (bridgeWidth.HasValue)
                {
                    CopyMaterialsManager.Log($"Captured bridge width: {bridgeWidth.Value} for {target.Def.PrefabID}");
                }

                CopyMaterialsWatcher.Attach(
                    target,
                    target.Def.PrefabID,
                    source.GetComponent<PrimaryElement>().ElementID,
                    target.GetComponent<Rotatable>()?.GetOrientation() ?? Orientation.Neutral,
                    storedConnections  // New parameter
                );
            }
        }

        [HarmonyPatch("OnDeactivateTool")]
        [HarmonyPostfix]
        public static void OnDeactivateTool_Postfix(CopySettingsTool __instance)
        {
            processedCells.Clear();
            processedBuildings.Clear(); // Clear processed buildings too
            sourceField.SetValue(__instance, null);
            isMaterialCopyMode = false;  // Reset flag
            CopyMaterialsManager.ClearSource();  // New: Clear source data
            // Don't clear stored connections here - they'll be cleared after restoration
        }
    }
}