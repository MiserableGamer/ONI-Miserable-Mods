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
        private static HashSet<Building> processedBuildings = new HashSet<Building>();
        private static readonly FieldInfo sourceField = AccessTools.Field(typeof(CopySettingsTool), "sourceGameObject");
        public static bool isMaterialCopyMode = false;

        [HarmonyPrefix]
        [HarmonyPatch("OnDragTool")]
        public static bool Prefix(CopySettingsTool __instance)
        {
            if (isMaterialCopyMode) return false;
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

            int layer = (int)source.Def.ObjectLayer;

            GameObject obj = Grid.Objects[cell, layer];
            if (obj == null) return;

            Building target = obj.GetComponent<Building>();
            if (target == null) return;

            if (target == source) return;

            if (target.Def.PrefabID != source.Def.PrefabID) return;

            if (processedBuildings.Contains(target)) return;
            processedBuildings.Add(target);
            
            if (processedCells.Add(cell))
            {
                var sourcePE = source.GetComponent<PrimaryElement>();
                var targetPE = target.GetComponent<PrimaryElement>();
                if (sourcePE == null || targetPE == null) return;

                bool isBridge = target.Def.PrefabID.Contains("Bridge");
                bool widthsMatch = true;
                if (isBridge)
                {
                    int? sourceWidth = CopyMaterialsManager.GetBridgeWidth(source);
                    int? targetWidth = CopyMaterialsManager.GetBridgeWidth(target);
                    widthsMatch = sourceWidth.HasValue && targetWidth.HasValue && sourceWidth.Value == targetWidth.Value;
                }

                if (sourcePE.ElementID == targetPE.ElementID && widthsMatch)
                {
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

                    Vector3 pos = obj.transform.position;
                    PopFXManager.Instance.SpawnFX(
                        PopFXManager.Instance.sprite_Plus,
                        "Settings Applied",
                        null,
                        pos,
                        2f
                    );
                    return;
                }

                UtilityConnections storedConnections = default(UtilityConnections);
                var networkItem = target.GetComponent<IHaveUtilityNetworkMgr>();
                if (networkItem != null)
                {
                    var mgr = networkItem.GetNetworkManager();
                    if (mgr != null)
                    {
                        storedConnections = mgr.GetConnections(cell, false);
                        
                        var getConnectionsMethod = target.GetType().GetMethod("GetConnections", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (getConnectionsMethod != null)
                        {
                            try
                            {
                                var directConnections = (UtilityConnections)getConnectionsMethod.Invoke(target, null);
                                if (directConnections != (UtilityConnections)0)
                                {
                                    storedConnections = directConnections;
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
                    deconstructable.QueueDeconstruction(true);
                }

                CopyMaterialsManager.StoreConnections(cell, layer, storedConnections);

                CopyMaterialsWatcher.Attach(
                    target,
                    target.Def.PrefabID,
                    source.GetComponent<PrimaryElement>().ElementID,
                    target.GetComponent<Rotatable>()?.GetOrientation() ?? Orientation.Neutral,
                    storedConnections
                );
            }
        }

        [HarmonyPatch("OnDeactivateTool")]
        [HarmonyPostfix]
        public static void OnDeactivateTool_Postfix(CopySettingsTool __instance)
        {
            processedCells.Clear();
            processedBuildings.Clear();
            sourceField.SetValue(__instance, null);
            isMaterialCopyMode = false;
            CopyMaterialsManager.ClearSource();
        }
    }
}