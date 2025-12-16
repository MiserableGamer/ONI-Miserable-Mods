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

        public static CopyMaterialsWatcher Attach(Building target, string prefabID, SimHashes material, Orientation orientation = Orientation.Neutral)
        {
            if (target == null) return null;

            GameObject root = Game.Instance?.gameObject ?? GameObject.Find("CopyMaterialsRoot");
            if (root == null)
            {
                root = new GameObject("CopyMaterialsRoot");
                UnityEngine.Object.DontDestroyOnLoad(root);
            }

            var watcher = root.AddComponent<CopyMaterialsWatcher>();
            watcher.originalCell = Grid.PosToCell(target);
            watcher.prefabID = prefabID;
            watcher.material = material;
            watcher.orientation = orientation;
            watcher.blueprintCreated = false;

            CopyMaterialsManager.Log($"Watcher attached for {prefabID} at cell {watcher.originalCell}");
            return watcher;
        }

        private void Update()
        {
            if (blueprintCreated)
            {
                Destroy(this);
                return;
            }

            bool buildingRemoved = true;
            if (Grid.Objects[originalCell, (int)ObjectLayer.Building] != null)
            {
                buildingRemoved = false;
            }

            if (buildingRemoved)
            {
                CreateBlueprintAtCell();
                blueprintCreated = true;
            }
        }

        private void CreateBlueprintAtCell()
        {
            var def = Assets.GetBuildingDef(prefabID);
            if (def == null)
            {
                CopyMaterialsManager.Warn($"BuildingDef not found for {prefabID}");
                return;
            }

            Vector3 worldPos = Grid.CellToPosCBC(originalCell, def.SceneLayer);
            IList<Tag> selectedElements = PlacementHelpers.BuildSelectedElementsFromMaterial(material);

            GameObject visualizer = null;
            if (def.BuildingPreview != null)
            {
                visualizer = GameUtil.KInstantiate(def.BuildingPreview, worldPos, def.SceneLayer);
                visualizer.SetActive(true);
                var rot = visualizer.GetComponent<Rotatable>();
                if (rot != null) rot.SetOrientation(orientation);
            }

            // Safe facadeID = null (avoids crash)
            var placed = def.TryPlace(visualizer, worldPos, orientation, selectedElements, null, 0);
            if (placed != null)
            {
                ConstructableCleanup.Attach(
                    placed,
                    def,
                    originalCell,
                    orientation,
                    visualizer,
                    material
                );
                CopyMaterialsManager.Log("Blueprint placed and cleanup attached");
            }
            else
            {
                CopyMaterialsManager.Warn("TryPlace failed");
            }
        }
    }
}