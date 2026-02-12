using System;
using System.Collections.Generic;
using HarmonyLib;
using PeterHan.PLib.Buildings;
using UnityEngine;
using ControlledMods.ModDetection;
using ControlledMods.Options;
using ControlledMods.ResourceSensor;

namespace ControlledMods.Patches.ResourceSensor
{
    public static class ResourceSensorPatches
    {
        private static readonly Dictionary<object, Action<object>> _deselectHandlers = new Dictionary<object, Action<object>>();
        private static readonly Dictionary<object, KToggle> _atmosphereToggles = new Dictionary<object, KToggle>();
        private static readonly Dictionary<object, KImage> _atmosphereCheckmarks = new Dictionary<object, KImage>();
        private static readonly Dictionary<object, KToggle> _storageToggles = new Dictionary<object, KToggle>();
        private static readonly Dictionary<object, KImage> _storageCheckmarks = new Dictionary<object, KImage>();
        private static readonly Dictionary<object, KToggle> _conduitsToggles = new Dictionary<object, KToggle>();
        private static readonly Dictionary<object, KImage> _conduitsCheckmarks = new Dictionary<object, KImage>();

        public static void ApplyPatches(Harmony harmony)
        {
            if (!ResourceSensorDetection.Loaded) return;
            var opts = ControlledModsOptions.Instance;
            if (!opts.EnableResourceSensor) return;

            // 1) ResourceSensor.ResourceSensorSideScreen - keep Berkay's UI, but disable Global mode (and any stray Count Eggs)
            var theirSideScreenType = AccessTools.TypeByName("ResourceSensor.ResourceSensorSideScreen")
                ?? FindTypeInAssembly("ResourceSensor", "ResourceSensor.ResourceSensorSideScreen");
            if (theirSideScreenType != null)
            {
                var onPrefabInit = AccessTools.Method(theirSideScreenType, "OnPrefabInit", Type.EmptyTypes);
                if (onPrefabInit != null)
                    harmony.Patch(onPrefabInit, postfix: new HarmonyMethod(typeof(ResourceSensorSideScreen_OnPrefabInit_Patch), nameof(ResourceSensorSideScreen_OnPrefabInit_Patch.Postfix)));

                var setTarget = AccessTools.Method(theirSideScreenType, "SetTarget", new[] { typeof(GameObject) });
                if (setTarget != null)
                    harmony.Patch(setTarget, postfix: new HarmonyMethod(typeof(ResourceSensorSideScreen_SetTarget_Patch), nameof(ResourceSensorSideScreen_SetTarget_Patch.Postfix)));

                var theirOnSpawn = AccessTools.Method(theirSideScreenType, "OnSpawn", Type.EmptyTypes);
                if (theirOnSpawn != null)
                    harmony.Patch(theirOnSpawn, postfix: new HarmonyMethod(typeof(ResourceSensorSideScreen_OnSpawn_Patch), nameof(ResourceSensorSideScreen_OnSpawn_Patch.Postfix)));

                // Prevent toggling to Global Mode from the UI
                var toggleGlobal = AccessTools.Method(theirSideScreenType, "ToggleGlobal", Type.EmptyTypes);
                if (toggleGlobal != null)
                    harmony.Patch(toggleGlobal, prefix: new HarmonyMethod(typeof(ResourceSensorSideScreen_ToggleGlobal_Patch), nameof(ResourceSensorSideScreen_ToggleGlobal_Patch.Prefix)));

                // Sync our Storage checkbox when Berkay's Include Storage is toggled (if that row is ever used)
                var toggleCountStorage = AccessTools.Method(theirSideScreenType, "ToggleCountStorage", Type.EmptyTypes);
                if (toggleCountStorage != null)
                    harmony.Patch(toggleCountStorage, postfix: new HarmonyMethod(typeof(ResourceSensorSideScreen_ToggleCountStorage_Patch), nameof(ResourceSensorSideScreen_ToggleCountStorage_Patch.Postfix)));
            }

            // 2) ThresholdSwitchSideScreen.SetTarget - character limit 8 for their sensor (if the vanilla threshold UI ever shows)
            var thresholdType = typeof(ThresholdSwitchSideScreen);
            var thresholdSetTarget = AccessTools.Method(thresholdType, "SetTarget", new[] { typeof(GameObject) });
            if (thresholdSetTarget != null)
                harmony.Patch(thresholdSetTarget, postfix: new HarmonyMethod(typeof(ThresholdSwitchSideScreen_SetTarget_Patch), nameof(ThresholdSwitchSideScreen_SetTarget_Patch.Postfix)));

            // 3) ColoredRangeVisualizer.OnSpawn - subscribe to SelectObject and clear on deselect
            var coloredType = typeof(ColoredRangeVisualizer);
            var onSpawn = AccessTools.Method(coloredType, "OnSpawn");
            if (onSpawn != null)
                harmony.Patch(onSpawn, postfix: new HarmonyMethod(typeof(ColoredRangeVisualizer_OnSpawn_Patch), nameof(ColoredRangeVisualizer_OnSpawn_Patch.Postfix)));

            // 4) ColoredRangeVisualizer.OnCleanUp - Unsubscribe
            var onCleanUp = AccessTools.Method(coloredType, "OnCleanUp");
            if (onCleanUp != null)
                harmony.Patch(onCleanUp, postfix: new HarmonyMethod(typeof(ColoredRangeVisualizer_OnCleanUp_Patch), nameof(ColoredRangeVisualizer_OnCleanUp_Patch.Postfix)));

            // 5) ResourceSensor.LogicResourceSensorConfig.ConfigureBuildingTemplate - set storage filters (liquids + gases)
            var configType = AccessTools.TypeByName("ResourceSensor.LogicResourceSensorConfig");
            var configureTemplate = AccessTools.Method(configType, "ConfigureBuildingTemplate", new[] { typeof(GameObject), typeof(Tag) });
            if (configureTemplate != null)
                harmony.Patch(configureTemplate, postfix: new HarmonyMethod(typeof(LogicResourceSensorConfig_ConfigureBuildingTemplate_Patch), nameof(LogicResourceSensorConfig_ConfigureBuildingTemplate_Patch.Postfix)));

            // 6) KSelectable.Unselect - clear resource sensor range overlay on deselect (more reliable than relying on SelectObject event)
            var unselect = AccessTools.Method(typeof(KSelectable), "Unselect", Type.EmptyTypes);
            if (unselect != null)
                harmony.Patch(unselect, postfix: new HarmonyMethod(typeof(KSelectable_Unselect_Patch), nameof(KSelectable_Unselect_Patch.Postfix)));

            // 7) ResourceSensor.LogicResourceSensor.CountCell - fix counting (expand category tags + include liquid/gas cell mass)
            var sensorType = AccessTools.TypeByName("ResourceSensor.LogicResourceSensor");
            var countCell = AccessTools.Method(sensorType, "CountCell", new[] { typeof(int) });
            if (countCell != null)
                harmony.Patch(countCell, prefix: new HarmonyMethod(typeof(LogicResourceSensor_CountCell_Patch), nameof(LogicResourceSensor_CountCell_Patch.Prefix)));

            // 8) ResourceSensor.LogicResourceSensor.CountBuilding - expand category tags for storage contents (needed for Include/Only storage)
            var countBuilding = AccessTools.Method(sensorType, "CountBuilding", new[] { typeof(GameObject) });
            if (countBuilding != null)
                harmony.Patch(countBuilding, prefix: new HarmonyMethod(typeof(LogicResourceSensor_CountBuilding_Patch), nameof(LogicResourceSensor_CountBuilding_Patch.Prefix)));

            ControlledModsMod.Log("Resource Sensor patches applied");
        }

