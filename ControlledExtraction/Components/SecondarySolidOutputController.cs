using UnityEngine;

namespace ControlledExtraction.Components
{
    // Handles solid output via conveyor for secondary ports
    public class SecondarySolidOutputController : KMonoBehaviour
    {
        private const float VANILLA_SOLID_CAPACITY = 20f;
        private static float? cachedMaxMass = null;

        [MyCmpReq] private Storage storage;
        [MyCmpReq] private Building building;

        private CellOffset outputOffset;
        private SimHashes elementFilter = SimHashes.Void;
        private int outputCell = -1;
        private SolidConduitFlow solidFlow;

        public void Initialize(CellOffset offset, SimHashes element)
        {
            outputOffset = offset;
            elementFilter = element;
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            outputCell = Grid.OffsetCell(building.GetCell(), outputOffset);
            solidFlow = Game.Instance.solidConduitFlow;
            solidFlow.AddConduitUpdater(ConduitUpdate, ConduitFlowPriority.Dispense);
        }

        protected override void OnCleanUp()
        {
            solidFlow?.RemoveConduitUpdater(ConduitUpdate);
            base.OnCleanUp();
        }

        private void ConduitUpdate(float dt)
        {
            if (outputCell < 0) return;

            var conduit = solidFlow.GetConduit(outputCell);
            if (conduit.idx == -1) return;

            var contents = conduit.GetContents(solidFlow);
            if (contents.pickupableHandle.IsValid()) return;

            foreach (var item in storage.items)
            {
                if (item == null) continue;

                var primaryElement = item.GetComponent<PrimaryElement>();
                if (primaryElement == null) continue;

                if (elementFilter != SimHashes.Void && primaryElement.ElementID != elementFilter)
                    continue;

                var pickupable = item.GetComponent<Pickupable>();
                if (pickupable == null) continue;

                float massToTransfer = Mathf.Min(pickupable.PrimaryElement.Mass, GetMaxMass());
                if (massToTransfer <= 0f) continue;

                var takenPickupable = pickupable.Take(massToTransfer);
                if (takenPickupable != null)
                {
                    solidFlow.AddPickupable(outputCell, takenPickupable);
                    break;
                }
            }
        }

        // Auto-detect solid conduit capacity (for mods that increase it)
        private static float GetMaxMass()
        {
            if (cachedMaxMass.HasValue)
                return cachedMaxMass.Value;

            // Check WarpConduitSender storage (same method as Piped Everything)
            var prefab = Assets.GetPrefab(new Tag("WarpConduitSender"));
            if (prefab != null)
            {
                var warpStorage = prefab.GetComponent<Storage>();
                if (warpStorage != null && warpStorage.capacityKg > 0)
                {
                    cachedMaxMass = warpStorage.capacityKg / 5f;
                    ControlledExtractionMod.Log($"Detected solid conduit capacity: {cachedMaxMass.Value} kg");
                    return cachedMaxMass.Value;
                }
            }

            cachedMaxMass = VANILLA_SOLID_CAPACITY;
            ControlledExtractionMod.Log($"Using vanilla solid conduit capacity: {VANILLA_SOLID_CAPACITY} kg");
            return VANILLA_SOLID_CAPACITY;
        }
    }
}
