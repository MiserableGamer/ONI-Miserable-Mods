using UnityEngine;

namespace ControlledPower.Components
{
    // Virtual consumer registered on the input circuit.
    // Used for diode-specific potential propagation without affecting actual power draw.
    public class PowerDiodeInputConsumer : KMonoBehaviour, IEnergyConsumer
    {
        public const int PowerSortOrderValue = 1000;

        [MyCmpGet] private Building _building;
        [MyCmpGet] private KSelectable _selectable;
        [MyCmpGet] private Operational _operational;

        // Set by CircuitManagerPatches during Sim200msFirst.
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

        // Always 0 so this virtual consumer never changes real circuit current draw.
        public float WattsUsed => 0f;

        public float WattsNeededWhenActive => _wattsNeededWhenActive;

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

        public void SetConnectionStatus(CircuitManager.ConnectionStatus connectionStatus)
        {
            switch (connectionStatus)
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
