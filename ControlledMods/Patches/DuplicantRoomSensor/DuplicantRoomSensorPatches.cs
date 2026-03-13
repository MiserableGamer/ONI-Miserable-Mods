using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ControlledMods.ModDetection;
using ControlledMods.Options;

namespace ControlledMods.Patches.DuplicantRoomSensor
{
    public static class DuplicantRoomSensorPatches
    {
        private static readonly Color ShowRangeDefaultColor = new Color(0f, 1f, 0.8f, 1f);
        private static Type _dupRoomSensorType;
        private static Type _dupRoomCavityMapType;
        private static FieldInfo _dupRoomCavityMapField;
        private static Type _showRangeSimParamsType;
        private static Type _showRangeSimVisualizerType;
        private static Type _showRangeRendererType;
        private static bool _showRangeTypesResolved;

        // Cached reflection for sensor fields (resolved once in ApplyPatches)
        private static FieldInfo _sensorSelectableField;
        private static FieldInfo _sensorCurrentCountField;
        private static FieldInfo _sensorCountThresholdField;
        private static FieldInfo _sensorActivateOnGreaterThanField;
        private static MethodInfo _sensorSetStateMethod;
        private static FieldInfo _sensorRoomStatusGUIDField;
        private static PropertyInfo _doorIsOpenProp;
        private static Func<Door, bool> _doorIsOpenGetter;

        // Cached reflection for ThresholdSwitchSideScreen fields
        private static FieldInfo _tsssAboveToggleField;
        private static FieldInfo _tsssNumberInputField;
        private static FieldInfo _tsssThresholdSliderField;
        private static FieldInfo _tsssTargetField;

        private static FieldInfo _showRangeVisualizersField;
        private static FieldInfo _showRangeWorstCaseRadiusField;
        private static FieldInfo _showRangeHighlightColorField;
        private static FieldInfo _showRangeRendererLastCellField;
        private static FieldInfo _showRangeRendererLastTransformField;
        private static Array _emptyVisualizerArray;
        private static UnityEngine.Object[] _cachedShowRangeRenderers;
        private static readonly Dictionary<ThresholdSwitchSideScreen, KToggle> _rangeLimitToggles = new Dictionary<ThresholdSwitchSideScreen, KToggle>();
        private static readonly Dictionary<ThresholdSwitchSideScreen, KNumberInputField> _rangeInputs = new Dictionary<ThresholdSwitchSideScreen, KNumberInputField>();
        private static readonly Dictionary<ThresholdSwitchSideScreen, GameObject> _rangeToggleRows = new Dictionary<ThresholdSwitchSideScreen, GameObject>();
        private static readonly Dictionary<ThresholdSwitchSideScreen, GameObject> _rangeInputRows = new Dictionary<ThresholdSwitchSideScreen, GameObject>();
        private static readonly Dictionary<ThresholdSwitchSideScreen, GameObject> _rangeShowButtonRows = new Dictionary<ThresholdSwitchSideScreen, GameObject>();
        private static readonly Dictionary<ThresholdSwitchSideScreen, GameObject> _rangeSpacerRows = new Dictionary<ThresholdSwitchSideScreen, GameObject>();
        private static readonly Dictionary<ThresholdSwitchSideScreen, System.Action> _rangeToggleHandlers = new Dictionary<ThresholdSwitchSideScreen, System.Action>();
        private static readonly Dictionary<ThresholdSwitchSideScreen, System.Action> _rangeInputHandlers = new Dictionary<ThresholdSwitchSideScreen, System.Action>();
        private static readonly Dictionary<ThresholdSwitchSideScreen, System.Action> _rangeShowButtonHandlers = new Dictionary<ThresholdSwitchSideScreen, System.Action>();

