using UnityEngine;

namespace ControlledExtraction.Components
{
    // Handles gas output via secondary conduit for any building.
    // Falls back to world emission when no gas pipe is connected.
    public class SecondaryGasOutputController : KMonoBehaviour
    {
#pragma warning disable CS0649
        [MyCmpReq] private Storage storage;
        [MyCmpReq] private Building building;
#pragma warning restore CS0649

        public SimHashes gasElement = SimHashes.CarbonDioxide;

        private int outputCell = -1;
        private ConduitFlow gasFlow;

        protected override void OnSpawn()
        {
            base.OnSpawn();

            foreach (var output in GetComponents<ConduitSecondaryOutput>())
            {
                if (output.portInfo.conduitType == ConduitType.Gas)
                {
                    outputCell = Grid.OffsetCell(building.GetCell(), output.portInfo.offset);
                    break;
                }
            }

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

            PrimaryElement gasInStorage = storage.FindPrimaryElement(gasElement);
            if (gasInStorage == null || gasInStorage.Mass <= 0f) return;

            if (IsConduitConnected())
            {
                var contents = gasFlow.GetContents(outputCell);
                if (contents.mass >= 1f) return;

                float available = Mathf.Min(gasInStorage.Mass, 1f - contents.mass);
                if (available <= 0f) return;

                float temperature = gasInStorage.Temperature;
                byte diseaseIdx = gasInStorage.DiseaseIdx;
                int diseaseCount = (int)(available / gasInStorage.Mass * gasInStorage.DiseaseCount);

                float added = gasFlow.AddElement(outputCell, gasElement, available, temperature, diseaseIdx, diseaseCount);
                if (added > 0f)
                {
                    gasInStorage.ModifyDiseaseCount(-diseaseCount, "SecondaryGasOutput");
                    gasInStorage.Mass -= added;
                }
            }
            else
            {
                // No conduit connected - emit to world like vanilla
                int emitCell = Grid.PosToCell(transform.GetPosition());
                SimMessages.AddRemoveSubstance(emitCell, gasElement,
                    CellEventLogger.Instance.Dumpable,
                    gasInStorage.Mass, gasInStorage.Temperature,
                    gasInStorage.DiseaseIdx, gasInStorage.DiseaseCount);
                gasInStorage.ModifyDiseaseCount(-gasInStorage.DiseaseCount, "SecondaryGasOutput.Fallback");
                gasInStorage.Mass = 0f;
            }
        }

        private bool IsConduitConnected()
        {
            var obj = Grid.Objects[outputCell, (int)ObjectLayer.GasConduit];
            return obj != null && obj.GetComponent<BuildingComplete>() != null;
        }
    }
}
