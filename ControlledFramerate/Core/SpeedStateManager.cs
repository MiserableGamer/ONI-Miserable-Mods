using ControlledFramerate.Options;
using UnityEngine;

namespace ControlledFramerate
{
    public static class SpeedStateManager
    {
        public enum SpeedMode
        {
            Fixed,
            Adaptive
        }

        public static SpeedMode CurrentMode { get; set; } = SpeedMode.Fixed;
        public static bool HasBenchmarkData { get; internal set; } = false;
        public static bool IsBenchmarkRunning { get; set; } = false;
        public static bool FramerateMonitorVisible { get; set; } = false;

        // Freeze adaptive adjustments while saving
        public static bool IsSaving { get; set; } = false;
        public static float SaveGraceEndTime { get; set; } = 0f;

        public static bool IsInSaveGracePeriod =>
            IsSaving || Time.realtimeSinceStartup < SaveGraceEndTime;

        public static float GetSpeedForButton(int buttonIndex)
        {
            var opts = ControlledFramerateOptions.Instance;
            switch (buttonIndex)
            {
                case 0: return opts.SlowSpeed;
                case 1: return opts.MediumSpeed;
                case 2: return opts.FastSpeed;
                default: return 1f;
            }
        }

        public static void OnSaveLoaded()
        {
            var opts = ControlledFramerateOptions.Instance;
            var profile = opts.GetCurrentSaveProfile();

            if (profile != null && profile.BenchmarkMaxFound > 0f)
            {
                opts.SlowSpeed = profile.SlowSpeed;
                opts.MediumSpeed = profile.MediumSpeed;
                opts.FastSpeed = profile.FastSpeed;
                HasBenchmarkData = true;

                CurrentMode = profile.AdaptiveEnabled ? SpeedMode.Adaptive : SpeedMode.Fixed;
                FramerateMonitorVisible = profile.MonitorEnabled;

                ControlledFramerateMod.Log(string.Format(
                    Strings.ControlledFramerateStrings.SaveProfileLoaded,
                    GetCurrentSaveName(), profile.SlowSpeed, profile.MediumSpeed, profile.FastSpeed));
                if (profile.AdaptiveEnabled)
                    ControlledFramerateMod.Log("Adaptive mode restored from save profile.");
            }
            else
            {
                HasBenchmarkData = false;
                CurrentMode = SpeedMode.Fixed;
                FramerateMonitorVisible = false;

                ControlledFramerateMod.Log(string.Format(
                    Strings.ControlledFramerateStrings.SaveProfileNotFound,
                    GetCurrentSaveName(), opts.SlowSpeed, opts.MediumSpeed, opts.FastSpeed));
            }
        }

        // Uses the save folder name as key (stable across autosaves)
        public static string GetCurrentSaveName()
        {
            try
            {
                if (SaveLoader.Instance == null) return null;
                string path = SaveLoader.GetActiveSaveFilePath();
                if (string.IsNullOrEmpty(path)) return null;

                string dirName = System.IO.Directory.GetParent(path)?.Name;
                if (!string.IsNullOrEmpty(dirName) && dirName.Contains("auto_save"))
                    dirName = System.IO.Directory.GetParent(System.IO.Directory.GetParent(path).FullName)?.Name;
                return dirName;
            }
            catch
            {
                return null;
            }
        }

        public static void ToggleAdaptive()
        {
            if (!HasBenchmarkData) return;

            if (CurrentMode == SpeedMode.Adaptive)
            {
                CurrentMode = SpeedMode.Fixed;
                ControlledFramerateMod.Log(Strings.ControlledFramerateStrings.AdaptiveDisabled);
            }
            else
            {
                CurrentMode = SpeedMode.Adaptive;
                var opts = ControlledFramerateOptions.Instance;
                ControlledFramerateMod.Log(string.Format(
                    Strings.ControlledFramerateStrings.AdaptiveEnabled,
                    opts.DesiredFps, opts.MinimumFps));
            }

            SaveAdaptiveState();
        }

        public static void ToggleMonitor()
        {
            FramerateMonitorVisible = !FramerateMonitorVisible;
            ControlledFramerateMod.Log(FramerateMonitorVisible
                ? "Framerate monitor enabled" : "Framerate monitor disabled");
            SaveMonitorState();
        }

        private static void SaveMonitorState()
        {
            var opts = ControlledFramerateOptions.Instance;
            var profile = opts.GetCurrentSaveProfile();
            if (profile != null)
            {
                profile.MonitorEnabled = FramerateMonitorVisible;
                ControlledFramerateOptions.Save();
            }
        }

        private static void SaveAdaptiveState()
        {
            var opts = ControlledFramerateOptions.Instance;
            var profile = opts.GetCurrentSaveProfile();
            if (profile != null)
            {
                profile.AdaptiveEnabled = (CurrentMode == SpeedMode.Adaptive);
                ControlledFramerateOptions.Save();
            }
        }

        public static void Reset()
        {
            CurrentMode = SpeedMode.Fixed;
            HasBenchmarkData = false;
            IsBenchmarkRunning = false;
            IsSaving = false;
            SaveGraceEndTime = 0f;
            FramerateMonitorVisible = false;
        }
    }
}
