using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiserableUtils.UI
{
    // Reusable collapsible section UI: header with arrow, click to expand/collapse children.
    // Header is the first child of parentTransform. Originally-inactive children stay hidden.
    public class CollapsibleSection
    {
        private readonly Transform _parentTransform;
        private readonly HashSet<string> _originallyInactive = new HashSet<string>();
        private readonly string _headerName;
        private TMPro.TextMeshProUGUI _arrowText;
        private bool _collapsed;

        public bool IsCollapsed => _collapsed;

        public CollapsibleSection(
            Transform parentTransform,
            string headerLabel,
            bool startCollapsed = true,
            string headerName = null)
        {
            _parentTransform = parentTransform;
            _collapsed = startCollapsed;
            _headerName = headerName ?? "CollapsibleSection_Header";

            // Snapshot originally-inactive children
            for (int i = 0; i < parentTransform.childCount; i++)
            {
                var child = parentTransform.GetChild(i);
                if (!child.gameObject.activeSelf)
                    _originallyInactive.Add(child.name);
            }

            var header = new GameObject(_headerName);
            header.transform.SetParent(parentTransform, false);
            header.transform.SetAsFirstSibling();

            var headerRT = header.AddComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0f, 1f);
            headerRT.anchorMax = Vector2.one;
            headerRT.pivot = new Vector2(0.5f, 1f);

            var headerLE = header.AddComponent<LayoutElement>();
            headerLE.minHeight = 28f;
            headerLE.preferredHeight = 28f;

            var hlg = header.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 6f;
            hlg.padding = new RectOffset(8, 8, 2, 2);

            var arrowGO = new GameObject("Arrow");
            arrowGO.transform.SetParent(header.transform, false);
            arrowGO.AddComponent<RectTransform>();
            var arrowText = arrowGO.AddComponent<TextMeshProUGUI>();
            arrowText.text = _collapsed ? "\u25B6" : "\u25BC";
            arrowText.fontSize = 14f;
            arrowText.alignment = TextAlignmentOptions.MidlineLeft;
            arrowText.raycastTarget = false;
            var arrowLE = arrowGO.AddComponent<LayoutElement>();
            arrowLE.minWidth = 16f;
            arrowLE.preferredWidth = 16f;
            _arrowText = arrowText;

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(header.transform, false);
            labelGO.AddComponent<RectTransform>();
            var labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.text = headerLabel;
            labelText.fontSize = 14f;
            labelText.fontStyle = FontStyles.Normal;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            labelText.raycastTarget = false;
            var labelLE = labelGO.AddComponent<LayoutElement>();
            labelLE.flexibleWidth = 1f;

            var headerImg = header.AddComponent<Image>();
            headerImg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            headerImg.raycastTarget = true;

            var btn = header.AddComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            btn.targetGraphic = headerImg;
            btn.onClick.AddListener(Toggle);

            Apply();
        }

        public void Toggle()
        {
            _collapsed = !_collapsed;
            Apply();
        }

        public void Apply()
        {
            if (_arrowText != null)
                _arrowText.text = _collapsed ? "\u25B6" : "\u25BC";

            if (_parentTransform == null) return;

            for (int i = 0; i < _parentTransform.childCount; i++)
            {
                var child = _parentTransform.GetChild(i);
                if (child.name == _headerName) continue;
                if (_originallyInactive.Contains(child.name))
                {
                    child.gameObject.SetActive(false);
                    continue;
                }
                child.gameObject.SetActive(!_collapsed);
            }
        }
    }
}
