using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ControlledMods.Patches.DeliveryTemperatureLimit
{
    /// <summary>
    /// Patches DTL's TemperatureLimitSideScreen to use our replacement widget instead of the
    /// broken TemperatureLimitWidget (which crashes due to old bundled PLib referencing TMPro.FaceInfo).
    ///
    /// We patch OnPrefabInit and SetTarget on the DTL sidescreen class dynamically (via reflection),
    /// replacing the widget creation with our DeliveryTemperatureLimitWidget that uses current PLib.
    /// </summary>
    public static class DeliveryTemperatureLimitPatches
    {
        private static Type _sideScreenType;
        private static Type _widgetType;

        public static void ApplyPatches(Harmony harmony)
        {
            _sideScreenType = AccessTools.TypeByName("DeliveryTemperatureLimit.TemperatureLimitSideScreen");
            _widgetType = AccessTools.TypeByName("DeliveryTemperatureLimit.TemperatureLimitWidget");

            if (_sideScreenType == null)
            {
                ControlledModsMod.LogWarning("DeliveryTemperatureLimit: Could not find TemperatureLimitSideScreen type — skipping patches");
                return;
            }

            try
            {
                // Patch OnPrefabInit — skip adding the broken TemperatureLimitWidget, add ours instead
                var onPrefabInit = AccessTools.Method(_sideScreenType, "OnPrefabInit");
                if (onPrefabInit != null)
                {
                    harmony.Patch(onPrefabInit,
                        prefix: new HarmonyMethod(typeof(DeliveryTemperatureLimitPatches), nameof(OnPrefabInit_Prefix)));
                    ControlledModsMod.Log("DeliveryTemperatureLimit: Patched TemperatureLimitSideScreen.OnPrefabInit");
                }

                // Patch SetTarget — route to our widget
                var setTarget = AccessTools.Method(_sideScreenType, "SetTarget", new[] { typeof(GameObject) });
                if (setTarget != null)
                {
                    harmony.Patch(setTarget,
                        prefix: new HarmonyMethod(typeof(DeliveryTemperatureLimitPatches), nameof(SetTarget_Prefix)));
                    ControlledModsMod.Log("DeliveryTemperatureLimit: Patched TemperatureLimitSideScreen.SetTarget");
                }

                ControlledModsMod.Log("DeliveryTemperatureLimit: All patches applied successfully");
            }
            catch (Exception ex)
            {
                ControlledModsMod.LogWarning($"DeliveryTemperatureLimit: Failed to apply patches — {ex.Message}");
            }
        }

        /// <summary>
        /// Replaces the original OnPrefabInit which would add the broken TemperatureLimitWidget.
        /// Instead, we add our DeliveryTemperatureLimitWidget and set up the ContentContainer.
        /// </summary>
        public static bool OnPrefabInit_Prefix(SideScreenContent __instance)
        {
            try
            {
                // Remove the broken widget component if it somehow got added
                if (_widgetType != null)
                {
                    var broken = __instance.gameObject.GetComponent(_widgetType);
                    if (broken != null)
                        UnityEngine.Object.Destroy(broken);
                }

                // Add our working replacement widget
                __instance.gameObject.AddOrGet<DeliveryTemperatureLimitWidget>();

                // Set ContentContainer (same as original)
                __instance.ContentContainer = __instance.gameObject;
            }
            catch (Exception ex)
            {
                ControlledModsMod.LogWarning($"DeliveryTemperatureLimit: OnPrefabInit_Prefix error — {ex.Message}");
            }

            return false; // skip original (which would add the broken widget)
        }

        /// <summary>
        /// Replaces the original SetTarget which calls the broken widget's SetTarget.
        /// Instead, we get the TemperatureLimit component via reflection and pass it to our widget.
        /// </summary>
        public static bool SetTarget_Prefix(SideScreenContent __instance, GameObject new_target)
        {
            try
            {
                if (new_target == null)
                {
                    Debug.LogError("[ControlledMods] DeliveryTemperatureLimit: Invalid gameObject received");
                    return false;
                }

                var tempLimit = DeliveryTemperatureLimitWidget.GetTemperatureLimit(new_target);
                if (tempLimit == null)
                {
                    Debug.LogError("[ControlledMods] DeliveryTemperatureLimit: Target does not have a TemperatureLimit component");
                    return false;
                }

                var widget = __instance.gameObject.AddOrGet<DeliveryTemperatureLimitWidget>();
                widget.SetTarget(tempLimit);
            }
            catch (Exception ex)
            {
                ControlledModsMod.LogWarning($"DeliveryTemperatureLimit: SetTarget_Prefix error — {ex.Message}");
            }

            return false; // skip original
        }
    }
}
