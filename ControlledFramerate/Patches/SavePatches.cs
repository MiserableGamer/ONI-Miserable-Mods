using System;
using ControlledFramerate.Options;
using HarmonyLib;
using UnityEngine;

namespace ControlledFramerate.Patches
{
    public static class SavePatches
    {
        [HarmonyPatch(typeof(SaveLoader), nameof(SaveLoader.Save), new[] { typeof(string), typeof(bool), typeof(bool) })]
        public static class SaveLoader_Save_Prefix
        {
            public static void Prefix()
            {
                try
                {
                    SpeedStateManager.IsSaving = true;
                }
                catch (Exception ex)
                {
                    ControlledFramerateMod.Log($"Error in Save Prefix: {ex}");
                }
            }

            public static void Postfix()
            {
                try
                {
                    SpeedStateManager.IsSaving = false;
                    SpeedStateManager.SaveGraceEndTime = Time.realtimeSinceStartup
                        + ControlledFramerateOptions.Instance.SaveIgnoreWindow;
                }
                catch (Exception ex)
                {
                    SpeedStateManager.IsSaving = false;
                    ControlledFramerateMod.Log($"Error in Save Postfix: {ex}");
                }
            }
        }

        [HarmonyPatch(typeof(Game), "Load")]
        public static class Game_Load_Postfix
        {
            public static void Postfix()
            {
                try
                {
                    ControlledFramerateOptions.Reload();
                    SpeedStateManager.OnSaveLoaded();
                    Core.FpsMonitor.Reset();
                    Core.AdaptiveSpeedController.Reset();
                }
                catch (Exception ex)
                {
                    ControlledFramerateMod.Log($"Error in Game.Load patch: {ex}");
                }
            }
        }

        [HarmonyPatch(typeof(Game), nameof(Game.DestroyInstances))]
        public static class Game_DestroyInstances_Prefix
        {
            public static void Prefix()
            {
                try
                {
                    SpeedStateManager.Reset();
                    Core.FpsMonitor.Reset();
                    Core.AdaptiveSpeedController.Reset();
                    UI.BenchmarkOverlay.Hide();
                    UI.ToolbarPopupMenu.Hide();
                    UI.TopBarButtons.DestroyButtons();
                    UI.AdaptiveStatusPanel.DestroyInstance();
                }
                catch (Exception ex)
                {
                    ControlledFramerateMod.Log($"Error in DestroyInstances patch: {ex}");
                }
            }
        }
    }
}
