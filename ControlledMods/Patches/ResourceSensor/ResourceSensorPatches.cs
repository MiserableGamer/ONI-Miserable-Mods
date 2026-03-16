using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Buildings;
using UnityEngine;
using UnityEngine.UI;
using ControlledMods.ModDetection;
using ControlledMods.Options;
using ControlledMods.ResourceSensor;
using TMPro;

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

        // Cached reflection for Berkay's sensor type (resolved once in ApplyPatches)
        private static Type _rsSensorType;
        private static FieldInfo _rsSensorTreeFilterableField;
        private static PropertyInfo _rsAcceptedTagsProp;
        private static FieldInfo _rsSensorLogicPortsField;
        private static FieldInfo _rsSensorVisualizerField;
        private static FieldInfo _rsVisCellsField;
        private static MethodInfo _rsRefreshMethod;
        private static PropertyInfo _rsSensorDistanceProp;
        private static FieldInfo _rsSensorIncludeStorageField;
        private static FieldInfo _rsSensorVisualiserDirtyField;
        private static FieldInfo _rsSensorSelectableField;
        private static MethodInfo _rsSensorCountCellMethod;
        private static MethodInfo _rsSensorCountBuildingMethod;
        private static PropertyInfo _rsSensorIncludeStorageProp;

        // Cached reflection for Berkay's sidescreen type
        private static FieldInfo _rsSideScreenCountStorageToggle;
        private static FieldInfo _rsSideScreenCountRoomToggle;
        private static FieldInfo _rsSideScreenCountGlobalToggle;
        private static FieldInfo _rsSideScreenRoomCheckmark;
        private static FieldInfo _rsSideScreenCountDistanceToggle;
        private static FieldInfo _rsSideScreenTargetSensor;

        // Cached reflection for ThresholdSwitchSideScreen fields
        private static FieldInfo _rsThresholdNumberInputField;
        private static FieldInfo _rsThresholdTargetField;

        // Filter popup: TreeFilterableSideScreen is hidden in the side screen and shown via popup
        private static readonly Dictionary<object, GameObject> _filterButtons = new Dictionary<object, GameObject>();
        private static Transform _activeFilterTransform;
        private static Transform _filterOriginalParent;
        private static int _filterOriginalSiblingIndex;
        private static GameObject _filterPopup;

        // Tag expansion: expanded from category tags to individual resource tags
        private static readonly HashSet<Tag> _cachedExpandedTags = new HashSet<Tag>();

        // Per-sensor cache: avoids repeated reflection + GetComponent inside CountCell/CountBuilding loops
        private static object _lastSensorInstance;
        private static object _lastSensorTreeFilterable;
        private static ICollection<Tag> _lastSensorTags;
        private static ResourceSensorStorageScope _lastSensorScope;
        private static HashSet<Tag> _lastSensorEffective;
        private static HashSet<SimHashes> _lastSensorElementIds;
        private static int _lastRefreshFrame = -1;

        // Pooled collections: reused across CountDistance/CountRoom to avoid GC pressure
        private static readonly HashSet<GameObject> _poolCountedBuildings = new HashSet<GameObject>();
        private static readonly HashSet<GameObject> _poolCountedTiles = new HashSet<GameObject>();

        // Reusable invoke args: avoids new object[] allocation per cell/building in MethodInfo.Invoke
        private static readonly object[] _invokeArgCell = new object[1];
        private static readonly object[] _invokeArgObj = new object[1];

        private static HashSet<Tag> ExpandTags(ICollection<Tag> sourceTags)
        {
            _cachedExpandedTags.Clear();
            _cachedExpandedTags.UnionWith(sourceTags);
            try
            {
                var discovered = DiscoveredResources.Instance;
                if (discovered != null)
                {
                    foreach (var tag in sourceTags)
                    {
                        var children = discovered.GetDiscoveredResourcesFromTag(tag);
                        if (children != null && children.Count > 0)
                            _cachedExpandedTags.UnionWith(children);
                    }
                }
            }
            catch { }
            return _cachedExpandedTags;
        }

        private static bool RefreshSensorCache(object sensorInstance)
        {
            int frame = Time.frameCount;

            // Within the same frame for the same sensor, reuse existing cache
            if (sensorInstance == _lastSensorInstance && _lastSensorTreeFilterable != null
                && frame == _lastRefreshFrame
                && _lastSensorEffective != null && _lastSensorElementIds != null)
            {
                return _lastSensorTags != null && _lastSensorTags.Count > 0;
            }

            _lastRefreshFrame = frame;

            // Re-fetch TreeFilterable if sensor instance changed
            if (sensorInstance != _lastSensorInstance)
            {
                _lastSensorInstance = sensorInstance;
                _lastSensorTreeFilterable = _rsSensorTreeFilterableField?.GetValue(sensorInstance);
                if (_lastSensorTreeFilterable == null)
                {
                    _lastSensorTags = null;
                    _lastSensorScope = null;
                    _lastSensorEffective = null;
                    _lastSensorElementIds = null;
                    return false;
                }
            }

            // Always re-read accepted tags (the collection is mutated by filter UI)
            _lastSensorTags = _rsAcceptedTagsProp?.GetValue(_lastSensorTreeFilterable) as ICollection<Tag>;
            if (_lastSensorTags == null || _lastSensorTags.Count == 0)
            {
                _lastSensorScope = null;
                _lastSensorEffective = null;
                _lastSensorElementIds = null;
                return false;
            }

            var cmp = sensorInstance as Component;
            _lastSensorScope = cmp != null ? cmp.GetComponent<ResourceSensorStorageScope>() : null;

            _lastSensorEffective = ExpandTags(_lastSensorTags);

            if (_lastSensorElementIds == null)
                _lastSensorElementIds = new HashSet<SimHashes>();
            else
                _lastSensorElementIds.Clear();
            foreach (var tag in _lastSensorEffective)
            {
                Element el = ElementLoader.GetElement(tag);
                if (el != null)
                    _lastSensorElementIds.Add(el.id);
            }

            return true;
        }

        public static void ApplyPatches(Harmony harmony)
        {
            if (!ResourceSensorDetection.Loaded) return;
            var opts = ControlledModsOptions.Instance;
            if (!opts.EnableResourceSensor) return;

            // Berkay's ResourceSensorSideScreen patches (Global hidden, Include Storage replaced with our scope toggles)
            var theirSideScreenType = AccessTools.TypeByName("ResourceSensor.ResourceSensorSideScreen")
                ?? FindTypeInAssembly("ResourceSensor", "ResourceSensor.ResourceSensorSideScreen");
            if (theirSideScreenType != null)
            {
                _rsSideScreenCountStorageToggle = AccessTools.Field(theirSideScreenType, "countStorageToggle");
                _rsSideScreenCountRoomToggle = AccessTools.Field(theirSideScreenType, "countRoomToggle");
                _rsSideScreenCountGlobalToggle = AccessTools.Field(theirSideScreenType, "countGlobalToggle");
                _rsSideScreenRoomCheckmark = AccessTools.Field(theirSideScreenType, "roomCheckmark");
                _rsSideScreenCountDistanceToggle = AccessTools.Field(theirSideScreenType, "countDistanceToggle");
                _rsSideScreenTargetSensor = AccessTools.Field(theirSideScreenType, "targetSensor");

                var onPrefabInit = AccessTools.Method(theirSideScreenType, "OnPrefabInit", Type.EmptyTypes);
                if (onPrefabInit != null)
                    harmony.Patch(onPrefabInit, postfix: new HarmonyMethod(typeof(ResourceSensorSideScreen_OnPrefabInit_Patch), nameof(ResourceSensorSideScreen_OnPrefabInit_Patch.Postfix)));

                var setTarget = AccessTools.Method(theirSideScreenType, "SetTarget", new[] { typeof(GameObject) });
                if (setTarget != null)
                    harmony.Patch(setTarget, postfix: new HarmonyMethod(typeof(ResourceSensorSideScreen_SetTarget_Patch), nameof(ResourceSensorSideScreen_SetTarget_Patch.Postfix)));

                var theirOnSpawn = AccessTools.Method(theirSideScreenType, "OnSpawn", Type.EmptyTypes);
                if (theirOnSpawn != null)
                    harmony.Patch(theirOnSpawn, postfix: new HarmonyMethod(typeof(ResourceSensorSideScreen_OnSpawn_Patch), nameof(ResourceSensorSideScreen_OnSpawn_Patch.Postfix)));

                // Sync our Storage checkbox when Berkay's Include Storage is toggled
                var toggleCountStorage = AccessTools.Method(theirSideScreenType, "ToggleCountStorage", Type.EmptyTypes);
                if (toggleCountStorage != null)
                    harmony.Patch(toggleCountStorage, postfix: new HarmonyMethod(typeof(ResourceSensorSideScreen_ToggleCountStorage_Patch), nameof(ResourceSensorSideScreen_ToggleCountStorage_Patch.Postfix)));

                // Re-hide Include Storage row after Distance/Room toggle (their UI refresh can make it reappear)
                var toggleDistance = AccessTools.Method(theirSideScreenType, "ToggleDistance", Type.EmptyTypes);
                if (toggleDistance != null)
                    harmony.Patch(toggleDistance, postfix: new HarmonyMethod(typeof(ResourceSensorSideScreen_ToggleDistance_Patch), nameof(ResourceSensorSideScreen_ToggleDistance_Patch.Postfix)));
                var toggleRoom = AccessTools.Method(theirSideScreenType, "ToggleRoom", Type.EmptyTypes);
                if (toggleRoom != null)
                    harmony.Patch(toggleRoom, postfix: new HarmonyMethod(typeof(ResourceSensorSideScreen_ToggleRoom_Patch), nameof(ResourceSensorSideScreen_ToggleRoom_Patch.Postfix)));
            }

            // Raise threshold input character limit from 6 to 8 (SetTarget + UpdateTargetThresholdLabel so it sticks)
            var thresholdType = typeof(ThresholdSwitchSideScreen);
            var thresholdSetTarget = AccessTools.Method(thresholdType, "SetTarget", new[] { typeof(GameObject) });
            if (thresholdSetTarget != null)
                harmony.Patch(thresholdSetTarget, postfix: new HarmonyMethod(typeof(ThresholdSwitchSideScreen_SetTarget_Patch), nameof(ThresholdSwitchSideScreen_SetTarget_Patch.Postfix)));
            var updateLabel = AccessTools.Method(thresholdType, "UpdateTargetThresholdLabel", Type.EmptyTypes);
            if (updateLabel != null)
                harmony.Patch(updateLabel, postfix: new HarmonyMethod(typeof(ThresholdSwitchSideScreen_UpdateTargetThresholdLabel_Patch), nameof(ThresholdSwitchSideScreen_UpdateTargetThresholdLabel_Patch.Postfix)));

            // Clear range overlay on deselect
            var coloredType = typeof(ColoredRangeVisualizer);
            var onSpawn = AccessTools.Method(coloredType, "OnSpawn");
            if (onSpawn != null)
                harmony.Patch(onSpawn, postfix: new HarmonyMethod(typeof(ColoredRangeVisualizer_OnSpawn_Patch), nameof(ColoredRangeVisualizer_OnSpawn_Patch.Postfix)));

            // Clean up deselect subscription
            var onCleanUp = AccessTools.Method(coloredType, "OnCleanUp");
            if (onCleanUp != null)
                harmony.Patch(onCleanUp, postfix: new HarmonyMethod(typeof(ColoredRangeVisualizer_OnCleanUp_Patch), nameof(ColoredRangeVisualizer_OnCleanUp_Patch.Postfix)));

            // Add liquid/gas storage filters and our scope component to the sensor prefab
            var configType = AccessTools.TypeByName("ResourceSensor.LogicResourceSensorConfig");
            var configureTemplate = AccessTools.Method(configType, "ConfigureBuildingTemplate", new[] { typeof(GameObject), typeof(Tag) });
            if (configureTemplate != null)
                harmony.Patch(configureTemplate, postfix: new HarmonyMethod(typeof(LogicResourceSensorConfig_ConfigureBuildingTemplate_Patch), nameof(LogicResourceSensorConfig_ConfigureBuildingTemplate_Patch.Postfix)));

            // Backup deselect clear (more reliable than SelectObject event alone)
            var unselect = AccessTools.Method(typeof(KSelectable), "Unselect", Type.EmptyTypes);
            if (unselect != null)
                harmony.Patch(unselect, postfix: new HarmonyMethod(typeof(KSelectable_Unselect_Patch), nameof(KSelectable_Unselect_Patch.Postfix)));

            _rsThresholdNumberInputField = AccessTools.Field(typeof(ThresholdSwitchSideScreen), "numberInput");
            _rsThresholdTargetField = AccessTools.Field(typeof(ThresholdSwitchSideScreen), "target");

            // Fix CountCell: expand category tags, include liquid/gas cell mass, respect scope
            var sensorType = AccessTools.TypeByName("ResourceSensor.LogicResourceSensor")
                ?? FindTypeInAssembly("ResourceSensor", "ResourceSensor.LogicResourceSensor");
            _rsSensorType = sensorType;
            if (sensorType != null)
            {
                _rsSensorTreeFilterableField = AccessTools.Field(sensorType, "treeFilterable");
                _rsSensorLogicPortsField = AccessTools.Field(sensorType, "logicPorts");
                _rsSensorVisualizerField = AccessTools.Field(sensorType, "visualizer");
                _rsSensorDistanceProp = AccessTools.Property(sensorType, "Distance");
                _rsSensorIncludeStorageField = AccessTools.Field(sensorType, "includeStorage");
                _rsSensorVisualiserDirtyField = AccessTools.Field(sensorType, "visualiserDirty");
                _rsSensorSelectableField = AccessTools.Field(sensorType, "selectable");
                _rsSensorCountCellMethod = AccessTools.Method(sensorType, "CountCell", new[] { typeof(int) });
                _rsSensorCountBuildingMethod = AccessTools.Method(sensorType, "CountBuilding", new[] { typeof(GameObject) });
                _rsSensorIncludeStorageProp = AccessTools.Property(sensorType, "IncludeStorage");

                if (_rsSensorTreeFilterableField != null)
                    _rsAcceptedTagsProp = AccessTools.Property(_rsSensorTreeFilterableField.FieldType, "AcceptedTags");

                if (_rsSensorVisualizerField != null)
                {
                    var vizType = _rsSensorVisualizerField.FieldType;
                    _rsVisCellsField = AccessTools.Field(vizType, "visCells");
                    _rsRefreshMethod = AccessTools.Method(vizType, "Refresh");
                }
            }

            var countCell = AccessTools.Method(sensorType, "CountCell", new[] { typeof(int) });
            if (countCell != null)
                harmony.Patch(countCell, prefix: new HarmonyMethod(typeof(LogicResourceSensor_CountCell_Patch), nameof(LogicResourceSensor_CountCell_Patch.Prefix)));

            // Fix CountBuilding: expand category tags, respect storage scope toggle
            var countBuilding = AccessTools.Method(sensorType, "CountBuilding", new[] { typeof(GameObject) });
            if (countBuilding != null)
                harmony.Patch(countBuilding, prefix: new HarmonyMethod(typeof(LogicResourceSensor_CountBuilding_Patch), nameof(LogicResourceSensor_CountBuilding_Patch.Prefix)));

            // Fix CountDistance: also check FoundationTile layer for tile-based storage (e.g. StorageTile)
            var countDistance = AccessTools.Method(sensorType, "CountDistance", Type.EmptyTypes);
            if (countDistance != null)
                harmony.Patch(countDistance, prefix: new HarmonyMethod(typeof(LogicResourceSensor_CountDistance_Patch), nameof(LogicResourceSensor_CountDistance_Patch.Prefix)));

            // Fix CountRoom: expand scan to include boundary tiles and check FoundationTile layer
            var countRoom = AccessTools.Method(sensorType, "CountRoom", new[] { typeof(Room) });
            if (countRoom != null)
                harmony.Patch(countRoom, prefix: new HarmonyMethod(typeof(LogicResourceSensor_CountRoom_Patch), nameof(LogicResourceSensor_CountRoom_Patch.Prefix)));

            // Threshold: raise max to 9999999, strip units from display
            var getRangeMaxInputField = AccessTools.Method(sensorType, "GetRangeMaxInputField", Type.EmptyTypes);
            if (getRangeMaxInputField != null)
                harmony.Patch(getRangeMaxInputField, postfix: new HarmonyMethod(typeof(LogicResourceSensor_GetRangeMaxInputField_Patch), nameof(LogicResourceSensor_GetRangeMaxInputField_Patch.Postfix)));
            var getRangeMax = AccessTools.Method(sensorType, "get_RangeMax", Type.EmptyTypes);
            if (getRangeMax != null)
                harmony.Patch(getRangeMax, postfix: new HarmonyMethod(typeof(LogicResourceSensor_GetRangeMax_Patch), nameof(LogicResourceSensor_GetRangeMax_Patch.Postfix)));
            var getGetRanges = AccessTools.Method(sensorType, "get_GetRanges", Type.EmptyTypes);
            if (getGetRanges != null)
                harmony.Patch(getGetRanges, postfix: new HarmonyMethod(typeof(LogicResourceSensor_GetRanges_Patch), nameof(LogicResourceSensor_GetRanges_Patch.Postfix)));
            var thresholdValueUnits = AccessTools.Method(sensorType, "ThresholdValueUnits", Type.EmptyTypes);
            if (thresholdValueUnits != null)
                harmony.Patch(thresholdValueUnits, postfix: new HarmonyMethod(typeof(LogicResourceSensor_ThresholdValueUnits_Patch), nameof(LogicResourceSensor_ThresholdValueUnits_Patch.Postfix)));
            // Format(float, bool) - strip " kg" from textbox and tooltips
            var format = AccessTools.Method(sensorType, "Format", new[] { typeof(float), typeof(bool) });
            if (format != null)
                harmony.Patch(format, prefix: new HarmonyMethod(typeof(LogicResourceSensor_Format_Patch), nameof(LogicResourceSensor_Format_Patch.Prefix)));

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

        // Hide "Include Storage Buildings" label+checkbox but keep the row container (avoids breaking layout)
        private static void HideIncludeStorageRow(object sideScreenInstance)
        {
            if (sideScreenInstance == null) return;
            var includeToggle = _rsSideScreenCountStorageToggle?.GetValue(sideScreenInstance) as KToggle;
            if (includeToggle == null) return;
            var checkboxGroup = includeToggle.transform.parent?.gameObject;
            if (checkboxGroup == null) return;
            var row = checkboxGroup.transform.parent;
            if (row == null) return;
            var label = row.Find("Label");
            if (label != null) label.gameObject.SetActive(false);
            checkboxGroup.SetActive(false);
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

                    var roomToggle = _rsSideScreenCountRoomToggle?.GetValue(__instance) as KToggle;

                    var globalToggle = _rsSideScreenCountGlobalToggle?.GetValue(__instance) as KToggle;
                    if (globalToggle != null)
                    {
                        var globalRow = globalToggle.transform.parent?.gameObject;
                        if (globalRow != null)
                            globalRow.SetActive(false);
                    }

                    // Fallback: create a Room Mode row if the toggle failed to bind (game UI changed)
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
                                    _rsSideScreenCountRoomToggle?.SetValue(__instance, newRoomToggle);
                                    if (newRoomCheckmark != null)
                                        _rsSideScreenRoomCheckmark?.SetValue(__instance, newRoomCheckmark);
                                }
                            }
                        }
                    }
                    else
                    {
                        var roomRow = roomToggle.transform.parent?.gameObject;
                        if (roomRow != null) roomRow.SetActive(true);
                    }

                    HideIncludeStorageRow(__instance);

                    // Add Atmosphere / Storage / Conduits rows as siblings of the mode toggles
                    GameObject templateRow = null;
                    if (roomToggle != null)
                        templateRow = roomToggle.transform.parent?.gameObject;
                    if (templateRow == null)
                    {
                        var distanceToggle = _rsSideScreenCountDistanceToggle?.GetValue(__instance) as KToggle;
                        if (distanceToggle != null)
                            templateRow = distanceToggle.transform.parent?.gameObject;
                    }
                    if (templateRow == null)
                    {
                        var contents = root.transform.Find("Contents");
                        var checkboxGroup = contents?.Find("CheckboxGroup") ?? contents?.Find("CheckBoxGroup");
                        if (checkboxGroup != null)
                            templateRow = checkboxGroup.gameObject;
                    }

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

                            var rowLE = row.GetComponent<LayoutElement>() ?? row.AddComponent<LayoutElement>();
                            rowLE.minHeight = 20f;
                        }

                        AddRow("ControlledMods_AtmosphereRow", "Atmosphere", _atmosphereToggles, _atmosphereCheckmarks);
                        AddRow("ControlledMods_StorageRow", "Storage", _storageToggles, _storageCheckmarks);
                        AddRow("ControlledMods_ConduitsRow", "Conduits", _conduitsToggles, _conduitsCheckmarks);

                        // "Element Filter..." button -- clone template row for layout, restyle as button
                        var filterBtnRow = UnityEngine.Object.Instantiate(templateRow, parent);
                        filterBtnRow.name = "ControlledMods_FilterBtnRow";
                        filterBtnRow.SetActive(true);
                        filterBtnRow.transform.SetSiblingIndex(siblingStart++);

                        foreach (Transform child in filterBtnRow.transform)
                        {
                            if (child.name != "Label")
                                child.gameObject.SetActive(false);
                        }

                        var filterLabel = filterBtnRow.transform.Find("Label")?.GetComponent<LocText>();
                        if (filterLabel != null)
                        {
                            filterLabel.SetText("Element Filter...");
                            filterLabel.alignment = TextAlignmentOptions.Center;
                            filterLabel.color = Color.white;
                            filterLabel.faceColor = new Color32(255, 255, 255, 255);
                            var labelRT = filterLabel.GetComponent<RectTransform>();
                            if (labelRT != null)
                                labelRT.anchoredPosition += new Vector2(0f, 3f);
                        }

                        // Dark blue-grey background (#3e4357)
                        var filterBtnImg = filterBtnRow.GetComponent<Image>() ?? filterBtnRow.AddComponent<Image>();
                        filterBtnImg.color = new Color(62f/255f, 67f/255f, 87f/255f, 1f);
                        filterBtnImg.raycastTarget = true;
                        var filterBtn = filterBtnRow.AddComponent<Button>();
                        filterBtn.targetGraphic = filterBtnImg;
                        var cb = ColorBlock.defaultColorBlock;
                        cb.normalColor = Color.white;
                        cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
                        cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                        cb.selectedColor = Color.white;
                        filterBtn.colors = cb;
                        filterBtn.onClick.AddListener(ShowFilterPopup);

                        var filterRowLE = filterBtnRow.GetComponent<LayoutElement>() ?? filterBtnRow.AddComponent<LayoutElement>();
                        filterRowLE.minHeight = 26f;
                        filterRowLE.preferredHeight = 26f;

                        _filterButtons[__instance] = filterBtnRow;

                        var vlg = parent.GetComponent<VerticalLayoutGroup>();
                        if (vlg != null)
                        {
                            vlg.spacing = Mathf.Min(vlg.spacing, 6f);
                            vlg.childForceExpandHeight = false;
                        }
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
                _rsSensorIncludeStorageProp?.SetValue(targetSensor, value);
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
                                var targetSensor = _rsSideScreenTargetSensor?.GetValue(__instance) as Component;
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
                                var targetSensor = _rsSideScreenTargetSensor?.GetValue(__instance) as Component;
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
                                var targetSensor = _rsSideScreenTargetSensor?.GetValue(__instance) as Component;
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

        public static class ResourceSensorSideScreen_ToggleDistance_Patch
        {
            public static void Postfix(object __instance)
            {
                HideIncludeStorageRow(__instance);
            }
        }

        public static class ResourceSensorSideScreen_ToggleRoom_Patch
        {
            public static void Postfix(object __instance)
            {
                HideIncludeStorageRow(__instance);
            }
        }

        public static class ResourceSensorSideScreen_SetTarget_Patch
        {
            public static void Postfix(object __instance, GameObject target)
            {
                try
                {
                    if (__instance == null || target == null) return;
                    if (_rsSensorType == null) return;
                    var sensor = target.GetComponent(_rsSensorType);
                    if (sensor == null) return;

                    var globalToggle = _rsSideScreenCountGlobalToggle?.GetValue(__instance) as KToggle;
                    if (globalToggle != null)
                    {
                        var globalRow = globalToggle.transform.parent?.gameObject;
                        if (globalRow != null)
                            globalRow.SetActive(false);
                    }
                    HideIncludeStorageRow(__instance);

                    // Sync scope checkmarks with persisted state
                    var scope = target.GetComponent<ControlledMods.ResourceSensor.ResourceSensorStorageScope>();
                    if (scope != null)
                    {
                        if (_atmosphereCheckmarks.TryGetValue(__instance, out var ac) && ac != null) ac.enabled = scope.IncludeAtmosphere;
                        if (_storageCheckmarks.TryGetValue(__instance, out var sc) && sc != null) sc.enabled = scope.IncludeStorage;
                        if (_conduitsCheckmarks.TryGetValue(__instance, out var cc) && cc != null) cc.enabled = scope.IncludeConduits;
                        _rsSensorIncludeStorageProp?.SetValue(sensor, scope.IncludeStorage);
                    }

                    var comp = __instance as Component;
                    if (comp != null)
                    {
                        var treeFilterableType = AccessTools.TypeByName("TreeFilterableSideScreen");
                        if (treeFilterableType != null)
                        {
                            var configTab = comp.gameObject.transform.parent;
                            if (configTab != null)
                            {
                                for (int i = 0; i < configTab.childCount; i++)
                                {
                                    var child = configTab.GetChild(i);
                                    if (child.GetComponent(treeFilterableType) != null)
                                    {
                                        // Hide the filter tree in the side screen; accessed via popup instead
                                        _activeFilterTransform = child;
                                        child.gameObject.SetActive(false);
                                        break;
                                    }
                                }
                            }
                        }
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
                    var targetSensor = _rsSideScreenTargetSensor?.GetValue(__instance) as Component;
                    if (targetSensor == null) return;
                    bool include = _rsSensorIncludeStorageProp?.GetValue(targetSensor) is bool b && b;
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

        private static void SetNumberInputCharacterLimit(object numberInput, int limit)
        {
            if (numberInput == null) return;
            var type = numberInput.GetType();
            foreach (var name in new[] { "field", "inputField", "input" })
            {
                var f = AccessTools.Field(type, name);
                if (f == null) continue;
                var obj = f.GetValue(numberInput);
                if (obj == null) continue;
                var t = obj.GetType();
                AccessTools.Property(t, "characterLimit")?.SetValue(obj, limit);
                AccessTools.Property(t, "CharacterLimit")?.SetValue(obj, limit);
                AccessTools.Property(t, "maxLength")?.SetValue(obj, limit);
                var charLimitField = AccessTools.Field(t, "m_CharacterLimit") ?? AccessTools.Field(t, "characterLimit");
                if (charLimitField != null) charLimitField.SetValue(obj, limit);
                break;
            }
        }

        public static class ThresholdSwitchSideScreen_SetTarget_Patch
        {
            public static void Postfix(ThresholdSwitchSideScreen __instance, GameObject new_target)
            {
                if (new_target == null) return;
                if (_rsSensorType == null) return;
                if (new_target.GetComponent(_rsSensorType) == null) return;

                var numberInput = _rsThresholdNumberInputField?.GetValue(__instance);
                SetNumberInputCharacterLimit(numberInput, 8);
            }
        }

        public static class ThresholdSwitchSideScreen_UpdateTargetThresholdLabel_Patch
        {
            public static void Postfix(ThresholdSwitchSideScreen __instance)
            {
                var target = _rsThresholdTargetField?.GetValue(__instance) as GameObject;
                if (target == null) return;
                if (_rsSensorType == null) return;
                if (target.GetComponent(_rsSensorType) == null) return;

                var numberInput = _rsThresholdNumberInputField?.GetValue(__instance);
                SetNumberInputCharacterLimit(numberInput, 8);
            }
        }

        public static class ColoredRangeVisualizer_OnSpawn_Patch
        {
            private static bool IsRangeVisualizerWithVisCells(ColoredRangeVisualizer v)
            {
                if (v == null) return false;
                var t = v.GetType();
                if (t == typeof(ColoredRangeVisualizer)) return false;
                return AccessTools.Field(t, "visCells") != null || AccessTools.Field(t, "m_visCells") != null;
            }

            public static void Postfix(ColoredRangeVisualizer __instance)
            {
                if (__instance == null) return;
                if (!IsRangeVisualizerWithVisCells(__instance)) return;

                Action<object> handler = data =>
                {
                    bool deselected = (data is bool b && !b) || (data == null);
                    if (deselected && __instance is Component c && c != null)
                        DistanceVisualizerHelper.ClearVisualizerLikeRoomMode(c.gameObject);
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

                // Attach persisted scope component for our Atmosphere/Storage/Conduits toggles
                go.AddOrGet<ControlledMods.ResourceSensor.ResourceSensorStorageScope>();

                // If ControlledAutomation is loaded with inversion enabled, add SensorInverter
                try
                {
                    var sensorInverterType = AccessTools.TypeByName("ControlledAutomation.Components.SensorInverter")
                        ?? FindTypeInAssembly("ControlledAutomation", "ControlledAutomation.Components.SensorInverter");
                    if (sensorInverterType == null) return; // ControlledAutomation not loaded
                    var inversionHelperType = AccessTools.TypeByName("ControlledAutomation.Patches.InversionHelper")
                        ?? FindTypeInAssembly("ControlledAutomation", "ControlledAutomation.Patches.InversionHelper");
                    var isEnabledMethod = inversionHelperType != null ? AccessTools.Method(inversionHelperType, "IsInversionEnabled") : null;
                    if (isEnabledMethod != null && isEnabledMethod.Invoke(null, null) is bool enabled && !enabled) return; // Inversion disabled in options
                    if (go.GetComponent(sensorInverterType) == null)
                        go.AddComponent(sensorInverterType);
                }
                catch { }
            }
        }

        public static class KSelectable_Unselect_Patch
        {
            public static void Postfix(KSelectable __instance)
            {
                try
                {
                    if (__instance == null) return;
                    DistanceVisualizerHelper.ClearVisualizerLikeRoomMode(__instance.gameObject);
                    CloseFilterPopup();
                }
                catch { }
            }
        }

        private static void ShowFilterPopup()
        {
            if (_activeFilterTransform == null || _filterPopup != null) return;

            _filterOriginalParent = _activeFilterTransform.parent;
            _filterOriginalSiblingIndex = _activeFilterTransform.GetSiblingIndex();

            var rootCanvas = _activeFilterTransform.GetComponentInParent<Canvas>();
            if (rootCanvas != null) rootCanvas = rootCanvas.rootCanvas;
            if (rootCanvas == null) return;

            // Full-screen overlay
            _filterPopup = new GameObject("ControlledMods_FilterPopup");
            _filterPopup.transform.SetParent(rootCanvas.transform, false);
            var overlayRT = _filterPopup.AddComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.sizeDelta = Vector2.zero;

            // Dim backdrop -- clicking it closes the popup
            var backdrop = new GameObject("Backdrop");
            backdrop.transform.SetParent(_filterPopup.transform, false);
            var backdropRT = backdrop.AddComponent<RectTransform>();
            backdropRT.anchorMin = Vector2.zero;
            backdropRT.anchorMax = Vector2.one;
            backdropRT.sizeDelta = Vector2.zero;
            var backdropImg = backdrop.AddComponent<Image>();
            backdropImg.color = new Color(0f, 0f, 0f, 0.5f);
            backdropImg.raycastTarget = true;
            var backdropBtn = backdrop.AddComponent<Button>();
            backdropBtn.onClick.AddListener(CloseFilterPopup);

            // Panel -- white background to match the filter tree's built-in styling
            var panel = new GameObject("Panel");
            panel.transform.SetParent(_filterPopup.transform, false);
            var panelRT = panel.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(280f, 460f);
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = Color.white;

            // Title bar -- dark header at top
            var titleBar = new GameObject("TitleBar");
            titleBar.transform.SetParent(panel.transform, false);
            var titleBarRT = titleBar.AddComponent<RectTransform>();
            titleBarRT.anchorMin = new Vector2(0f, 1f);
            titleBarRT.anchorMax = new Vector2(1f, 1f);
            titleBarRT.pivot = new Vector2(0.5f, 1f);
            titleBarRT.sizeDelta = new Vector2(0f, 36f);
            var titleBarImg = titleBar.AddComponent<Image>();
            titleBarImg.color = new Color(62f/255f, 67f/255f, 87f/255f, 1f);
            var titleBarHLG = titleBar.AddComponent<HorizontalLayoutGroup>();
            titleBarHLG.childAlignment = TextAnchor.MiddleCenter;
            titleBarHLG.childForceExpandWidth = false;
            titleBarHLG.childForceExpandHeight = false;
            titleBarHLG.padding = new RectOffset(12, 6, 4, 4);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(titleBar.transform, false);
            titleGO.AddComponent<RectTransform>();
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "Element Filter";
            titleText.fontSize = 16f;
            titleText.color = Color.white;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.raycastTarget = false;
            var titleLE = titleGO.AddComponent<LayoutElement>();
            titleLE.flexibleWidth = 1f;

            var closeBtnGO = new GameObject("CloseBtn");
            closeBtnGO.transform.SetParent(titleBar.transform, false);
            closeBtnGO.AddComponent<RectTransform>();
            var closeBtnLE = closeBtnGO.AddComponent<LayoutElement>();
            closeBtnLE.minWidth = 28f;
            closeBtnLE.minHeight = 28f;
            var closeBtnImg = closeBtnGO.AddComponent<Image>();
            closeBtnImg.color = new Color(0.35f, 0.15f, 0.15f, 1f);
            closeBtnImg.raycastTarget = true;
            var closeBtn = closeBtnGO.AddComponent<Button>();
            closeBtn.targetGraphic = closeBtnImg;
            var closeBtnColors = closeBtn.colors;
            closeBtnColors.highlightedColor = new Color(0.55f, 0.20f, 0.20f, 1f);
            closeBtnColors.pressedColor = new Color(0.25f, 0.10f, 0.10f, 1f);
            closeBtn.colors = closeBtnColors;
            closeBtn.onClick.AddListener(CloseFilterPopup);

            var closeX = new GameObject("X");
            closeX.transform.SetParent(closeBtnGO.transform, false);
            var closeXRT = closeX.AddComponent<RectTransform>();
            closeXRT.anchorMin = Vector2.zero;
            closeXRT.anchorMax = Vector2.one;
            closeXRT.sizeDelta = Vector2.zero;
            var closeXText = closeX.AddComponent<TextMeshProUGUI>();
            closeXText.text = "\u2715";
            closeXText.fontSize = 16f;
            closeXText.color = Color.white;
            closeXText.alignment = TextAlignmentOptions.Center;
            closeXText.raycastTarget = false;

            // Content area -- positioned below title bar, fills remaining space, clips overflow
            var content = new GameObject("Content");
            content.transform.SetParent(panel.transform, false);
            var contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = Vector2.zero;
            contentRT.anchorMax = Vector2.one;
            contentRT.offsetMin = new Vector2(0f, 0f);
            contentRT.offsetMax = new Vector2(0f, -36f);
            content.AddComponent<RectMask2D>();

            // Reparent the TreeFilterableSideScreen into the popup
            _activeFilterTransform.SetParent(content.transform, false);
            _activeFilterTransform.gameObject.SetActive(true);

            // Stretch the filter to fill the content area
            var filterRT = _activeFilterTransform as RectTransform;
            if (filterRT != null)
            {
                filterRT.anchorMin = Vector2.zero;
                filterRT.anchorMax = Vector2.one;
                filterRT.offsetMin = Vector2.zero;
                filterRT.offsetMax = Vector2.zero;
            }

            // Remove any LayoutElement constraints that prevent stretching
            var filterLE = _activeFilterTransform.GetComponent<LayoutElement>();
            if (filterLE != null)
            {
                filterLE.preferredWidth = -1f;
                filterLE.minWidth = -1f;
            }

            // Force all child ScrollRects to fill available width
            foreach (var sr in _activeFilterTransform.GetComponentsInChildren<ScrollRect>(true))
            {
                var srRT = sr.GetComponent<RectTransform>();
                if (srRT != null)
                {
                    srRT.anchorMin = Vector2.zero;
                    srRT.anchorMax = Vector2.one;
                    srRT.offsetMin = Vector2.zero;
                    srRT.offsetMax = Vector2.zero;
                }
            }
        }

        private static void CloseFilterPopup()
        {
            if (_filterPopup == null) return;

            if (_activeFilterTransform != null && _filterOriginalParent != null)
            {
                _activeFilterTransform.SetParent(_filterOriginalParent, false);
                _activeFilterTransform.SetSiblingIndex(_filterOriginalSiblingIndex);
                _activeFilterTransform.gameObject.SetActive(false);
            }

            UnityEngine.Object.Destroy(_filterPopup);
            _filterPopup = null;
        }

        public static class LogicResourceSensor_CountCell_Patch
        {
            public static bool Prefix(object __instance, int cell, ref float __result)
            {
                try
                {
                    if (__instance == null) return true;

                    if (!RefreshSensorCache(__instance))
                    {
                        __result = 0f;
                        return false;
                    }

                    var scope = _lastSensorScope;
                    var effective = _lastSensorEffective;
                    var elementIds = _lastSensorElementIds;

                    float totalMass = 0f;

                    if (scope == null || scope.IncludeAtmosphere)
                    {
                        if (Grid.IsValidCell(cell))
                        {
                            Element cellElement = Grid.Element[cell];
                            if (cellElement != null && cellElement.id != SimHashes.Vacuum
                                && elementIds.Contains(cellElement.id))
                            {
                                totalMass += Grid.Mass[cell];
                            }
                        }

                        GameObject obj = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
                        if (obj != null && obj.TryGetComponent<Pickupable>(out var pickupable))
                        {
                            ObjectLayerListItem objectLayerListItem = pickupable.objectLayerListItem;
                            while (objectLayerListItem != null)
                            {
                                GameObject obj2 = objectLayerListItem.gameObject;
                                objectLayerListItem = objectLayerListItem.nextItem;

                                if (obj2 != null && obj2.TryGetComponent<MinionIdentity>(out _) == false
                                    && obj2.TryGetComponent<KPrefabID>(out KPrefabID kPrefabID))
                                {
                                    foreach (var tag in effective)
                                    {
                                        if (kPrefabID.HasTag(tag))
                                        {
                                            if (obj2.TryGetComponent<PrimaryElement>(out var pe))
                                                totalMass += pe.Mass;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (scope != null && scope.IncludeConduits && Grid.IsValidCell(cell))
                    {
                        var gasFlow = Game.Instance?.gasConduitFlow;
                        if (gasFlow != null)
                        {
                            var gasContents = gasFlow.GetContents(cell);
                            if (gasContents.mass > 0f && gasContents.element != SimHashes.Vacuum
                                && elementIds.Contains(gasContents.element))
                            {
                                totalMass += gasContents.mass;
                            }
                        }
                        var liquidFlow = Game.Instance?.liquidConduitFlow;
                        if (liquidFlow != null)
                        {
                            var liquidContents = liquidFlow.GetContents(cell);
                            if (liquidContents.mass > 0f && liquidContents.element != SimHashes.Vacuum
                                && elementIds.Contains(liquidContents.element))
                            {
                                totalMass += liquidContents.mass;
                            }
                        }
                        var solidFlow = Game.Instance?.solidConduitFlow;
                        if (solidFlow != null)
                        {
                            var solidContents = solidFlow.GetContents(cell);
                            if (solidContents.pickupableHandle.IsValid())
                            {
                                var solidPickupable = solidFlow.GetPickupable(solidContents.pickupableHandle);
                                if (solidPickupable != null && solidPickupable.GetComponent<MinionIdentity>() == null
                                    && solidPickupable.TryGetComponent<KPrefabID>(out var kPrefabID)
                                    && solidPickupable.TryGetComponent<PrimaryElement>(out var sPe))
                                {
                                    foreach (var tag in effective)
                                    {
                                        if (kPrefabID.HasTag(tag))
                                        {
                                            totalMass += sPe.Mass;
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

                    if (!RefreshSensorCache(__instance))
                    {
                        __result = 0f;
                        return false;
                    }

                    var scope = _lastSensorScope;
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

                    if (!obj.TryGetComponent(out Storage storage))
                    {
                        __result = 0f;
                        return false;
                    }

                    var effective = _lastSensorEffective;

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

        // CountDistance: also check FoundationTile layer for tile-based storage buildings
        public static class LogicResourceSensor_CountDistance_Patch
        {
            public static bool Prefix(object __instance, ref float __result)
            {
                try
                {
                    if (__instance == null) return true;

                    var cmp = __instance as Component;
                    if (cmp == null) return true;

                    var scope = cmp.GetComponent<ControlledMods.ResourceSensor.ResourceSensorStorageScope>();

                    var logicPorts = _rsSensorLogicPortsField?.GetValue(__instance) as LogicPorts;
                    if (logicPorts == null) return true;
                    int cell = logicPorts.GetPortCell(LogicSwitch.PORT_ID);

                    var visualizer = _rsSensorVisualizerField?.GetValue(__instance);
                    var visCells = visualizer != null ? _rsVisCellsField?.GetValue(visualizer) as System.Collections.IList : null;

                    int distance = _rsSensorDistanceProp != null ? (int)_rsSensorDistanceProp.GetValue(__instance) : 0;

                    bool includeStorage = _rsSensorIncludeStorageField != null && (bool)_rsSensorIncludeStorageField.GetValue(__instance);

                    bool visualiserDirty = _rsSensorVisualiserDirtyField != null && (bool)_rsSensorVisualiserDirtyField.GetValue(__instance);

                    var selectable = _rsSensorSelectableField?.GetValue(__instance) as KSelectable;

                    visCells?.Clear();

                    if (distance == 0)
                    {
                        if (visualiserDirty && selectable != null && selectable.IsSelected)
                        {
                            _rsRefreshMethod?.Invoke(visualizer, null);
                        }

                        _invokeArgCell[0] = cell;
                        float cellMass = _rsSensorCountCellMethod != null ? (float)_rsSensorCountCellMethod.Invoke(__instance, _invokeArgCell) : 0f;

                        float tileMass = 0f;
                        if (includeStorage && (scope == null || scope.IncludeStorage))
                            tileMass = CountFoundationTileStorage(__instance, cell);

                        __result = cellMass + tileMass;
                        return false;
                    }

                    _poolCountedBuildings.Clear();
                    _poolCountedTiles.Clear();

                    Grid.CellToXY(cell, out int cellX, out int cellY);
                    int minX = cellX - distance;
                    int maxX = cellX + distance;
                    int minY = cellY - distance;
                    int maxY = cellY + distance;

                    float totalMass = 0f;

                    for (int x = minX; x <= maxX; x++)
                    {
                        for (int y = minY; y <= maxY; y++)
                        {
                            int searchCell = Grid.XYToCell(x, y);

                            if (!Grid.IsSolidCell(searchCell))
                            {
                                visCells?.Add(searchCell);

                                if (_rsSensorCountCellMethod != null)
                                {
                                    _invokeArgCell[0] = searchCell;
                                    totalMass += (float)_rsSensorCountCellMethod.Invoke(__instance, _invokeArgCell);
                                }

                                if (includeStorage)
                                {
                                    GameObject obj = Grid.Objects[searchCell, (int)ObjectLayer.Building];
                                    if (obj != null && _poolCountedBuildings.Add(obj) && _rsSensorCountBuildingMethod != null)
                                    {
                                        _invokeArgObj[0] = obj;
                                        totalMass += (float)_rsSensorCountBuildingMethod.Invoke(__instance, _invokeArgObj);
                                    }
                                }
                            }

                            if (includeStorage && (scope == null || scope.IncludeStorage))
                            {
                                GameObject tileObj = Grid.Objects[searchCell, (int)ObjectLayer.FoundationTile];
                                if (tileObj != null && _poolCountedTiles.Add(tileObj) && _rsSensorCountBuildingMethod != null)
                                {
                                    _invokeArgObj[0] = tileObj;
                                    totalMass += (float)_rsSensorCountBuildingMethod.Invoke(__instance, _invokeArgObj);
                                }
                            }
                        }
                    }

                    if (visualiserDirty && selectable != null && selectable.IsSelected)
                    {
                        _rsRefreshMethod?.Invoke(visualizer, null);
                    }

                    __result = totalMass;
                    return false;
                }
                catch { }
                return true;
            }
        }

        // CountRoom: expand scan to include boundary tiles (floor/walls/ceiling) and check FoundationTile
        public static class LogicResourceSensor_CountRoom_Patch
        {
            public static bool Prefix(object __instance, Room room, ref float __result)
            {
                try
                {
                    if (__instance == null || room == null || room.cavity == null) return true;

                    var cmp = __instance as Component;
                    if (cmp == null) return true;

                    var scope = cmp.GetComponent<ControlledMods.ResourceSensor.ResourceSensorStorageScope>();

                    bool includeStorage = _rsSensorIncludeStorageField != null && (bool)_rsSensorIncludeStorageField.GetValue(__instance);

                    int minX = room.cavity.minX;
                    int maxX = room.cavity.maxX;
                    int minY = room.cavity.minY;
                    int maxY = room.cavity.maxY;

                    _poolCountedBuildings.Clear();
                    _poolCountedTiles.Clear();

                    RoomProber roomProber = Game.Instance.roomProber;

                    float totalMass = 0f;

                    // Expanded scan: 1 extra cell in each direction to catch boundary tiles
                    for (int x = minX - 1; x <= maxX + 1; x++)
                    {
                        for (int y = minY - 1; y <= maxY + 1; y++)
                        {
                            int cell = Grid.XYToCell(x, y);

                            if (!Grid.IsSolidCell(cell) && roomProber.GetCavityForCell(cell) == room.cavity)
                            {
                                if (_rsSensorCountCellMethod != null)
                                {
                                    _invokeArgCell[0] = cell;
                                    totalMass += (float)_rsSensorCountCellMethod.Invoke(__instance, _invokeArgCell);
                                }

                                if (includeStorage)
                                {
                                    GameObject obj = Grid.Objects[cell, (int)ObjectLayer.Building];
                                    if (obj != null && _poolCountedBuildings.Add(obj) && _rsSensorCountBuildingMethod != null)
                                    {
                                        _invokeArgObj[0] = obj;
                                        totalMass += (float)_rsSensorCountBuildingMethod.Invoke(__instance, _invokeArgObj);
                                    }
                                }
                            }

                            if (includeStorage && (scope == null || scope.IncludeStorage))
                            {
                                GameObject tileObj = Grid.Objects[cell, (int)ObjectLayer.FoundationTile];
                                if (tileObj != null && _poolCountedTiles.Add(tileObj) && _rsSensorCountBuildingMethod != null)
                                {
                                    _invokeArgObj[0] = tileObj;
                                    totalMass += (float)_rsSensorCountBuildingMethod.Invoke(__instance, _invokeArgObj);
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

        // Helper: count storage mass in a FoundationTile building at a single cell
        private static float CountFoundationTileStorage(object sensorInstance, int cell)
        {
            GameObject tileObj = Grid.Objects[cell, (int)ObjectLayer.FoundationTile];
            if (tileObj == null || _rsSensorCountBuildingMethod == null) return 0f;

            _invokeArgObj[0] = tileObj;
            return (float)_rsSensorCountBuildingMethod.Invoke(sensorInstance, _invokeArgObj);
        }

        public static class LogicResourceSensor_GetRangeMaxInputField_Patch
        {
            public static void Postfix(ref float __result)
            {
                __result = 9999999f;
            }
        }

        public static class LogicResourceSensor_GetRangeMax_Patch
        {
            public static void Postfix(ref float __result)
            {
                __result = 9999999f;
            }
        }

        public static class LogicResourceSensor_GetRanges_Patch
        {
            public static void Postfix(ref object __result)
            {
                try
                {
                    var nonLinearSliderType = AccessTools.TypeByName("NonLinearSlider");
                    var getDefaultRange = nonLinearSliderType != null ? AccessTools.Method(nonLinearSliderType, "GetDefaultRange", new[] { typeof(float) }) : null;
                    if (getDefaultRange != null)
                        __result = getDefaultRange.Invoke(null, new object[] { 9999999f });
                }
                catch { }
            }
        }

        public static class LogicResourceSensor_ThresholdValueUnits_Patch
        {
            public static void Postfix(ref LocString __result)
            {
                __result = (LocString)"";
            }
        }

        // Strip " kg" from threshold display (both textbox and tooltips)
        public static class LogicResourceSensor_Format_Patch
        {
            public static bool Prefix(float value, ref string __result)
            {
                __result = value >= 1000000f ? value.ToString("0") : value.ToString("0.##");
                return false;
            }
        }
    }
}
