using System;
using System.Collections;
using ControlledFramerate.Options;
using ControlledFramerate.Strings;
using ControlledFramerate.UI;
using UnityEngine;

namespace ControlledFramerate.Core
{
    public static class BenchmarkEngine
    {
        private const float SettleSeconds = 2f;
        private const float MeasureSeconds = 3f;

        public static bool IsRunning => SpeedStateManager.IsBenchmarkRunning;

        private static Coroutine runningCoroutine;

        public static void Start()
        {
            if (IsRunning) return;
            if (Game.Instance == null) return;

            SpeedStateManager.IsBenchmarkRunning = true;
            SpeedStateManager.CurrentMode = SpeedStateManager.SpeedMode.Fixed;
            runningCoroutine = Game.Instance.StartCoroutine(RunBenchmark());
        }

        public static void Cancel()
        {
            if (!IsRunning) return;

            if (runningCoroutine != null && Game.Instance != null)
            {
                Game.Instance.StopCoroutine(runningCoroutine);
                runningCoroutine = null;
            }

            SpeedStateManager.IsBenchmarkRunning = false;
            BenchmarkOverlay.Hide();
            ControlledFramerateMod.Log(ControlledFramerateStrings.BenchmarkCancelled);

            if (SpeedControlScreen.Instance != null)
                SpeedControlScreen.Instance.SetSpeed(SpeedControlScreen.Instance.GetSpeed());
        }

        private static IEnumerator RunBenchmark()
        {
            var opts = ControlledFramerateOptions.Instance;
            float maxTestSpeed = opts.BenchmarkMaxSpeed;
            float stepSize = opts.BenchmarkStepSize;
            float desiredFps = opts.DesiredFps;
            float minFps = opts.MinimumFps;

            int totalSteps = (int)Math.Ceiling((maxTestSpeed - 1f) / stepSize) + 1;
            int currentStep = 0;
            float foundMaxSpeed = 1f;
            float highestFps = 0f;
            float lowestFps = float.MaxValue;

            bool wasPaused = SpeedControlScreen.Instance != null && SpeedControlScreen.Instance.IsPaused;
            if (wasPaused && SpeedControlScreen.Instance != null)
                SpeedControlScreen.Instance.Unpause(false);

            BenchmarkOverlay.ShowRunning(totalSteps, desiredFps, minFps, maxTestSpeed, stepSize);

            for (float testSpeed = maxTestSpeed; testSpeed >= 1f; testSpeed -= stepSize)
            {
                if (testSpeed < 1f) testSpeed = 1f;
                int stepIndex = currentStep;
                currentStep++;

                Time.timeScale = testSpeed;

                // --- Settle phase ---
                BenchmarkOverlay.UpdateStatus(
                    string.Format(ControlledFramerateStrings.BenchmarkTesting, testSpeed),
                    string.Format("Settling... Step {0} of {1} | Target: {2} FPS", currentStep, totalSteps, desiredFps));

                float settleEnd = Time.realtimeSinceStartup + SettleSeconds;
                while (Time.realtimeSinceStartup < settleEnd)
                {
                    FpsMonitor.Update();
                    float fps = FpsMonitor.SmoothedFps;
                    BenchmarkOverlay.UpdateFps(fps);
                    BenchmarkOverlay.UpdateLiveStep(stepIndex, fps, desiredFps);
                    yield return null;
                }

                // --- Measure phase ---
                BenchmarkOverlay.UpdatePhase(
                    string.Format("Measuring... Step {0} of {1} | Target: {2} FPS", currentStep, totalSteps, desiredFps));
                FpsMonitor.Reset();

                float measureEnd = Time.realtimeSinceStartup + MeasureSeconds;
                while (Time.realtimeSinceStartup < measureEnd)
                {
                    FpsMonitor.Update();
                    float fps = FpsMonitor.SmoothedFps;
                    BenchmarkOverlay.UpdateFps(fps);
                    BenchmarkOverlay.UpdateLiveStep(stepIndex, fps, desiredFps);
                    yield return null;
                }

                float avgFps = FpsMonitor.SmoothedFps;
                if (avgFps > highestFps) highestFps = avgFps;
                if (avgFps < lowestFps) lowestFps = avgFps;

                BenchmarkOverlay.AddStepResult(stepIndex, avgFps, desiredFps, minFps);

                // Threshold allows a margin below target (e.g. 10% means 27 FPS passes for a 30 FPS target)
                float passThreshold = desiredFps * (1f - opts.AcceptableThreshold / 100f);
                if (avgFps >= passThreshold)
                {
                    foundMaxSpeed = testSpeed;
                    break;
                }

                if (testSpeed <= 1f)
                {
                    foundMaxSpeed = 1f;
                    break;
                }
            }

            // Proposed speeds -- not applied until user accepts
            float slow = 1f;
            float medium = Math.Max(1f, (float)Math.Floor(foundMaxSpeed * 0.5f * 2f) / 2f);
            float fast = foundMaxSpeed;
            if (medium >= fast) medium = Math.Max(1f, fast - 0.5f);

            if (lowestFps == float.MaxValue) lowestFps = 0f;
            BenchmarkOverlay.ShowResults(foundMaxSpeed, slow, medium, fast, desiredFps, highestFps, lowestFps);

            ControlledFramerateMod.Log(string.Format(
                "Benchmark finished. Max speed found: {0:F1}x. Awaiting user decision.", foundMaxSpeed));

            Time.timeScale = 1f;

            runningCoroutine = null;
        }
    }
}
