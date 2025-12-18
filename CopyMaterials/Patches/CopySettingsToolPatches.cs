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

            if (target.Def.PrefabID != source.Def.PrefabID) return;

            if (processedCells.Add(cell))
            {
                var sourcePE = source.GetComponent<PrimaryElement>();
                var targetPE = target.GetComponent<PrimaryElement>();
                if (sourcePE == null || targetPE == null) return;

                if (sourcePE.ElementID == targetPE.ElementID)
                {
                    // Apply settings directly if materials match
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

                // New: Store connections if network item
                UtilityConnections storedConnections = default(UtilityConnections);
                var networkItem = target.GetComponent<IHaveUtilityNetworkMgr>();
                if (networkItem != null)
                {
                    var mgr = networkItem.GetNetworkManager();
                    if (mgr != null)
                    {
                        storedConnections = mgr.GetConnections(cell, false);
                    }
                }

                var deconstructable = target.GetComponent<Deconstructable>();
                if (deconstructable != null)
                {
                    deconstructable.QueueDeconstruction(true);
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
            sourceField.SetValue(__instance, null);
            isMaterialCopyMode = false;  // Reset flag
            CopyMaterialsManager.ClearSource();  // New: Clear source data
        }
    }
}