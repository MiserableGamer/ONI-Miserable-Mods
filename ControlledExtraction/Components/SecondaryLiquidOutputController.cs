using UnityEngine;

namespace ControlledExtraction.Components
{
    // Handles liquid output via secondary conduit for any building
    public class SecondaryLiquidOutputController : KMonoBehaviour
    {
        [MyCmpReq] private Storage storage;
        [MyCmpReq] private Building building;

        public SimHashes liquidElement = SimHashes.DirtyWater;

        private int outputCell = -1;
        private ConduitFlow liquidFlow;

        protected override void OnSpawn()
        {
            base.OnSpawn();

            var secondaryOutput = GetComponent<ConduitSecondaryOutput>();
            if (secondaryOutput != null)
                outputCell = Grid.OffsetCell(building.GetCell(), secondaryOutput.portInfo.offset);

            liquidFlow = Game.Instance.liquidConduitFlow;
            liquidFlow.AddConduitUpdater(ConduitUpdate, ConduitFlowPriority.Dispense);
        }

        protected override void OnCleanUp()
        {
            liquidFlow?.RemoveConduitUpdater(ConduitUpdate);
            base.OnCleanUp();
        }

        private void ConduitUpdate(float dt)
        {
            if (outputCell < 0) return;

            var contents = liquidFlow.GetContents(outputCell);
            if (contents.mass >= 10f) return;

            PrimaryElement liquidInStorage = storage.FindPrimaryElement(liquidElement);
            if (liquidInStorage == null || liquidInStorage.Mass <= 0f) return;

            float available = Mathf.Min(liquidInStorage.Mass, 10f - contents.mass);
            if (available <= 0f) return;

            float temperature = liquidInStorage.Temperature;
            byte diseaseIdx = liquidInStorage.DiseaseIdx;
            int diseaseCount = (int)(available / liquidInStorage.Mass * liquidInStorage.DiseaseCount);

            float added = liquidFlow.AddElement(outputCell, liquidElement, available, temperature, diseaseIdx, diseaseCount);

            if (added > 0f)
            {
                liquidInStorage.ModifyDiseaseCount(-diseaseCount, "SecondaryLiquidOutputController");
                liquidInStorage.Mass -= added;
            }
        }
    }
}
