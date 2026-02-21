using System;
using System.Reflection;
using ControlledFramerate.Core;
using ControlledFramerate.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ControlledFramerate.UI
{
    // Consumes all key events except ESC (which cancels the benchmark)
    internal class BenchmarkRunningScreen : KScreen
    {
        public override float GetSortKey() => 50f;

        public override void OnKeyDown(KButtonEvent e)
        {
            if (e.TryConsume(global::Action.Escape))
            {
                if (SpeedStateManager.IsBenchmarkRunning)
                    BenchmarkEngine.Cancel();
                return;
            }
            e.Consumed = true;
        }

        public override void OnKeyUp(KButtonEvent e)
        {
            e.Consumed = true;
        }
    }

    public static class BenchmarkOverlay
    {
        private static InfoDialogScreen configDialog;
        private static int cfgDesiredFps;
        private static float cfgMaxSpeed;
        private static float cfgStepSize;
        private static int cfgThreshold;
        private static TextMeshProUGUI cfgDesiredFpsText;
        private static TextMeshProUGUI cfgMaxSpeedText;
        private static TextMeshProUGUI cfgStepSizeText;
        private static TextMeshProUGUI cfgThresholdText;

        private static GameObject runningOverlay;
        private static BenchmarkRunningScreen runningScreen;
        private static TextMeshProUGUI titleText;
        private static TextMeshProUGUI phaseText;
        private static TextMeshProUGUI fpsText;

        private static InfoDialogScreen resultsDialog;
        private static float pendingMaxSpeed;
        private static bool hasPendingResults;

        private static FieldInfo contentContainerField;
        private static FieldInfo escapeClosesField;

        // =====================================================================
        // CONFIG SCREEN
        // =====================================================================

        public static void ShowConfig()
        {
            ControlledFramerateMod.Log("[BenchmarkOverlay] ShowConfig called");
            Hide();

            var opts = ControlledFramerateOptions.Instance;
            cfgDesiredFps = opts.DesiredFps;
            cfgMaxSpeed = opts.BenchmarkMaxSpeed;
            cfgStepSize = opts.BenchmarkStepSize;
            cfgThreshold = opts.AcceptableThreshold;

            GameObject parent = null;
            if (GameScreenManager.Instance != null)
                parent = GameScreenManager.Instance.ssOverlayCanvas;
            if (parent == null) return;

            configDialog = Util.KInstantiateUI<InfoDialogScreen>(
                ScreenPrefabs.Instance.InfoDialogScreen.gameObject, parent, false);

            configDialog.SetHeader("FPS Benchmark Setup");

            var container = GetContentContainer(configDialog);
            if (container != null)
            {
                cfgDesiredFpsText = CreateStepperRow(container.transform, "Target FPS",
                    cfgDesiredFps.ToString(),
                    () => { cfgDesiredFps = Math.Max(15, cfgDesiredFps - 1); cfgDesiredFpsText.text = cfgDesiredFps.ToString(); },
                    () => { cfgDesiredFps = Math.Min(120, cfgDesiredFps + 1); cfgDesiredFpsText.text = cfgDesiredFps.ToString(); });

                cfgMaxSpeedText = CreateStepperRow(container.transform, "Max Speed",
                    cfgMaxSpeed.ToString("F1"),
                    () => { cfgMaxSpeed = Mathf.Max(2f, cfgMaxSpeed - 1f); cfgMaxSpeedText.text = cfgMaxSpeed.ToString("F1"); },
                    () => { cfgMaxSpeed = Mathf.Min(30f, cfgMaxSpeed + 1f); cfgMaxSpeedText.text = cfgMaxSpeed.ToString("F1"); });

                cfgStepSizeText = CreateStepperRow(container.transform, "Step Size",
                    cfgStepSize.ToString("F1"),
                    () => { cfgStepSize = Mathf.Max(0.5f, cfgStepSize - 0.5f); cfgStepSizeText.text = cfgStepSize.ToString("F1"); },
                    () => { cfgStepSize = Mathf.Min(5f, cfgStepSize + 0.5f); cfgStepSizeText.text = cfgStepSize.ToString("F1"); });

                cfgThresholdText = CreateStepperRow(container.transform, "Threshold %",
                    cfgThreshold.ToString(),
                    () => { cfgThreshold = Math.Max(0, cfgThreshold - 1); cfgThresholdText.text = cfgThreshold.ToString(); },
                    () => { cfgThreshold = Math.Min(50, cfgThreshold + 1); cfgThresholdText.text = cfgThreshold.ToString(); });
            }

            configDialog.AddOption("Cancel", d =>
            {
                d.Deactivate();
                configDialog = null;
            }, false);
            configDialog.AddOption("START BENCHMARK", d => { OnStartClicked(d); }, true);

            SetEscapeCloses(configDialog, true);

            configDialog.Activate();
        }

        private static void OnStartClicked(InfoDialogScreen dialog)
        {
            try
            {
                ControlledFramerateMod.Log("[OnStartClicked] Reading stepper values...");

                var opts = ControlledFramerateOptions.Instance;
                opts.DesiredFps = cfgDesiredFps;
                opts.BenchmarkMaxSpeed = cfgMaxSpeed;
                opts.BenchmarkStepSize = cfgStepSize;
                opts.AcceptableThreshold = cfgThreshold;

                ControlledFramerateMod.Log(
                    $"  DesiredFps={cfgDesiredFps}, MaxSpeed={cfgMaxSpeed:F1}, Step={cfgStepSize:F1}, Threshold={cfgThreshold}");

                ControlledFramerateOptions.Save();
                ControlledFramerateMod.Log("[OnStartClicked] Options saved, starting benchmark...");

                dialog.Deactivate();
                configDialog = null;

                BenchmarkEngine.Start();
                ControlledFramerateMod.Log("[OnStartClicked] BenchmarkEngine.Start() called");
            }
            catch (Exception ex)
            {
                ControlledFramerateMod.Log($"[OnStartClicked] ERROR: {ex}");
            }
        }

        // =====================================================================
        // RUNNING SCREEN (custom overlay with absolute-positioned chart)
        // =====================================================================

        public static void ShowRunning(int totalSteps, float desiredFps, float minFps, float maxTestSpeed, float stepSize)
        {
            HideConfig();
            HideResults();

            GameObject parentCanvas = null;
            if (GameScreenManager.Instance != null)
                parentCanvas = GameScreenManager.Instance.ssOverlayCanvas;
            if (parentCanvas == null) return;

            runningOverlay = new GameObject("ControlledFramerate_BenchmarkRunning");
            runningOverlay.transform.SetParent(parentCanvas.transform, false);

            runningScreen = runningOverlay.AddComponent<BenchmarkRunningScreen>();
            runningScreen.activateOnSpawn = true;
            runningScreen.Activate();

            var blockerRT = runningOverlay.AddComponent<RectTransform>();
            blockerRT.anchorMin = Vector2.zero;
            blockerRT.anchorMax = Vector2.one;
            blockerRT.offsetMin = Vector2.zero;
            blockerRT.offsetMax = Vector2.zero;
            var blockerImg = runningOverlay.AddComponent<Image>();
            blockerImg.color = new Color(0f, 0f, 0f, 0.3f);

            float chartHeight = totalSteps * 22f;
            float panelHeight = 120f + chartHeight + 80f;
            if (panelHeight < 300f) panelHeight = 300f;
            if (panelHeight > 800f) panelHeight = 800f;
            float panelWidth = 660f;

            var panel = CreateChild(runningOverlay.transform, "Panel");
            var panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(panelWidth, panelHeight);
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            panelImg.raycastTarget = true;

            // Chart created first so it renders behind text overlays
            float chartY = -90f;
            float chartMaxFps = Mathf.Max(desiredFps * 1.5f, desiredFps + 20f);
            BenchmarkChart.Init(panel.transform, totalSteps, chartMaxFps, desiredFps,
                maxTestSpeed, stepSize, chartY, panelWidth);

            titleText = CreateAbsoluteText(panel.transform, "Title", "",
                0f, -16f, panelWidth - 40f, 28f, TextAlignmentOptions.Center, 16f,
                FontStyles.Bold, Color.white, 0.5f, 1f);

            phaseText = CreateAbsoluteText(panel.transform, "Phase", "",
                0f, -48f, panelWidth - 40f, 24f, TextAlignmentOptions.Center, 12f,
                FontStyles.Normal, new Color(0.8f, 0.8f, 0.8f), 0.5f, 1f);

            float fpsY = chartY - chartHeight - 12f;
            fpsText = CreateAbsoluteText(panel.transform, "FPS", "",
                0f, fpsY, panelWidth - 40f, 30f, TextAlignmentOptions.Center, 18f,
                FontStyles.Bold, new Color(0.4f, 0.9f, 0.4f), 0.5f, 1f);

            float cancelY = fpsY - 32f;
            CreateAbsoluteText(panel.transform, "CancelHint", "Press ESC to cancel",
                0f, cancelY, panelWidth - 40f, 18f, TextAlignmentOptions.Center, 11f,
                FontStyles.Italic, new Color(0.5f, 0.5f, 0.5f), 0.5f, 1f);
        }

        // =====================================================================
        // RESULTS SCREEN
        // =====================================================================

        public static void ShowResults(float maxSpeed, float slowSpeed, float mediumSpeed, float fastSpeed,
            float targetFps = 0f, float highestFps = 0f, float lowestFps = 0f)
        {
            pendingMaxSpeed = maxSpeed;
            hasPendingResults = true;

            HideRunning();

            GameObject parent = null;
            if (GameScreenManager.Instance != null)
                parent = GameScreenManager.Instance.ssOverlayCanvas;
            if (parent == null) return;

            resultsDialog = Util.KInstantiateUI<InfoDialogScreen>(
                ScreenPrefabs.Instance.InfoDialogScreen.gameObject, parent, false);

            resultsDialog.SetHeader("Benchmark Results");

            if (targetFps > 0f)
            {
                resultsDialog.AddPlainText(string.Format(
                    "Target FPS: {0:F0}    Highest: {1:F0}    Lowest: {2:F0}",
                    targetFps, highestFps, lowestFps));
                resultsDialog.AddSpacer(4f);
            }

            if (maxSpeed > 1f)
            {
                resultsDialog.AddPlainText(string.Format(
                    "Max sustainable speed: <b>{0:F1}x</b>", maxSpeed));
                resultsDialog.AddSpacer(8f);
                resultsDialog.AddPlainText(string.Format(
                    "Proposed speeds:\n  Slow: {0:F1}x\n  Medium: {1:F1}x\n  Fast: {2:F1}x",
                    slowSpeed, mediumSpeed, fastSpeed));
            }
            else
            {
                resultsDialog.AddPlainText(
                    "No speed met the target FPS.\nSpeeds will remain at defaults.");
                resultsDialog.AddSpacer(8f);
                resultsDialog.AddPlainText(string.Format(
                    "Default speeds:\n  Slow: {0:F1}x\n  Medium: {1:F1}x\n  Fast: {2:F1}x",
                    slowSpeed, mediumSpeed, fastSpeed));
            }

            resultsDialog.AddOption("Discard", d => { OnDiscardResults(d); }, false);
            resultsDialog.AddOption("ACCEPT RESULTS", d => { OnAcceptResults(d); }, true);

            resultsDialog.Activate();
        }

        private static void OnAcceptResults(InfoDialogScreen dialog)
        {
            ControlledFramerateMod.Log("[OnAcceptResults] Called. hasPendingResults=" + hasPendingResults);
            if (hasPendingResults)
            {
                var opts = ControlledFramerateOptions.Instance;
                opts.ApplyBenchmarkResults(pendingMaxSpeed);
                SpeedStateManager.HasBenchmarkData = true;
                ControlledFramerateMod.Log(string.Format(
                    "Benchmark results accepted. Max speed: {0:F1}x, Speeds: {1:F1}/{2:F1}/{3:F1}",
                    pendingMaxSpeed, opts.SlowSpeed, opts.MediumSpeed, opts.FastSpeed));
                hasPendingResults = false;
            }
            dialog.Deactivate();
            resultsDialog = null;
            FinishBenchmark();
        }

        private static void OnDiscardResults(InfoDialogScreen dialog)
        {
            ControlledFramerateMod.Log("[OnDiscardResults] Called. hasPendingResults=" + hasPendingResults);
            hasPendingResults = false;
            dialog.Deactivate();
            resultsDialog = null;
            FinishBenchmark();
        }

        private static void FinishBenchmark()
        {
            SpeedStateManager.IsBenchmarkRunning = false;

            if (SpeedControlScreen.Instance != null)
                SpeedControlScreen.Instance.SetSpeed(0);

            TopBarButtons.RefreshButtonStates();
            TopBarButtons.UpdateSpeedTooltips();
        }

        // =====================================================================
        // HIDE / CLEANUP
        // =====================================================================

        public static void Hide()
        {
            ControlledFramerateMod.Log("[BenchmarkOverlay] Hide() called");
            HideConfig();
            HideRunning();
            HideResults();
        }

        private static void HideConfig()
        {
            if (configDialog != null)
            {
                configDialog.Deactivate();
                configDialog = null;
            }
            cfgDesiredFpsText = null;
            cfgMaxSpeedText = null;
            cfgStepSizeText = null;
            cfgThresholdText = null;
        }

        private static void HideRunning()
        {
            BenchmarkChart.Clear();
            if (runningScreen != null)
            {
                runningScreen.isEditing = false;
                runningScreen.Deactivate();
            }
            if (runningOverlay != null)
            {
                UnityEngine.Object.Destroy(runningOverlay);
                runningOverlay = null;
            }
            runningScreen = null;
            titleText = null;
            phaseText = null;
            fpsText = null;
        }

        private static void HideResults()
        {
            if (resultsDialog != null)
            {
                resultsDialog.Deactivate();
                resultsDialog = null;
            }
            hasPendingResults = false;
        }

        // =====================================================================
        // UPDATE METHODS (called by BenchmarkEngine during running)
        // =====================================================================

        public static void UpdateStatus(string title, string phase)
        {
            if (titleText != null) titleText.text = title;
            if (phaseText != null) phaseText.text = phase;
        }

        public static void UpdatePhase(string phase)
        {
            if (phaseText != null) phaseText.text = phase;
        }

        public static void UpdateFps(float fps)
        {
            if (fpsText != null)
                fpsText.text = string.Format("Current FPS: {0:F0}", fps);
        }

        public static void UpdateLiveStep(int stepIndex, float fps, float desiredFps)
        {
            BenchmarkChart.UpdateLiveBar(stepIndex, fps, desiredFps);
            BenchmarkChart.HighlightRow(stepIndex);
        }

        public static void AddStepResult(int stepIndex, float fps, float desiredFps, float minFps)
        {
            BenchmarkChart.SetStepResult(stepIndex, fps, desiredFps, minFps);
        }

        // =====================================================================
        // UI HELPERS
        // =====================================================================

        private static GameObject GetContentContainer(InfoDialogScreen dialog)
        {
            if (contentContainerField == null)
            {
                contentContainerField = typeof(InfoDialogScreen).GetField("contentContainer",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }
            return contentContainerField?.GetValue(dialog) as GameObject;
        }

        private static void SetEscapeCloses(InfoDialogScreen dialog, bool value)
        {
            if (escapeClosesField == null)
            {
                escapeClosesField = typeof(InfoDialogScreen).GetField("escapeCloses",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }
            escapeClosesField?.SetValue(dialog, value);
        }

        private static TextMeshProUGUI CreateStepperRow(Transform parent, string label,
            string initialValue, System.Action onMinus, System.Action onPlus)
        {
            var rowGO = new GameObject($"Stepper_{label}");
            rowGO.transform.SetParent(parent, false);
            rowGO.AddComponent<RectTransform>();

            var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.padding = new RectOffset(16, 16, 4, 4);

            var rowLE = rowGO.AddComponent<LayoutElement>();
            rowLE.minHeight = 36f;
            rowLE.preferredHeight = 36f;

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(rowGO.transform, false);
            labelGO.AddComponent<RectTransform>();
            var labelTmp = labelGO.AddComponent<TextMeshProUGUI>();
            labelTmp.text = label;
            labelTmp.fontSize = 14;
            labelTmp.color = Color.white;
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
            labelTmp.enableWordWrapping = false;
            var labelLE = labelGO.AddComponent<LayoutElement>();
            labelLE.flexibleWidth = 1f;
            labelLE.minWidth = 100f;

            CreateStepperButton(rowGO.transform, "BtnMinus", "\u2212", onMinus);

            var valueGO = new GameObject("Value");
            valueGO.transform.SetParent(rowGO.transform, false);
            valueGO.AddComponent<RectTransform>();
            var valueTmp = valueGO.AddComponent<TextMeshProUGUI>();
            valueTmp.text = initialValue;
            valueTmp.fontSize = 14;
            valueTmp.fontStyle = FontStyles.Bold;
            valueTmp.color = Color.white;
            valueTmp.alignment = TextAlignmentOptions.Center;
            valueTmp.enableWordWrapping = false;
            var valueLE = valueGO.AddComponent<LayoutElement>();
            valueLE.minWidth = 50f;
            valueLE.preferredWidth = 50f;

            CreateStepperButton(rowGO.transform, "BtnPlus", "+", onPlus);

            rowGO.SetActive(true);
            return valueTmp;
        }

        private static void CreateStepperButton(Transform parent, string name, string symbol,
            System.Action onClick)
        {
            var btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            btnGO.AddComponent<RectTransform>();

            var btnLE = btnGO.AddComponent<LayoutElement>();
            btnLE.minWidth = 27f;
            btnLE.preferredWidth = 27f;
            btnLE.minHeight = 27f;
            btnLE.preferredHeight = 27f;

            var btnBg = btnGO.AddComponent<Image>();
            btnBg.color = new Color(0.25f, 0.28f, 0.35f, 1f);

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnBg;
            var colors = btn.colors;
            colors.normalColor = new Color(0.25f, 0.28f, 0.35f, 1f);
            colors.highlightedColor = new Color(0.35f, 0.4f, 0.5f, 1f);
            colors.pressedColor = new Color(0.5f, 0.55f, 0.65f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(new UnityEngine.Events.UnityAction(onClick));

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
            var textTmp = textGO.AddComponent<TextMeshProUGUI>();
            textTmp.text = symbol;
            textTmp.fontSize = 16f;
            textTmp.fontStyle = FontStyles.Bold;
            textTmp.color = Color.white;
            textTmp.alignment = TextAlignmentOptions.Center;
            textTmp.raycastTarget = false;
        }

        internal static TextMeshProUGUI CreateAbsoluteText(Transform parent, string name,
            string text, float x, float y, float width, float height,
            TextAlignmentOptions alignment, float fontSize,
            FontStyles style, Color color,
            float anchorX = 0.5f, float anchorY = 1f)
        {
            var go = CreateChild(parent, name);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(anchorX, anchorY);
            rt.anchorMax = new Vector2(anchorX, anchorY);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(width, height);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Truncate;
            tmp.raycastTarget = false;
            return tmp;
        }

        internal static GameObject CreateChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }
    }
}
