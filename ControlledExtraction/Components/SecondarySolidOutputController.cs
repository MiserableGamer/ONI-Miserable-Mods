using UnityEngine;

namespace ControlledExtraction.Components
{
    // Handles secondary solid conduit output with element filtering and network registration.
    // Vanilla SolidConduitDispenser.FindSuitableItem ignores elementFilter, so we handle
    // dispensing ourselves. When no conduit is connected, falls back to vanilla world emission.
    // Pattern: RailGunPayloadOpener (network reg) + PipedEverything (dispensing + filtering).
    public class SecondarySolidOutputController : KMonoBehaviour, ISecondaryOutput
    {
        private const float VANILLA_SOLID_CAPACITY = 20f;
        private static float? cachedMaxMass = null;

#pragma warning disable CS0649
        [MyCmpReq] private Storage storage;
        [MyCmpReq] private Building building;
#pragma warning restore CS0649

        [SerializeField]
        public CellOffset outputOffset;

        [SerializeField]
        public SimHashes elementFilter = SimHashes.Void;

        private int outputCell = -1;
        private SolidConduitFlow solidFlow;
        private FlowUtilityNetwork.NetworkItem networkItem;

        // For controlling AlgaeDistillery world emission based on connection
        private AlgaeDistillery algaeDistillery;
        private float originalEmitMass;

        public void Initialize(CellOffset offset, SimHashes element)
        {
            outputOffset = offset;
            elementFilter = element;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            outputCell = Grid.OffsetCell(building.NaturalBuildingCell(), outputOffset);
            solidFlow = Game.Instance.solidConduitFlow;

            // Register with solid conduit network so conveyor rails can connect
            networkItem = new FlowUtilityNetwork.NetworkItem(
                ConduitType.Solid, Endpoint.Source, outputCell, gameObject);
            Game.Instance.solidConduitSystem.AddToNetworks(outputCell, networkItem, true);

            solidFlow.AddConduitUpdater(ConduitUpdate, ConduitFlowPriority.Dispense);

            // Store original emit mass so we can restore it when disconnected
            algaeDistillery = GetComponent<AlgaeDistillery>();
            if (algaeDistillery != null)
                originalEmitMass = algaeDistillery.emitMass;
        }

        public override void OnCleanUp()
        {
            solidFlow?.RemoveConduitUpdater(ConduitUpdate);

            if (outputCell >= 0)
                Game.Instance.solidConduitSystem.RemoveFromNetworks(outputCell, networkItem, true);

            if (algaeDistillery != null)
                algaeDistillery.emitMass = originalEmitMass;

            base.OnCleanUp();
        }

        private void ConduitUpdate(float dt)
        {
            if (outputCell < 0) return;

            bool connected = IsConduitConnected();

            // When connected: suppress world emission so conduit gets the output
            // When disconnected: restore vanilla emission so dirt drops to ground
            if (algaeDistillery != null)
                algaeDistillery.emitMass = connected ? float.MaxValue : originalEmitMass;

            if (!connected) return;

            if (!solidFlow.IsConduitEmpty(outputCell)) return;

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

        private bool IsConduitConnected()
        {
            var conduitObj = Grid.Objects[outputCell, (int)ObjectLayer.SolidConduit];
            return conduitObj != null && conduitObj.GetComponent<BuildingComplete>() != null;
        }

        // ISecondaryOutput - tells the game this port exists for rendering and routing
        public bool HasSecondaryConduitType(ConduitType type)
        {
            return type == ConduitType.Solid;
        }

        public CellOffset GetSecondaryConduitOffset(ConduitType type)
        {
            return type == ConduitType.Solid ? outputOffset : CellOffset.none;
        }

        // Auto-detect solid conduit capacity (for mods that increase it)
        private static float GetMaxMass()
        {
            if (cachedMaxMass.HasValue)
                return cachedMaxMass.Value;

            var prefab = Assets.GetPrefab(new Tag("WarpConduitSender"));
            if (prefab != null)
            {
                var warpStorage = prefab.GetComponent<Storage>();
                if (warpStorage != null && warpStorage.capacityKg > 0)
                {
                    cachedMaxMass = warpStorage.capacityKg / 5f;
                    return cachedMaxMass.Value;
                }
            }

            cachedMaxMass = VANILLA_SOLID_CAPACITY;
            return VANILLA_SOLID_CAPACITY;
        }
    }
}
