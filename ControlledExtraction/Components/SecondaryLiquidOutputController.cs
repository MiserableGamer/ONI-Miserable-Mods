using UnityEngine;

namespace ControlledExtraction.Components
{
    // Handles liquid output via secondary conduit for any building.
    // Falls back to world emission when no liquid pipe is connected.
    public class SecondaryLiquidOutputController : KMonoBehaviour
    {
#pragma warning disable CS0649
        [MyCmpReq] private Storage storage;
        [MyCmpReq] private Building building;
#pragma warning restore CS0649

        public SimHashes liquidElement = SimHashes.DirtyWater;

        // When true, only output the specific liquidElement.
        // When false, output any liquid found in storage.
        public bool filterByElement = true;

        // When >= 0, use this storage index instead of the auto-resolved first one.
        // Needed for buildings with multiple Storage components (e.g. Ice Kettle).
        public int storageIndex = -1;

        private int outputCell = -1;
        private ConduitFlow liquidFlow;

        protected override void OnSpawn()
        {
            base.OnSpawn();

            if (storageIndex >= 0)
            {
                var storages = GetComponents<Storage>();
                if (storageIndex < storages.Length)
                    storage = storages[storageIndex];
            }

            foreach (var output in GetComponents<ConduitSecondaryOutput>())
            {
                if (output.portInfo.conduitType == ConduitType.Liquid)
                {
                    outputCell = Grid.OffsetCell(building.GetCell(), output.portInfo.offset);
                    break;
                }
            }

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

            PrimaryElement liquidInStorage = FindLiquid();
            if (liquidInStorage == null || liquidInStorage.Mass <= 0f) return;

            SimHashes elementHash = liquidInStorage.ElementID;

            if (IsConduitConnected())
            {
                var contents = liquidFlow.GetContents(outputCell);
                if (contents.mass >= 10f) return;

                // Can only add the same element or vacuum
                if (contents.element != elementHash && contents.element != SimHashes.Vacuum && contents.mass > 0f)
                    return;

                float available = Mathf.Min(liquidInStorage.Mass, 10f - contents.mass);
                if (available <= 0f) return;

                float temperature = liquidInStorage.Temperature;
                byte diseaseIdx = liquidInStorage.DiseaseIdx;
                int diseaseCount = (int)(available / liquidInStorage.Mass * liquidInStorage.DiseaseCount);

                float added = liquidFlow.AddElement(outputCell, elementHash, available, temperature, diseaseIdx, diseaseCount);
                if (added > 0f)
                {
                    liquidInStorage.ModifyDiseaseCount(-diseaseCount, "SecondaryLiquidOutput");
                    liquidInStorage.Mass -= added;
                }
            }
            else
            {
                // No conduit connected - emit to world like vanilla
                int emitCell = Grid.PosToCell(transform.GetPosition());
                SimMessages.AddRemoveSubstance(emitCell, elementHash,
                    CellEventLogger.Instance.Dumpable,
                    liquidInStorage.Mass, liquidInStorage.Temperature,
                    liquidInStorage.DiseaseIdx, liquidInStorage.DiseaseCount);
                liquidInStorage.ModifyDiseaseCount(-liquidInStorage.DiseaseCount, "SecondaryLiquidOutput.Fallback");
                liquidInStorage.Mass = 0f;
            }
        }

        private PrimaryElement FindLiquid()
        {
            if (filterByElement)
                return storage.FindPrimaryElement(liquidElement);

            foreach (var item in storage.items)
            {
                if (item == null) continue;
                var pe = item.GetComponent<PrimaryElement>();
                if (pe != null && pe.Mass > 0f && pe.Element.IsLiquid)
                    return pe;
            }
            return null;
        }

        private bool IsConduitConnected()
        {
            var obj = Grid.Objects[outputCell, (int)ObjectLayer.LiquidConduit];
            return obj != null && obj.GetComponent<BuildingComplete>() != null;
        }
    }
}
