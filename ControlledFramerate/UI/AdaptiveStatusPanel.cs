using ControlledFramerate.Core;
using ControlledFramerate.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ControlledFramerate.UI
{
    // Live FPS/speed panel, positioned below the Resources panel
    public class AdaptiveStatusPanel : MonoBehaviour
    {
        public static AdaptiveStatusPanel Instance { get; private set; }

        private GameObject panelGO;
        private GameObject rowContainer;
        private TextMeshProUGUI headerArrow;
        private bool collapsed;

        private TMP_FontAsset gameFont;
        private Material gameFontMaterial;
        private Sprite panelSprite;
        private float cachedWidth = 220f;
        private RectTransform panelRect;
        private readonly Vector3[] cornerBuf = new Vector3[4];

        private static readonly Color PanelColor = new Color(62f / 255f, 67f / 255f, 87f / 255f, 1f);
        private static readonly Color HeaderColor = new Color(52f / 255f, 57f / 255f, 77f / 255f, 1f);

        private TextMeshProUGUI fpsValue;
        private TextMeshProUGUI speedValue;
        private TextMeshProUGUI modeValue;
        private TextMeshProUGUI targetFpsValue;
        private TextMeshProUGUI ceilingValue;

        public static void Create()
        {
            if (Instance != null)
                return;

            var pinnedPanel = PinnedResourcesPanel.Instance;
            if (pinnedPanel == null)
                return;

            var rootGO = new GameObject("AdaptiveStatusPanel");
            rootGO.transform.SetParent(pinnedPanel.transform, false);
            rootGO.transform.SetAsLastSibling();

            Instance = rootGO.AddComponent<AdaptiveStatusPanel>();
            Instance.CacheGameStyles(pinnedPanel);
            Instance.BuildPanel();
            Instance.InvokeRepeating(nameof(RefreshPanel), 0.25f, 0.25f);
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
            var headerTexts = pinnedPanel.headerButton?.GetComponentsInChildren<LocText>(true);
            if (headerTexts != null && headerTexts.Length > 0)
            {
                gameFont = headerTexts[0].font;
                gameFontMaterial = headerTexts[0].fontMaterial;
            }

            var images = pinnedPanel.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.sprite != null && img.type == Image.Type.Sliced)
                {
                    panelSprite = img.sprite;
                    break;
                }
            }
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

            var headerRT = pinnedPanel.headerButton?.GetComponent<RectTransform>();
            if (headerRT != null && headerRT.rect.width > 50f)
                cachedWidth = Mathf.Max(headerRT.rect.width + 22f, 220f);
            else
                cachedWidth = 220f;
        }

        private void BuildPanel()
        {
            panelGO = new GameObject("FramerateMonitorContainer");
            panelGO.transform.SetParent(transform, false);

            panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(cachedWidth, 0f);

            var bg = panelGO.AddComponent<Image>();
            if (panelSprite != null)
            {
                bg.sprite = panelSprite;
                bg.type = Image.Type.Sliced;
            }
            bg.color = PanelColor;

            var fitter = panelGO.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var layout = panelGO.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 0f;
            layout.padding = new RectOffset(0, 0, 0, 0);

            BuildHeader();

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

            fpsValue = CreateRow("FPS", "—");
            speedValue = CreateRow("Speed", "—");
            modeValue = CreateRow("Mode", "—");
            targetFpsValue = CreateAdjustableRow("Target", "—", OnTargetFpsMinus, OnTargetFpsPlus);
            ceilingValue = CreateAdjustableRow("Ceiling", "—", OnCeilingMinus, OnCeilingPlus);

            panelGO.SetActive(false);
        }

        private void BuildHeader()
        {
            var headerGO = new GameObject("Header");
            headerGO.transform.SetParent(panelGO.transform, false);
            headerGO.AddComponent<RectTransform>();

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

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(headerGO.transform, false);
            titleGO.AddComponent<RectTransform>();
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(titleText);
            titleText.text = "Framerate Monitor";
            titleText.fontSize = 14;
            titleText.fontStyle = FontStyles.Normal;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.raycastTarget = false;
            var titleLE = titleGO.AddComponent<LayoutElement>();
            titleLE.flexibleWidth = 1f;

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

        private void PositionBelowResources()
        {
            if (panelRect == null) return;

            var pinnedPanel = PinnedResourcesPanel.Instance;
            if (pinnedPanel == null) return;

            GameObject anchor = null;
            if (pinnedPanel.seeAllButton != null && pinnedPanel.seeAllButton.gameObject.activeInHierarchy)
                anchor = pinnedPanel.seeAllButton.gameObject;
            else if (pinnedPanel.headerButton != null)
                anchor = pinnedPanel.headerButton.gameObject;

            if (anchor == null) return;

            var anchorRT = anchor.GetComponent<RectTransform>();
            if (anchorRT == null) return;

            anchorRT.GetWorldCorners(cornerBuf);
            float lowestWorldY = cornerBuf[0].y;
            Vector3 lowestWorldPoint = cornerBuf[0];

            // Stack below other right-side panels by finding lowest active sibling with ContentSizeFitter
            foreach (Transform sibling in pinnedPanel.transform)
            {
                if (sibling == transform) continue;
                if (!sibling.gameObject.activeInHierarchy) continue;

                var csf = sibling.GetComponentInChildren<ContentSizeFitter>(false);
                if (csf == null) continue;

                var sibRect = csf.GetComponent<RectTransform>();
                if (sibRect == null) continue;

                sibRect.GetWorldCorners(cornerBuf);
                if (cornerBuf[0].y < lowestWorldY)
                {
                    lowestWorldY = cornerBuf[0].y;
                    lowestWorldPoint = cornerBuf[0];
                }
            }

            var localBottom = transform.InverseTransformPoint(lowestWorldPoint);
            float gap = 4f;
            panelRect.anchoredPosition = new Vector2(0f, localBottom.y - gap);
        }

        private void RefreshPanel()
        {
            bool shouldShow = SpeedStateManager.FramerateMonitorVisible
                && !SpeedStateManager.IsBenchmarkRunning
                && SpeedControlScreen.Instance != null;

            if (!shouldShow)
            {
                if (panelGO != null && panelGO.activeSelf)
                    panelGO.SetActive(false);
                return;
            }

            if (panelGO != null && !panelGO.activeSelf)
                panelGO.SetActive(true);

            PositionBelowResources();

            float fps = FpsMonitor.IsValid ? FpsMonitor.SmoothedFps : 0f;
            var opts = ControlledFramerateOptions.Instance;
            int selectedBtn = SpeedControlScreen.Instance.GetSpeed();
            float ceiling = SpeedStateManager.GetSpeedForButton(selectedBtn);

            if (fpsValue != null)
            {
                fpsValue.text = FpsMonitor.IsValid ? $"{fps:F0}" : "...";
                if (fps >= opts.DesiredFps)
                    fpsValue.color = new Color(0.4f, 0.9f, 0.4f);
                else if (fps >= opts.MinimumFps)
                    fpsValue.color = new Color(0.9f, 0.7f, 0.2f);
                else
                    fpsValue.color = new Color(0.9f, 0.3f, 0.3f);
            }

            if (speedValue != null)
            {
                float displaySpeed = SpeedStateManager.CurrentMode == SpeedStateManager.SpeedMode.Adaptive
                    ? AdaptiveSpeedController.CurrentAdaptiveSpeed
                    : Time.timeScale;
                speedValue.text = $"{displaySpeed:F1}x";
            }

            if (modeValue != null)
                modeValue.text = SpeedStateManager.CurrentMode == SpeedStateManager.SpeedMode.Adaptive
                    ? "Adaptive" : "Fixed";

            if (targetFpsValue != null)
            {
                targetFpsValue.text = $"{opts.DesiredFps:F0}";
                if (fps >= opts.DesiredFps)
                    targetFpsValue.color = new Color(0.4f, 0.9f, 0.4f);
                else
                    targetFpsValue.color = new Color(0.75f, 0.75f, 0.75f);
            }

            if (ceilingValue != null)
                ceilingValue.text = $"{ceiling:F1}x";
        }

        private TextMeshProUGUI CreateRow(string label, string initialValue)
        {
            var rowGO = new GameObject($"Row_{label}");
            rowGO.transform.SetParent(rowContainer.transform, false);
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

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(rowGO.transform, false);
            labelGO.AddComponent<RectTransform>();
            var labelTmp = labelGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(labelTmp);
            labelTmp.text = label;
            labelTmp.fontSize = 12;
            labelTmp.color = Color.white;
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
            labelTmp.enableWordWrapping = false;
            labelTmp.raycastTarget = false;
            var labelLE = labelGO.AddComponent<LayoutElement>();
            labelLE.minWidth = 60f;
            labelLE.preferredWidth = 60f;

            var valueGO = new GameObject("Value");
            valueGO.transform.SetParent(rowGO.transform, false);
            valueGO.AddComponent<RectTransform>();
            var valueTmp = valueGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(valueTmp);
            valueTmp.text = initialValue;
            valueTmp.fontSize = 12;
            valueTmp.color = new Color(0.75f, 0.75f, 0.75f);
            valueTmp.alignment = TextAlignmentOptions.MidlineRight;
            valueTmp.enableWordWrapping = false;
            valueTmp.raycastTarget = false;
            var valueLE = valueGO.AddComponent<LayoutElement>();
            valueLE.flexibleWidth = 1f;
            valueLE.minWidth = 60f;

            return valueTmp;
        }

        private TextMeshProUGUI CreateAdjustableRow(string label, string initialValue,
            UnityEngine.Events.UnityAction onMinus, UnityEngine.Events.UnityAction onPlus)
        {
            var rowGO = new GameObject($"Row_{label}");
            rowGO.transform.SetParent(rowContainer.transform, false);
            rowGO.AddComponent<RectTransform>();

            var rowLayout = rowGO.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;
            rowLayout.spacing = 4f;
            rowLayout.padding = new RectOffset(0, 0, 0, 0);

            var rowLE = rowGO.AddComponent<LayoutElement>();
            rowLE.minHeight = 22f;
            rowLE.preferredHeight = 22f;

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(rowGO.transform, false);
            labelGO.AddComponent<RectTransform>();
            var labelTmp = labelGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(labelTmp);
            labelTmp.text = label;
            labelTmp.fontSize = 12;
            labelTmp.color = Color.white;
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
            labelTmp.enableWordWrapping = false;
            labelTmp.raycastTarget = false;
            var labelLE = labelGO.AddComponent<LayoutElement>();
            labelLE.minWidth = 60f;
            labelLE.preferredWidth = 60f;

            CreateSmallButton(rowGO.transform, "BtnMinus", "\u2212", onMinus);

            var valueGO = new GameObject("Value");
            valueGO.transform.SetParent(rowGO.transform, false);
            valueGO.AddComponent<RectTransform>();
            var valueTmp = valueGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(valueTmp);
            valueTmp.text = initialValue;
            valueTmp.fontSize = 12;
            valueTmp.color = new Color(0.75f, 0.75f, 0.75f);
            valueTmp.alignment = TextAlignmentOptions.Center;
            valueTmp.enableWordWrapping = false;
            valueTmp.raycastTarget = false;
            var valueLE = valueGO.AddComponent<LayoutElement>();
            valueLE.flexibleWidth = 1f;
            valueLE.minWidth = 32f;

            CreateSmallButton(rowGO.transform, "BtnPlus", "+", onPlus);

            return valueTmp;
        }

        private void CreateSmallButton(Transform parent, string name, string symbol,
            UnityEngine.Events.UnityAction onClick)
        {
            var btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            btnGO.AddComponent<RectTransform>();

            var btnLE = btnGO.AddComponent<LayoutElement>();
            btnLE.minWidth = 20f;
            btnLE.preferredWidth = 20f;
            btnLE.minHeight = 18f;
            btnLE.preferredHeight = 18f;

            var btnBg = btnGO.AddComponent<Image>();
            btnBg.color = new Color(0.3f, 0.35f, 0.45f, 1f);

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnBg;

            var colors = btn.colors;
            colors.normalColor = new Color(0.3f, 0.35f, 0.45f, 1f);
            colors.highlightedColor = new Color(0.4f, 0.45f, 0.55f, 1f);
            colors.pressedColor = new Color(0.5f, 0.55f, 0.65f, 1f);
            btn.colors = colors;

            btn.onClick.AddListener(onClick);

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var textTmp = textGO.AddComponent<TextMeshProUGUI>();
            ApplyFont(textTmp);
            textTmp.text = symbol;
            textTmp.fontSize = 14;
            textTmp.fontStyle = FontStyles.Bold;
            textTmp.color = Color.white;
            textTmp.alignment = TextAlignmentOptions.Center;
            textTmp.raycastTarget = false;
        }

        private void OnTargetFpsMinus()
        {
            var opts = ControlledFramerateOptions.Instance;
            opts.DesiredFps = System.Math.Max(15, opts.DesiredFps - 5);
            if (opts.MinimumFps >= opts.DesiredFps)
                opts.MinimumFps = System.Math.Max(5, opts.DesiredFps - 5);
            ControlledFramerateOptions.Save();
            ControlledFramerateMod.Log($"[FramerateMonitor] Target FPS decreased to {opts.DesiredFps}");
            TopBarButtons.UpdateSpeedTooltips();
        }

        private void OnTargetFpsPlus()
        {
            var opts = ControlledFramerateOptions.Instance;
            opts.DesiredFps = System.Math.Min(120, opts.DesiredFps + 5);
            ControlledFramerateOptions.Save();
            ControlledFramerateMod.Log($"[FramerateMonitor] Target FPS increased to {opts.DesiredFps}");
            TopBarButtons.UpdateSpeedTooltips();
        }

        private void OnCeilingMinus()
        {
            var opts = ControlledFramerateOptions.Instance;
            int selectedBtn = SpeedControlScreen.Instance != null ? SpeedControlScreen.Instance.GetSpeed() : 2;
            float current = SpeedStateManager.GetSpeedForButton(selectedBtn);
            float newVal = Mathf.Max(1f, current - 0.5f);
            SetSpeedForButton(selectedBtn, newVal);
            ControlledFramerateOptions.Save();
            ControlledFramerateMod.Log($"[FramerateMonitor] Ceiling (button {selectedBtn}) decreased to {newVal:F1}x");
            TopBarButtons.UpdateSpeedTooltips();
        }

        private void OnCeilingPlus()
        {
            var opts = ControlledFramerateOptions.Instance;
            int selectedBtn = SpeedControlScreen.Instance != null ? SpeedControlScreen.Instance.GetSpeed() : 2;
            float current = SpeedStateManager.GetSpeedForButton(selectedBtn);
            float newVal = Mathf.Min(20f, current + 0.5f);
            SetSpeedForButton(selectedBtn, newVal);
            ControlledFramerateOptions.Save();
            ControlledFramerateMod.Log($"[FramerateMonitor] Ceiling (button {selectedBtn}) increased to {newVal:F1}x");
            TopBarButtons.UpdateSpeedTooltips();
        }

        private static void SetSpeedForButton(int buttonIndex, float speed)
        {
            var opts = ControlledFramerateOptions.Instance;
            switch (buttonIndex)
            {
                case 0: opts.SlowSpeed = speed; break;
                case 1: opts.MediumSpeed = speed; break;
                case 2: opts.FastSpeed = speed; break;
            }

            string saveName = SpeedStateManager.GetCurrentSaveName();
            if (!string.IsNullOrEmpty(saveName) && opts.SaveProfiles != null &&
                opts.SaveProfiles.TryGetValue(saveName, out var profile))
            {
                switch (buttonIndex)
                {
                    case 0: profile.SlowSpeed = speed; break;
                    case 1: profile.MediumSpeed = speed; break;
                    case 2: profile.FastSpeed = speed; break;
                }
            }
        }
    }
}
