using HarmonyLib;
using MiserableMods.Shared;
using PeterHan.PLib.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using ConduitFlowMesh = ConduitFlowVisualizer.ConduitFlowMesh;

namespace ControlledVisuals.Patches
{
    /// <summary>
    /// Reduces the frame rate of conduit flow visual updates based on quality settings.
    /// Based on FastTrack's implementation by Peter Han.
    /// 
    /// Uses manual patching to avoid issues with ConduitFlowVisualizer's static initializers.
    /// </summary>
    public static class ConduitFlowVisualizerPatches
    {
        private const int MAX_ZOOM = 128;

        private static readonly IDictionary<ConduitFlowVisualizer, double> NEXT_UPDATE =
            new Dictionary<ConduitFlowVisualizer, double>(8);

        private const double UPDATE_RATE_MINIMAL = 0.5;
        private const double UPDATE_RATE_REDUCED = 0.1;
        private const double UPDATE_RATE_ZOOMED = 1.0;

        private static double updateRate;
        private static bool patchApplied = false;
        private static bool fastTrackDetected = false;

        /// <summary>
        /// Whether conduit flow throttling is active.
        /// </summary>
        internal static bool ReduceFlowUpdates { get; private set; }

        /// <summary>
        /// Applies the conduit flow visualizer patches manually.
        /// Must be called from OnLoad after determining if throttling should be enabled.
        /// 
        /// Manual patching is required because ConduitFlowVisualizer has static initializers
        /// that can cause issues if the type is accessed too early via attribute-based patching.
        /// </summary>
        /// <param name="harmony">The Harmony instance to use for patching.</param>
        internal static void ApplyPatches(Harmony harmony)
        {
            var quality = ControlledVisualsOptions.Instance.ConduitAnimation;
            ReduceFlowUpdates = quality != ControlledVisualsOptions.ConduitAnimationQuality.Full;

            // Check if FastTrack is present and might be handling this already
            fastTrackDetected = DetectFastTrack();
            if (fastTrackDetected)
            {
                Debug.Log("[ControlledVisuals] FastTrack detected - skipping conduit throttling to avoid conflicts");
                ReduceFlowUpdates = false;
                return;
            }

            if (!ReduceFlowUpdates)
            {
                Debug.Log("[ControlledVisuals] Conduit animation quality set to Full - no throttling applied");
                return;
            }

            // Use PPatchTools.GetTypeSafe to avoid triggering static initializers prematurely
            var targetType = PPatchTools.GetTypeSafe(nameof(ConduitFlowVisualizer));
            if (targetType == null)
            {
                Debug.LogWarning("[ControlledVisuals] Could not find ConduitFlowVisualizer type");
                ReduceFlowUpdates = false;
                return;
            }

            try
            {
                // Get the Render method
                var renderMethod = targetType.GetMethod(nameof(ConduitFlowVisualizer.Render),
                    BindingFlags.Public | BindingFlags.Instance);

                if (renderMethod == null)
                {
                    Debug.LogWarning("[ControlledVisuals] Could not find Render method on ConduitFlowVisualizer");
                    ReduceFlowUpdates = false;
                    return;
                }

                // Apply the prefix patch manually
                var prefixMethod = typeof(ConduitFlowVisualizerPatches).GetMethod(nameof(Render_Prefix),
                    BindingFlags.NonPublic | BindingFlags.Static);

                harmony.Patch(renderMethod, prefix: new HarmonyMethod(prefixMethod) { priority = Priority.Low });
                patchApplied = true;

                Debug.Log($"[ControlledVisuals] Conduit throttling patch applied - Quality: {quality}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ControlledVisuals] Failed to apply conduit throttling patch: {ex.Message}");
                ReduceFlowUpdates = false;
            }
        }

