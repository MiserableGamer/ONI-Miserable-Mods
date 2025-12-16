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
                def.RunOnArea(originCell, orientation, delegate (int c) {
                    var obj = Grid.Objects[c, (int)def.ObjectLayer];
                    if (obj == null) return;
                    var bc = obj.GetComponent<BuildingComplete>();
                    if (bc == null) return;

                    int targetId = obj.GetInstanceID();
                    CopyMaterialsManager.Log($"Found BuildingComplete id={targetId} — applying settings.");

                    bool eventTriggered = false;
                    try
                    {
                        if (sourceSettingsObject != null)
                        {
                            var kpid = obj.GetComponent<KPrefabID>();
                            if (kpid != null)
                            {
                                kpid.Trigger(GameHashes.CopySettings, sourceSettingsObject);
                                CopyMaterialsManager.Log($"CopySettings event triggered on target id={targetId} from source {sourceSettingsObject.name}");
                                eventTriggered = true;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        CopyMaterialsManager.Warn($"CopySettings event trigger failed for id={targetId}: {e}");
                    }

                    if (!eventTriggered)
                    {
                        try
                        {
                            if (materialToApply != SimHashes.Vacuum)
                            {
                                CopyMaterialsManager.TrySetPrimaryElement(obj, materialToApply);
                                CopyMaterialsManager.Log($"Applied captured material {materialToApply} to id={targetId}");
                            }

                            CopyMaterialsManager.ApplyPriorityToObject(obj, priorityToApply);
                            CopyMaterialsManager.Log($"Applied captured priority to id={targetId}");

                            if (!string.IsNullOrEmpty(facadeID))
                            {
                                var facadeComp = obj.GetComponent("BuildingFacade");
                                if (facadeComp != null)
                                {
                                    var prop = facadeComp.GetType().GetProperty("CurrentFacade",
                                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    if (prop != null && prop.CanWrite)
                                    {
                                        prop.SetValue(facadeComp, facadeID, null);
                                        CopyMaterialsManager.Log($"Applied captured facade '{facadeID}' to id={targetId}");
                                    }
                                }
                            }

                            if (copyGroupTag != Tag.Invalid)
                            {
                                var cbs = obj.GetComponent<CopyBuildingSettings>();
                                if (cbs != null)
                                {
                                    var field = typeof(CopyBuildingSettings).GetField("copyGroupTag",
                                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    if (field != null)
                                    {
                                        field.SetValue(cbs, copyGroupTag);
                                        CopyMaterialsManager.Log($"Applied captured copyGroupTag '{copyGroupTag}' to id={targetId}");
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            CopyMaterialsManager.Warn($"Fallback apply settings failed for id={targetId}: {e}");
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