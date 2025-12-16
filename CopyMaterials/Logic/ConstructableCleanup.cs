using System;
using System.Collections;
using UnityEngine;
using static Grid.Restriction;

namespace CopyMaterials.Logic
{
    public class ConstructableCleanup : KMonoBehaviour
    {
        // … fields omitted for brevity …

        private void Update()
        {
            // … existing logic …
            if (foundCompleteNow)
            {
                CopyMaterialsManager.Log($"BuildingComplete found in area for cell={originCell}; applying settings");
                ApplySettingsHelper.ApplySettingsToBuiltObjects(
                    def, originCell, orientation, sourceSettingsObject,
                    materialToApply, priorityToApply, facadeID, copyGroupTag
                );
                RemoveLeftoverConstructablesStatic(originCell, def, orientation, visualizerToRemove, true);
                Game.Instance.StartCoroutine(DelayedDestroyVisualizer(visualizerToRemove, 0.25f));
                Destroy(this);
                return;
            }
            // … rest unchanged …
        }

        // RemoveLeftoverConstructablesStatic and DelayedDestroyVisualizer remain here
    }
}