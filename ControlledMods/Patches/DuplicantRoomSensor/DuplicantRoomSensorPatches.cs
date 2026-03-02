using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using ControlledMods.ModDetection;

namespace ControlledMods.Patches.DuplicantRoomSensor
{
    public static class DuplicantRoomSensorPatches
    {
        private static readonly Color ShowRangeDefaultColor = new Color(0f, 1f, 0.8f, 1f);
        private const float ReachableRebuildIntervalSeconds = 2f;
        private static Type _dupRoomSensorType;
        private static Type _dupRoomCavityMapType;
        private static FieldInfo _dupRoomCavityMapField;
        private static Type _showRangeSimParamsType;
        private static Type _showRangeSimVisualizerType;
        private static Type _showRangeRendererType;
        private static bool _showRangeTypesResolved;

        private static FieldInfo _showRangeVisualizersField;
        private static FieldInfo _showRangeWorstCaseRadiusField;
        private static FieldInfo _showRangeHighlightColorField;
        private static FieldInfo _showRangeRendererLastCellField;
        private static FieldInfo _showRangeRendererLastTransformField;
        private static readonly Dictionary<ThresholdSwitchSideScreen, KToggle> _rangeLimitToggles = new Dictionary<ThresholdSwitchSideScreen, KToggle>();
        private static readonly Dictionary<ThresholdSwitchSideScreen, KNumberInputField> _rangeInputs = new Dictionary<ThresholdSwitchSideScreen, KNumberInputField>();
        private static readonly Dictionary<ThresholdSwitchSideScreen, GameObject> _rangeToggleRows = new Dictionary<ThresholdSwitchSideScreen, GameObject>();
        private static readonly Dictionary<ThresholdSwitchSideScreen, GameObject> _rangeInputRows = new Dictionary<ThresholdSwitchSideScreen, GameObject>();
        private static readonly Dictionary<ThresholdSwitchSideScreen, System.Action> _rangeToggleHandlers = new Dictionary<ThresholdSwitchSideScreen, System.Action>();
        private static readonly Dictionary<ThresholdSwitchSideScreen, System.Action> _rangeInputHandlers = new Dictionary<ThresholdSwitchSideScreen, System.Action>();

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

            ControlledModsMod.Log("DuplicantRoomSensor compatibility patches applied");
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

            var aboveToggle = AccessTools.Field(typeof(ThresholdSwitchSideScreen), "aboveToggle")?.GetValue(screen) as KToggle;
            var numberInput = AccessTools.Field(typeof(ThresholdSwitchSideScreen), "numberInput")?.GetValue(screen) as KNumberInputField;
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
            var thresholdSlider = AccessTools.Field(typeof(ThresholdSwitchSideScreen), "thresholdSlider")?.GetValue(screen) as NonLinearSlider;
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

            _rangeLimitToggles[screen] = toggle;
            _rangeInputs[screen] = input;
            _rangeToggleRows[screen] = toggleRow;
            _rangeInputRows[screen] = rangeInputRow;
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
        }

        private static void SyncRowVisibility(ThresholdSwitchSideScreen screen, bool visible)
        {
            if (screen == null)
                return;

            if (_rangeToggleRows.TryGetValue(screen, out var toggleRow) && toggleRow != null)
                toggleRow.SetActive(visible);
            if (_rangeInputRows.TryGetValue(screen, out var inputRow) && inputRow != null)
                inputRow.SetActive(visible);
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

            UpdateShowRangeVisualizer(target, settings);
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
            // Use game solidity (not just element solidity) so tile buildings like Mesh Tile block traversal.
            if (Grid.IsSolidCell(cell))
                return false;

            var building = Grid.Objects[cell, (int)ObjectLayer.Building]
                ?? Grid.Objects[cell, (int)ObjectLayer.FoundationTile];
            if (building != null && building.TryGetComponent<Door>(out var door))
            {
                bool isOpen = false;
                try
                {
                    var isOpenProp = AccessTools.Property(door.GetType(), "IsOpen") ?? AccessTools.Property(door.GetType(), "isOpen");
                    if (isOpenProp != null && isOpenProp.GetValue(door) is bool open)
                        isOpen = open;
                }
                catch
                {
                }

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
            bool fresh = (Time.unscaledTime - settings.LastReachableRebuildTime) < ReachableRebuildIntervalSeconds;
            if (sameKey && fresh)
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
                var renderers = Resources.FindObjectsOfTypeAll(_showRangeRendererType);
                if (renderers == null)
                    return;

                for (int i = 0; i < renderers.Length; i++)
                {
                    object renderer = renderers[i];
                    _showRangeRendererLastCellField?.SetValue(renderer, Grid.InvalidCell);
                    _showRangeRendererLastTransformField?.SetValue(renderer, null);
                }
            }
            catch
            {
            }
        }

