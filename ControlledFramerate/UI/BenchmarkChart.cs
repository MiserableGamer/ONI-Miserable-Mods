using System.Collections.Generic;
using ControlledFramerate.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ControlledFramerate.UI
{
    internal static class BenchmarkChart
    {
        private static readonly Color ColorPass = new Color(0.4f, 0.9f, 0.4f);
        private static readonly Color ColorMarginal = new Color(0.9f, 0.7f, 0.2f);
        private static readonly Color ColorFail = new Color(0.9f, 0.3f, 0.3f);
        private static readonly Color ColorPending = new Color(0.3f, 0.3f, 0.3f);
        private static readonly Color ColorBarBg = new Color(0.15f, 0.15f, 0.2f);
        private static readonly Color ColorTargetLine = new Color(1f, 1f, 1f, 0.5f);

        private const float LabelX = 10f;
        private const float LabelWidth = 48f;
        private const float BarX = 62f;
        private const float FpsLabelGap = 8f;
        private const float FpsLabelWidth = 55f;
        private const float ResultLabelWidth = 40f;
        private const float RowHeight = 20f;
        private const float RowSpacing = 2f;

        private static GameObject chartRoot;
        private static GameObject targetLineGO;
        private static readonly List<ChartRow> rows = new List<ChartRow>();
        private static float chartMaxFps;
        private static float barWidth;

        private struct ChartRow
        {
            public TextMeshProUGUI SpeedLabel;
            public Image BarBg;
            public Image BarFill;
            public RectTransform BarFillRT;
            public TextMeshProUGUI FpsLabel;
            public TextMeshProUGUI ResultLabel;
        }

        internal static void Init(Transform parent, int maxSteps, float maxFps,
            float desiredFps, float maxTestSpeed, float stepSize,
            float chartY, float panelWidth)
        {
            Clear();
            chartMaxFps = Mathf.Max(maxFps, 1f);

            float fpsX = panelWidth - 20f - ResultLabelWidth - FpsLabelGap - FpsLabelWidth;
            barWidth = fpsX - BarX - FpsLabelGap;
            if (barWidth < 100f) barWidth = 100f;

            float chartHeight = maxSteps * (RowHeight + RowSpacing);

            chartRoot = BenchmarkOverlay.CreateChild(parent, "ChartArea");
            var chartRT = chartRoot.GetComponent<RectTransform>();
            chartRT.anchorMin = new Vector2(0f, 1f);
            chartRT.anchorMax = new Vector2(1f, 1f);
            chartRT.pivot = new Vector2(0.5f, 1f);
            chartRT.anchoredPosition = new Vector2(0f, chartY);
            chartRT.sizeDelta = new Vector2(0f, chartHeight);

            for (int i = 0; i < maxSteps; i++)
            {
                float speed = maxTestSpeed - i * stepSize;
                if (speed < 1f) speed = 1f;
                float rowY = -i * (RowHeight + RowSpacing);
                rows.Add(CreateRow(chartRoot.transform, speed, rowY, panelWidth));
            }

            CreateTargetLine(chartRoot.transform, desiredFps, panelWidth);
        }

        private static ChartRow CreateRow(Transform parent, float speed, float yPos, float panelWidth)
        {
            var rowGO = BenchmarkOverlay.CreateChild(parent, $"Row_{speed:F1}");
            var rowRT = rowGO.GetComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0f, 1f);
            rowRT.anchorMax = new Vector2(1f, 1f);
            rowRT.pivot = new Vector2(0.5f, 1f);
            rowRT.anchoredPosition = new Vector2(0f, yPos);
            rowRT.sizeDelta = new Vector2(0f, RowHeight);

            // Creation order matters for z-ordering: bars first, then text on top
            var barBg = CreateRowImage(rowGO.transform, "BarBg", BarX, barWidth, ColorBarBg);
            var barFill = CreateRowImage(rowGO.transform, "BarFill", BarX, 0f, ColorPending);
            var barFillRT = barFill.rectTransform;

            var speedLabel = CreateRowText(rowGO.transform, "Speed", $"{speed:F1}x",
                LabelX, LabelWidth, 12f, Color.white,
                TextAlignmentOptions.MidlineRight);

            float fpsX = BarX + barWidth + FpsLabelGap;
            var fpsLabel = CreateRowText(rowGO.transform, "FPS", "",
                fpsX, FpsLabelWidth, 11f, new Color(0.8f, 0.8f, 0.8f),
                TextAlignmentOptions.MidlineLeft);

            float resultX = fpsX + FpsLabelWidth + 2f;
            var resultLabel = CreateRowText(rowGO.transform, "Result", "",
                resultX, ResultLabelWidth, 10f, ColorPending,
                TextAlignmentOptions.MidlineLeft, FontStyles.Bold);

            return new ChartRow
            {
                SpeedLabel = speedLabel,
                BarBg = barBg,
                BarFill = barFill,
                BarFillRT = barFillRT,
                FpsLabel = fpsLabel,
                ResultLabel = resultLabel
            };
        }

        internal static void UpdateLiveBar(int stepIndex, float fps, float desiredFps)
        {
            if (stepIndex < 0 || stepIndex >= rows.Count) return;
            var row = rows[stepIndex];

            float ratio = Mathf.Clamp01(fps / chartMaxFps);
            row.BarFillRT.sizeDelta = new Vector2(ratio * barWidth, 0f);
            row.BarFill.color = GetBarColor(fps, desiredFps, desiredFps * 0.67f);
            row.FpsLabel.text = $"{fps:F0} FPS";
            row.ResultLabel.text = "";
        }

        internal static void SetStepResult(int stepIndex, float fps, float desiredFps, float minFps)
        {
            if (stepIndex < 0 || stepIndex >= rows.Count) return;
            var row = rows[stepIndex];

            float ratio = Mathf.Clamp01(fps / chartMaxFps);
            row.BarFillRT.sizeDelta = new Vector2(ratio * barWidth, 0f);

            Color barColor = GetBarColor(fps, desiredFps, minFps);
            row.BarFill.color = barColor;
            row.FpsLabel.text = $"{fps:F0} FPS";

            float passThreshold = desiredFps * (1f - ControlledFramerateOptions.Instance.AcceptableThreshold / 100f);
            if (fps >= passThreshold)
            {
                row.ResultLabel.text = "PASS";
                row.ResultLabel.color = ColorPass;
            }
            else if (fps >= minFps)
            {
                row.ResultLabel.text = "LOW";
                row.ResultLabel.color = ColorMarginal;
            }
            else
            {
                row.ResultLabel.text = "FAIL";
                row.ResultLabel.color = ColorFail;
            }
        }

        internal static void HighlightRow(int stepIndex)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].SpeedLabel != null)
                    rows[i].SpeedLabel.color = (i == stepIndex)
                        ? Color.white
                        : new Color(0.8f, 0.8f, 0.8f);
            }
        }

        internal static void Clear()
        {
            rows.Clear();
            if (targetLineGO != null)
            {
                Object.Destroy(targetLineGO);
                targetLineGO = null;
            }
            if (chartRoot != null)
            {
                Object.Destroy(chartRoot);
                chartRoot = null;
            }
            chartMaxFps = 60f;
            barWidth = 400f;
        }

        private static void CreateTargetLine(Transform parent, float desiredFps, float panelWidth)
        {
            if (chartMaxFps <= 0f) return;
            float ratio = Mathf.Clamp01(desiredFps / chartMaxFps);

            targetLineGO = BenchmarkOverlay.CreateChild(parent, "TargetLine");
            var lineRT = targetLineGO.GetComponent<RectTransform>();

            float xPos = BarX + ratio * barWidth;
            lineRT.anchorMin = new Vector2(0f, 0f);
            lineRT.anchorMax = new Vector2(0f, 1f);
            lineRT.pivot = new Vector2(0.5f, 0.5f);
            lineRT.anchoredPosition = new Vector2(xPos, 0f);
            lineRT.sizeDelta = new Vector2(2f, 0f);

            var lineImg = targetLineGO.AddComponent<Image>();
            lineImg.color = ColorTargetLine;
            lineImg.raycastTarget = false;
        }

        private static Color GetBarColor(float fps, float desiredFps, float minFps)
        {
            float passThreshold = desiredFps * (1f - ControlledFramerateOptions.Instance.AcceptableThreshold / 100f);
            if (fps >= passThreshold) return ColorPass;
            if (fps >= minFps) return ColorMarginal;
            return ColorFail;
        }

        private static TextMeshProUGUI CreateRowText(Transform parent, string name, string text,
            float x, float width, float fontSize, Color color,
            TextAlignmentOptions alignment, FontStyles style = FontStyles.Normal)
        {
            var go = BenchmarkOverlay.CreateChild(parent, name);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.sizeDelta = new Vector2(width, 0f);

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

        private static Image CreateRowImage(Transform parent, string name,
            float x, float width, Color color)
        {
            var go = BenchmarkOverlay.CreateChild(parent, name);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.sizeDelta = new Vector2(width, 0f);

            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }
    }
}
