using System.Collections.Generic;
using UnityEngine;

namespace ResourceLimpet
{
    public class ResourceLimpetComponent : KMonoBehaviour, ISim200ms
    {
        public static readonly HashedString OUTPUT_PORT_ID = (HashedString)"ResourceLimpetOutput";

        [MyCmpReq]
        private LogicPorts logicPorts;

        [MyCmpReq]
        private Building building;

        private Storage storage;

        [SerializeField]
        public float lowThreshold = 0.2f; // 20% of capacity

        [SerializeField]
        public float highThreshold = 0.8f; // 80% of capacity

        private bool lowSignalActive = false;
        private bool highSignalActive = false;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            FindAttachedStorage();
        }

        private void FindAttachedStorage()
        {
            if (building == null) return;

            int cell = Grid.PosToCell(gameObject);
            
            // Check the cell this building is on
            CheckCellForStorage(cell);
            
            // Also check adjacent cells
            foreach (int offset in new[] { 1, -1, Grid.WidthInCells, -Grid.WidthInCells })
            {
                int adjacentCell = cell + offset;
                if (Grid.IsValidCell(adjacentCell))
                {
                    CheckCellForStorage(adjacentCell);
                }
            }
        }

        private void CheckCellForStorage(int cell)
        {
            // Check all object layers for storage buildings
            // Use generic Storage component which works for all storage types (gas, liquid, solid)
            foreach (ObjectLayer layer in new[] { ObjectLayer.Building, ObjectLayer.FoundationTile })
            {
                GameObject obj = Grid.Objects[cell, (int)layer];
                if (obj != null)
                {
                    // Skip ourselves
                    if (obj == gameObject) continue;

                    var foundStorage = obj.GetComponent<Storage>();
                    if (foundStorage != null)
                    {
                        storage = foundStorage;
                        return; // Found storage, we're done
                    }
                }
            }
        }

        public void Sim200ms(float dt)
        {
            if (logicPorts == null) return;

            // Re-find storage periodically in case it was built after us
            if (storage == null)
            {
                FindAttachedStorage();
            }

            // No storage found
            if (storage == null) return;

            // Get current storage amount and capacity
            // Storage.MassStored() and Storage.capacityKg work for all storage types (gas, liquid, solid)
            float currentAmount = storage.MassStored();
            float capacity = storage.capacityKg;

            if (capacity <= 0f) return;

            float fillPercentage = currentAmount / capacity;

            // Check low threshold (channel 0)
            bool newLowSignal = fillPercentage <= lowThreshold;
            if (newLowSignal != lowSignalActive)
            {
                lowSignalActive = newLowSignal;
                UpdateRibbonOutput();
            }

            // Check high threshold (channel 1)
            bool newHighSignal = fillPercentage >= highThreshold;
            if (newHighSignal != highSignalActive)
            {
                highSignalActive = newHighSignal;
                UpdateRibbonOutput();
            }
        }

        private void UpdateRibbonOutput()
        {
            if (logicPorts == null) return;

            // Create ribbon signal with two channels
            // Channel 0 = low threshold, Channel 1 = high threshold
            int ribbonValue = 0;
            if (lowSignalActive)
            {
                ribbonValue |= 1 << 0; // Set bit 0
            }
            if (highSignalActive)
            {
                ribbonValue |= 1 << 1; // Set bit 1
            }

            logicPorts.SendSignal(OUTPUT_PORT_ID, ribbonValue);
        }
    }
}

