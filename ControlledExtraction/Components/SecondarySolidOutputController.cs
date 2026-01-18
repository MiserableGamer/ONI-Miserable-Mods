using UnityEngine;

namespace ControlledExtraction.Components
{
    // Solid output controller for secondary conveyor ports
    public class SecondarySolidOutputController : KMonoBehaviour
    {
        // Cached max mass for solid conduits - detected once at runtime
        private static float? cachedSolidMaxMass = null;

        [MyCmpReq]
        private Storage storage;

        [MyCmpReq]
        private Building building;

        private CellOffset outputOffset;
        private SimHashes elementFilter = SimHashes.Void;
        private int outputCell = -1;
        private SolidConduitFlow solidFlow;

        // Auto-detect solid conduit capacity from game data
        private static float GetSolidMaxMass()
        {
            if (cachedSolidMaxMass.HasValue)
                return cachedSolidMaxMass.Value;

            // Method 1: Check SolidTransferArm (Auto-Sweeper) storage - it holds 5 conduit loads
            var sweeper = Assets.GetBuildingDef("SolidTransferArm");
            if (sweeper != null)
            {
                // Auto-sweeper has storage for 1000kg vanilla, which is 50 pickups of 20kg
                // But more reliably, check the SolidConduitInbox (Conveyor Loader)
            }

            // Method 2: Check Conveyor Loader - its capacity relates to conduit capacity
            var loaderDef = Assets.GetBuildingDef("SolidConduitInbox");
            if (loaderDef != null)
            {
                // Conveyor Loader stores 1000kg vanilla = 50 conduit loads of 20kg
                // So maxMass = loaderCapacity / 50
                // But this isn't perfectly reliable either
            }

            // Method 3: Check the teleporter/warp storage like Piped Everything does
            var warpDef = Assets.GetBuildingDef("WarpConduitSender");
            if (warpDef != null)
            {
                // Warp conduit stores 100kg = 5 conduit loads, so maxMass = warpCapacity / 5
                var prefab = Assets.GetPrefab(new Tag("WarpConduitSender"));
                if (prefab != null)
                {
                    var warpStorage = prefab.GetComponent<Storage>();
                    if (warpStorage != null && warpStorage.capacityKg > 0)
                    {
                        cachedSolidMaxMass = warpStorage.capacityKg / 5f;
                        ControlledExtractionMod.Log($"Auto-detected solid conduit capacity: {cachedSolidMaxMass.Value} kg (from WarpConduitSender)");
                        return cachedSolidMaxMass.Value;
                    }
                }
            }

            // Fallback: Use vanilla default (20kg)
            cachedSolidMaxMass = 20f;
            ControlledExtractionMod.Log($"Using vanilla solid conduit capacity: 20 kg");
            return 20f;
        }

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
            if (solidFlow != null)
            {
                solidFlow.RemoveConduitUpdater(ConduitUpdate);
            }
            base.OnCleanUp();
        }

        private void ConduitUpdate(float dt)
        {
            if (outputCell < 0) return;

            // Check if conduit exists and has room
            var conduit = solidFlow.GetConduit(outputCell);
            if (conduit.idx == -1) return;

            var contents = conduit.GetContents(solidFlow);
            if (contents.pickupableHandle.IsValid()) return; // Already has something

            // Find matching item in storage
            foreach (var item in storage.items)
            {
                if (item == null) continue;

                var primaryElement = item.GetComponent<PrimaryElement>();
                if (primaryElement == null) continue;

                // Check element filter
                if (elementFilter != SimHashes.Void && primaryElement.ElementID != elementFilter)
                    continue;

                var pickupable = item.GetComponent<Pickupable>();
                if (pickupable == null) continue;

                // Take from storage and add to conduit (auto-detected or user-configured capacity)
                float maxMass = GetSolidMaxMass();
                float massToTransfer = Mathf.Min(pickupable.PrimaryElement.Mass, maxMass);
                if (massToTransfer <= 0f) continue;

                var takenPickupable = pickupable.Take(massToTransfer);
                if (takenPickupable != null)
                {
                    solidFlow.AddPickupable(outputCell, takenPickupable);
                    break;
                }
            }
        }
    }
}
