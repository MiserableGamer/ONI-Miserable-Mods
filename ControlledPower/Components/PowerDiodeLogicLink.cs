using System.Collections.Generic;

namespace ControlledPower.Components
{
    // Tracks all spawned diodes and exposes input/output circuit IDs for patch logic.
    public class PowerDiodeLogicLink : KMonoBehaviour
    {
        internal static readonly List<PowerDiodeLogicLink> LinkedDiodes = new List<PowerDiodeLogicLink>();

        [MyCmpGet] private Building _building;

        public override void OnSpawn()
        {
            base.OnSpawn();
            if (_building != null && !LinkedDiodes.Contains(this))
                LinkedDiodes.Add(this);
        }

        public override void OnCleanUp()
        {
            LinkedDiodes.Remove(this);
            base.OnCleanUp();
        }

        internal bool GetCircuitIds(out ushort inputCircuitId, out ushort outputCircuitId)
        {
            inputCircuitId = ushort.MaxValue;
            outputCircuitId = ushort.MaxValue;
            if (_building == null || Game.Instance?.circuitManager == null)
                return false;
            int inputCell = _building.GetPowerInputCell();
            int outputCell = _building.GetPowerOutputCell();
            inputCircuitId = (ushort)Game.Instance.circuitManager.GetCircuitID(inputCell);
            outputCircuitId = (ushort)Game.Instance.circuitManager.GetCircuitID(outputCell);
            return inputCircuitId != ushort.MaxValue && outputCircuitId != ushort.MaxValue;
        }
    }
}
