using KSerialization;
using UnityEngine;

namespace ControlledPower.Components
{
    // Sizes diode battery capacity/charge from downstream circuit load.
    [SerializationConfig(MemberSerialization.OptIn)]
    public class PowerDiodeCapacityController : KMonoBehaviour
    {
        private const float LoadMargin = 1.05f;
        private const float MinCapacityJ = 1000f;  // 1 kJ floor
        private const float MinChargeW = 1f;
        private const float FallbackMaxWattage = 2000f;
        private const float SimStepSeconds = 0.2f;
        private const float LoadDecayHalfLifeSeconds = 20f;

        [MyCmpGet] private Building _building;
        [MyCmpGet] private Battery _battery;

        [Serialize]
        [SerializeField]
        private float _lastSeenLoadW;

        // Cached total current load on this diode's output circuit.
        [Serialize]
        [SerializeField]
        private float _storedOutputCircuitLoadW;

        // Exposed for UI/debug (Circuit Overview).
        public float StoredOutputCircuitLoadW => _storedOutputCircuitLoadW;

        // Applies cached output load to battery capacity/charge and stores it.
        internal void ApplyOutputCircuitLoad(float outputCircuitWatts)
        {
            if (_building == null || _battery == null || Game.Instance?.circuitManager == null)
                return;

            int inputCell = _building.GetPowerInputCell();
            ushort inputCircuitId = (ushort)Game.Instance.circuitManager.GetCircuitID(inputCell);
            float maxW = Game.Instance.circuitManager.GetMaxSafeWattageForCircuit(inputCircuitId);
            if (maxW <= 0f)
                maxW = FallbackMaxWattage;

            if (outputCircuitWatts > 0f)
            {
                // Fast-up, slow-down tracking:
                // - rises immediately to match higher demand
                // - decays gradually when demand stays lower
                float decayPerTick = Mathf.Pow(0.5f, SimStepSeconds / LoadDecayHalfLifeSeconds);
                float decayedRememberedLoad = _lastSeenLoadW * decayPerTick;
                _lastSeenLoadW = Mathf.Max(outputCircuitWatts, decayedRememberedLoad);
                _storedOutputCircuitLoadW = outputCircuitWatts;
            }
            else if (_lastSeenLoadW > 0f)
            {
                float decayPerTick = Mathf.Pow(0.5f, SimStepSeconds / LoadDecayHalfLifeSeconds);
                _lastSeenLoadW *= decayPerTick;
            }

            float loadW = outputCircuitWatts > 0f
                ? _lastSeenLoadW
                : _lastSeenLoadW;
            float maxCapacityJ = Mathf.Min(_building.Def.GeneratorWattageRating * 1000f, maxW * 1000f);
            float targetW = loadW > 0f
                ? Mathf.Clamp(loadW * LoadMargin, MinChargeW, maxW)
                : maxW;
            float targetJ = Mathf.Max(MinCapacityJ, targetW);
            targetJ = Mathf.Clamp(targetJ, MinCapacityJ, maxCapacityJ);

            _battery.capacity = targetJ;
            _battery.chargeWattage = Mathf.Max(MinChargeW, targetW);
        }
    }
}
