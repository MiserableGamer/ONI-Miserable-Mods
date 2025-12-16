using System.Collections;
using UnityEngine;

namespace CopyMaterials.Logic
{
    public class ConstructableCleanup : KMonoBehaviour
    {
        private BuildingDef def;
        private int originCell;
        private Orientation orientation;
        private GameObject visualizerToRemove;

        private SimHashes materialToApply = SimHashes.Vacuum;

        public static void Attach(
            GameObject placed,
            BuildingDef def,
            int originCell,
            Orientation orientation,
            GameObject visualizer,
            SimHashes materialToApply
        )
        {
            var cleanup = placed.AddComponent<ConstructableCleanup>();
            cleanup.def = def;
            cleanup.originCell = originCell;
            cleanup.orientation = orientation;
            cleanup.visualizerToRemove = visualizer;
            cleanup.materialToApply = materialToApply;
        }

        private void Update()
        {
            GameObject completeGO = Grid.Objects[originCell, (int)ObjectLayer.Building];
            if (completeGO != null)
            {
                var bc = completeGO.GetComponent<BuildingComplete>();
                if (bc != null && bc.Def.PrefabID == def.PrefabID)
                {
                    // Apply material
                    if (materialToApply != SimHashes.Vacuum)
                    {
                        var pe = completeGO.GetComponent<PrimaryElement>();
                        if (pe != null)
                            pe.ElementID = materialToApply;
                    }

                    // Destroy ghost visualizer with delay (fixes ghost)
                    if (visualizerToRemove != null)
                    {
                        Game.Instance.StartCoroutine(DelayedDestroyVisualizer(visualizerToRemove, 0.25f));
                    }

                    // Popup
                    Vector3 pos = Grid.CellToPosCCC(originCell, Grid.SceneLayer.Building);
                    PopFXManager.Instance.SpawnFX(
                        PopFXManager.Instance.sprite_Plus,
                        "Materials applied",
                        null,
                        pos,
                        2f
                    );

                    Destroy(this);
                }
            }
        }

        private IEnumerator DelayedDestroyVisualizer(GameObject viz, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (viz != null)
            {
                Util.KDestroyGameObject(viz);
            }
        }
    }
}