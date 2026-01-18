using UnityEngine;

namespace ControlledExtraction.Components
{
    // Handles gas output via secondary conduit for Oil Well Cap
    public class GasOutputController : KMonoBehaviour
    {
        [MyCmpReq] private Storage storage;
        [MyCmpReq] private OilWellCap oilWellCap;
        [MyCmpReq] private Building building;

        private int outputCell = -1;
        private ConduitFlow gasFlow;

        protected override void OnSpawn()
        {
            base.OnSpawn();

            var secondaryOutput = GetComponent<ConduitSecondaryOutput>();
            if (secondaryOutput != null)
                outputCell = Grid.OffsetCell(building.GetCell(), secondaryOutput.portInfo.offset);

            gasFlow = Game.Instance.gasConduitFlow;
            gasFlow.AddConduitUpdater(ConduitUpdate, ConduitFlowPriority.Dispense);
        }

        protected override void OnCleanUp()
        {
            gasFlow?.RemoveConduitUpdater(ConduitUpdate);
            base.OnCleanUp();
        }

        private void ConduitUpdate(float dt)
        {
            if (outputCell < 0) return;

            var contents = gasFlow.GetContents(outputCell);
            if (contents.mass >= 1f) return;

            PrimaryElement gasInStorage = storage.FindPrimaryElement(oilWellCap.gasElement);
            if (gasInStorage == null || gasInStorage.Mass <= 0f) return;

            float available = Mathf.Min(gasInStorage.Mass, 1f - contents.mass);
            if (available <= 0f) return;

            float temperature = gasInStorage.Temperature;
            byte diseaseIdx = gasInStorage.DiseaseIdx;
            int diseaseCount = (int)(available / gasInStorage.Mass * gasInStorage.DiseaseCount);

            float added = gasFlow.AddElement(outputCell, oilWellCap.gasElement, available, temperature, diseaseIdx, diseaseCount);

            if (added > 0f)
            {
                gasInStorage.ModifyDiseaseCount(-diseaseCount, "GasOutputController");
                gasInStorage.Mass -= added;
            }
        }
    }
}
