using UnityEngine;

namespace ControlledExtraction.Components
{
    // Generic liquid output controller for secondary ports on any building
    public class SecondaryLiquidOutputController : KMonoBehaviour
    {
        [MyCmpReq]
        private Storage storage;

        [MyCmpReq]
        private Building building;

        public SimHashes liquidElement = SimHashes.DirtyWater;

        private int outputCell = -1;
        private ConduitFlow liquidFlow;

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
            liquidFlow = Game.Instance.liquidConduitFlow;
            liquidFlow.AddConduitUpdater(ConduitUpdate, ConduitFlowPriority.Dispense);
        }

        protected override void OnCleanUp()
        {
            if (liquidFlow != null)
            {
                liquidFlow.RemoveConduitUpdater(ConduitUpdate);
            }
            base.OnCleanUp();
        }

        private void ConduitUpdate(float dt)
        {
            if (outputCell < 0) return;

            // Check if conduit has room
            var contents = liquidFlow.GetContents(outputCell);
            if (contents.mass >= 10f) return; // Liquid pipes hold 10kg max

            // Find liquid in storage
            PrimaryElement liquidInStorage = storage.FindPrimaryElement(liquidElement);
            if (liquidInStorage == null || liquidInStorage.Mass <= 0f) return;

            // Calculate how much we can output
            float available = Mathf.Min(liquidInStorage.Mass, 10f - contents.mass);
            if (available <= 0f) return;

            // Transfer from storage to conduit
            float temperature = liquidInStorage.Temperature;
            byte diseaseIdx = liquidInStorage.DiseaseIdx;
            int diseaseCount = (int)(available / liquidInStorage.Mass * liquidInStorage.DiseaseCount);

            float added = liquidFlow.AddElement(outputCell, liquidElement, available, temperature, diseaseIdx, diseaseCount);
            
            if (added > 0f)
            {
                liquidInStorage.ModifyDiseaseCount(-diseaseCount, "SecondaryLiquidOutputController");
                liquidInStorage.Mass -= added;
                storage.Trigger((int)GameHashes.OnStorageChange, liquidInStorage.gameObject);
            }
        }
    }
}
