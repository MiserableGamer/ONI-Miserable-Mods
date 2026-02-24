using HarmonyLib;
using UnityEngine;

namespace ControlledVisuals.Patches
{
    // Fix vanilla bug: conveyor items sometimes render in front of drywall because their
    // KBatchedAnimController.sceneLayer is not SolidConduitContents. We re-apply the layer
    // every tick for every item in the conveyor so they always draw behind buildings/drywall.
    // (Vanilla only updates position for *moving* items in RenderEveryTick; we fix layer for
    // all conduit contents, including stationary items.)
    [HarmonyPatch(typeof(SolidConduitFlow), nameof(SolidConduitFlow.RenderEveryTick))]
    public static class SolidConduitFlow_RenderEveryTick_Patch
    {
        public static void Postfix(SolidConduitFlow __instance)
        {
            var soaInfo = __instance.GetSOAInfo();
            for (int i = 0; i < soaInfo.NumEntries; i++)
            {
                var conduit = soaInfo.GetConduit(i);
                int cell = conduit.GetCell(__instance);
                var contents = __instance.GetContents(cell);
                if (!contents.pickupableHandle.IsValid())
                    continue;

                var pickupable = __instance.GetPickupable(contents.pickupableHandle);
                if (pickupable == null)
                    continue;

                var kbac = pickupable.GetComponent<KBatchedAnimController>();
                if (kbac == null)
                    continue;

                if (kbac.sceneLayer != Grid.SceneLayer.SolidConduitContents)
                    kbac.SetSceneLayer(Grid.SceneLayer.SolidConduitContents);
            }
        }
    }
}
