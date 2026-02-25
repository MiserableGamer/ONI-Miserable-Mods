using KSerialization;
using UnityEngine;

namespace ControlledPower.Components
{
    /// <summary>
    /// Sets the diode's battery capacity and charge rate from the output circuit load + 5%,
    /// capped by the input circuit's wire rating. Receives the load from PowerDiodeLogicLink (which
    /// uses a per-frame cache so the value is cumulative across multiple diodes). Stores the
    /// output circuit's total load so it persists and is available for display / logic.
    /// </summary>
    [SerializationConfig(MemberSerialization.OptIn)]
    public class PowerDiodeCapacityController : KMonoBehaviour
    {
        private const float LoadMargin = 1.05f;
        private const float MinCapacityJ = 1000f;  // 1 kJ floor
        private const float MinChargeW = 1f;
        private const float FallbackMaxWattage = 2000f;

        [MyCmpGet] private Building building;
        [MyCmpGet] private Battery battery;

        [Serialize]
        [SerializeField]
        private float _lastSeenLoadW;

        /// <summary>
        /// This diode's output circuit's total load (watts), updated from the per-frame cache.
        /// Cumulative in a chain: each diode stores the load of its output segment (which already
        /// includes downstream draws from last frame), so the next diode left reads that via the cache.
        /// </summary>
        [Serialize]
        [SerializeField]
        private float _storedOutputCircuitLoadW;

        /// <summary>
        /// Exposed for UI/debug (e.g. Circuit Overview in the details sidescreen).
        /// </summary>
        public float StoredOutputCircuitLoadW => _storedOutputCircuitLoadW;

        /// <summary>
        /// Called by PowerDiodeLogicLink.Sim200ms with the cached watts for our output circuit.
        /// Applies that load to battery capacity/charge and stores it.
        /// </summary>
        internal void ApplyOutputCircuitLoad(float outputCircuitWatts)
        {
            if (building == null || battery == null || Game.Instance?.circuitManager == null)
                return;

            int inputCell = building.GetPowerInputCell();
            ushort inputCircuitId = (ushort)Game.Instance.circuitManager.GetCircuitID(inputCell);
            float maxW = Game.Instance.circuitManager.GetMaxSafeWattageForCircuit(inputCircuitId);
            if (maxW <= 0f)
                maxW = FallbackMaxWattage;

            if (outputCircuitWatts > 0f)
            {
                _lastSeenLoadW = outputCircuitWatts;
                _storedOutputCircuitLoadW = outputCircuitWatts;
            }

            float loadW = outputCircuitWatts > 0f ? outputCircuitWatts : _lastSeenLoadW;
            float maxCapacityJ = Mathf.Min(building.Def.GeneratorWattageRating * 1000f, maxW * 1000f);
            float targetW = loadW > 0f
                ? Mathf.Clamp(loadW * LoadMargin, MinChargeW, maxW)
                : maxW;
            float targetJ = Mathf.Max(MinCapacityJ, targetW);
            targetJ = Mathf.Clamp(targetJ, MinCapacityJ, maxCapacityJ);

            battery.capacity = targetJ;
            battery.chargeWattage = Mathf.Max(MinChargeW, targetW);
        }
    }
}
