using System;
using ControlledFramerate.Core;
using ControlledFramerate.Strings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ControlledFramerate.UI
{
    public class ToolbarPopupMenu : MonoBehaviour
    {
        public static ToolbarPopupMenu Instance { get; private set; }

        private GameObject backdropGO;
        private GameObject panelGO;
        private RectTransform panelRect;
        private TextMeshProUGUI benchmarkLabel;
        private TextMeshProUGUI adaptiveLabel;
        private TextMeshProUGUI monitorLabel;
        private Button adaptiveButton;
        private TMP_FontAsset gameFont;
        private Material gameFontMaterial;
        private static readonly Color PanelColor = new Color(62f / 255f, 67f / 255f, 87f / 255f, 0.98f);
        private static readonly Color RowHoverColor = new Color(0.35f, 0.4f, 0.55f, 1f);
        private static readonly Color RowNormalColor = new Color(0.28f, 0.32f, 0.45f, 1f);
        private static readonly Color RowDisabledColor = new Color(0.2f, 0.2f, 0.25f, 1f);
        private const float PanelWidth = 220f;
        private const float RowHeight = 32f;
        private readonly Vector3[] cornerBuf = new Vector3[4];

        public static void Toggle(RectTransform anchorRect)
        {
            if (Instance != null && Instance.gameObject != null && Instance.gameObject.activeSelf)
            {
                Hide();
                return;
            }

            if (GameScreenManager.Instance == null || GameScreenManager.Instance.ssOverlayCanvas == null)
                return;

            if (Instance == null)
            {
                var rootGO = new GameObject("ToolbarPopupMenu");
                rootGO.transform.SetParent(GameScreenManager.Instance.ssOverlayCanvas.transform, false);
                Instance = rootGO.AddComponent<ToolbarPopupMenu>();
                Instance.CacheFont();
                Instance.BuildPopup();
            }

            Instance.RefreshLabels();
            Instance.PositionPanel(anchorRect);
            Instance.gameObject.SetActive(true);
            KMonoBehaviour.PlaySound(GlobalAssets.GetSound("HUD_Click"));
        }

        public static void Hide()
        {
            if (Instance != null && Instance.gameObject != null)
                Instance.gameObject.SetActive(false);
        }

        public static bool IsOpen => Instance != null && Instance.gameObject != null && Instance.gameObject.activeSelf;

        private void CacheFont()
        {
            var pinnedPanel = PinnedResourcesPanel.Instance;
            if (pinnedPanel?.headerButton != null)
            {
                var headerTexts = pinnedPanel.headerButton.GetComponentsInChildren<LocText>(true);
                if (headerTexts != null && headerTexts.Length > 0)
                {
                    gameFont = headerTexts[0].font;
                    gameFontMaterial = headerTexts[0].fontMaterial;
                }
            }
        }

        private void ApplyFont(TextMeshProUGUI tmp)
        {
            if (gameFont != null) tmp.font = gameFont;
            if (gameFontMaterial != null) tmp.fontMaterial = gameFontMaterial;
        }

        private void BuildPopup()
        {
            var root = gameObject;

            var backdrop = new GameObject("Backdrop");
            backdrop.transform.SetParent(root.transform, false);
            var backdropRT = backdrop.AddComponent<RectTransform>();
            backdropRT.anchorMin = Vector2.zero;
            backdropRT.anchorMax = Vector2.one;
            backdropRT.offsetMin = Vector2.zero;
            backdropRT.offsetMax = Vector2.zero;
            var backdropImg = backdrop.AddComponent<Image>();
            backdropImg.color = new Color(0f, 0f, 0f, 0f);
            backdropImg.raycastTarget = true;
            var backdropBtn = backdrop.AddComponent<Button>();
            backdropBtn.transition = Selectable.Transition.None;
            backdropBtn.onClick.AddListener(OnBackdropClicked);
            backdropGO = backdrop;

            panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(root.transform, false);
            panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.sizeDelta = new Vector2(PanelWidth, RowHeight * 3f);

            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = PanelColor;
            panelImg.raycastTarget = true;

            var layout = panelGO.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 0f;
            layout.padding = new RectOffset(0, 0, 8, 8);

            var benchmarkRow = CreateRowButton(PanelWidth, RowHeight, ControlledFramerateStrings.PopupRunBenchmark, new System.Action(OnBenchmarkClicked));
            benchmarkLabel = benchmarkRow.label;

            var adaptiveRow = CreateRowButton(PanelWidth, RowHeight, ControlledFramerateStrings.PopupAdaptiveOff, new System.Action(OnAdaptiveClicked));
            adaptiveLabel = adaptiveRow.label;
            adaptiveButton = adaptiveRow.button;

            var monitorRow = CreateRowButton(PanelWidth, RowHeight, ControlledFramerateStrings.PopupMonitorOff, new System.Action(OnMonitorClicked));
            monitorLabel = monitorRow.label;

            root.SetActive(false);
        }

        private (Button button, TextMeshProUGUI label) CreateRowButton(float width, float height, string labelText, System.Action onClick)
        {
            var rowGO = new GameObject("Row");
            rowGO.transform.SetParent(panelGO.transform, false);
            rowGO.AddComponent<RectTransform>();

            var rowLayout = rowGO.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;
            rowLayout.spacing = 0f;
            rowLayout.padding = new RectOffset(12, 12, 0, 0);

            var rowLE = rowGO.AddComponent<LayoutElement>();
            rowLE.minHeight = height;
            rowLE.preferredHeight = height;
            rowLE.flexibleWidth = 1f;

            var rowBg = rowGO.AddComponent<Image>();
            rowBg.color = RowNormalColor;
            rowBg.raycastTarget = true;

            var btn = rowGO.AddComponent<Button>();
            btn.targetGraphic = rowBg;
            btn.transition = Selectable.Transition.ColorTint;
            var colors = btn.colors;
            colors.normalColor = RowNormalColor;
            colors.highlightedColor = RowHoverColor;
            colors.pressedColor = RowHoverColor;
            colors.disabledColor = RowDisabledColor;
            btn.colors = colors;
            btn.onClick.AddListener(() =>
            {
                onClick();
                Hide();
                TopBarButtons.RefreshButtonStates();
            });

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(rowGO.transform, false);
            labelGO.AddComponent<RectTransform>();
            var labelLE = labelGO.AddComponent<LayoutElement>();
            labelLE.flexibleWidth = 1f;
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(label);
            label.text = labelText;
            label.fontSize = 13;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.raycastTarget = false;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;

            return (btn, label);
        }

        private void PositionPanel(RectTransform anchorRect)
        {
            if (panelRect == null || anchorRect == null) return;

            var overlayCanvas = panelRect.GetComponentInParent<Canvas>();
            if (overlayCanvas == null) return;

            RectTransform overlayRT = overlayCanvas.GetComponent<RectTransform>();
            anchorRect.GetWorldCorners(cornerBuf);
            Vector3 worldBottomLeft = cornerBuf[0];
            Vector2 localPoint = overlayRT.InverseTransformPoint(worldBottomLeft);
            float gap = 4f;
            panelRect.anchoredPosition = new Vector2(localPoint.x, localPoint.y - gap);
        }

        private void RefreshLabels()
        {
            if (benchmarkLabel != null)
                benchmarkLabel.text = BenchmarkEngine.IsRunning
                    ? ControlledFramerateStrings.PopupCancelBenchmark
                    : ControlledFramerateStrings.PopupRunBenchmark;

            if (adaptiveLabel != null)
            {
                if (!SpeedStateManager.HasBenchmarkData)
                    adaptiveLabel.text = ControlledFramerateStrings.PopupAdaptiveDisabled;
                else
                    adaptiveLabel.text = SpeedStateManager.CurrentMode == SpeedStateManager.SpeedMode.Adaptive
                        ? ControlledFramerateStrings.PopupAdaptiveOn
                        : ControlledFramerateStrings.PopupAdaptiveOff;
            }

            if (adaptiveButton != null)
                adaptiveButton.interactable = SpeedStateManager.HasBenchmarkData;

            if (monitorLabel != null)
                monitorLabel.text = SpeedStateManager.FramerateMonitorVisible
                    ? ControlledFramerateStrings.PopupMonitorOn
                    : ControlledFramerateStrings.PopupMonitorOff;
        }

        private void OnBackdropClicked()
        {
            Hide();
            TopBarButtons.RefreshButtonStates();
        }

        private void OnBenchmarkClicked()
        {
            if (BenchmarkEngine.IsRunning)
                BenchmarkEngine.Cancel();
            else
                BenchmarkOverlay.ShowConfig();
        }

        private void OnAdaptiveClicked()
        {
            if (!SpeedStateManager.HasBenchmarkData) return;
            KMonoBehaviour.PlaySound(GlobalAssets.GetSound("HUD_Click"));
            SpeedStateManager.ToggleAdaptive();
            TopBarButtons.UpdateSpeedTooltips();
        }

        private void OnMonitorClicked()
        {
            KMonoBehaviour.PlaySound(GlobalAssets.GetSound("HUD_Click"));
            SpeedStateManager.ToggleMonitor();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Hide();
                TopBarButtons.RefreshButtonStates();
            }
        }
    }
}
