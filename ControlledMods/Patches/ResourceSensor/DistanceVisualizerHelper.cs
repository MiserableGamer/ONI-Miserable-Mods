using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Buildings;
using UnityEngine;

namespace ControlledMods.ResourceSensor
{
    /// <summary>
    /// Clear range overlay on deselect by invoking the same path as when switching to Room mode (sensor's visualizer).
    /// </summary>
    public static class DistanceVisualizerHelper
    {
        private static Type _cachedSensorType;
        private static FieldInfo _cachedVisualizerField;
        private static FieldInfo _cachedVisCellsField;
        private static MethodInfo _cachedRefreshMethod;
        private static bool _reflectionResolved;

        private static bool ResolveReflection()
        {
            if (_reflectionResolved) return _cachedSensorType != null;
            _reflectionResolved = true;

            _cachedSensorType = AccessTools.TypeByName("ResourceSensor.LogicResourceSensor")
                ?? AccessTools.TypeByName("ResourceSensorFIXED.LogicResourceSensor");
            if (_cachedSensorType == null) return false;

            _cachedVisualizerField = AccessTools.Field(_cachedSensorType, "visualizer");
            return true;
        }

        /// <summary>
        /// Same as when switching to Room mode: get the sensor's visualizer and do visCells.Clear() + Refresh().
        /// This is the exact logic that makes the visualizer disappear when you toggle mode.
        /// </summary>
        public static void ClearVisualizerLikeRoomMode(GameObject building)
        {
            if (building == null) return;
            try
            {
                if (!ResolveReflection()) return;

                object sensor = building.GetComponent(_cachedSensorType);
                if (sensor == null) return;

                object visualizer = _cachedVisualizerField?.GetValue(sensor);
                if (visualizer == null) return;

                if (_cachedVisCellsField == null)
                {
                    var vizType = visualizer.GetType();
                    _cachedVisCellsField = AccessTools.Field(vizType, "visCells")
                        ?? AccessTools.Field(vizType, "m_visCells");
                }
                if (_cachedRefreshMethod == null)
                {
                    _cachedRefreshMethod = AccessTools.Method(visualizer.GetType(), "Refresh")
                        ?? AccessTools.Method(typeof(ColoredRangeVisualizer), "CreateVisualizers");
                }

                var list = _cachedVisCellsField?.GetValue(visualizer) as IList;
                list?.Clear();
                _cachedRefreshMethod?.Invoke(visualizer, null);
            }
            catch (Exception ex)
            {
                ControlledModsMod.LogWarning($"ResourceSensor ClearVisualizerLikeRoomMode: {ex.Message}");
            }
        }

        /// <summary>
        /// Fallback: clear a visualizer by reference (used from SelectObject handler when we don't have the building).
        /// </summary>
        public static void ClearAndRefreshOnDeselect(object theirVisualizer)
        {
            if (theirVisualizer == null) return;
            try
            {
                var t = theirVisualizer.GetType();
                var visCellsField = AccessTools.Field(t, "visCells") ?? AccessTools.Field(t, "m_visCells");
                var list = visCellsField?.GetValue(theirVisualizer) as IList;
                list?.Clear();
                var refreshMethod = AccessTools.Method(t, "Refresh");
                if (refreshMethod == null)
                    refreshMethod = AccessTools.Method(typeof(ColoredRangeVisualizer), "CreateVisualizers");
                refreshMethod?.Invoke(theirVisualizer, null);
            }
            catch (Exception ex)
            {
                ControlledModsMod.LogWarning($"ResourceSensor DistanceVisualizer ClearAndRefresh: {ex.Message}");
            }
        }
    }
}
