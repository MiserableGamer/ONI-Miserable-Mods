using System.Collections.Generic;
using KSerialization;
using UnityEngine;

namespace ControlledPower.Components
{
    // Tracks all spawned diodes and exposes input/output circuit IDs for patch logic.
    [SerializationConfig(MemberSerialization.OptIn)]
    public class PowerDiodeLogicLink : KMonoBehaviour
    {
        internal static readonly List<PowerDiodeLogicLink> LinkedDiodes = new List<PowerDiodeLogicLink>();
        private static readonly EventSystem.IntraObjectHandler<PowerDiodeLogicLink> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<PowerDiodeLogicLink>(
                (component, data) => component.OnCopySettings(data));

        [MyCmpGet] private Building _building;
        [Serialize]
        [SerializeField]
        private bool _isLogicLinkEnabled = true;

        public bool IsLogicLinkEnabled
        {
            get => _isLogicLinkEnabled;
            set => _isLogicLinkEnabled = value;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            Subscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
            if (_building != null && !LinkedDiodes.Contains(this))
                LinkedDiodes.Add(this);
        }

        public override void OnCleanUp()
        {
            Unsubscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
            LinkedDiodes.Remove(this);
            base.OnCleanUp();
        }

        private void OnCopySettings(object data)
        {
            GameObject source = data as GameObject;
            if (source == null)
                return;
            PowerDiodeLogicLink other = source.GetComponent<PowerDiodeLogicLink>();
            if (other != null)
                _isLogicLinkEnabled = other._isLogicLinkEnabled;
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
