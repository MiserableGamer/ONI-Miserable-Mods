using System;
using System.Collections;
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
        /// <summary>
        /// Same as when switching to Room mode: get the sensor's visualizer and do visCells.Clear() + Refresh().
        /// This is the exact logic that makes the visualizer disappear when you toggle mode.
        /// </summary>
        public static void ClearVisualizerLikeRoomMode(GameObject building)
        {
            if (building == null) return;
            try
            {
                object sensor = null;
                Type sensorType = AccessTools.TypeByName("ResourceSensor.LogicResourceSensor")
                    ?? AccessTools.TypeByName("ResourceSensorFIXED.LogicResourceSensor");
                if (sensorType != null)
                    sensor = building.GetComponent(sensorType);
                if (sensor == null) return;

                var visualizerField = AccessTools.Field(sensor.GetType(), "visualizer");
                object visualizer = visualizerField?.GetValue(sensor);
                if (visualizer == null) return;

                var visCellsField = AccessTools.Field(visualizer.GetType(), "visCells") ?? AccessTools.Field(visualizer.GetType(), "m_visCells");
                var list = visCellsField?.GetValue(visualizer) as IList;
                list?.Clear();
                var refreshMethod = AccessTools.Method(visualizer.GetType(), "Refresh");
                if (refreshMethod == null)
                    refreshMethod = AccessTools.Method(typeof(ColoredRangeVisualizer), "CreateVisualizers");
                refreshMethod?.Invoke(visualizer, null);
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