        /// <summary>
        /// Detects if FastTrack mod is installed and has conduit throttling enabled.
        /// </summary>
        private static bool DetectFastTrack()
        {
            try
            {
                // Check for FastTrack's ConduitFlowVisualizerPatches class
                var fastTrackType = PPatchTools.GetTypeSafe("PeterHan.FastTrack.ConduitPatches.ConduitFlowVisualizerPatches");
                if (fastTrackType != null)
                {
                    // Check if their throttling is enabled
                    var reduceProperty = fastTrackType.GetProperty("ReduceFlowUpdates",
                        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                    if (reduceProperty != null)
                    {
                        var value = reduceProperty.GetValue(null);
                        if (value is bool isEnabled && isEnabled)
                        {
                            return true;
                        }
                    }
                    // FastTrack is present even if throttling is disabled
                    return true;
                }
            }
            catch
            {
                // FastTrack not found or error accessing it
            }
            return false;
        }

        /// <summary>
        /// Initializes the throttling settings when the game starts.
        /// Called from Game.OnSpawn patch.
        /// </summary>
        internal static void Init()
        {
            NEXT_UPDATE.Clear();

            if (!patchApplied || fastTrackDetected)
                return;

            var quality = ControlledVisualsOptions.Instance.ConduitAnimation;

            switch (quality)
            {
                case ControlledVisualsOptions.ConduitAnimationQuality.Reduced:
                    updateRate = UPDATE_RATE_REDUCED;
                    break;
                case ControlledVisualsOptions.ConduitAnimationQuality.Minimal:
                    updateRate = UPDATE_RATE_MINIMAL;
                    break;
                case ControlledVisualsOptions.ConduitAnimationQuality.Full:
                default:
                    updateRate = 0.0;
                    break;
            }

            DevDebug.Watch("Conduit Quality", quality.ToString());
            DevDebug.Watch("Throttling", ReduceFlowUpdates ? "Active" : "Off");
            DevDebug.Watch("FastTrack", fastTrackDetected ? "Detected" : "Not Found");
        }

        /// <summary>
        /// Cleans up when the game is destroyed.
        /// </summary>
        internal static void Cleanup()
        {
            NEXT_UPDATE.Clear();
        }

        /// <summary>
        /// Forces a conduit update to run next time.
        /// </summary>
        /// <param name="instance">The conduit flow visualizer to invalidate, or null to invalidate all.</param>
        internal static void ForceUpdate(ConduitFlowVisualizer instance = null)
        {
            if (instance == null)
                NEXT_UPDATE.Clear();
            else if (NEXT_UPDATE.Count > 0)
                NEXT_UPDATE.Remove(instance);
        }

        /// <summary>
        /// Draws an existing ConduitFlowMesh without updating it.
        /// </summary>
        private static void DrawMesh(ConduitFlowMesh flowMesh, float z, int layer)
        {
            if (flowMesh?.mesh != null)
            {
                Graphics.DrawMesh(flowMesh.mesh, new Vector3(0.5f, 0.5f, z - 0.1f),
                    Quaternion.identity, flowMesh.material, layer);
            }
        }

        /// <summary>
        /// Prefix patch for ConduitFlowVisualizer.Render.
        /// Returns false to skip the original method when throttling, true to run it.
        /// </summary>
        [HarmonyPriority(Priority.Low)]
        private static bool Render_Prefix(ConduitFlowVisualizer __instance, float z)
        {
            // Safety check - if not configured to throttle, run original
            if (!ReduceFlowUpdates || updateRate <= 0.0)
                return true;

            try
            {
                double now = Time.unscaledTime;
                double calcUpdateRate = updateRate;
                var cc = CameraController.Instance;
                bool update = true;

                if (updateRate > 0.0 && cc != null)
                {
                    // Set updates to 1 Hz if zoomed way out
                    var area = cc.VisibleArea.CurrentArea;
                    var max = area.Max;
                    var min = area.Min;
                    if (max.x - min.x > MAX_ZOOM || max.y - min.y > MAX_ZOOM)
                        calcUpdateRate = UPDATE_RATE_ZOOMED;

                    // Check if enough time has passed since last update
                    if (NEXT_UPDATE.TryGetValue(__instance, out double nextConduitUpdate))
                        update = now > nextConduitUpdate;

                    if (update)
                        NEXT_UPDATE[__instance] = now + calcUpdateRate;
                }

                // Always update if showing contents (in overlay mode)
                update |= __instance.showContents;

                // If not updating, render the last mesh without recalculating
                if (!update)
                {
                    __instance.animTime += Time.deltaTime;
                    int layer = __instance.layer;
                    DrawMesh(__instance.movingBallMesh, z, layer);
                    DrawMesh(__instance.staticBallMesh, z, layer);
                }

#if DEBUG
                if (Time.frameCount % 120 == 0)
                {
                    DevDebug.Watch("Render", update ? "Update" : "Throttled");
                }
#endif

                return update;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ControlledVisuals] Conduit throttling failed: {ex.Message}");
                // Disable throttling on error to prevent repeated failures
                ReduceFlowUpdates = false;
                return true;
            }
        }

        /// <summary>
        /// Patch for Game.DestroyInstances to clean up tracking dictionary.
        /// </summary>
        [HarmonyPatch(typeof(Game), nameof(Game.DestroyInstances))]
        public static class Game_DestroyInstances_Patch
        {
            public static void Postfix()
            {
                Cleanup();
            }
        }

        /// <summary>
        /// Patch for Game.OnSpawn to initialize throttling settings.
        /// </summary>
        [HarmonyPatch(typeof(Game), nameof(Game.OnSpawn))]
        public static class Game_OnSpawn_Patch
        {
            public static void Postfix()
            {
                Init();
            }
        }
    }
}
