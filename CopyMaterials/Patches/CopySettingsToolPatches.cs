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

        [HarmonyPatch("OnDragTool")]
        [HarmonyPostfix]
        public static void OnDragTool_Postfix(CopySettingsTool __instance, int cell, int distFromOrigin)
        {
            GameObject sourceGO = (GameObject)sourceField.GetValue(__instance);
            if (sourceGO == null) return;

            GameObject obj = Grid.Objects[cell, (int)ObjectLayer.Building];
            if (obj == null) return;

            Building target = obj.GetComponent<Building>();
            if (target == null) return;

            Building source = sourceGO.GetComponent<Building>();
            if (source == null || target == source) return;

            if (target.Def.PrefabID != source.Def.PrefabID) return;

            if (processedCells.Add(cell))
            {
                var deconstructable = target.GetComponent<Deconstructable>();
                if (deconstructable != null)
                {
                    deconstructable.QueueDeconstruction(true);
                }

                PrioritySetting targetPriority = default(PrioritySetting);
                string targetFacadeID = null;
                Tag targetCopyGroupTag = Tag.Invalid;

                var p = target.GetComponent<Prioritizable>();
                if (p != null) targetPriority = p.GetMasterPriority();

                var facade = target.GetComponent<BuildingFacade>();
                if (facade != null) targetFacadeID = facade.CurrentFacade;

                var cbs = target.GetComponent<CopyBuildingSettings>();
                if (cbs != null) targetCopyGroupTag = cbs.copyGroupTag;

                CopyMaterialsWatcher.Attach(
                    target,
                    target.Def.PrefabID,
                    source.GetComponent<PrimaryElement>().ElementID,
                    target.GetComponent<Rotatable>()?.GetOrientation() ?? Orientation.Neutral
                );
            }
        }

        [HarmonyPatch("OnDeactivateTool")]
        [HarmonyPostfix]
        public static void OnDeactivateTool_Postfix(CopySettingsTool __instance)
        {
            processedCells.Clear();
            sourceField.SetValue(__instance, null);
        }
    }
}