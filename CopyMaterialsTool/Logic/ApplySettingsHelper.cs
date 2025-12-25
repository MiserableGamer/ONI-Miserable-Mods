using System;
using System.Reflection;
using UnityEngine;

namespace CopyMaterials.Logic
{
    public static class ApplySettingsHelper
    {
        public static void ApplySettingsToBuiltObjects(
            BuildingDef def,
            int originCell,
            Orientation orientation,
            GameObject sourceSettingsObject,
            SimHashes materialToApply,
            PrioritySetting priorityToApply,
            string facadeID,
            Tag copyGroupTag
        )
        {
            try
            {
                if (def == null) return;

                def.RunOnArea(originCell, orientation, delegate (int c)
                {
                    var obj = Grid.Objects[c, (int)def.ObjectLayer];
                    if (obj == null) return;

                    var bc = obj.GetComponent<BuildingComplete>();
                    if (bc == null) return;

                    int targetId = obj.GetInstanceID();
                    CopyMaterialsManager.Log($"Applying fallback settings to rebuilt building id={targetId}");

                    // Apply material
                    if (materialToApply != SimHashes.Vacuum)
                    {
                        CopyMaterialsManager.TrySetPrimaryElement(obj, materialToApply);
                    }

                    // Apply priority
                    CopyMaterialsManager.ApplyPriorityToObject(obj, priorityToApply);

                    // Apply facade
                    if (!string.IsNullOrEmpty(facadeID))
                    {
                        var facadeComp = obj.GetComponent<BuildingFacade>();
                        if (facadeComp != null)
                        {
                            var field = typeof(BuildingFacade).GetField("currentFacade", BindingFlags.Instance | BindingFlags.NonPublic);
                            if (field != null)
                            {
                                field.SetValue(facadeComp, facadeID);
                            }
                        }
                    }

                    // Apply copy group tag
                    if (copyGroupTag != Tag.Invalid)
                    {
                        var cbs = obj.GetComponent<CopyBuildingSettings>();
                        if (cbs != null)
                        {
                            var field = typeof(CopyBuildingSettings).GetField("copyGroupTag", BindingFlags.Instance | BindingFlags.NonPublic);
                            if (field != null)
                            {
                                field.SetValue(cbs, copyGroupTag);
                            }
                        }
                    }
                });
            }
            catch (Exception e)
            {
                CopyMaterialsManager.Warn($"ApplySettingsToBuiltObjects exception: {e}");
            }
        }
    }
}