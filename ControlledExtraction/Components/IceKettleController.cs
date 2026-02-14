using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSerialization;
using ControlledExtraction.Options;

namespace ControlledExtraction.Components
{
    // Manages per-building meltable type selection for the Ice Kettle.
    // Supports multi-select via extra ManualDeliveryKG instances per selected type.
    [SerializationConfig(MemberSerialization.OptIn)]
    public class IceKettleController : KMonoBehaviour
    {
        private static readonly EventSystem.IntraObjectHandler<IceKettleController> OnCopySettingsHandler =
            new EventSystem.IntraObjectHandler<IceKettleController>((cmp, data) => cmp.OnCopySettings(data));

        private static Element[] enabledIceOres;

        [Serialize]
        private Tag[] selectedIces;

        private Storage kettleStorage;
        private Storage outputStorage;
        private ManualDeliveryKG originalIceMdkg;
        private List<ManualDeliveryKG> extraDeliveries = new List<ManualDeliveryKG>();

#pragma warning disable CS0649
        [MySmiReq]
        private IceKettle.Instance kettle;
#pragma warning restore CS0649

        public static Element[] GetEnabledIceOres() => enabledIceOres;

        public static bool IsVanillaElement(Element el)
        {
            return Enum.IsDefined(typeof(SimHashes), el.id);
        }

        public Tag[] GetSelectedIces() => selectedIces ?? new Tag[0];

        public static bool HasMultipleOptions() => enabledIceOres != null && enabledIceOres.Length > 1;

        // MeltNextBatch Prefix calls this to pick which element to melt next (most mass wins)
        public Element FindBestElementToMelt()
        {
            Element best = null;
            float bestMass = 0f;

            foreach (var item in kettleStorage.items)
            {
                if (item == null) continue;
                var pe = item.GetComponent<PrimaryElement>();
                if (pe == null || pe.Mass <= 0f || !pe.Element.IsSolid) continue;

                if (pe.Mass > bestMass)
                {
                    best = pe.Element;
                    bestMass = pe.Mass;
                }
            }

            return best;
        }

        public override void OnPrefabInit()
        {
            base.OnPrefabInit();
            if (enabledIceOres == null)
                BuildEnabledList();

            // Default: only Ice selected (will be overwritten by deserialization if saved)
            if (selectedIces == null || selectedIces.Length == 0)
                selectedIces = new Tag[] { IceKettleConfig.TARGET_ELEMENT_TAG };

            Subscribe((int)GameHashes.CopySettings, OnCopySettingsHandler);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            var storages = GetComponents<Storage>();
            kettleStorage = storages[1];
            outputStorage = storages[2];

            var mdkgs = GetComponents<ManualDeliveryKG>();
            foreach (var mdkg in mdkgs)
            {
                if (mdkg.DebugStorage == kettleStorage)
                {
                    originalIceMdkg = mdkg;
                    break;
                }
            }

            // Cache the vanilla values before ApplyDeliveries modifies them
            originalCapacity = originalIceMdkg.capacity;
            originalRefillMass = originalIceMdkg.refillMass;

            selectedIces = ValidateSelection(selectedIces);

            // State machine needs a value even though we resolve dynamically in MeltNextBatch
            kettle.elementToMelt = ElementLoader.GetElement(selectedIces[0]);

            ApplyDeliveries();
        }

        public override void OnCleanUp()
        {
            DestroyExtraDeliveries();
            base.OnCleanUp();
        }

        public void SetSelectedIces(Tag[] newSelection)
        {
            selectedIces = newSelection ?? new Tag[0];

            if (selectedIces.Length > 0)
                kettle.elementToMelt = ElementLoader.GetElement(selectedIces[0]);

            // Cancel melting if mid-batch (element might have changed or cleared)
            if (kettle.IsInsideState(kettle.sm.operational.melting.working))
            {
                kettle.GoTo(kettle.sm.operational.melting.exit);
                IceKettle.ResetMeltingTimer(kettle);
            }

            DropDeselectedItems();
            ApplyDeliveries();
        }

        // Cached from the original MDKG before we start modifying it
        private float originalCapacity;
        private float originalRefillMass;

