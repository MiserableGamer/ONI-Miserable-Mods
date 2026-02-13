using KSerialization;
using UnityEngine;

namespace ControlledMods.ResourceSensor
{
    // Persists Atmosphere / Storage / Conduits checkboxes and allows Copy Settings to copy them.
    [SerializationConfig(MemberSerialization.OptIn)]
    public sealed class ResourceSensorStorageScope : KMonoBehaviour, ISaveLoadable
    {
        [Serialize]
        public bool IncludeAtmosphere = true;

        [Serialize]
        public bool IncludeStorage = true;

        [Serialize]
        public bool IncludeConduits;

        private static readonly EventSystem.IntraObjectHandler<ResourceSensorStorageScope> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<ResourceSensorStorageScope>((cmp, data) => cmp.OnCopySettings(data));

        public override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
        }

        private void OnCopySettings(object data)
        {
            var srcGo = data as GameObject;
            if (srcGo == null) return;
            var src = srcGo.GetComponent<ResourceSensorStorageScope>();
            if (src != null)
            {
                IncludeAtmosphere = src.IncludeAtmosphere;
                IncludeStorage = src.IncludeStorage;
                IncludeConduits = src.IncludeConduits;
            }
        }
    }
}