        public static void ApplyPatches(Harmony harmony)
        {
            if (!DuplicantRoomSensorDetection.Loaded)
                return;

            var configType = AccessTools.TypeByName("DuplicantRoomSensor.DuplicantRoomSensorConfig");
            var doPostConfigureComplete = AccessTools.Method(configType, "DoPostConfigureComplete", new[] { typeof(GameObject) });
            if (doPostConfigureComplete != null)
            {
                harmony.Patch(doPostConfigureComplete, postfix: new HarmonyMethod(typeof(DuplicantRoomSensorConfig_DoPostConfigureComplete_Patch), nameof(DuplicantRoomSensorConfig_DoPostConfigureComplete_Patch.Postfix)));
            }

            var sensorType = AccessTools.TypeByName("DuplicantRoomSensor.LogicDuplicantCountSensor");
            if (sensorType != null)
            {
                _sensorSelectableField = AccessTools.Field(sensorType, "selectable");
                _sensorCurrentCountField = AccessTools.Field(sensorType, "currentCount");
                _sensorCountThresholdField = AccessTools.Field(sensorType, "countThreshold");
                _sensorActivateOnGreaterThanField = AccessTools.Field(sensorType, "activateOnGreaterThan");
                _sensorSetStateMethod = AccessTools.Method(sensorType, "SetState", new[] { typeof(bool) });
                _sensorRoomStatusGUIDField = AccessTools.Field(sensorType, "roomStatusGUID");
            }

            _doorIsOpenProp = AccessTools.Property(typeof(Door), "IsOpen")
                ?? AccessTools.Property(typeof(Door), "isOpen");
            if (_doorIsOpenProp?.GetGetMethod() is MethodInfo doorGetter)
                _doorIsOpenGetter = (Func<Door, bool>)Delegate.CreateDelegate(typeof(Func<Door, bool>), doorGetter);

            _tsssAboveToggleField = AccessTools.Field(typeof(ThresholdSwitchSideScreen), "aboveToggle");
            _tsssNumberInputField = AccessTools.Field(typeof(ThresholdSwitchSideScreen), "numberInput");
            _tsssThresholdSliderField = AccessTools.Field(typeof(ThresholdSwitchSideScreen), "thresholdSlider");
            _tsssTargetField = AccessTools.Field(typeof(ThresholdSwitchSideScreen), "target");

            var sim1000ms = AccessTools.Method(sensorType, "Sim1000ms", new[] { typeof(float) });
            if (sim1000ms != null)
            {
                harmony.Patch(sim1000ms, prefix: new HarmonyMethod(typeof(LogicDuplicantCountSensor_Sim1000ms_Patch), nameof(LogicDuplicantCountSensor_Sim1000ms_Patch.Prefix)));
            }

            var setTarget = AccessTools.Method(typeof(ThresholdSwitchSideScreen), "SetTarget", new[] { typeof(GameObject) });
            if (setTarget != null)
            {
                harmony.Patch(setTarget, postfix: new HarmonyMethod(typeof(ThresholdSwitchSideScreen_SetTarget_Patch), nameof(ThresholdSwitchSideScreen_SetTarget_Patch.Postfix)));
            }

            var updateTargetThresholdLabel = AccessTools.Method(typeof(ThresholdSwitchSideScreen), "UpdateTargetThresholdLabel", Type.EmptyTypes);
            if (updateTargetThresholdLabel != null)
            {
                harmony.Patch(updateTargetThresholdLabel, postfix: new HarmonyMethod(typeof(ThresholdSwitchSideScreen_UpdateTargetThresholdLabel_Patch), nameof(ThresholdSwitchSideScreen_UpdateTargetThresholdLabel_Patch.Postfix)));
            }

            var unselect = AccessTools.Method(typeof(KSelectable), "Unselect", Type.EmptyTypes);
            if (unselect != null)
                harmony.Patch(unselect, postfix: new HarmonyMethod(typeof(KSelectable_Unselect_DupRoom_Patch), nameof(KSelectable_Unselect_DupRoom_Patch.Postfix)));

            ControlledModsMod.Log("Duplicant Room Sensor patches applied");
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
                    if (t != null)
                        return t;
                }
                catch
                {
                }
            }
            return null;
        }

        private static bool IsDuplicantRoomSensorTarget(GameObject target, out Component sensor)
        {
            sensor = null;
            if (target == null)
                return false;

            if (_dupRoomSensorType == null)
            {
                _dupRoomSensorType = AccessTools.TypeByName("DuplicantRoomSensor.LogicDuplicantCountSensor")
                    ?? FindTypeInAssembly("DuplicantRoomSensor", "DuplicantRoomSensor.LogicDuplicantCountSensor");
            }
            if (_dupRoomSensorType == null)
                return false;

            sensor = target.GetComponent(_dupRoomSensorType);
            return sensor != null;
        }

        private static void InvalidateRangeCache(DuplicantRoomSensorRangeSettings settings)
        {
            if (settings == null)
                return;
            settings.CachedOriginCell = Grid.InvalidCell;
            settings.CachedRange = -1;
            settings.CachedCavity = null;
            settings.CachedMinX = int.MinValue;
            settings.CachedMaxX = int.MinValue;
            settings.CachedMinY = int.MinValue;
            settings.CachedMaxY = int.MinValue;
            settings.CachedReachableCells.Clear();
            settings.LastReachableRebuildTime = -9999f;
            settings.LastCurrentCount = -1;
            settings.LastWasInRoom = false;
        }

        /// <summary>Clears the ShowRange visualizer for a Duplicant Room Sensor (e.g. when sidescreen closes or building is deselected).</summary>
        private static void ClearShowRangeVisualizer(GameObject building)
        {
            if (building == null || !ShowRangeDetection.Loaded || !TryResolveShowRangeTypes())
                return;
            var settings = building.GetComponent<DuplicantRoomSensorRangeSettings>();
            if (settings == null)
                return;
            try
            {
                var simParams = building.GetComponent(_showRangeSimParamsType);
                if (simParams != null && _showRangeVisualizersField != null)
                {
                    if (_emptyVisualizerArray == null)
                        _emptyVisualizerArray = Array.CreateInstance(_showRangeSimVisualizerType, 0);
                    _showRangeVisualizersField.SetValue(simParams, _emptyVisualizerArray);
                    _showRangeWorstCaseRadiusField?.SetValue(simParams, 0);
                }
                InvalidateShowRangeCache(settings);
                ForceShowRangeRefresh();
            }
            catch { }
        }

        private static void InvalidateShowRangeCache(DuplicantRoomSensorRangeSettings settings)
        {
            if (settings == null)
                return;

            settings.LastShowRangeEnabled = false;
            settings.LastShowRangeRange = int.MinValue;
            settings.LastShowRangeOriginCell = Grid.InvalidCell;
            settings.LastShowRangeReachableCount = -1;
            settings.LastShowRangeReachableXor = int.MinValue;
            settings.LastShowRangeReachableSum = long.MinValue;
        }

        private static void EnsureRows(ThresholdSwitchSideScreen screen)
        {
            if (screen == null)
                return;

            if (_rangeLimitToggles.TryGetValue(screen, out var existingToggle) && existingToggle != null
                && _rangeInputs.TryGetValue(screen, out var existingInput) && existingInput != null)
                return;

            var aboveToggle = _tsssAboveToggleField?.GetValue(screen) as KToggle;
            var numberInput = _tsssNumberInputField?.GetValue(screen) as KNumberInputField;
            if (aboveToggle == null || numberInput == null)
                return;

            var toggleTemplate = aboveToggle.gameObject;
            var numberInputRowTemplate = numberInput.transform.parent?.gameObject;
            if (toggleTemplate == null || numberInputRowTemplate == null)
                return;

            Transform parent = numberInputRowTemplate.transform.parent;
            if (parent == null)
                return;

            var toggleRow = parent.Find("ControlledMods_DupRoomRangeToggle")?.gameObject;
            if (toggleRow == null)
            {
                toggleRow = UnityEngine.Object.Instantiate(toggleTemplate, parent);
                toggleRow.name = "ControlledMods_DupRoomRangeToggle";
            }
            int startIndex = numberInputRowTemplate.transform.GetSiblingIndex() + 1;
            var thresholdSlider = _tsssThresholdSliderField?.GetValue(screen) as NonLinearSlider;
            var sliderRow = thresholdSlider != null ? thresholdSlider.transform.parent : null;
            if (sliderRow != null && sliderRow.parent == parent)
                startIndex = sliderRow.GetSiblingIndex() + 1;

            toggleRow.transform.SetSiblingIndex(startIndex);

            var rangeInputRow = parent.Find("ControlledMods_DupRoomRangeInput")?.gameObject;
            if (rangeInputRow == null)
            {
                rangeInputRow = UnityEngine.Object.Instantiate(numberInputRowTemplate, parent);
                rangeInputRow.name = "ControlledMods_DupRoomRangeInput";
            }
            rangeInputRow.transform.SetSiblingIndex(startIndex + 1);

            var spacerRow = parent.Find("ControlledMods_DupRoomRangeSpacer")?.gameObject;
            if (spacerRow == null)
            {
                spacerRow = new GameObject("ControlledMods_DupRoomRangeSpacer", typeof(RectTransform));
                spacerRow.transform.SetParent(parent, false);
                var spacerLE = spacerRow.AddComponent<LayoutElement>();
                spacerLE.minHeight = 8f;
            }
            spacerRow.transform.SetSiblingIndex(startIndex + 2);

            var showRangeButtonRow = parent.Find("ControlledMods_DupRoomShowRangeButton")?.gameObject;
            if (showRangeButtonRow == null)
            {
                showRangeButtonRow = UnityEngine.Object.Instantiate(toggleTemplate, parent);
                showRangeButtonRow.name = "ControlledMods_DupRoomShowRangeButton";
            }
            showRangeButtonRow.transform.SetSiblingIndex(startIndex + 3);

            _rangeSpacerRows[screen] = spacerRow;

            var toggle = toggleRow.GetComponent<KToggle>() ?? toggleRow.GetComponentInChildren<KToggle>(true);
            if (toggle == null)
                return;

            var toggleText = toggleRow.GetComponentInChildren<TMP_Text>(true);
            if (toggleText != null)
                toggleText.text = "Enable Range Limit";

            var input = rangeInputRow.GetComponentInChildren<KNumberInputField>(true);
            if (input == null)
                return;

            var inputLabel = rangeInputRow.GetComponentInChildren<LocText>(true);
            if (inputLabel != null)
                inputLabel.text = "Range (cells)";

            input.minValue = DuplicantRoomSensorRangeSettings.MinRange;
            input.maxValue = DuplicantRoomSensorRangeSettings.MaxRange;
            input.decimalPlaces = 0;

            var showRangeButton = showRangeButtonRow.GetComponent<KToggle>() ?? showRangeButtonRow.GetComponentInChildren<KToggle>(true);
            var showRangeButtonText = showRangeButtonRow.GetComponentInChildren<TMP_Text>(true);
            if (showRangeButtonText != null)
            {
                showRangeButtonText.text = "Show Range";
                showRangeButtonText.enableWordWrapping = false;
                showRangeButtonText.overflowMode = TMPro.TextOverflowModes.Overflow;
            }
            var showRangeButtonLE = showRangeButtonRow.GetComponent<LayoutElement>() ?? showRangeButtonRow.AddComponent<LayoutElement>();
            showRangeButtonLE.minWidth = 120f;

            _rangeLimitToggles[screen] = toggle;
            _rangeInputs[screen] = input;
            _rangeToggleRows[screen] = toggleRow;
            _rangeInputRows[screen] = rangeInputRow;
            _rangeShowButtonRows[screen] = showRangeButtonRow;
        }

        private static void BindRowHandlers(ThresholdSwitchSideScreen screen, GameObject target, DuplicantRoomSensorRangeSettings settings)
        {
            if (screen == null || target == null || settings == null)
                return;

            if (!_rangeLimitToggles.TryGetValue(screen, out var toggle) || toggle == null)
                return;
            if (!_rangeInputs.TryGetValue(screen, out var input) || input == null)
                return;

            if (_rangeToggleHandlers.TryGetValue(screen, out var oldToggleHandler))
                toggle.onClick -= oldToggleHandler;
            System.Action toggleHandler = () =>
            {
                settings.EnableRangeLimit = !settings.EnableRangeLimit;
                InvalidateRangeCache(settings);
                SyncRowState(screen, target, settings);
            };
            _rangeToggleHandlers[screen] = toggleHandler;
            toggle.onClick += toggleHandler;

            if (_rangeInputHandlers.TryGetValue(screen, out var oldInputHandler))
                input.onEndEdit -= oldInputHandler;
            System.Action inputHandler = () =>
            {
                settings.RangeCells = Mathf.RoundToInt(input.currentValue);
                settings.RangeCells = settings.GetClampedRange();
                InvalidateRangeCache(settings);
                SyncRowState(screen, target, settings);
            };
            _rangeInputHandlers[screen] = inputHandler;
            input.onEndEdit += inputHandler;

            if (_rangeInputRows.TryGetValue(screen, out var rangeInputRow) && rangeInputRow != null)
            {
                var buttons = rangeInputRow.GetComponentsInChildren<IncrementorToggle>(true);
                if (buttons != null && buttons.Length >= 4)
                {
                    Array.Sort(buttons, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
                    int[] deltas = { -10, -1, 1, 10 };
                    for (int i = 0; i < buttons.Length; i++)
                    {
                        var button = buttons[i];
                        if (button == null)
                            continue;
                        int delta = deltas[Mathf.Clamp(i, 0, deltas.Length - 1)];

                        button.onClick = () =>
                        {
                            settings.RangeCells = Mathf.Clamp(settings.RangeCells + delta, DuplicantRoomSensorRangeSettings.MinRange, DuplicantRoomSensorRangeSettings.MaxRange);
                            InvalidateRangeCache(settings);
                            SyncRowState(screen, target, settings);
                            button.ChangeState(1);
                        };
                        button.onStopHold = () => button.ChangeState(0);
                        button.enabled = true;
                    }
                }
            }

            if (_rangeShowButtonRows.TryGetValue(screen, out var showRangeButtonRow) && showRangeButtonRow != null)
            {
                var showRangeButton = showRangeButtonRow.GetComponent<KToggle>() ?? showRangeButtonRow.GetComponentInChildren<KToggle>(true);
                if (showRangeButton != null)
                {
                    if (_rangeShowButtonHandlers.TryGetValue(screen, out var oldHandler))
                        showRangeButton.onClick -= oldHandler;
                    System.Action showRangeHandler = () =>
                    {
                        bool currentlyOn = settings.LastShowRangeEnabled;
                        UpdateShowRangeVisualizer(target, settings, forceEnabled: !currentlyOn);
                        showRangeButton.isOn = !currentlyOn;
                    };
                    _rangeShowButtonHandlers[screen] = showRangeHandler;
                    showRangeButton.onClick += showRangeHandler;
                }
            }
        }

        private static void SyncRowVisibility(ThresholdSwitchSideScreen screen, bool visible)
        {
            if (screen == null)
                return;

            if (_rangeToggleRows.TryGetValue(screen, out var toggleRow) && toggleRow != null)
                toggleRow.SetActive(visible);
            if (_rangeInputRows.TryGetValue(screen, out var inputRow) && inputRow != null)
                inputRow.SetActive(visible);
            if (_rangeShowButtonRows.TryGetValue(screen, out var showButtonRow) && showButtonRow != null)
                showButtonRow.SetActive(visible);
            if (_rangeSpacerRows.TryGetValue(screen, out var spacerRow) && spacerRow != null)
                spacerRow.SetActive(visible);
        }

        private static void SyncRowState(ThresholdSwitchSideScreen screen, GameObject target, DuplicantRoomSensorRangeSettings settings)
        {
            if (screen == null || target == null || settings == null)
                return;

            if (_rangeLimitToggles.TryGetValue(screen, out var toggle) && toggle != null)
            {
                toggle.isOn = settings.EnableRangeLimit;
                var imgState = toggle.GetComponent<ImageToggleState>();
                if (imgState != null)
                    imgState.SetState(settings.EnableRangeLimit ? ImageToggleState.State.Active : ImageToggleState.State.Inactive);
            }

            if (_rangeInputs.TryGetValue(screen, out var input) && input != null)
            {
                int clamped = settings.GetClampedRange();
                if (settings.RangeCells != clamped)
                    settings.RangeCells = clamped;

                input.minValue = DuplicantRoomSensorRangeSettings.MinRange;
                input.maxValue = DuplicantRoomSensorRangeSettings.MaxRange;
                if (Mathf.Abs(input.currentValue - clamped) > 0.01f)
                {
                    input.SetDisplayValue(clamped.ToString());
                    input.currentValue = clamped;
                }
            }

            EnsureShowRangeVisualizersNotNull(target);
            if (ControlledModsOptions.Instance.ShowRangeOnSidescreenOpen)
                UpdateShowRangeVisualizer(target, settings);

            if (_rangeShowButtonRows.TryGetValue(screen, out var showButtonRow) && showButtonRow != null)
            {
                var showBtn = showButtonRow.GetComponent<KToggle>() ?? showButtonRow.GetComponentInChildren<KToggle>(true);
                if (showBtn != null)
                    showBtn.isOn = settings.LastShowRangeEnabled;
            }
        }

        private static IDictionary TryGetCavityMap()
        {
            if (_dupRoomCavityMapType == null)
            {
                _dupRoomCavityMapType = AccessTools.TypeByName("DuplicantRoomSensor.CavityInfoDuplicants")
                    ?? FindTypeInAssembly("DuplicantRoomSensor", "DuplicantRoomSensor.CavityInfoDuplicants");
                if (_dupRoomCavityMapType != null)
                    _dupRoomCavityMapField = AccessTools.Field(_dupRoomCavityMapType, "map");
            }

            if (_dupRoomCavityMapType == null || _dupRoomCavityMapField == null)
                return null;
            return _dupRoomCavityMapField.GetValue(null) as IDictionary;
        }

        private static bool IsCellPassable(int cell)
        {
            if (!Grid.IsValidCell(cell))
                return false;
            // Use element solidity (like ShowRange) so mesh/airflow tiles are passable for visualization.
            var elem = Grid.Element[cell];
            if (elem != null && elem.IsSolid)
                return false;

            var building = Grid.Objects[cell, (int)ObjectLayer.Building]
                ?? Grid.Objects[cell, (int)ObjectLayer.FoundationTile];
            if (building != null && building.TryGetComponent<Door>(out var door))
            {
                bool isOpen = _doorIsOpenGetter != null ? _doorIsOpenGetter(door) : false;

                if (!isOpen)
                    return false;
            }
            else if (Grid.HasDoor[cell])
            {
                // If a door exists but couldn't be resolved, be conservative and block.
                return false;
            }

            return true;
        }

        private static bool HasLineOfSight(int originCell, int targetCell)
        {
            if (!Grid.IsValidCell(originCell) || !Grid.IsValidCell(targetCell))
                return false;
            if (originCell == targetCell)
                return true;

            Grid.CellToXY(originCell, out int x0, out int y0);
            Grid.CellToXY(targetCell, out int x1, out int y1);

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            int x = x0;
            int y = y0;
            while (!(x == x1 && y == y1))
            {
                int e2 = err * 2;
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }

                int cell = Grid.XYToCell(x, y);
                if (!Grid.IsValidCell(cell))
                    return false;

                // Target cell itself can be visible; blockers only apply to cells along the ray.
                if (cell == targetCell)
                    return true;

                if (!IsCellPassable(cell))
                    return false;
            }

            return true;
        }

        private const float ReachableCellTtlSeconds = 5f;

        private static HashSet<int> GetOrBuildReachableCells(DuplicantRoomSensorRangeSettings settings, CavityInfo cavity, int originCell, int range)
        {
            if (settings == null)
                return new HashSet<int>();

            int minX = cavity != null ? cavity.minX : int.MinValue;
            int maxX = cavity != null ? cavity.maxX : int.MinValue;
            int minY = cavity != null ? cavity.minY : int.MinValue;
            int maxY = cavity != null ? cavity.maxY : int.MinValue;

            bool sameKey = settings.CachedOriginCell == originCell && settings.CachedRange == range
                && settings.CachedMinX == minX && settings.CachedMaxX == maxX
                && settings.CachedMinY == minY && settings.CachedMaxY == maxY;

            // Force rebuild if TTL expired (catches door open/close changes that don't alter cavity bounds)
            bool ttlExpired = Time.unscaledTime - settings.LastReachableRebuildTime > ReachableCellTtlSeconds;

            if (sameKey && !ttlExpired)
                return settings.CachedReachableCells;

            settings.CachedReachableCells.Clear();
            BuildReachableCellsInto(settings.CachedReachableCells, cavity, originCell, range);
            settings.CachedOriginCell = originCell;
            settings.CachedRange = range;
            settings.CachedCavity = cavity;
            settings.CachedMinX = minX;
            settings.CachedMaxX = maxX;
            settings.CachedMinY = minY;
            settings.CachedMaxY = maxY;
            settings.LastReachableRebuildTime = Time.unscaledTime;
            return settings.CachedReachableCells;
        }

        private static void BuildReachableCellsInto(HashSet<int> reachable, CavityInfo cavity, int originCell, int range)
        {
            if (reachable == null)
                return;
            reachable.Clear();
            if (!Grid.IsValidCell(originCell))
                return;

            Grid.CellToXY(originCell, out int originX, out int originY);
            int minX = originX - range;
            int maxX = originX + range;
            int minY = originY - range;
            int maxY = originY + range;

            bool InBounds(int cell)
            {
                Grid.CellToXY(cell, out int x, out int y);
                if (x < minX || x > maxX || y < minY || y > maxY)
                    return false;
                int dx = Mathf.Abs(x - originX);
                int dy = Mathf.Abs(y - originY);
                return dx + dy <= range;
            }

            if (!InBounds(originCell) || !IsCellPassable(originCell))
                return;
            if (Game.Instance.roomProber.GetCavityForCell(originCell) != cavity)
                return;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int dx = Mathf.Abs(x - originX);
                    int dy = Mathf.Abs(y - originY);
                    if (dx + dy > range)
                        continue;

                    int candidate = Grid.XYToCell(x, y);
                    if (!Grid.IsValidCell(candidate))
                        continue;
                    if (!IsCellPassable(candidate))
                        continue;
                    if (Game.Instance.roomProber.GetCavityForCell(candidate) != cavity)
                        continue;
                    if (!HasLineOfSight(originCell, candidate))
                        continue;

                    reachable.Add(candidate);
                }
            }
        }

        private static int CountDupesInRange(IList duplicates, HashSet<int> reachable)
        {
            if (duplicates == null || reachable == null || reachable.Count == 0)
                return 0;
            int count = 0;
            for (int i = 0; i < duplicates.Count; i++)
            {
                if (!(duplicates[i] is KPrefabID dup) || dup == null)
                    continue;

                int dupCell = Grid.PosToCell(dup);
                if (reachable.Contains(dupCell))
                    count++;
            }
            return count;
        }

        private static bool TryResolveShowRangeTypes()
        {
            if (_showRangeTypesResolved)
                return _showRangeSimParamsType != null && _showRangeSimVisualizerType != null;

            _showRangeTypesResolved = true;
            _showRangeSimParamsType = AccessTools.TypeByName("PeterHan.ShowRange.SimVisualizerParams")
                ?? FindTypeInAssembly("ShowRange", "PeterHan.ShowRange.SimVisualizerParams");
            _showRangeSimVisualizerType = AccessTools.TypeByName("PeterHan.ShowRange.SimVisualizer")
                ?? FindTypeInAssembly("ShowRange", "PeterHan.ShowRange.SimVisualizer");
            _showRangeRendererType = AccessTools.TypeByName("PeterHan.ShowRange.SimRangeVisualizer")
                ?? FindTypeInAssembly("ShowRange", "PeterHan.ShowRange.SimRangeVisualizer");

            if (_showRangeSimParamsType != null)
            {
                _showRangeVisualizersField = AccessTools.Field(_showRangeSimParamsType, "visualizers");
                _showRangeWorstCaseRadiusField = AccessTools.Field(_showRangeSimParamsType, "worstCaseRadius");
                _showRangeHighlightColorField = AccessTools.Field(_showRangeSimParamsType, "highlightColor");
            }
            if (_showRangeRendererType != null)
            {
                _showRangeRendererLastCellField = AccessTools.Field(_showRangeRendererType, "lastCell");
                _showRangeRendererLastTransformField = AccessTools.Field(_showRangeRendererType, "lastTransform");
            }

            return _showRangeSimParamsType != null && _showRangeSimVisualizerType != null;
        }

        private static void ForceShowRangeRefresh()
        {
            if (_showRangeRendererType == null)
                return;

            try
            {
                if (_cachedShowRangeRenderers == null)
                    _cachedShowRangeRenderers = Resources.FindObjectsOfTypeAll(_showRangeRendererType);
                if (_cachedShowRangeRenderers == null)
                    return;

                bool anyValid = false;
                for (int i = 0; i < _cachedShowRangeRenderers.Length; i++)
                {
                    var robj = _cachedShowRangeRenderers[i] as UnityEngine.Object;
                    if (robj == null)
                        continue;
                    anyValid = true;
                    _showRangeRendererLastCellField?.SetValue(robj, Grid.InvalidCell);
                    _showRangeRendererLastTransformField?.SetValue(robj, null);
                }
                if (!anyValid)
                    _cachedShowRangeRenderers = null;
            }
            catch
            {
                _cachedShowRangeRenderers = null;
            }
        }

        /// <summary>Ensures SimVisualizerParams.visualizers is never null to prevent NRE in ShowRange's UpdateLocation.
        /// Call when sidescreen opens so the component is safe before ShowRange's OnPostRender runs.</summary>
        private static void EnsureShowRangeVisualizersNotNull(GameObject target)
        {
            if (target == null || !ShowRangeDetection.Loaded || !TryResolveShowRangeTypes())
                return;
            try
            {
                var simParams = target.GetComponent(_showRangeSimParamsType);
                if (simParams != null && _showRangeVisualizersField != null
                    && _showRangeVisualizersField.GetValue(simParams) == null)
                {
                    if (_emptyVisualizerArray == null)
                        _emptyVisualizerArray = Array.CreateInstance(_showRangeSimVisualizerType, 0);
                    _showRangeVisualizersField.SetValue(simParams, _emptyVisualizerArray);
                }
            }
            catch { }
        }

        private static void UpdateShowRangeVisualizer(GameObject target, DuplicantRoomSensorRangeSettings settings,
            CavityInfo cachedCavity = null, int cachedOriginCell = -1, int cachedRange = -1, HashSet<int> cachedReachable = null, bool? forceEnabled = null)
        {
            if (target == null || settings == null)
                return;

            if (!ShowRangeDetection.Loaded)
                return;

            try
            {
                if (!TryResolveShowRangeTypes())
                    return;

                bool shouldEnable = forceEnabled ?? settings.EnableRangeLimit;
                int targetRange = cachedRange >= 0 ? Mathf.Clamp(cachedRange, DuplicantRoomSensorRangeSettings.MinRange, DuplicantRoomSensorRangeSettings.MaxRange) : settings.GetClampedRange();
                int originCell = cachedOriginCell;
                HashSet<int> reachable = cachedReachable;
                int reachableCount = 0;
                int reachableXor = 0;
                long reachableSum = 0L;

                if (shouldEnable)
                {
                    if (originCell == Grid.InvalidCell)
                        originCell = Grid.PosToCell(target);

                    CavityInfo cavity = cachedCavity;
                    if (cavity == null)
                    {
                        Room room = Game.Instance.roomProber.GetRoomOfGameObject(target);
                        cavity = room != null ? room.cavity : null;
                    }

                    if (cavity == null || !Grid.IsValidCell(originCell))
                    {
                        shouldEnable = false;
                    }
                    else
                    {
                        if (reachable == null)
                            reachable = GetOrBuildReachableCells(settings, cavity, originCell, targetRange);
                        if (reachable == null || reachable.Count == 0)
                        {
                            shouldEnable = false;
                        }
                        else
                        {
                            foreach (int cell in reachable)
                            {
                                reachableCount++;
                                reachableXor ^= cell;
                                reachableSum += cell;
                            }
                        }
                    }
                }

                if (settings.LastShowRangeEnabled == shouldEnable
                    && settings.LastShowRangeRange == targetRange
                    && settings.LastShowRangeOriginCell == originCell
                    && settings.LastShowRangeReachableCount == reachableCount
                    && settings.LastShowRangeReachableXor == reachableXor
                    && settings.LastShowRangeReachableSum == reachableSum)
                {
                    var existingParams = target.GetComponent(_showRangeSimParamsType);
                    if (existingParams != null && _showRangeVisualizersField != null
                        && _showRangeVisualizersField.GetValue(existingParams) == null)
                    {
                        if (_emptyVisualizerArray == null)
                            _emptyVisualizerArray = Array.CreateInstance(_showRangeSimVisualizerType, 0);
                        _showRangeVisualizersField.SetValue(existingParams, _emptyVisualizerArray);
                    }
                    return;
                }

                var simParams = target.GetComponent(_showRangeSimParamsType) ?? target.AddComponent(_showRangeSimParamsType);

                Array visualizerArray = Array.CreateInstance(_showRangeSimVisualizerType, shouldEnable ? reachableCount : 0);
                if (shouldEnable)
                {
                    Grid.CellToXY(originCell, out int originX, out int originY);
                    int i = 0;
                    foreach (int cell in reachable)
                    {
                        Grid.CellToXY(cell, out int x, out int y);
                        var offset = new CellOffset(x - originX, y - originY);
                        object simVisualizer = Activator.CreateInstance(_showRangeSimVisualizerType, new object[] { offset, 0 });
                        visualizerArray.SetValue(simVisualizer, i++);
                    }
                }

                _showRangeVisualizersField?.SetValue(simParams, visualizerArray);
                _showRangeWorstCaseRadiusField?.SetValue(simParams, shouldEnable ? targetRange : 0);
                _showRangeHighlightColorField?.SetValue(simParams, ShowRangeDefaultColor);

                settings.LastShowRangeEnabled = shouldEnable;
                settings.LastShowRangeRange = targetRange;
                settings.LastShowRangeOriginCell = originCell;
                settings.LastShowRangeReachableCount = reachableCount;
                settings.LastShowRangeReachableXor = reachableXor;
                settings.LastShowRangeReachableSum = reachableSum;
                ForceShowRangeRefresh();
            }
            catch
            {
            }
        }

        public static class DuplicantRoomSensorConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                if (go == null)
                    return;

                go.AddOrGet<CopyBuildingSettings>();
                var settings = go.AddOrGet<DuplicantRoomSensorRangeSettings>();
                settings.RangeCells = settings.GetClampedRange();
                InvalidateRangeCache(settings);
                InvalidateShowRangeCache(settings);
                UpdateShowRangeVisualizer(go, settings);
            }
        }

        public static class ThresholdSwitchSideScreen_SetTarget_Patch
        {
            public static void Postfix(ThresholdSwitchSideScreen __instance, GameObject new_target)
            {
                try
                {
                    if (__instance == null)
                        return;

                    EnsureRows(__instance);
                    if (!IsDuplicantRoomSensorTarget(new_target, out _))
                    {
                        SyncRowVisibility(__instance, false);
                        return;
                    }

                    var settings = new_target.AddOrGet<DuplicantRoomSensorRangeSettings>();
                    settings.RangeCells = settings.GetClampedRange();

                    SyncRowVisibility(__instance, true);
                    BindRowHandlers(__instance, new_target, settings);
                    SyncRowState(__instance, new_target, settings);
                }
                catch
                {
                }
            }
        }

        public static class ThresholdSwitchSideScreen_UpdateTargetThresholdLabel_Patch
        {
            public static void Postfix(ThresholdSwitchSideScreen __instance)
            {
                try
                {
                    if (__instance == null)
                        return;

                    var target = _tsssTargetField?.GetValue(__instance) as GameObject;
                    if (!IsDuplicantRoomSensorTarget(target, out _))
                    {
                        SyncRowVisibility(__instance, false);
                        return;
                    }

                    EnsureRows(__instance);
                    var settings = target.AddOrGet<DuplicantRoomSensorRangeSettings>();
                    SyncRowVisibility(__instance, true);
                    SyncRowState(__instance, target, settings);
                }
                catch
                {
                }
            }
        }

        public static class LogicDuplicantCountSensor_Sim1000ms_Patch
        {
            private static readonly object[] _setStateTrue = new object[] { true };
            private static readonly object[] _setStateFalse = new object[] { false };
            private static StatusItem _notInAnyRoomStatus;

            public static bool Prefix(object __instance, float dt)
            {
                try
                {
                    if (!(__instance is Component component) || component == null)
                        return true;

                    var settings = component.GetComponent<DuplicantRoomSensorRangeSettings>();
                    if (settings == null || !settings.EnableRangeLimit)
                        return true;

                    Room room = Game.Instance.roomProber.GetRoomOfGameObject(component.gameObject);
                    var selectable = _sensorSelectableField?.GetValue(__instance) as KSelectable;
                    if (selectable == null)
                    {
                        selectable = component.GetComponent<KSelectable>();
                        _sensorSelectableField?.SetValue(__instance, selectable);
                    }

                    if (room != null && room.cavity != null)
                    {
                        int originCell = Grid.PosToCell(component.gameObject);
                        int range = settings.GetClampedRange();
                        HashSet<int> reachable = GetOrBuildReachableCells(settings, room.cavity, originCell, range);
                        if (ControlledModsOptions.Instance.ShowRangeOnSidescreenOpen && selectable != null && selectable.IsSelected)
                            UpdateShowRangeVisualizer(component.gameObject, settings, room.cavity, originCell, range, reachable);

                        int currentCount = 0;
                        IDictionary cavityMap = TryGetCavityMap();
                        if (cavityMap != null && cavityMap.Contains(room.cavity))
                        {
                            var dupes = cavityMap[room.cavity] as IList;
                            currentCount = CountDupesInRange(dupes, reachable);
                        }

                        bool countChanged = currentCount != settings.LastCurrentCount || !settings.LastWasInRoom;
                        settings.LastCurrentCount = currentCount;
                        settings.LastWasInRoom = true;

                        if (countChanged)
                        {
                            _sensorCurrentCountField?.SetValue(__instance, currentCount);

                            int threshold = _sensorCountThresholdField?.GetValue(__instance) is int t ? t : 0;
                            bool activateOnGreaterThan = _sensorActivateOnGreaterThanField?.GetValue(__instance) is bool b && b;
                            bool state = !activateOnGreaterThan ? (currentCount < threshold) : (currentCount > threshold);
                            _sensorSetStateMethod?.Invoke(__instance, state ? _setStateTrue : _setStateFalse);
                        }

                        if (_notInAnyRoomStatus == null) _notInAnyRoomStatus = Db.Get().BuildingStatusItems.NotInAnyRoom;
                        if (selectable != null && selectable.HasStatusItem(_notInAnyRoomStatus))
                        {
                            var guid = _sensorRoomStatusGUIDField?.GetValue(__instance) is Guid g ? g : Guid.Empty;
                            selectable.RemoveStatusItem(guid, false);
                        }
                    }
                    else
                    {
                        if (!settings.LastWasInRoom && settings.LastCurrentCount == 0)
                        {
                            // Already in not-in-room state with count 0, skip redundant work
                        }
                        else
                        {
                            settings.LastCurrentCount = 0;
                            settings.LastWasInRoom = false;

                            if (_notInAnyRoomStatus == null) _notInAnyRoomStatus = Db.Get().BuildingStatusItems.NotInAnyRoom;
                            if (selectable != null && !selectable.HasStatusItem(_notInAnyRoomStatus))
                            {
                                Guid guid = selectable.AddStatusItem(_notInAnyRoomStatus, null);
                                _sensorRoomStatusGUIDField?.SetValue(__instance, guid);
                            }
                            _sensorSetStateMethod?.Invoke(__instance, _setStateFalse);
                        }
                    }

                    return false;
                }
                catch
                {
                    return true;
                }
            }
        }

        public static class KSelectable_Unselect_DupRoom_Patch
        {
            public static void Postfix(KSelectable __instance)
            {
                if (__instance == null) return;
                ClearShowRangeVisualizer(__instance.gameObject);
            }
        }
    }
}
