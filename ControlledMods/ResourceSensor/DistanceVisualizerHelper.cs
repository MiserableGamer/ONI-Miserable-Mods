using System;
using System.Collections;
using HarmonyLib;

namespace ControlledMods.ResourceSensor
{
    /// <summary>
    /// Static helper for the visualization fix: clear range overlay when the original mod's DistanceVisualizer is deselected.
    /// Used by Harmony patches that target ResourceSensor.DistanceVisualizer.
    /// </summary>
    public static class DistanceVisualizerHelper
    {
        /// <summary>
        /// Clears visCells and calls Refresh() on the given visualizer instance (their DistanceVisualizer).
        /// Uses reflection so we don't reference their assembly.
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
                var refreshMethod = AccessTools.Method(t, "Refresh") ?? AccessTools.Method(t, "Refresh", Type.EmptyTypes);
                refreshMethod?.Invoke(theirVisualizer, null);
            }
            catch (Exception ex)
            {
                ControlledModsMod.LogWarning($"ResourceSensor DistanceVisualizer ClearAndRefresh: {ex.Message}");
            }
        }
    }
}
