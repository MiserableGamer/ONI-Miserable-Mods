using System;
using ControlledFramerate.Core;
using ControlledFramerate.Options;
using HarmonyLib;
using UnityEngine;

namespace ControlledFramerate.Patches
{
    public static class SpeedPatches
    {
        [HarmonyPatch(typeof(SpeedControlScreen), "OnChanged")]
        [HarmonyPriority(Priority.Last)]
        public static class SpeedControlScreen_OnChanged_Postfix
        {
            public static void Postfix(SpeedControlScreen __instance)
            {
                try
                {
                    if (SpeedStateManager.IsBenchmarkRunning) return;

                    if (__instance.IsPaused)
                    {
                        Time.timeScale = 0f;
                        return;
                    }

                    int speed = __instance.GetSpeed();
                    float targetSpeed = SpeedStateManager.GetSpeedForButton(speed);

                    if (SpeedStateManager.CurrentMode == SpeedStateManager.SpeedMode.Adaptive)
                        AdaptiveSpeedController.SetSpeed(targetSpeed);

                    Time.timeScale = targetSpeed;

                    UI.TopBarButtons.UpdateSpeedTooltips();
                }
                catch (Exception ex)
                {
                    ControlledFramerateMod.Log($"Error in OnChanged patch: {ex}");
                }
            }
        }

        [HarmonyPatch(typeof(Game), "LateUpdate")]
        [HarmonyPriority(Priority.Last)]
        public static class Game_LateUpdate_Postfix
        {
            public static void Postfix()
            {
                try
                {
                    FpsMonitor.Update();

                    if (SpeedStateManager.CurrentMode == SpeedStateManager.SpeedMode.Adaptive
                        && !SpeedStateManager.IsBenchmarkRunning
                        && SpeedControlScreen.Instance != null
                        && !SpeedControlScreen.Instance.IsPaused)
                    {
                        int selectedSpeed = SpeedControlScreen.Instance.GetSpeed();
                        AdaptiveSpeedController.Update(selectedSpeed);
                    }
                }
                catch (Exception ex)
                {
                    ControlledFramerateMod.LogOnce("LateUpdate", $"Error in LateUpdate patch: {ex}");
                }
            }
        }

        [HarmonyPatch(typeof(SpeedControlScreen), nameof(SpeedControlScreen.ResetToolTip))]
        public static class SpeedControlScreen_ResetToolTip_Postfix
        {
            public static void Postfix()
            {
                try
                {
                    UI.TopBarButtons.UpdateSpeedTooltips();
                }
                catch (Exception ex)
                {
                    ControlledFramerateMod.LogOnce("ResetToolTip", $"Error in ResetToolTip patch: {ex}");
                }
            }
        }

        [HarmonyPatch(typeof(SpeedControlScreen), "OnKeyDown")]
        public static class SpeedControlScreen_OnKeyDown_Prefix
        {
            public static bool Prefix(KButtonEvent e)
            {
                try
                {
                    if (SpeedStateManager.IsBenchmarkRunning && e.TryConsume(global::Action.Escape))
                    {
                        BenchmarkEngine.Cancel();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    ControlledFramerateMod.Log($"Error in OnKeyDown patch: {ex}");
                }
                return true;
            }
        }
    }
}
