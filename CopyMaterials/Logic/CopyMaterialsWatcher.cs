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
        private Orientation orientation = Orientation.Neutral;
        private ObjectLayer objectLayer;
        private UtilityConnections storedConnections;

        public static CopyMaterialsWatcher Attach(Building target, string prefabID, SimHashes material, Orientation orientation = Orientation.Neutral, UtilityConnections connections = default(UtilityConnections))
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
            watcher.objectLayer = target.Def.ObjectLayer;
            watcher.storedConnections = connections;
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
            if (Grid.Objects[originalCell, (int)objectLayer] != null)
            {
                buildingRemoved = false;
            }

            if (buildingRemoved)
            {
                ConstructableCleanup.RefreshNeighbors(originalCell, (int)objectLayer);  // Update neighbors after removal
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

            var placed = def.TryPlace(null, worldPos, orientation, selectedElements, null, 0);
            if (placed != null)
            {
                ConstructableCleanup.Attach(
                    placed,
                    def,
                    originalCell,
                    orientation,
                    null,
                    material,
                    storedConnections
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