using UnityEngine;

namespace ControlledExtraction.Components
{
    // Generic gas output controller for secondary ports on any building
    public class SecondaryGasOutputController : KMonoBehaviour
    {
        [MyCmpReq]
        private Storage storage;

        [MyCmpReq]
        private Building building;

        public SimHashes gasElement = SimHashes.CarbonDioxide;

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

            // Register with conduit flow system
            gasFlow = Game.Instance.gasConduitFlow;
            gasFlow.AddConduitUpdater(ConduitUpdate, ConduitFlowPriority.Dispense);
        }

        protected override void OnCleanUp()
        {
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
            if (contents.mass >= 1f) return;

            // Find gas in storage
            PrimaryElement gasInStorage = storage.FindPrimaryElement(gasElement);
            if (gasInStorage == null || gasInStorage.Mass <= 0f) return;

            // Calculate how much we can output
            float available = Mathf.Min(gasInStorage.Mass, 1f - contents.mass);
            if (available <= 0f) return;

            // Transfer from storage to conduit
            float temperature = gasInStorage.Temperature;
            byte diseaseIdx = gasInStorage.DiseaseIdx;
            int diseaseCount = (int)(available / gasInStorage.Mass * gasInStorage.DiseaseCount);

            float added = gasFlow.AddElement(outputCell, gasElement, available, temperature, diseaseIdx, diseaseCount);
            
            if (added > 0f)
            {
                gasInStorage.ModifyDiseaseCount(-diseaseCount, "SecondaryGasOutputController");
                gasInStorage.Mass -= added;
                storage.Trigger((int)GameHashes.OnStorageChange, gasInStorage.gameObject);
            }
        }
    }
}
