using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using ControlledMods.Options;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ControlledMods.Patches.UndergroundConduit
{
    public static class ChannelFilterPatches
    {
        private static Type _sideScreenType;
        private static Type _treeFilterableSideScreenType;
        private static FieldInfo _listContainerField;
        private static FieldInfo _rowsField;
        private static FieldInfo _channelNameField;
        private static FieldInfo _kscreenIsEditingField;
        private static PropertyInfo _kscreenIsEditingProperty;
        private static FieldInfo _treeFilterableInputFieldField;
        private static FieldInfo _detailsScreenSideScreensField;

        public static void ApplyPatches(Harmony harmony)
        {
            _sideScreenType = AccessTools.TypeByName("UndergroundConduit.SideScreen");
            if (_sideScreenType == null) return;

            _treeFilterableSideScreenType = typeof(TreeFilterableSideScreen);
            _treeFilterableInputFieldField = AccessTools.Field(_treeFilterableSideScreenType, "inputField");
            _detailsScreenSideScreensField = AccessTools.Field(typeof(DetailsScreen), "sideScreens");

            _listContainerField = AccessTools.Field(_sideScreenType, "listContainer");
            _rowsField = AccessTools.Field(_sideScreenType, "rows");
            if (_listContainerField == null || _rowsField == null) return;

            var channelBaseType = AccessTools.TypeByName("UndergroundConduit.ChannelBase");
            _channelNameField = channelBaseType != null ? AccessTools.Field(channelBaseType, "Name") : null;
            if (_channelNameField == null) return;

            var kscreenType = typeof(KScreen);
            _kscreenIsEditingField = AccessTools.Field(kscreenType, "isEditing")
                ?? AccessTools.Field(kscreenType, "m_isEditing");
            _kscreenIsEditingProperty = AccessTools.Property(kscreenType, "isEditing");

            var setTarget = AccessTools.Method(_sideScreenType, "SetTarget", new[] { typeof(GameObject) });
            if (setTarget != null)
                harmony.Patch(setTarget, postfix: new HarmonyMethod(typeof(ChannelFilterPatches), nameof(SideScreen_SetTarget_Postfix)));

            var refresh = AccessTools.Method(_sideScreenType, "Refresh", Type.EmptyTypes);
            if (refresh != null)
                harmony.Patch(refresh, postfix: new HarmonyMethod(typeof(ChannelFilterPatches), nameof(SideScreen_Refresh_Postfix)));

            // Harmony requires patching the declaring type, not an inheriting type.
            // UndergroundConduit.SideScreen inherits GetSortKey/OnKeyDown/OnKeyUp from KScreen
            // without overriding them, so we must patch KScreen and type-check in the callbacks.
            var getSortKey = AccessTools.Method(typeof(KScreen), "GetSortKey", Type.EmptyTypes);
            if (getSortKey != null)
                harmony.Patch(getSortKey, postfix: new HarmonyMethod(typeof(ChannelFilterPatches), nameof(SideScreen_GetSortKey_Postfix)));

            var onKeyDown = AccessTools.Method(typeof(KScreen), "OnKeyDown", new[] { typeof(KButtonEvent) });
            var onKeyUp = AccessTools.Method(typeof(KScreen), "OnKeyUp", new[] { typeof(KButtonEvent) });
            if (onKeyDown != null)
                harmony.Patch(onKeyDown, prefix: new HarmonyMethod(typeof(ChannelFilterPatches), nameof(SideScreen_OnKeyDown_Prefix)));
            if (onKeyUp != null)
                harmony.Patch(onKeyUp, prefix: new HarmonyMethod(typeof(ChannelFilterPatches), nameof(SideScreen_OnKeyUp_Prefix)));

            // CameraController handles camera/game shortcuts separately from the KScreen stack.
            // Extend its input-field check so it recognises our filter field.
            var withinInputField = AccessTools.Method(typeof(CameraController), "WithinInputField", Type.EmptyTypes);
            if (withinInputField != null)
                harmony.Patch(withinInputField, postfix: new HarmonyMethod(typeof(ChannelFilterPatches), nameof(WithinInputField_Postfix)));

            var editingResolution = _kscreenIsEditingField != null ? "field" : _kscreenIsEditingProperty != null ? "property" : "NOT FOUND";
            ControlledModsMod.Log($"KIN Underground Conduit channel filter patches applied (isEditing: {editingResolution})");
        }

        private static bool IsSideScreenEditing(KScreen instance)
        {
            if (instance == null || _sideScreenType == null) return false;
            if (!_sideScreenType.IsInstanceOfType(instance)) return false;

            if (_kscreenIsEditingField != null)
            {
                var editing = _kscreenIsEditingField.GetValue(instance);
                if (editing is bool b && b) return true;
            }
            else if (_kscreenIsEditingProperty != null)
            {
                var editing = _kscreenIsEditingProperty.GetValue(instance, null);
                if (editing is bool b && b) return true;
            }

            var helper = instance.GetComponent<ChannelFilterHelper>();
            return helper != null && helper.IsFilterFocused();
        }

        public static void WithinInputField_Postfix(ref bool __result)
        {
            if (__result) return;
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es?.currentSelectedGameObject == null) return;
            var go = es.currentSelectedGameObject;
            if (go.GetComponent<KInputTextField>() != null
                || go.GetComponentInParent<KInputTextField>() != null
                || go.GetComponent<TMP_InputField>() != null
                || go.GetComponentInParent<TMP_InputField>() != null)
                __result = true;
        }

        public static void SideScreen_GetSortKey_Postfix(KScreen __instance, ref float __result)
        {
            if (IsSideScreenEditing(__instance))
                __result = 50f;
        }

        public static bool SideScreen_OnKeyDown_Prefix(KScreen __instance, KButtonEvent e)
        {
            if (IsSideScreenEditing(__instance))
            {
                e.Consumed = true;
                return false;
            }
            return true;
        }

        public static bool SideScreen_OnKeyUp_Prefix(KScreen __instance, KButtonEvent e)
        {
            if (IsSideScreenEditing(__instance))
            {
                e.Consumed = true;
                return false;
            }
            return true;
        }

        public static void SideScreen_SetTarget_Postfix(object __instance)
        {
            if (__instance == null || !ControlledModsOptions.Instance.EnableChannelFilter) return;
            var comp = __instance as Component;
            if (comp == null || comp.gameObject == null) return;

            var helper = comp.gameObject.GetComponent<ChannelFilterHelper>();
            if (helper == null)
                helper = comp.gameObject.AddComponent<ChannelFilterHelper>();
            helper.SetSideScreen(__instance);
            helper.EnsureFilterUI();
            helper.ApplyFilter();
        }

        public static void SideScreen_Refresh_Postfix(object __instance)
        {
            if (__instance == null || !ControlledModsOptions.Instance.EnableChannelFilter) return;
            var comp = __instance as Component;
            if (comp == null) return;

            var helper = comp.gameObject.GetComponent<ChannelFilterHelper>();
            if (helper != null)
                helper.ApplyFilter();
        }

        private sealed class ChannelFilterHelper : KMonoBehaviour
        {
            private object _sideScreen;
            private KInputTextField _kInputField;
            private TMP_InputField _fallbackInput;
            private GameObject _clearButton;
            private string _filterText = "";

            public void SetSideScreen(object sideScreen)
            {
                _sideScreen = sideScreen;
            }

            public bool IsFilterFocused()
            {
                if (_kInputField != null) return _kInputField.isFocused;
                if (_fallbackInput != null) return _fallbackInput.isFocused;
                return false;
            }

            public void EnsureFilterUI()
            {
                if ((_kInputField != null || _fallbackInput != null) || _sideScreen == null) return;

                var listContainer = _listContainerField?.GetValue(_sideScreen) as GameObject;
                if (listContainer == null) return;

                var parent = listContainer.transform.parent;
                if (parent == null) return;

                var rowGO = new GameObject("ChannelFilterRow");
                rowGO.transform.SetParent(parent, false);
                rowGO.transform.SetAsFirstSibling();
                var rowRect = rowGO.AddComponent<RectTransform>();
                rowRect.sizeDelta = new Vector2(0f, 26f);
                var rowLayout = rowGO.AddComponent<LayoutElement>();
                rowLayout.preferredHeight = 26f;
                rowLayout.flexibleWidth = 1f;
                var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 4f;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = true;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.padding = new RectOffset(2, 2, 0, 0);

                var sourceInput = TryGetTreeFilterableInputField();
                if (sourceInput != null)
                {
                    var clone = UnityEngine.Object.Instantiate(sourceInput.gameObject, rowGO.transform, false);
                    clone.name = "ChannelFilterInput";
                    clone.SetActive(true);

                    _kInputField = clone.GetComponent<KInputTextField>();
                    if (_kInputField != null)
                    {
                        var inputLayout = clone.GetComponent<LayoutElement>();
                        if (inputLayout == null) inputLayout = clone.AddComponent<LayoutElement>();
                        inputLayout.flexibleWidth = 1f;
                        inputLayout.preferredHeight = 26f;

                        var placeholder = _kInputField.placeholder;
                        if (placeholder != null)
                        {
                            var t = placeholder.GetComponent<TextMeshProUGUI>();
                            if (t != null) t.text = "Filter channels...";
                        }
                        _kInputField.text = "";
                        _kInputField.onFocus = (System.Action)Delegate.Combine(_kInputField.onFocus, new System.Action(() => SetParentEditing(true)));
                        _kInputField.onEndEdit.AddListener(_ => SetParentEditing(false));
                        _kInputField.onValueChanged.AddListener(OnFilterChanged);

                        CreateClearButton(rowGO.transform);
                        ControlledModsMod.Log("[ChannelFilter] UI created via KInputTextField clone");
                        return;
                    }
                    UnityEngine.Object.Destroy(clone);
                }

                UnityEngine.Object.Destroy(rowGO);

                var fallbackRow = new GameObject("ChannelFilterRow");
                fallbackRow.transform.SetParent(parent, false);
                fallbackRow.transform.SetAsFirstSibling();
                var fbRowRect = fallbackRow.AddComponent<RectTransform>();
                fbRowRect.sizeDelta = new Vector2(0f, 26f);
                var fbRowLayout = fallbackRow.AddComponent<LayoutElement>();
                fbRowLayout.preferredHeight = 26f;
                fbRowLayout.flexibleWidth = 1f;
                var fbHlg = fallbackRow.AddComponent<HorizontalLayoutGroup>();
                fbHlg.spacing = 4f;
                fbHlg.childForceExpandWidth = false;
                fbHlg.childForceExpandHeight = true;
                fbHlg.childAlignment = TextAnchor.MiddleLeft;
                fbHlg.padding = new RectOffset(2, 2, 0, 0);

                EnsureFilterUI_Fallback(fallbackRow.transform);
                CreateClearButton(fallbackRow.transform);
                ControlledModsMod.Log("[ChannelFilter] UI created via TMP_InputField fallback");
            }

            private void CreateClearButton(Transform parent)
            {
                var btnGO = new GameObject("ClearFilterButton");
                btnGO.transform.SetParent(parent, false);

                var btnRect = btnGO.AddComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(22f, 22f);

                var btnLayout = btnGO.AddComponent<LayoutElement>();
                btnLayout.preferredWidth = 22f;
                btnLayout.preferredHeight = 22f;
                btnLayout.flexibleWidth = 0f;

                var btnImage = btnGO.AddComponent<Image>();
                btnImage.color = new Color(0.35f, 0.35f, 0.38f, 1f);
                btnImage.raycastTarget = true;

                var textGO = new GameObject("Text");
                textGO.transform.SetParent(btnGO.transform, false);
                var textRect = textGO.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                var label = textGO.AddComponent<TextMeshProUGUI>();
                label.text = "\u2715";
                label.fontSize = 14f;
                label.color = new Color(0.85f, 0.85f, 0.85f, 1f);
                label.alignment = TextAlignmentOptions.Center;
                label.raycastTarget = false;

                var btn = btnGO.AddComponent<Button>();
                btn.targetGraphic = btnImage;
                var colors = btn.colors;
                colors.normalColor = new Color(0.35f, 0.35f, 0.38f, 1f);
                colors.highlightedColor = new Color(0.5f, 0.5f, 0.55f, 1f);
                colors.pressedColor = new Color(0.6f, 0.6f, 0.65f, 1f);
                btn.colors = colors;
                btn.onClick.AddListener(ClearFilter);

                _clearButton = btnGO;
            }

            private static KInputTextField TryGetTreeFilterableInputField()
            {
                // 1. Find existing TreeFilterableSideScreen instance (may be inactive from previous selection)
                var instance = UnityEngine.Object.FindObjectOfType<TreeFilterableSideScreen>(true);
                if (instance != null && _treeFilterableInputFieldField != null)
                {
                    var input = _treeFilterableInputFieldField.GetValue(instance) as KInputTextField;
                    if (input != null) return input;
                }
                // 2. Get prefab from DetailsScreen sideScreens
                if (DetailsScreen.Instance != null && _detailsScreenSideScreensField != null)
                {
                    var sideScreens = _detailsScreenSideScreensField.GetValue(DetailsScreen.Instance) as IList;
                    if (sideScreens != null && sideScreens.Count > 0)
                    {
                        var screenPrefabField = AccessTools.Field(sideScreens[0].GetType(), "screenPrefab");
                        foreach (var refObj in sideScreens)
                        {
                            var screenPrefab = screenPrefabField?.GetValue(refObj) as SideScreenContent;
                            if (screenPrefab != null && _treeFilterableSideScreenType != null && _treeFilterableSideScreenType.IsInstanceOfType(screenPrefab))
                            {
                                var input = _treeFilterableInputFieldField?.GetValue(screenPrefab) as KInputTextField;
                                if (input != null) return input;
                            }
                        }
                    }
                }
                return null;
            }

            private void EnsureFilterUI_Fallback(Transform parent)
            {
                var filterGO = new GameObject("ChannelFilterInput");
                filterGO.transform.SetParent(parent, false);

                var rectTransform = filterGO.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(0f, 24f);

                var layout = filterGO.AddComponent<LayoutElement>();
                layout.preferredHeight = 24f;
                layout.flexibleWidth = 1f;

                var image = filterGO.AddComponent<Image>();
                var bgSprite = Assets.GetSprite("white");
                if (bgSprite != null) image.sprite = bgSprite;
                image.color = new Color(0.28f, 0.28f, 0.30f, 1f);
                image.raycastTarget = true;

                var outline = filterGO.AddComponent<Outline>();
                outline.effectColor = new Color(0.45f, 0.45f, 0.48f, 1f);
                outline.effectDistance = new Vector2(1f, 1f);

                var textAreaGO = new GameObject("TextArea");
                textAreaGO.transform.SetParent(filterGO.transform, false);
                var textAreaRect = textAreaGO.AddComponent<RectTransform>();
                textAreaRect.anchorMin = Vector2.zero;
                textAreaRect.anchorMax = Vector2.one;
                textAreaRect.offsetMin = new Vector2(6f, 3f);
                textAreaRect.offsetMax = new Vector2(-6f, -3f);
                textAreaGO.AddComponent<RectMask2D>();

                var textGO = new GameObject("Text");
                textGO.transform.SetParent(textAreaGO.transform, false);
                var textRect = textGO.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                var textComponent = textGO.AddComponent<TextMeshProUGUI>();
                textComponent.fontSize = 12f;
                textComponent.color = Color.white;
                textComponent.alignment = TextAlignmentOptions.MidlineLeft;

                var placeholderGO = new GameObject("Placeholder");
                placeholderGO.transform.SetParent(textAreaGO.transform, false);
                var placeholderRect = placeholderGO.AddComponent<RectTransform>();
                placeholderRect.anchorMin = Vector2.zero;
                placeholderRect.anchorMax = Vector2.one;
                placeholderRect.offsetMin = Vector2.zero;
                placeholderRect.offsetMax = Vector2.zero;
                var placeholderText = placeholderGO.AddComponent<TextMeshProUGUI>();
                placeholderText.text = "Filter channels...";
                placeholderText.fontSize = 12f;
                placeholderText.color = new Color(0.65f, 0.65f, 0.65f, 1f);
                placeholderText.alignment = TextAlignmentOptions.MidlineLeft;

                _fallbackInput = filterGO.AddComponent<TMP_InputField>();
                _fallbackInput.textViewport = textAreaRect;
                _fallbackInput.textComponent = textComponent;
                _fallbackInput.placeholder = placeholderText;
                _fallbackInput.contentType = TMP_InputField.ContentType.Standard;
                _fallbackInput.characterLimit = 64;
                _fallbackInput.caretColor = Color.white;
                _fallbackInput.caretWidth = 1;
                _fallbackInput.selectionColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
                _fallbackInput.onValueChanged.AddListener(OnFilterChanged);
                _fallbackInput.onSelect.AddListener(_ => SetParentEditing(true));
                _fallbackInput.onDeselect.AddListener(_ => SetParentEditing(false));
            }

            private void ClearFilter()
            {
                if (_kInputField != null) _kInputField.text = "";
                else if (_fallbackInput != null) _fallbackInput.text = "";
                _filterText = "";
                ApplyFilter();
            }

            private void OnFilterChanged(string text)
            {
                _filterText = text ?? "";
                ApplyFilter();
            }

            private string GetFilterText()
            {
                if (_kInputField != null) return _kInputField.text ?? "";
                if (_fallbackInput != null) return _fallbackInput.text ?? "";
                return _filterText;
            }

            private void SetParentEditing(bool editing)
            {
                var kscreen = GetComponent<KScreen>();
                if (kscreen != null)
                {
                    if (_kscreenIsEditingField != null)
                        _kscreenIsEditingField.SetValue(kscreen, editing);
                    else if (_kscreenIsEditingProperty != null)
                        _kscreenIsEditingProperty.SetValue(kscreen, editing, null);
                    if (KScreenManager.Instance != null)
                        KScreenManager.Instance.RefreshStack();
                }
            }

            public void ApplyFilter()
            {
                if (_sideScreen == null || _rowsField == null || _channelNameField == null) return;

                var rows = _rowsField.GetValue(_sideScreen);
                if (rows == null) return;

                var filter = GetFilterText()?.Trim() ?? "";
                var hasFilter = filter.Length > 0;

                if (rows is IDictionary dict)
                {
                    foreach (DictionaryEntry kvp in dict)
                    {
                        var channel = kvp.Key;
                        var row = kvp.Value as GameObject;
                        if (row == null) continue;

                        bool visible = true;
                        if (hasFilter && channel != null)
                        {
                            var name = _channelNameField.GetValue(channel) as string ?? "";
                            visible = name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
                        }
                        row.SetActive(visible);
                    }
                }
            }
        }
    }
}
