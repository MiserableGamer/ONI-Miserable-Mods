using System.Collections.Generic;
using System.Linq;
using AdvancedWattageSensor.Components;
using AdvancedWattageSensor.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedWattageSensor.UI
{
    // Persistent right-side panel that displays all labeled sensors and their wattage status.
    // Inserted into PinnedResourcesPanel's internal layout container so the game handles positioning.
    public class WattageMonitorPanel : MonoBehaviour
    {
        public static WattageMonitorPanel Instance { get; private set; }

        private GameObject panelGO;
        private GameObject rowContainer;
        private TextMeshProUGUI headerArrow;
        private bool collapsed;

        // Cached visual properties from the game's Resources panel
        private TMP_FontAsset gameFont;
        private Material gameFontMaterial;
        private Sprite panelSprite; // 9-sliced sprite for rounded corners
        private float cachedWidth = 220f; // measured once from headerButton
        private RectTransform panelRect;
        private readonly Vector3[] cornerBuf = new Vector3[4]; // reused to avoid GC

        // Game UI colors sampled from PinnedResourcesPanel: #3e4357
        private static readonly Color PanelColor = new Color(62f / 255f, 67f / 255f, 87f / 255f, 1f);
        private static readonly Color HeaderColor = new Color(52f / 255f, 57f / 255f, 77f / 255f, 1f);

        private readonly Dictionary<AdvancedWattageSensorComponent, GameObject> rows =
            new Dictionary<AdvancedWattageSensorComponent, GameObject>();

        public static void Create()
        {
            if (Instance != null)
                return;

            var pinnedPanel = PinnedResourcesPanel.Instance;
            if (pinnedPanel == null)
                return;

            // Parent to PinnedResourcesPanel's root transform (not the internal
            // QuickLayout container which requires fixed-size elements).
            // We position ourselves relative to the panel's top-right anchor point.
            var rootGO = new GameObject("WattageMonitorPanel");
            rootGO.transform.SetParent(pinnedPanel.transform, false);
            rootGO.transform.SetAsLastSibling();

            Instance = rootGO.AddComponent<WattageMonitorPanel>();
            Instance.CacheGameStyles(pinnedPanel);
            Instance.BuildPanel();
            Instance.InvokeRepeating(nameof(RefreshPanel), 1f, 1f);
        }

        public static void DestroyInstance()
        {
            if (Instance != null)
            {
                Instance.CancelInvoke();
                Destroy(Instance.panelGO);
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }

        private void CacheGameStyles(PinnedResourcesPanel pinnedPanel)
        {
            // Get the font from the header text
            var headerTexts = pinnedPanel.headerButton?.GetComponentsInChildren<LocText>(true);
            if (headerTexts != null && headerTexts.Length > 0)
            {
                gameFont = headerTexts[0].font;
                gameFontMaterial = headerTexts[0].fontMaterial;
            }

            // Get a 9-sliced sprite for rounded corners by searching all Images in the panel
            var images = pinnedPanel.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.sprite != null && img.type == Image.Type.Sliced)
                {
                    panelSprite = img.sprite;
                    break;
                }
            }
            // Fallback: any Image with a non-null sprite
            if (panelSprite == null)
            {
                foreach (var img in images)
                {
                    if (img.sprite != null)
                    {
                        panelSprite = img.sprite;
                        break;
                    }
                }
            }

            // Measure width from the header button (one-time at creation)
            // Minimum 280px to fit three wattage columns (c / a / t)
            var headerRT = pinnedPanel.headerButton?.GetComponent<RectTransform>();
            if (headerRT != null && headerRT.rect.width > 50f)
                cachedWidth = Mathf.Max(headerRT.rect.width + 22f, 290f);
            else
                cachedWidth = 290f;
        }

        private void BuildPanel()
        {
            panelGO = new GameObject("PowerMonitorContainer");
            panelGO.transform.SetParent(transform, false);

            panelRect = panelGO.AddComponent<RectTransform>();
            // Anchor top-right (matching PinnedResourcesPanel's own anchor), grow downward
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(cachedWidth, 0f);

            // Background - use the game's 9-sliced sprite for rounded corners, tinted to #3e4357
            var bg = panelGO.AddComponent<Image>();
            if (panelSprite != null)
            {
                bg.sprite = panelSprite;
                bg.type = Image.Type.Sliced;
            }
            bg.color = PanelColor;

            // Fit content height
            var fitter = panelGO.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Vertical layout
            var layout = panelGO.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 0f;
            layout.padding = new RectOffset(0, 0, 0, 0);

            BuildHeader();

            // Row container (collapsible)
            rowContainer = new GameObject("RowContainer");
            rowContainer.transform.SetParent(panelGO.transform, false);
            rowContainer.AddComponent<RectTransform>();
            var rowLayout = rowContainer.AddComponent<VerticalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.UpperLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = false;
            rowLayout.spacing = 0f;
            rowLayout.padding = new RectOffset(10, 10, 4, 6);

            panelGO.SetActive(false);
        }

        private void BuildHeader()
        {
            var headerGO = new GameObject("Header");
            headerGO.transform.SetParent(panelGO.transform, false);
            headerGO.AddComponent<RectTransform>();

            // Header background - slightly darker, same rounded sprite
            var headerBg = headerGO.AddComponent<Image>();
            if (panelSprite != null)
            {
                headerBg.sprite = panelSprite;
                headerBg.type = Image.Type.Sliced;
            }
            headerBg.color = HeaderColor;

            var headerLE = headerGO.AddComponent<LayoutElement>();
            headerLE.minHeight = 28f;
            headerLE.preferredHeight = 28f;

            var headerLayout = headerGO.AddComponent<HorizontalLayoutGroup>();
            headerLayout.childAlignment = TextAnchor.MiddleLeft;
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = true;
            headerLayout.spacing = 4f;
            headerLayout.padding = new RectOffset(10, 10, 0, 0);

            // Collapse arrow
            var arrowGO = new GameObject("Arrow");
            arrowGO.transform.SetParent(headerGO.transform, false);
            arrowGO.AddComponent<RectTransform>();
            headerArrow = arrowGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(headerArrow);
            headerArrow.text = "\u25BC";
            headerArrow.fontSize = 12;
            headerArrow.color = Color.white;
            headerArrow.alignment = TextAlignmentOptions.MidlineLeft;
            headerArrow.raycastTarget = false;
            var arrowLE = arrowGO.AddComponent<LayoutElement>();
            arrowLE.minWidth = 16f;
            arrowLE.preferredWidth = 16f;

            // Title text
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(headerGO.transform, false);
            titleGO.AddComponent<RectTransform>();
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(titleText);
            titleText.text = "Power Monitor";
            titleText.fontSize = 14;
            titleText.fontStyle = FontStyles.Normal;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.raycastTarget = false;
            var titleLE = titleGO.AddComponent<LayoutElement>();
            titleLE.flexibleWidth = 1f;

            // Column legend (right-aligned)
            var legendGO = new GameObject("Legend");
            legendGO.transform.SetParent(headerGO.transform, false);
            legendGO.AddComponent<RectTransform>();
            var legendText = legendGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(legendText);
            legendText.text = "(current / average / total)";
            legendText.fontSize = 11;
            legendText.fontStyle = FontStyles.Normal;
            legendText.color = new Color(0.7f, 0.7f, 0.7f);
            legendText.alignment = TextAlignmentOptions.MidlineRight;
            legendText.raycastTarget = false;

            var button = headerGO.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(ToggleCollapse);
        }

        private void ApplyFont(TextMeshProUGUI tmp)
        {
            if (gameFont != null) tmp.font = gameFont;
            if (gameFontMaterial != null) tmp.fontMaterial = gameFontMaterial;
        }

        private void ToggleCollapse()
        {
            collapsed = !collapsed;
            rowContainer.SetActive(!collapsed);
            headerArrow.text = collapsed ? "\u25B6" : "\u25BC";
        }

        // Lightweight Y-position sync: read the bottom edge of seeAllButton (or headerButton
        // when collapsed) to place our panel just below the Resources content.
        // Only reads one RectTransform per call, reuses a cached corner buffer -- no GC allocation.
        private void PositionBelowResources()
        {
            if (panelRect == null) return;

            var pinnedPanel = PinnedResourcesPanel.Instance;
            if (pinnedPanel == null) return;

            // Pick the anchor element: seeAllButton when visible, headerButton as fallback
            GameObject anchor = null;
            if (pinnedPanel.seeAllButton != null && pinnedPanel.seeAllButton.gameObject.activeInHierarchy)
                anchor = pinnedPanel.seeAllButton.gameObject;
            else if (pinnedPanel.headerButton != null)
                anchor = pinnedPanel.headerButton.gameObject;

            if (anchor == null) return;

            var anchorRT = anchor.GetComponent<RectTransform>();
            if (anchorRT == null) return;

            // Get the bottom edge of the anchor in world space, convert to our parent's local space
            anchorRT.GetWorldCorners(cornerBuf);
            // cornerBuf[0] = bottom-left in world space
            var localBottom = transform.InverseTransformPoint(cornerBuf[0]);
            float gap = 4f;
            panelRect.anchoredPosition = new Vector2(0f, localBottom.y - gap);
        }

        private void RefreshPanel()
        {
            PositionBelowResources();

            var sensors = AdvancedWattageSensorComponent.AllSensors;
            if (sensors == null)
                return;

            var labeled = new List<AdvancedWattageSensorComponent>();
            foreach (var sensor in sensors)
            {
                if (sensor != null && sensor.HasLabel)
                    labeled.Add(sensor);
            }
            labeled = labeled.OrderBy(s => s.sensorLabel, System.StringComparer.OrdinalIgnoreCase).ToList();

            if (labeled.Count == 0)
            {
                if (panelGO != null && panelGO.activeSelf)
                    panelGO.SetActive(false);
                return;
            }

            if (panelGO != null && !panelGO.activeSelf)
                panelGO.SetActive(true);

            // Remove rows for sensors that no longer exist or lost their label
            var toRemove = new List<AdvancedWattageSensorComponent>();
            foreach (var kvp in rows)
            {
                if (kvp.Key == null || !kvp.Key.HasLabel)
                    toRemove.Add(kvp.Key);
            }
            foreach (var key in toRemove)
            {
                if (rows.TryGetValue(key, out var rowGO))
                    Destroy(rowGO);
                rows.Remove(key);
            }

            int warningPercent = AdvancedWattageSensorOptions.Instance.WarningPercent;

            foreach (var sensor in labeled)
            {
                if (!rows.ContainsKey(sensor))
                {
                    var rowGO = CreateRow();
                    rowGO.transform.SetParent(rowContainer.transform, false);
                    rows[sensor] = rowGO;
                }

                UpdateRow(sensor, rows[sensor], warningPercent);
            }
        }

        private void UpdateRow(AdvancedWattageSensorComponent sensor, GameObject rowGO, int warningPercent)
        {
            var labelText = rowGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            var valueText = rowGO.transform.Find("Value")?.GetComponent<TextMeshProUGUI>();
            if (labelText == null || valueText == null)
                return;

            float current = sensor.currentWattage;
            float cycleAvg = sensor.lastCycleAverageWattage;
            float threshold = sensor.thresholdWattage;

            labelText.text = sensor.sensorLabel;

            string currentStr = GameUtil.GetFormattedWattage(current, GameUtil.WattageFormatterUnit.Automatic, true);
            string avgStr = GameUtil.GetFormattedWattage(cycleAvg, GameUtil.WattageFormatterUnit.Automatic, true);
            string thresholdStr = GameUtil.GetFormattedWattage(threshold, GameUtil.WattageFormatterUnit.Automatic, true);
            valueText.text = $"{currentStr} / {avgStr} / {thresholdStr}";

            float warningLevel = threshold * (1f - warningPercent / 100f);
            bool warning = threshold > 0f && current >= warningLevel;
            valueText.color = warning ? new Color(1f, 0.35f, 0.35f) : new Color(0.75f, 0.75f, 0.75f);
            labelText.color = warning ? new Color(1f, 0.35f, 0.35f) : Color.white;
        }

        private GameObject CreateRow()
        {
            var rowGO = new GameObject("SensorRow");
            rowGO.AddComponent<RectTransform>();
            var rowLayout = rowGO.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;
            rowLayout.spacing = 6f;
            rowLayout.padding = new RectOffset(0, 0, 0, 0);

            var rowLE = rowGO.AddComponent<LayoutElement>();
            rowLE.minHeight = 22f;
            rowLE.preferredHeight = 22f;

            // Label (left, flexible width)
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(rowGO.transform, false);
            labelGO.AddComponent<RectTransform>();
            var labelTmp = labelGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(labelTmp);
            labelTmp.fontSize = 12;
            labelTmp.color = Color.white;
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
            labelTmp.enableWordWrapping = false;
            labelTmp.overflowMode = TextOverflowModes.Ellipsis;
            labelTmp.raycastTarget = false;
            var labelLE = labelGO.AddComponent<LayoutElement>();
            labelLE.flexibleWidth = 0f;
            labelLE.minWidth = 30f;
            labelLE.preferredWidth = 80f;

            // Value (right, flexible width)
            var valueGO = new GameObject("Value");
            valueGO.transform.SetParent(rowGO.transform, false);
            valueGO.AddComponent<RectTransform>();
            var valueTmp = valueGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(valueTmp);
            valueTmp.fontSize = 12;
            valueTmp.color = new Color(0.75f, 0.75f, 0.75f);
            valueTmp.alignment = TextAlignmentOptions.MidlineRight;
            valueTmp.enableWordWrapping = false;
            valueTmp.overflowMode = TextOverflowModes.Ellipsis;
            valueTmp.raycastTarget = false;
            var valueLE = valueGO.AddComponent<LayoutElement>();
            valueLE.flexibleWidth = 1f;
            valueLE.minWidth = 80f;

            return rowGO;
        }
    }
}
