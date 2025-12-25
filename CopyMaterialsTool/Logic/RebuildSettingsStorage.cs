using System.Reflection;
using UnityEngine;

namespace CopyMaterials.Logic
{
    public class RebuildSettingsStorage : KMonoBehaviour
    {
        public PrioritySetting priority;
        public string facadeID;
        public Tag copyGroupTag;

        public static void CaptureAndAttach(GameObject target, GameObject source)
        {
            if (target == null || source == null) return;

            var storage = target.AddComponent<RebuildSettingsStorage>();

            var p = source.GetComponent<Prioritizable>();
            if (p != null) storage.priority = p.GetMasterPriority();

            var facade = source.GetComponent<BuildingFacade>();
            if (facade != null) storage.facadeID = facade.CurrentFacade;

            var cbs = source.GetComponent<CopyBuildingSettings>();
            if (cbs != null) storage.copyGroupTag = cbs.copyGroupTag;

            CopyMaterialsManager.Log("Captured settings for rebuild");
        }

        public void ApplyTo(GameObject newBuilding)
        {
            if (newBuilding == null) return;

            // Apply priority (full object)
            var p = newBuilding.GetComponent<Prioritizable>();
            if (p != null && priority.priority_value != 0)
            {
                p.SetMasterPriority(priority);
            }

            // Apply facade
            if (!string.IsNullOrEmpty(facadeID))
            {
                var facade = newBuilding.GetComponent<BuildingFacade>();
                if (facade != null)
                {
                    var field = typeof(BuildingFacade).GetField("currentFacade", BindingFlags.Instance | BindingFlags.NonPublic);
                    field?.SetValue(facade, facadeID);
                }
            }

            // Apply copy group tag
            if (copyGroupTag != Tag.Invalid)
            {
                var cbs = newBuilding.GetComponent<CopyBuildingSettings>();
                if (cbs != null)
                {
                    var field = typeof(CopyBuildingSettings).GetField("copyGroupTag", BindingFlags.Instance | BindingFlags.NonPublic);
                    field?.SetValue(cbs, copyGroupTag);
                }
            }

            Destroy(this);
        }
    }
}