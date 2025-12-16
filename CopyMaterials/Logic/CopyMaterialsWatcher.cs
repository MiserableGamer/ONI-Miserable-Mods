using System;
using System.Collections.Generic;
using UnityEngine;

namespace CopyMaterials.Logic
{
    public class CopyMaterialsWatcher : KMonoBehaviour
    {
        private int originalCell;
        private string prefabID;
        private SimHashes material;
        private bool blueprintCreated;
        private WeakReference<Building> targetRef;
        private Orientation orientation = Orientation.Neutral;
        private string facadeID = null;

        public static CopyMaterialsWatcher Attach(Building target, string prefabID, SimHashes material, Orientation orientation = Orientation.Neutral, string facadeID = null)
        {
            if (target == null) return null;

            GameObject root = Game.Instance?.gameObject ?? GameObject.Find("CopyMaterialsRoot");
            if (root == null)
            {
                root = new GameObject("CopyMaterialsRoot");
                UnityEngine.Object.DontDestroyOnLoad(root);
            }

            var watcher = root.AddComponent<CopyMaterialsWatcher>();
            watcher.Initialize(target, prefabID, material, orientation, facadeID);
            CopyMaterialsManager.Log($"Watcher attached for {prefabID} at cell {watcher.originalCell}");
            return watcher;
        }

        private void Initialize(Building target, string prefabID, SimHashes material, Orientation orientation, string facadeID)
        {
            this.targetRef = new WeakReference<Building>(target);
            this.prefabID = prefabID;
            this.material = material;
            this.blueprintCreated = false;
            this.orientation = orientation;
            this.facadeID = facadeID;
            try
            {
                this.originalCell = Grid.PosToCell(target);
            }
            catch
            {
                this.originalCell = -1;
            }
        }

        private void Update()
        {
            if (blueprintCreated)
            {
                Destroy(this);
                return;
            }

            Building target;
            bool targetAlive = targetRef != null && targetRef.TryGetTarget(out target) && target != null;
            bool buildingRemoved = !targetAlive;

            if (!buildingRemoved && originalCell >= 0)
            {
                var occupant = Grid.Objects[originalCell, (int)ObjectLayer.Building];
                var occBuilding = occupant?.GetComponent<Building>();
                if (occBuilding == null || occBuilding.Def?.PrefabID != prefabID)
                    buildingRemoved = true;
            }

            if (buildingRemoved)
            {
                CopyMaterialsManager.Log($"Detected removal of {prefabID} at cell {originalCell}; creating blueprint.");
                CreateBlueprintAtCell(originalCell, prefabID, material, orientation, facadeID);
                blueprintCreated = true;
            }
        }

        private void CreateBlueprintAtCell(int cell, string prefabID, SimHashes material, Orientation orientation, string facadeID)
        {
            var def = Assets.GetBuildingDef(prefabID);
            if (def == null)
            {
                CopyMaterialsManager.Warn($"BuildingDef not found for {prefabID}");
                return;
            }

            Vector3 worldPos = Grid.CellToPosCBC(cell, def.SceneLayer);
            IList<Tag> selectedElements = PlacementHelpers.BuildSelectedElementsFromMaterial(material);

            GameObject visualizer = null;
            if (def.BuildingPreview != null)
            {
                visualizer = GameUtil.KInstantiate(def.BuildingPreview, worldPos, def.SceneLayer, null, LayerMask.NameToLayer("Place"));
                var rot = visualizer.GetComponent<Rotatable>();
                if (rot != null) rot.SetOrientation(orientation);
                visualizer.SetActive(true);
            }

            // Capture settings from source building
            GameObject sourceSettingsObj = CopyMaterialsManager.GetSourceBuilding()?.gameObject;
            Tag capturedCopyGroupTag = Tag.Invalid;
            string capturedFacadeID = null;
            SimHashes capturedPrimaryElement = SimHashes.Vacuum;
            PrioritySetting capturedPriority = default(PrioritySetting);

            if (sourceSettingsObj != null)
            {
                var cbs = sourceSettingsObj.GetComponent<CopyBuildingSettings>();
                if (cbs != null) capturedCopyGroupTag = cbs.copyGroupTag;

                var pe = sourceSettingsObj.GetComponent<PrimaryElement>();
                if (pe != null)
                {
                    capturedPrimaryElement = pe.ElementID;
                    CopyMaterialsManager.Log($"Captured PrimaryElement from source: {pe.Element?.tag} id={capturedPrimaryElement}");
                }

                var p = sourceSettingsObj.GetComponent<Prioritizable>();
                if (p != null) capturedPriority = p.GetMasterPriority();
            }

            // Place blueprint
            var placed = def.TryPlace(visualizer, worldPos, orientation, selectedElements, facadeID, 0);
            if (placed != null)
            {
                ConstructableCleanup.Attach(
                    placed,
                    def,
                    cell,
                    orientation,
                    visualizer,
                    materialToApply: capturedPrimaryElement,
                    priorityToApply: capturedPriority,
                    facadeID: capturedFacadeID,
                    sourceSettingsObject: sourceSettingsObj,
                    copyGroupTag: capturedCopyGroupTag
                );
                CopyMaterialsManager.Log("Attached ConstructableCleanup to placed constructable.");
            }
        }
    }
}