        private static Type FindTypeInAssembly(string assemblyNameContains, string fullTypeName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (asm.GetName().Name.IndexOf(assemblyNameContains, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                    var t = asm.GetType(fullTypeName);
                    if (t != null) return t;
                }
                catch { /* ignore */ }
            }
            return null;
        }

        public static class ResourceSensorSideScreen_OnPrefabInit_Patch
        {
            public static void Postfix(object __instance)
            {
                try
                {
                    var comp = __instance as Component;
                    var root = comp != null ? comp.gameObject : null;
                    if (root == null) return;

                    // Hide Global Mode row by disabling the container which holds countGlobalToggle.
                    var globalToggle = AccessTools.Field(__instance.GetType(), "countGlobalToggle")?.GetValue(__instance) as KToggle;
                    if (globalToggle != null)
                    {
                        var globalRow = globalToggle.transform.parent?.gameObject;
                        if (globalRow != null) globalRow.SetActive(false);
                    }

                    // If Room Mode toggle failed to bind (game UI changed), create a fallback row so Room Mode still exists.
                    var roomToggle = AccessTools.Field(__instance.GetType(), "countRoomToggle")?.GetValue(__instance) as KToggle;
                    if (roomToggle == null)
                    {
                        var distanceContainer = root.transform.Find("Contents/CheckboxGroup")?.gameObject;
                        if (distanceContainer != null && distanceContainer.transform.parent != null)
                        {
                            var roomContainer = UnityEngine.Object.Instantiate(distanceContainer, distanceContainer.transform.parent);
                            roomContainer.name = "ControlledMods_RoomModeRow";
                            roomContainer.transform.SetSiblingIndex(distanceContainer.transform.GetSiblingIndex() + 2);

                            var label = roomContainer.transform.Find("Label")?.GetComponent<LocText>();
                            if (label != null) label.SetText("Room Mode");

                            var checkBoxGroup = roomContainer.transform.Find("CrittersCheckBox");
                            if (checkBoxGroup != null)
                            {
                                checkBoxGroup.name = "CountRoomCheckBox";
                                var newRoomToggle = checkBoxGroup.GetComponent<KToggle>();
                                var newRoomCheckmark = newRoomToggle != null
                                    ? newRoomToggle.transform.Find("CheckMark")?.GetComponent<KImage>()
                                    : null;

                                if (newRoomToggle != null)
                                {
                                    // Wire into Berkay's fields so their OnSpawn/ToggleRoom logic works.
                                    AccessTools.Field(__instance.GetType(), "countRoomToggle")?.SetValue(__instance, newRoomToggle);
                                    if (newRoomCheckmark != null)
                                        AccessTools.Field(__instance.GetType(), "roomCheckmark")?.SetValue(__instance, newRoomCheckmark);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Ensure existing room row isn't accidentally hidden.
                        var roomRow = roomToggle.transform.parent?.gameObject;
                        if (roomRow != null) roomRow.SetActive(true);
                    }

                    // Hide Berkay's "Include Storage Buildings" row (full row = label + checkbox; toggle's parent may be the checkbox group)
                    var includeToggle = AccessTools.Field(__instance.GetType(), "countStorageToggle")?.GetValue(__instance) as KToggle;
                    if (includeToggle != null)
                    {
                        var includeRow = includeToggle.transform.parent?.parent?.gameObject ?? includeToggle.transform.parent?.gameObject;
                        if (includeRow != null) includeRow.SetActive(false);
                    }

                    // Add our three rows (Atmosphere, Storage, Conduits) by cloning the Room row - a simple label+checkbox row, not the Include Storage container
                    var templateToggle = AccessTools.Field(__instance.GetType(), "countRoomToggle")?.GetValue(__instance) as KToggle;
                    var templateRow = templateToggle != null ? templateToggle.transform.parent?.gameObject : null;
                    if (templateRow == null)
                        templateToggle = AccessTools.Field(__instance.GetType(), "countDistanceToggle")?.GetValue(__instance) as KToggle;
                    if (templateToggle != null && templateRow == null)
                        templateRow = templateToggle.transform.parent?.gameObject;

                    if (templateRow != null && templateRow.transform.parent != null && !_atmosphereToggles.ContainsKey(__instance))
                    {
                        var parent = templateRow.transform.parent;
                        int siblingStart = templateRow.transform.GetSiblingIndex() + 1;

                        void AddRow(string name, string labelText, Dictionary<object, KToggle> toggles, Dictionary<object, KImage> checkmarks)
                        {
                            var row = UnityEngine.Object.Instantiate(templateRow, parent);
                            row.name = name;
                            row.SetActive(true);
                            row.transform.SetSiblingIndex(siblingStart++);

                            var lbl = row.transform.Find("Label")?.GetComponent<LocText>();
                            if (lbl != null) lbl.SetText(labelText);

                            var cbg = row.transform.Find("CountRoomCheckBox") ?? row.transform.Find("IncludeStorageCheckBox") ?? row.transform.Find("CountDistanceCheckBox") ?? row.transform.Find("CrittersCheckBox");
                            if (cbg != null) cbg.name = name + "CheckBox";

                            var toggle = row.GetComponentInChildren<KToggle>(true);
                            var checkmark = toggle != null ? toggle.transform.Find("CheckMark")?.GetComponent<KImage>() : null;
                            if (toggle != null)
                            {
                                toggles[__instance] = toggle;
                                if (checkmark != null) checkmarks[__instance] = checkmark;
                            }
                        }

                        AddRow("ControlledMods_AtmosphereRow", "Atmosphere", _atmosphereToggles, _atmosphereCheckmarks);
                        AddRow("ControlledMods_StorageRow", "Storage", _storageToggles, _storageCheckmarks);
                        AddRow("ControlledMods_ConduitsRow", "Conduits", _conduitsToggles, _conduitsCheckmarks);
                    }
                }
                catch { }
            }
        }

        public static class ResourceSensorSideScreen_OnSpawn_Patch
        {
            private static void SyncBerkayIncludeStorage(Component targetSensor, bool value)
            {
                if (targetSensor == null) return;
                AccessTools.Property(targetSensor.GetType(), "IncludeStorage")?.SetValue(targetSensor, value);
            }

            public static void Postfix(object __instance)
            {
                try
                {
                    if (__instance == null) return;

                    if (_atmosphereToggles.TryGetValue(__instance, out var atmosphereToggle) && atmosphereToggle != null)
                        atmosphereToggle.onClick += () =>
                        {
                            try
                            {
                                var targetSensor = AccessTools.Field(__instance.GetType(), "targetSensor")?.GetValue(__instance) as Component;
                                if (targetSensor == null) return;
                                var scope = targetSensor.GetComponent<ControlledMods.ResourceSensor.ResourceSensorStorageScope>()
                                    ?? targetSensor.gameObject.AddOrGet<ControlledMods.ResourceSensor.ResourceSensorStorageScope>();
                                scope.IncludeAtmosphere = !scope.IncludeAtmosphere;
                                if (_atmosphereCheckmarks.TryGetValue(__instance, out var c) && c != null) c.enabled = scope.IncludeAtmosphere;
                            }
                            catch { }
                        };

                    if (_storageToggles.TryGetValue(__instance, out var storageToggle) && storageToggle != null)
                        storageToggle.onClick += () =>
                        {
                            try
                            {
                                var targetSensor = AccessTools.Field(__instance.GetType(), "targetSensor")?.GetValue(__instance) as Component;
                                if (targetSensor == null) return;
                                var scope = targetSensor.GetComponent<ControlledMods.ResourceSensor.ResourceSensorStorageScope>()
                                    ?? targetSensor.gameObject.AddOrGet<ControlledMods.ResourceSensor.ResourceSensorStorageScope>();
                                scope.IncludeStorage = !scope.IncludeStorage;
                                SyncBerkayIncludeStorage(targetSensor, scope.IncludeStorage);
                                if (_storageCheckmarks.TryGetValue(__instance, out var c) && c != null) c.enabled = scope.IncludeStorage;
                            }
                            catch { }
                        };

                    if (_conduitsToggles.TryGetValue(__instance, out var conduitsToggle) && conduitsToggle != null)
                        conduitsToggle.onClick += () =>
                        {
                            try
                            {
                                var targetSensor = AccessTools.Field(__instance.GetType(), "targetSensor")?.GetValue(__instance) as Component;
                                if (targetSensor == null) return;
                                var scope = targetSensor.GetComponent<ControlledMods.ResourceSensor.ResourceSensorStorageScope>()
                                    ?? targetSensor.gameObject.AddOrGet<ControlledMods.ResourceSensor.ResourceSensorStorageScope>();
                                scope.IncludeConduits = !scope.IncludeConduits;
                                if (_conduitsCheckmarks.TryGetValue(__instance, out var c) && c != null) c.enabled = scope.IncludeConduits;
                            }
                            catch { }
                        };
                }
                catch { }
            }
        }

        public static class ResourceSensorSideScreen_ToggleGlobal_Patch
        {
            public static bool Prefix()
            {
                // Skip original ToggleGlobal (disallow Global mode)
                return false;
            }
        }

        public static class ResourceSensorSideScreen_SetTarget_Patch
        {
            public static void Postfix(object __instance, GameObject target)
            {
                try
                {
                    if (__instance == null || target == null) return;
                    var sensorType = AccessTools.TypeByName("ResourceSensor.LogicResourceSensor") ?? FindTypeInAssembly("ResourceSensor", "ResourceSensor.LogicResourceSensor");
                    if (sensorType == null) return;
                    var sensor = target.GetComponent(sensorType);
                    if (sensor == null) return;

                    // If an existing building is in Global mode, force it back to Distance
                    var modeProp = AccessTools.Property(sensorType, "Mode");
                    var modeVal = modeProp?.GetValue(sensor);
                    if (modeVal == null) return;
                    if (string.Equals(modeVal.ToString(), "Global", StringComparison.OrdinalIgnoreCase))
                    {
                        var toggleDistance = AccessTools.Method(__instance.GetType(), "ToggleDistance", Type.EmptyTypes);
                        toggleDistance?.Invoke(__instance, null);
                    }

                    // Also ensure the Global and Include Storage rows stay hidden after SetTarget refreshes UI.
                    var globalToggle = AccessTools.Field(__instance.GetType(), "countGlobalToggle")?.GetValue(__instance) as KToggle;
                    if (globalToggle != null)
                    {
                        var globalRow = globalToggle.transform.parent?.gameObject;
                        if (globalRow != null) globalRow.SetActive(false);
                    }
                    var includeToggle = AccessTools.Field(__instance.GetType(), "countStorageToggle")?.GetValue(__instance) as KToggle;
                    if (includeToggle != null)
                    {
                        var includeRow = includeToggle.transform.parent?.parent?.gameObject ?? includeToggle.transform.parent?.gameObject;
                        if (includeRow != null) includeRow.SetActive(false);
                    }

                    // Sync Atmosphere / Storage / Conduits checkmarks and Berkay's IncludeStorage from our scope
                    var scope = target.GetComponent<ControlledMods.ResourceSensor.ResourceSensorStorageScope>();
                    if (scope != null)
                    {
                        if (_atmosphereCheckmarks.TryGetValue(__instance, out var ac) && ac != null) ac.enabled = scope.IncludeAtmosphere;
                        if (_storageCheckmarks.TryGetValue(__instance, out var sc) && sc != null) sc.enabled = scope.IncludeStorage;
                        if (_conduitsCheckmarks.TryGetValue(__instance, out var cc) && cc != null) cc.enabled = scope.IncludeConduits;
                        AccessTools.Property(sensorType, "IncludeStorage")?.SetValue(sensor, scope.IncludeStorage);
                    }
                }
                catch { }
            }
        }

        public static class ResourceSensorSideScreen_ToggleCountStorage_Patch
        {
            public static void Postfix(object __instance)
            {
                try
                {
                    if (__instance == null) return;
                    var targetSensor = AccessTools.Field(__instance.GetType(), "targetSensor")?.GetValue(__instance) as Component;
                    if (targetSensor == null) return;
                    var includeProp = AccessTools.Property(targetSensor.GetType(), "IncludeStorage");
                    bool include = includeProp?.GetValue(targetSensor) is bool b && b;
                    var scope = targetSensor.GetComponent<ControlledMods.ResourceSensor.ResourceSensorStorageScope>();
                    if (scope != null)
                    {
                        scope.IncludeStorage = include;
                        if (_storageCheckmarks.TryGetValue(__instance, out var sc) && sc != null) sc.enabled = include;
                    }
                }
                catch { }
            }
        }

        public static class ThresholdSwitchSideScreen_SetTarget_Patch
        {
            public static void Postfix(ThresholdSwitchSideScreen __instance, GameObject new_target)
            {
                if (new_target == null) return;
                var sensorType = AccessTools.TypeByName("ResourceSensor.LogicResourceSensor");
                if (sensorType == null) return;
                if (new_target.GetComponent(sensorType) == null) return;

                var numberInput = AccessTools.Field(typeof(ThresholdSwitchSideScreen), "numberInput")?.GetValue(__instance);
                if (numberInput != null)
                {
                    var fieldObj = AccessTools.Field(numberInput.GetType(), "field")?.GetValue(numberInput);
                    if (fieldObj != null)
                    {
                        var charLimitProp = AccessTools.Property(fieldObj.GetType(), "characterLimit") ?? AccessTools.Property(fieldObj.GetType(), "CharacterLimit");
                        charLimitProp?.SetValue(fieldObj, 8);
                    }
                }
            }
        }

        public static class ColoredRangeVisualizer_OnSpawn_Patch
        {
            private static bool IsResourceSensorVisualizer(ColoredRangeVisualizer v)
            {
                if (v == null) return false;
                var t = v.GetType();
                if (t.Assembly.GetName().Name.IndexOf("ResourceSensor", StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
                return t.Name.IndexOf("Visualizer", StringComparison.OrdinalIgnoreCase) >= 0;
            }

            public static void Postfix(ColoredRangeVisualizer __instance)
            {
                if (__instance == null) return;
                if (!IsResourceSensorVisualizer(__instance)) return;

                Action<object> handler = data =>
                {
                    bool deselected = (data is bool b && !b) || (data == null);
                    if (deselected)
                        DistanceVisualizerHelper.ClearAndRefreshOnDeselect(__instance);
                };
                lock (_deselectHandlers)
                    _deselectHandlers[__instance] = handler;
                __instance.Subscribe((int)GameHashes.SelectObject, handler);
            }
        }

        public static class ColoredRangeVisualizer_OnCleanUp_Patch
        {
            public static void Postfix(ColoredRangeVisualizer __instance)
            {
                if (__instance == null) return;
                lock (_deselectHandlers)
                {
                    if (_deselectHandlers.TryGetValue(__instance, out var handler))
                    {
                        __instance.Unsubscribe((int)GameHashes.SelectObject, handler);
                        _deselectHandlers.Remove(__instance);
                    }
                }
            }
        }

        public static class LogicResourceSensorConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(GameObject go)
            {
                if (go == null) return;
                var storage = go.GetComponent<Storage>();
                if (storage != null)
                    storage.storageFilters = LogicResourceSensorConfigHelper.GetStorageFilterList();

                // Add our persisted storage scope toggle (OnlyStorage) so it can be saved and copied.
                go.AddOrGet<ControlledMods.ResourceSensor.ResourceSensorStorageScope>();
            }
        }

        public static class KSelectable_Unselect_Patch
        {
            public static void Postfix(KSelectable __instance)
            {
                try
                {
                    if (__instance == null) return;
                    // If this selectable has Berkay's DistanceVisualizer, clear it when deselected.
                    var dv = __instance.GetComponent<ColoredRangeVisualizer>();
                    if (dv == null) return;
                    var t = dv.GetType();
                    if (t.Assembly.GetName().Name.IndexOf("ResourceSensor", StringComparison.OrdinalIgnoreCase) < 0) return;
                    if (t.Name.IndexOf("DistanceVisualizer", StringComparison.OrdinalIgnoreCase) < 0) return;
                    DistanceVisualizerHelper.ClearAndRefreshOnDeselect(dv);
                }
                catch { }
            }
        }

        public static class LogicResourceSensor_CountCell_Patch
        {
            public static bool Prefix(object __instance, int cell, ref float __result)
            {
                try
                {
                    if (__instance == null) return true;

                    var cmp = __instance as Component;
                    var scope = cmp != null ? cmp.GetComponent<ControlledMods.ResourceSensor.ResourceSensorStorageScope>() : null;

                    var treeFilterable = AccessTools.Field(__instance.GetType(), "treeFilterable")?.GetValue(__instance);
                    if (treeFilterable == null) return true;

                    var tagsProp = AccessTools.Property(treeFilterable.GetType(), "AcceptedTags");
                    var tags = tagsProp?.GetValue(treeFilterable) as ICollection<Tag>;
                    if (tags == null || tags.Count == 0)
                    {
                        __result = 0f;
                        return false;
                    }

                    // Expand any category tags to discovered leaf resources (TreeFilterableSideScreen treats selecting a category as selecting all children)
                    var effective = new HashSet<Tag>(tags);
                    try
                    {
                        var discovered = DiscoveredResources.Instance;
                        if (discovered != null)
                        {
                            foreach (var tag in tags)
                            {
                                var children = discovered.GetDiscoveredResourcesFromTag(tag);
                                if (children != null && children.Count > 0)
                                    effective.UnionWith(children);
                            }
                        }
                    }
                    catch { }

                    float totalMass = 0f;

                    // Atmosphere: world element (liquid/gas in tile) + pickupables (solids on floor)
                    if (scope == null || scope.IncludeAtmosphere)
                    {
                        if (Grid.IsValidCell(cell))
                        {
                            Element cellElement = Grid.Element[cell];
                            if (cellElement != null && cellElement.id != SimHashes.Vacuum)
                            {
                                foreach (var tag in effective)
                                {
                                    Element filterElement = ElementLoader.GetElement(tag);
                                    if (filterElement != null && filterElement.id == cellElement.id)
                                    {
                                        totalMass += Grid.Mass[cell];
                                        break;
                                    }
                                }
                            }
                        }

                        GameObject obj = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
                        if (obj != null)
                        {
                            ObjectLayerListItem objectLayerListItem = obj.GetComponent<Pickupable>().objectLayerListItem;
                            while (objectLayerListItem != null)
                            {
                                GameObject obj2 = objectLayerListItem.gameObject;
                                objectLayerListItem = objectLayerListItem.nextItem;

                                if (obj2 != null && obj2.TryGetComponent<MinionIdentity>(out _) == false && obj2.TryGetComponent<KPrefabID>(out KPrefabID kPrefabID))
                                {
                                    foreach (var tag in effective)
                                    {
                                        if (kPrefabID.HasTag(tag))
                                        {
                                            totalMass += obj2.GetComponent<PrimaryElement>().Mass;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Conduits: gas/liquid/solid conduit contents at this cell
                    if (scope != null && scope.IncludeConduits && Grid.IsValidCell(cell))
                    {
                        var gasFlow = Game.Instance?.gasConduitFlow;
                        if (gasFlow != null)
                        {
                            var gasContents = gasFlow.GetContents(cell);
                            if (gasContents.mass > 0f && gasContents.element != SimHashes.Vacuum)
                            {
                                foreach (var tag in effective)
                                {
                                    Element filterElement = ElementLoader.GetElement(tag);
                                    if (filterElement != null && filterElement.id == gasContents.element)
                                    {
                                        totalMass += gasContents.mass;
                                        break;
                                    }
                                }
                            }
                        }
                        var liquidFlow = Game.Instance?.liquidConduitFlow;
                        if (liquidFlow != null)
                        {
                            var liquidContents = liquidFlow.GetContents(cell);
                            if (liquidContents.mass > 0f && liquidContents.element != SimHashes.Vacuum)
                            {
                                foreach (var tag in effective)
                                {
                                    Element filterElement = ElementLoader.GetElement(tag);
                                    if (filterElement != null && filterElement.id == liquidContents.element)
                                    {
                                        totalMass += liquidContents.mass;
                                        break;
                                    }
                                }
                            }
                        }
                        var solidFlow = Game.Instance?.solidConduitFlow;
                        if (solidFlow != null)
                        {
                            var solidContents = solidFlow.GetContents(cell);
                            if (solidContents.pickupableHandle.IsValid())
                            {
                                var pickupable = solidFlow.GetPickupable(solidContents.pickupableHandle);
                                if (pickupable != null && pickupable.GetComponent<MinionIdentity>() == null
                                    && pickupable.TryGetComponent<KPrefabID>(out var kPrefabID)
                                    && pickupable.TryGetComponent<PrimaryElement>(out var pe))
                                {
                                    foreach (var tag in effective)
                                    {
                                        if (kPrefabID.HasTag(tag))
                                        {
                                            totalMass += pe.Mass;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    __result = totalMass;
                    return false;
                }
                catch { }
                return true;
            }
        }

        public static class LogicResourceSensor_CountBuilding_Patch
        {
            public static bool Prefix(object __instance, GameObject obj, ref float __result)
            {
                try
                {
                    if (__instance == null || obj == null)
                        return true;

                    var cmp = __instance as Component;
                    var scope = cmp != null ? cmp.GetComponent<ControlledMods.ResourceSensor.ResourceSensorStorageScope>() : null;
                    if (scope != null && !scope.IncludeStorage)
                    {
                        __result = 0f;
                        return false;
                    }

                    if (obj.TryGetComponent(out BuildingUnderConstruction _))
                    {
                        __result = 0f;
                        return false;
                    }

                    // Count any building that has storage (lockers, loaders, receptacles, reservoirs, etc.)
                    if (!obj.TryGetComponent(out Storage storage))
                    {
                        __result = 0f;
                        return false;
                    }

                    var treeFilterable = AccessTools.Field(__instance.GetType(), "treeFilterable")?.GetValue(__instance);
                    var tagsProp = treeFilterable != null ? AccessTools.Property(treeFilterable.GetType(), "AcceptedTags") : null;
                    var tags = tagsProp?.GetValue(treeFilterable) as ICollection<Tag>;
                    if (tags == null || tags.Count == 0)
                    {
                        __result = 0f;
                        return false;
                    }

                    var effective = new HashSet<Tag>(tags);
                    try
                    {
                        var discovered = DiscoveredResources.Instance;
                        if (discovered != null)
                        {
                            foreach (var tag in tags)
                            {
                                var children = discovered.GetDiscoveredResourcesFromTag(tag);
                                if (children != null && children.Count > 0)
                                    effective.UnionWith(children);
                            }
                        }
                    }
                    catch { }

                    float totalMass = 0f;
                    foreach (var item in storage.items)
                    {
                        if (item == null) continue;
                        foreach (var tag in effective)
                        {
                            if (item.HasTag(tag))
                            {
                                if (item.TryGetComponent<PrimaryElement>(out var pe))
                                    totalMass += pe.Mass;
                                break;
                            }
                        }
                    }

                    __result = totalMass;
                    return false;
                }
                catch { }
                return true;
            }
        }
    }
}
