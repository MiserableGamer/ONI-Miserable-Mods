using HarmonyLib;
using PeterHan.PLib.Core;
using ControlledVisuals.Options;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using ConduitFlowMesh = ConduitFlowVisualizer.ConduitFlowMesh;

namespace ControlledVisuals.Patches
{
    // Conduit flow throttling (disabled for now). Manual patching to avoid ConduitFlowVisualizer static initializers.
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

        internal static bool ReduceFlowUpdates { get; private set; }

        // Called from OnLoad when conduit throttling is re-enabled. Manual patch to avoid touching type too early.
        internal static void ApplyPatches(Harmony harmony)
        {
            var quality = ControlledVisualsOptions.Instance.ConduitAnimation;
            ReduceFlowUpdates = quality != ControlledVisualsOptions.ConduitAnimationQuality.Full;

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

            // GetTypeSafe so we don't trigger ConduitFlowVisualizer static initializers too early
            var targetType = PPatchTools.GetTypeSafe(nameof(ConduitFlowVisualizer));
            if (targetType == null)
            {
                Debug.LogWarning("[ControlledVisuals] Could not find ConduitFlowVisualizer type");
                ReduceFlowUpdates = false;
                return;
            }

            try
            {
                var renderMethod = targetType.GetMethod(nameof(ConduitFlowVisualizer.Render),
                    BindingFlags.Public | BindingFlags.Instance);

                if (renderMethod == null)
                {
                    Debug.LogWarning("[ControlledVisuals] Could not find Render method on ConduitFlowVisualizer");
                    ReduceFlowUpdates = false;
                    return;
                }

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

        private static bool DetectFastTrack()
        {
            try
            {
                var fastTrackType = PPatchTools.GetTypeSafe("PeterHan.FastTrack.ConduitPatches.ConduitFlowVisualizerPatches");
                if (fastTrackType != null)
                {
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
                    return true;
                }
            }
            catch
            {
                // FastTrack not found or error accessing it
            }
            return false;
        }

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
        }

        internal static void Cleanup()
        {
            NEXT_UPDATE.Clear();
        }

        internal static void ForceUpdate(ConduitFlowVisualizer instance = null)
        {
            if (instance == null)
                NEXT_UPDATE.Clear();
            else if (NEXT_UPDATE.Count > 0)
                NEXT_UPDATE.Remove(instance);
        }

        private static void DrawMesh(ConduitFlowMesh flowMesh, float z, int layer)
        {
            if (flowMesh?.mesh != null)
            {
                Graphics.DrawMesh(flowMesh.mesh, new Vector3(0.5f, 0.5f, z - 0.1f),
                    Quaternion.identity, flowMesh.material, layer);
            }
        }

        [HarmonyPriority(Priority.Low)]
        private static bool Render_Prefix(ConduitFlowVisualizer __instance, float z)
        {
            // Not throttling: run original
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
                    var area = cc.VisibleArea.CurrentArea;
                    var max = area.Max;
                    var min = area.Min;
                    if (max.x - min.x > MAX_ZOOM || max.y - min.y > MAX_ZOOM)
                        calcUpdateRate = UPDATE_RATE_ZOOMED;

                    if (NEXT_UPDATE.TryGetValue(__instance, out double nextConduitUpdate))
                        update = now > nextConduitUpdate;

                    if (update)
                        NEXT_UPDATE[__instance] = now + calcUpdateRate;
                }

                update |= __instance.showContents;

                if (!update)
                {
                    __instance.animTime += Time.deltaTime;
                    int layer = __instance.layer;
                    DrawMesh(__instance.movingBallMesh, z, layer);
                    DrawMesh(__instance.staticBallMesh, z, layer);
                }

                return update;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ControlledVisuals] Conduit throttling failed: {ex.Message}");
                ReduceFlowUpdates = false;
                return true;
            }
        }

        [HarmonyPatch(typeof(Game), nameof(Game.DestroyInstances))]
        public static class Game_DestroyInstances_Patch
        {
            public static void Postfix()
            {
                Cleanup();
            }
        }

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
