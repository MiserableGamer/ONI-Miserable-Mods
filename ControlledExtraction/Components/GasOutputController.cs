using UnityEngine;

namespace ControlledExtraction.Components
{
    // Handles gas output via secondary conduit - uses conduit flow updater for performance
    public class GasOutputController : KMonoBehaviour
    {
        [MyCmpReq]
        private Storage storage;

        [MyCmpReq]
        private OilWellCap oilWellCap;

        [MyCmpReq]
        private Building building;

        private int outputCell = -1;
        private ConduitFlow gasFlow;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            
            // Find the secondary output cell
            var secondaryOutput = GetComponent<ConduitSecondaryOutput>();
            if (secondaryOutput != null)
            {
                outputCell = Grid.OffsetCell(building.GetCell(), secondaryOutput.portInfo.offset);
            }

            // Register with conduit flow system - much better than ISim200ms!
            gasFlow = Game.Instance.gasConduitFlow;
            gasFlow.AddConduitUpdater(ConduitUpdate, ConduitFlowPriority.Dispense);
        }

        protected override void OnCleanUp()
        {
            // Unregister from conduit flow system
            if (gasFlow != null)
            {
                gasFlow.RemoveConduitUpdater(ConduitUpdate);
            }
            base.OnCleanUp();
        }

        private void ConduitUpdate(float dt)
        {
            if (outputCell < 0) return;

            // Check if conduit has room
            var contents = gasFlow.GetContents(outputCell);
            if (contents.mass >= 1f) return; // Conduit full

            // Find gas in storage
            SimHashes gasElement = oilWellCap.gasElement;
            PrimaryElement gasInStorage = storage.FindPrimaryElement(gasElement);
            
            if (gasInStorage == null || gasInStorage.Mass <= 0f) return;

            // Calculate how much we can output (max 1kg per packet)
            float available = Mathf.Min(gasInStorage.Mass, 1f - contents.mass);
            if (available <= 0f) return;

            // Transfer from storage to conduit
            float temperature = gasInStorage.Temperature;
            byte diseaseIdx = gasInStorage.DiseaseIdx;
            int diseaseCount = (int)(available / gasInStorage.Mass * gasInStorage.DiseaseCount);

            float added = gasFlow.AddElement(outputCell, gasElement, available, temperature, diseaseIdx, diseaseCount);
            
            if (added > 0f)
            {
                gasInStorage.ModifyDiseaseCount(-diseaseCount, "GasOutputController");
                gasInStorage.Mass -= added;
                storage.Trigger((int)GameHashes.OnStorageChange, gasInStorage.gameObject);
            }
        }
    }
}
