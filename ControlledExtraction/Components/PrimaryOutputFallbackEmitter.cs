using UnityEngine;

namespace ControlledExtraction.Components
{
    // Emits stored elements to the world when the primary conduit output is not connected.
    // Also overrides vanilla operational flags so the building doesn't stop operating
    // when no conduit is connected. Runs at Last priority to override RequireOutputs
    // (First priority) and ConduitDispenser (Dispense priority) within the same tick.
    public class PrimaryOutputFallbackEmitter : KMonoBehaviour
    {
#pragma warning disable CS0649
        [MyCmpReq] private Storage storage;
        [MyCmpReq] private Building building;
        [MyCmpReq] private Operational operational;
#pragma warning restore CS0649

        public ConduitType conduitType;
        public SimHashes element;

        private int outputCell = -1;

        public override void OnSpawn()
        {
            base.OnSpawn();
            outputCell = building.GetUtilityOutputCell();

            if (conduitType == ConduitType.Gas)
                Game.Instance.gasConduitFlow.AddConduitUpdater(ConduitUpdate, ConduitFlowPriority.LastPostUpdate);
            else if (conduitType == ConduitType.Liquid)
                Game.Instance.liquidConduitFlow.AddConduitUpdater(ConduitUpdate, ConduitFlowPriority.LastPostUpdate);
        }

        public override void OnCleanUp()
        {
            if (conduitType == ConduitType.Gas)
                Game.Instance.gasConduitFlow?.RemoveConduitUpdater(ConduitUpdate);
            else if (conduitType == ConduitType.Liquid)
                Game.Instance.liquidConduitFlow?.RemoveConduitUpdater(ConduitUpdate);
            base.OnCleanUp();
        }

        private void ConduitUpdate(float dt)
        {
            if (outputCell < 0) return;

            // Override vanilla operational flags - building should work without conduit.
            // RequireOutputs (at First priority) and ConduitDispenser (at Dispense priority)
            // both set these flags to false when disconnected. We run at Last priority
            // to override them within the same tick.
            operational.SetFlag(ConduitDispenser.outputConduitFlag, true);
            operational.SetFlag(RequireOutputs.outputConnectedFlag, true);

            if (IsConduitConnected()) return;

            // Conduit not connected - emit stored element to world like vanilla
            PrimaryElement stored = storage.FindPrimaryElement(element);
            if (stored == null || stored.Mass <= 0f) return;

            int emitCell = Grid.PosToCell(transform.GetPosition());
            SimMessages.AddRemoveSubstance(emitCell, element,
                CellEventLogger.Instance.Dumpable,
                stored.Mass, stored.Temperature,
                stored.DiseaseIdx, stored.DiseaseCount);
            stored.ModifyDiseaseCount(-stored.DiseaseCount, "PrimaryFallback");
            stored.Mass = 0f;
        }

        private bool IsConduitConnected()
        {
            int layer = conduitType == ConduitType.Gas
                ? (int)ObjectLayer.GasConduit
                : (int)ObjectLayer.LiquidConduit;
            var obj = Grid.Objects[outputCell, layer];
            return obj != null && obj.GetComponent<BuildingComplete>() != null;
        }
    }
}
