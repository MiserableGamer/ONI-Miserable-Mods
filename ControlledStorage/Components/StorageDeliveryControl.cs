using KSerialization;
using UnityEngine;

namespace ControlledStorage
{
    // Dupe/sweeper deposit and extract toggles; patches enforce at chore/fetch level.
    [SerializationConfig(MemberSerialization.OptIn)]
    [AddComponentMenu("KMonoBehaviour/scripts/StorageDeliveryControl")]
    public class StorageDeliveryControl : KMonoBehaviour, ISaveLoadable
    {
        // Hash for CopySettings event (from CopyBuildingSettings.ApplyCopy)
        private const int CopySettingsHash = -905833192;
        
        [Serialize]
        private bool allowDupeDeposit = true;
        
        [Serialize]
        private bool allowDupeExtract = true;
        
        [Serialize]
        private bool allowSweeperDeposit = true;
        
        [Serialize]
        private bool allowSweeperExtract = true;

        private Storage storage;

        public bool AllowDupeDeposit
        {
            get => allowDupeDeposit;
            set
            {
                if (allowDupeDeposit != value)
                {
                    allowDupeDeposit = value;
                    UpdateStorageSettings();
                }
            }
        }

        public bool AllowDupeExtract
        {
            get => allowDupeExtract;
            set
            {
                if (allowDupeExtract != value)
                {
                    allowDupeExtract = value;
                    UpdateStorageSettings();
                }
            }
        }

        public bool AllowSweeperDeposit
        {
            get => allowSweeperDeposit;
            set
            {
                if (allowSweeperDeposit != value)
                {
                    allowSweeperDeposit = value;
                }
            }
        }

        public bool AllowSweeperExtract
        {
            get => allowSweeperExtract;
            set
            {
                if (allowSweeperExtract != value)
                {
                    allowSweeperExtract = value;
                    UpdateStorageSettings();
                }
            }
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            storage ??= GetComponent<Storage>();
            UpdateStorageSettings();
        }

        // Storage.allowItemRemoval blocks all extraction at the storage level - must be true if EITHER dupe or sweeper can extract.
        // Our patches (FindFetchTarget, IsFetchablePickup, CouldBePickedUpByTransferArm) filter who actually gets the errand.
        private void UpdateStorageSettings()
        {
            if (storage == null) return;
            storage.allowItemRemoval = allowDupeExtract || allowSweeperExtract;
        }

        // Used by Copy Delivery Settings so only our four toggles are copied.
        public static void CopyDeliveryControlOnly(StorageDeliveryControl source, StorageDeliveryControl target)
        {
            if (source == null || target == null) return;
            target.AllowDupeDeposit = source.AllowDupeDeposit;
            target.AllowDupeExtract = source.AllowDupeExtract;
            target.AllowSweeperDeposit = source.AllowSweeperDeposit;
            target.AllowSweeperExtract = source.AllowSweeperExtract;
        }

        // CopySettings event from CopyBuildingSettings.
        private void OnCopySettings(object data)
        {
            var source = ((GameObject)data)?.GetComponent<StorageDeliveryControl>();
            if (source != null)
            {
                AllowDupeDeposit = source.AllowDupeDeposit;
                AllowDupeExtract = source.AllowDupeExtract;
                AllowSweeperDeposit = source.AllowSweeperDeposit;
                AllowSweeperExtract = source.AllowSweeperExtract;
            }
        }

        private static readonly EventSystem.IntraObjectHandler<StorageDeliveryControl> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<StorageDeliveryControl>(
                (component, data) => component.OnCopySettings(data)
            );

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe(CopySettingsHash, OnCopySettingsDelegate);
        }

        protected override void OnCleanUp()
        {
            Unsubscribe(CopySettingsHash, OnCopySettingsDelegate);
            base.OnCleanUp();
        }
    }
}
