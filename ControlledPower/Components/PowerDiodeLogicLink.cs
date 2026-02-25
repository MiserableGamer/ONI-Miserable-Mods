using System.Collections.Generic;
using UnityEngine;

namespace ControlledPower.Components
{
    /// <summary>
    /// Registers this diode so CircuitManager patches can add the output circuit's load and potential
    /// to the input circuit (and vice versa) for logic — wattage sensors on either side see combined values.
    /// No sidescreen: linking is always on.
    /// Diode draw/capacity is updated from CircuitManagerPatches.Sim200msFirst prefix (runs after previous
    /// Sim200msLast so GetWattsUsedByCircuit is cumulative; no ISim200ms here to avoid ordering issues).
    /// </summary>
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