        private static void UpdateShowRangeVisualizer(GameObject target, DuplicantRoomSensorRangeSettings settings,
            CavityInfo cachedCavity = null, int cachedOriginCell = -1, int cachedRange = -1, HashSet<int> cachedReachable = null)
        {
            if (target == null || settings == null)
                return;

            if (!ShowRangeDetection.Loaded)
                return;

            try
            {
                if (!TryResolveShowRangeTypes())
                    return;

                bool shouldEnable = settings.EnableRangeLimit;
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
                    return;

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
                    InvalidateRangeCache(settings);
                    InvalidateShowRangeCache(settings);

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

                    var target = AccessTools.Field(typeof(ThresholdSwitchSideScreen), "target")?.GetValue(__instance) as GameObject;
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
                    var selectable = AccessTools.Field(__instance.GetType(), "selectable")?.GetValue(__instance) as KSelectable;
                    if (selectable == null)
                    {
                        selectable = component.GetComponent<KSelectable>();
                        AccessTools.Field(__instance.GetType(), "selectable")?.SetValue(__instance, selectable);
                    }

                    if (room != null && room.cavity != null)
                    {
                        int originCell = Grid.PosToCell(component.gameObject);
                        int range = settings.GetClampedRange();
                        HashSet<int> reachable = GetOrBuildReachableCells(settings, room.cavity, originCell, range);
                        var selected = component.GetComponent<KSelectable>();
                        if (selected != null && selected.IsSelected)
                            UpdateShowRangeVisualizer(component.gameObject, settings, room.cavity, originCell, range, reachable);

                        int currentCount = 0;
                        IDictionary cavityMap = TryGetCavityMap();
                        if (cavityMap != null && cavityMap.Contains(room.cavity))
                        {
                            var dupes = cavityMap[room.cavity] as IList;
                            currentCount = CountDupesInRange(dupes, reachable);
                        }

                        AccessTools.Field(__instance.GetType(), "currentCount")?.SetValue(__instance, currentCount);

                        int threshold = AccessTools.Field(__instance.GetType(), "countThreshold")?.GetValue(__instance) is int t ? t : 0;
                        bool activateOnGreaterThan = AccessTools.Field(__instance.GetType(), "activateOnGreaterThan")?.GetValue(__instance) is bool b && b;
                        bool state = !activateOnGreaterThan ? (currentCount < threshold) : (currentCount > threshold);
                        AccessTools.Method(__instance.GetType(), "SetState", new[] { typeof(bool) })?.Invoke(__instance, new object[] { state });

                        if (selectable != null && selectable.HasStatusItem(Db.Get().BuildingStatusItems.NotInAnyRoom))
                        {
                            var guid = AccessTools.Field(__instance.GetType(), "roomStatusGUID")?.GetValue(__instance) is Guid g ? g : Guid.Empty;
                            selectable.RemoveStatusItem(guid, false);
                        }
                    }
                    else
                    {
                        if (selectable != null && !selectable.HasStatusItem(Db.Get().BuildingStatusItems.NotInAnyRoom))
                        {
                            Guid guid = selectable.AddStatusItem(Db.Get().BuildingStatusItems.NotInAnyRoom, null);
                            AccessTools.Field(__instance.GetType(), "roomStatusGUID")?.SetValue(__instance, guid);
                        }
                        AccessTools.Method(__instance.GetType(), "SetState", new[] { typeof(bool) })?.Invoke(__instance, new object[] { false });
                    }

                    return false;
                }
                catch
                {
                    return true;
                }
            }
        }
    }
}
