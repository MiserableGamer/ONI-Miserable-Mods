using System;
using UnityEngine;

namespace ControlledPower.Components
{
    /// <summary>
    /// Virtual consumer on the diode's input circuit so vanilla GetWattsNeededWhenActive and
    /// wattsUsed sum include downstream load. WattsNeededWhenActive is set periodically from
    /// the output circuit's potential (in topological order); WattsUsed reflects current draw
    /// from PowerDiodeCapacityController.
    /// </summary>
    public class PowerDiodeInputConsumer : KMonoBehaviour, IEnergyConsumer
    {
        public const int PowerSortOrderValue = 1000;

        [MyCmpGet] private Building _building;
        [MyCmpGet] private KSelectable _selectable;
        [MyCmpGet] private Operational _operational;

        /// <summary>
        /// Set by CircuitManagerPatches in topological order to the output circuit's
        /// GetWattsNeededWhenActive (downstream potential).
        /// </summary>
        [SerializeField]
        private float _wattsNeededWhenActive;

        public int PowerSortOrder => PowerSortOrderValue;
        public int PowerCell { get; private set; }
        public ushort CircuitID { get; private set; }
        public bool IsVirtual => false;
        public object VirtualCircuitKey => null;

        public bool IsPowered
        {
            get => _operational != null && _operational.GetFlag(EnergyConsumer.PoweredFlag);
            set { if (_operational != null) _operational.SetFlag(EnergyConsumer.PoweredFlag, value); }
        }

        public bool IsConnected => CircuitID != CircuitManager.INVALID_ID;
        public string Name => _selectable != null ? _selectable.GetName() : name;

        /// <summary>
        /// Always 0 so the sim never allocates power to this consumer (no PowerFromGenerator/PowerFromBatteries).
        /// Displayed current load on the input circuit is augmented via GetWattsUsedByCircuit postfix instead.
        /// </summary>
        public float WattsUsed => 0f;

        public float WattsNeededWhenActive => _wattsNeededWhenActive;

        /// <summary>
        /// Called by CircuitManagerPatches.Sim200msFirst in topological order.
        /// </summary>
        internal void SetWattsNeededWhenActive(float value)
        {
            _wattsNeededWhenActive = value >= 0f ? value : 0f;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            if (_building == null)
                return;
            PowerCell = _building.GetPowerInputCell();
            CircuitID = CircuitManager.INVALID_ID;
            if (Game.Instance?.circuitManager != null)
                Game.Instance.circuitManager.Connect(this);
        }

        public override void OnCleanUp()
        {
            if (Game.Instance?.circuitManager != null)
                Game.Instance.circuitManager.Disconnect(this, true);
            base.OnCleanUp();
        }

        public void EnergySim200ms(float dt)
        {
            if (Game.Instance?.circuitManager == null)
                return;
            CircuitID = Game.Instance.circuitManager.GetCircuitID(this);
            if (!IsConnected)
                IsPowered = false;
        }

        public void SetConnectionStatus(CircuitManager.ConnectionStatus connection_status)
        {
            switch (connection_status)
            {
                case CircuitManager.ConnectionStatus.NotConnected:
                    IsPowered = false;
                    break;
                case CircuitManager.ConnectionStatus.Unpowered:
                    IsPowered = false;
                    break;
                case CircuitManager.ConnectionStatus.Powered:
                    IsPowered = true;
                    break;
            }
        }
    }
}