        private void ApplyDeliveries()
        {
            DestroyExtraDeliveries();

            if (selectedIces.Length == 0)
            {
                // Nothing selected - pause the original delivery so nothing gets fetched
                originalIceMdkg.Pause(true, "No meltables selected");
                return;
            }

            // Divide storage capacity evenly so combined deliveries don't massively overfill.
            // ManualDeliveryKG only tracks its own tag's mass, not total storage,
            // so without splitting each MDKG would independently request the full capacity.
            float perTypeCapacity = kettleStorage.capacityKg / selectedIces.Length;
            float perTypeRefill = originalRefillMass / selectedIces.Length;

            // Ensure at least one batch worth can be requested per type
            float batchSize = 100f;
            perTypeCapacity = Mathf.Max(perTypeCapacity, batchSize);
            perTypeRefill = Mathf.Max(perTypeRefill, batchSize * 0.1f);

            originalIceMdkg.Pause(false, "Meltables selected");
            originalIceMdkg.RequestedItemTag = selectedIces[0];
            originalIceMdkg.capacity = perTypeCapacity;
            originalIceMdkg.refillMass = perTypeRefill;

            for (int i = 1; i < selectedIces.Length; i++)
            {
                var mdkg = gameObject.AddComponent<ManualDeliveryKG>();
                mdkg.SetStorage(kettleStorage);
                mdkg.RequestedItemTag = selectedIces[i];
                mdkg.capacity = perTypeCapacity;
                mdkg.refillMass = perTypeRefill;
                mdkg.MinimumMass = originalIceMdkg.MinimumMass;
                mdkg.choreTypeIDHash = originalIceMdkg.choreTypeIDHash;
                mdkg.ShowStatusItem = false;
                extraDeliveries.Add(mdkg);
            }
        }

        private void DestroyExtraDeliveries()
        {
            foreach (var mdkg in extraDeliveries)
            {
                if (mdkg != null)
                    Destroy(mdkg);
            }
            extraDeliveries.Clear();
        }

        private void DropDeselectedItems()
        {
            var selected = new HashSet<Tag>(selectedIces);
            for (int i = kettleStorage.items.Count - 1; i >= 0; i--)
            {
                var item = kettleStorage.items[i];
                if (item == null) continue;
                var pe = item.GetComponent<PrimaryElement>();
                if (pe != null && !selected.Contains(pe.Element.tag))
                    kettleStorage.Drop(item);
            }
        }

        private Tag[] ValidateSelection(Tag[] saved)
        {
            if (saved == null || saved.Length == 0)
                return new Tag[] { IceKettleConfig.TARGET_ELEMENT_TAG };

            var enabledTags = new HashSet<Tag>();
            foreach (var ore in enabledIceOres)
                enabledTags.Add(ore.tag);

            // Keep only tags that are still enabled in global options
            var valid = saved.Where(t => enabledTags.Contains(t)).ToArray();
            if (valid.Length == 0)
                return new Tag[] { IceKettleConfig.TARGET_ELEMENT_TAG };

            return valid;
        }

        private void OnCopySettings(object data)
        {
            var go = data as GameObject;
            if (go != null && go.TryGetComponent(out IceKettleController other))
                SetSelectedIces(other.selectedIces);
        }

        private static void BuildEnabledList()
        {
            var opts = ControlledExtractionOptions.Instance.IceKettleMeltables
                ?? new IceKettleMeltableOptions();

            // Discover all solid elements tagged Liquifiable (covers vanilla, DLC, and mods)
            var allLiquefiable = ElementLoader.FindElements(
                element => element.IsSolid && element.HasTag(GameTags.Liquifiable));

            var list = new List<Element>();

            foreach (var el in allLiquefiable)
            {
                if (el == null) continue;

                bool isVanilla = Enum.IsDefined(typeof(SimHashes), el.id);

                if (isVanilla)
                {
                    string key = el.id.ToString();
                    if (opts.IsElementEnabled(key))
                        list.Add(el);
                }
                else
                {
                    // Modded element (Ronivan's Legacy, etc.) - single toggle
                    if (opts.EnableModdedMeltables)
                        list.Add(el);
                }
            }

            // Always have at least Ice available
            if (list.Count == 0)
            {
                var ice = ElementLoader.FindElementByHash(SimHashes.Ice);
                if (ice != null) list.Add(ice);
            }

            enabledIceOres = list.ToArray();
        }
    }
}
