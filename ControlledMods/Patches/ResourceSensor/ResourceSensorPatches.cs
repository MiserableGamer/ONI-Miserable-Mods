using System;
using System.Collections.Generic;
using HarmonyLib;
using PeterHan.PLib.Buildings;
using UnityEngine;
using UnityEngine.UI;
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

            // Berkay's ResourceSensorSideScreen patches (Global hidden, Include Storage replaced with our scope toggles)
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

            // Fix CountCell: expand category tags, include liquid/gas cell mass, respect scope
            var sensorType = AccessTools.TypeByName("ResourceSensor.LogicResourceSensor");
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
            var includeToggle = AccessTools.Field(sideScreenInstance.GetType(), "countStorageToggle")?.GetValue(sideScreenInstance) as KToggle;
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

                    var roomToggle = AccessTools.Field(__instance.GetType(), "countRoomToggle")?.GetValue(__instance) as KToggle;

                    // Hide Global row (not supported yet)
                    var globalToggle = AccessTools.Field(__instance.GetType(), "countGlobalToggle")?.GetValue(__instance) as KToggle;
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
                                    AccessTools.Field(__instance.GetType(), "countRoomToggle")?.SetValue(__instance, newRoomToggle);
                                    if (newRoomCheckmark != null)
                                        AccessTools.Field(__instance.GetType(), "roomCheckmark")?.SetValue(__instance, newRoomCheckmark);
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
                        var distanceToggle = AccessTools.Field(__instance.GetType(), "countDistanceToggle")?.GetValue(__instance) as KToggle;
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
                    var sensorType = AccessTools.TypeByName("ResourceSensor.LogicResourceSensor") ?? FindTypeInAssembly("ResourceSensor", "ResourceSensor.LogicResourceSensor");
                    if (sensorType == null) return;
                    var sensor = target.GetComponent(sensorType);
                    if (sensor == null) return;

                    // Hide Global row (not supported yet)
                    var globalToggle = AccessTools.Field(__instance.GetType(), "countGlobalToggle")?.GetValue(__instance) as KToggle;
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
                var sensorType = AccessTools.TypeByName("ResourceSensor.LogicResourceSensor");
                if (sensorType == null) return;
                if (new_target.GetComponent(sensorType) == null) return;

                var numberInput = AccessTools.Field(typeof(ThresholdSwitchSideScreen), "numberInput")?.GetValue(__instance);
                SetNumberInputCharacterLimit(numberInput, 8);
            }
        }

        public static class ThresholdSwitchSideScreen_UpdateTargetThresholdLabel_Patch
        {
            public static void Postfix(ThresholdSwitchSideScreen __instance)
            {
                var target = AccessTools.Field(typeof(ThresholdSwitchSideScreen), "target")?.GetValue(__instance) as GameObject;
                if (target == null) return;
                var sensorType = AccessTools.TypeByName("ResourceSensor.LogicResourceSensor");
                if (sensorType == null) return;
                if (target.GetComponent(sensorType) == null) return;

                var numberInput = AccessTools.Field(typeof(ThresholdSwitchSideScreen), "numberInput")?.GetValue(__instance);
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
                    // Clear range overlay (same path as Room mode switch)
                    DistanceVisualizerHelper.ClearVisualizerLikeRoomMode(__instance.gameObject);
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

                    // Expand category tags to leaf resources (selecting a category means all children)
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

                    // Atmosphere: cell element mass + pickupables on the floor
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

                    // Conduits: gas/liquid/solid pipe contents at this cell
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

                    // Count any building with storage
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

                    var logicPortsField = AccessTools.Field(__instance.GetType(), "logicPorts");
                    var logicPorts = logicPortsField?.GetValue(__instance) as LogicPorts;
                    if (logicPorts == null) return true;
                    int cell = logicPorts.GetPortCell(LogicSwitch.PORT_ID);

                    var visualizerField = AccessTools.Field(__instance.GetType(), "visualizer");
                    var visualizer = visualizerField?.GetValue(__instance);
                    var visCellsField = visualizer != null ? AccessTools.Field(visualizer.GetType(), "visCells") : null;
                    var visCells = visCellsField?.GetValue(visualizer) as System.Collections.IList;

                    var distanceProp = AccessTools.Property(__instance.GetType(), "Distance");
                    int distance = distanceProp != null ? (int)distanceProp.GetValue(__instance) : 0;

                    var includeStorageField = AccessTools.Field(__instance.GetType(), "includeStorage");
                    bool includeStorage = includeStorageField != null && (bool)includeStorageField.GetValue(__instance);

                    var visualiserDirtyField = AccessTools.Field(__instance.GetType(), "visualiserDirty");
                    bool visualiserDirty = visualiserDirtyField != null && (bool)visualiserDirtyField.GetValue(__instance);

                    var selectableField = AccessTools.Field(__instance.GetType(), "selectable");
                    var selectable = selectableField?.GetValue(__instance) as KSelectable;

                    visCells?.Clear();

                    if (distance == 0)
                    {
                        if (visualiserDirty && selectable != null && selectable.IsSelected)
                        {
                            var refreshMethod = visualizer != null ? AccessTools.Method(visualizer.GetType(), "Refresh") : null;
                            refreshMethod?.Invoke(visualizer, null);
                        }

                        // CountCell handles atmosphere/pickupables; also check FoundationTile at this cell
                        var countCellMethod = AccessTools.Method(__instance.GetType(), "CountCell", new[] { typeof(int) });
                        float cellMass = countCellMethod != null ? (float)countCellMethod.Invoke(__instance, new object[] { cell }) : 0f;

                        float tileMass = 0f;
                        if (includeStorage && (scope == null || scope.IncludeStorage))
                            tileMass = CountFoundationTileStorage(__instance, cell);

                        __result = cellMass + tileMass;
                        return false;
                    }

                    HashSet<GameObject> countedBuildings = new HashSet<GameObject>();
                    HashSet<GameObject> countedTiles = new HashSet<GameObject>();

                    Grid.CellToXY(cell, out int cellX, out int cellY);
                    int minX = cellX - distance;
                    int maxX = cellX + distance;
                    int minY = cellY - distance;
                    int maxY = cellY + distance;

                    float totalMass = 0f;
                    var countCellM = AccessTools.Method(__instance.GetType(), "CountCell", new[] { typeof(int) });
                    var countBuildingM = AccessTools.Method(__instance.GetType(), "CountBuilding", new[] { typeof(GameObject) });

                    for (int x = minX; x <= maxX; x++)
                    {
                        for (int y = minY; y <= maxY; y++)
                        {
                            int searchCell = Grid.XYToCell(x, y);

                            if (!Grid.IsSolidCell(searchCell))
                            {
                                visCells?.Add(searchCell);

                                if (countCellM != null)
                                    totalMass += (float)countCellM.Invoke(__instance, new object[] { searchCell });

                                if (includeStorage)
                                {
                                    GameObject obj = Grid.Objects[searchCell, (int)ObjectLayer.Building];
                                    if (obj != null && countedBuildings.Add(obj) && countBuildingM != null)
                                        totalMass += (float)countBuildingM.Invoke(__instance, new object[] { obj });
                                }
                            }

                            // Check FoundationTile layer for tile-based storage (e.g. StorageTile) on ALL cells
                            if (includeStorage && (scope == null || scope.IncludeStorage))
                            {
                                GameObject tileObj = Grid.Objects[searchCell, (int)ObjectLayer.FoundationTile];
                                if (tileObj != null && countedTiles.Add(tileObj) && countBuildingM != null)
                                    totalMass += (float)countBuildingM.Invoke(__instance, new object[] { tileObj });
                            }
                        }
                    }

                    if (visualiserDirty && selectable != null && selectable.IsSelected)
                    {
                        var refreshMethod = visualizer != null ? AccessTools.Method(visualizer.GetType(), "Refresh") : null;
                        refreshMethod?.Invoke(visualizer, null);
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

                    var includeStorageField = AccessTools.Field(__instance.GetType(), "includeStorage");
                    bool includeStorage = includeStorageField != null && (bool)includeStorageField.GetValue(__instance);

                    int minX = room.cavity.minX;
                    int maxX = room.cavity.maxX;
                    int minY = room.cavity.minY;
                    int maxY = room.cavity.maxY;

                    HashSet<GameObject> countedBuildings = new HashSet<GameObject>();
                    HashSet<GameObject> countedTiles = new HashSet<GameObject>();

                    RoomProber roomProber = Game.Instance.roomProber;
                    var countCellM = AccessTools.Method(__instance.GetType(), "CountCell", new[] { typeof(int) });
                    var countBuildingM = AccessTools.Method(__instance.GetType(), "CountBuilding", new[] { typeof(GameObject) });

                    float totalMass = 0f;

                    // Expanded scan: 1 extra cell in each direction to catch boundary tiles
                    for (int x = minX - 1; x <= maxX + 1; x++)
                    {
                        for (int y = minY - 1; y <= maxY + 1; y++)
                        {
                            int cell = Grid.XYToCell(x, y);

                            // Original cavity logic for non-solid cells inside the room
                            if (!Grid.IsSolidCell(cell) && roomProber.GetCavityForCell(cell) == room.cavity)
                            {
                                if (countCellM != null)
                                    totalMass += (float)countCellM.Invoke(__instance, new object[] { cell });

                                if (includeStorage)
                                {
                                    GameObject obj = Grid.Objects[cell, (int)ObjectLayer.Building];
                                    if (obj != null && countedBuildings.Add(obj) && countBuildingM != null)
                                        totalMass += (float)countBuildingM.Invoke(__instance, new object[] { obj });
                                }
                            }

                            // Check FoundationTile layer for tile-based storage on ALL cells in the expanded area
                            if (includeStorage && (scope == null || scope.IncludeStorage))
                            {
                                GameObject tileObj = Grid.Objects[cell, (int)ObjectLayer.FoundationTile];
                                if (tileObj != null && countedTiles.Add(tileObj) && countBuildingM != null)
                                    totalMass += (float)countBuildingM.Invoke(__instance, new object[] { tileObj });
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
            if (tileObj == null) return 0f;

            var countBuildingM = AccessTools.Method(sensorInstance.GetType(), "CountBuilding", new[] { typeof(GameObject) });
            if (countBuildingM == null) return 0f;

            return (float)countBuildingM.Invoke(sensorInstance, new object[] { tileObj });
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
