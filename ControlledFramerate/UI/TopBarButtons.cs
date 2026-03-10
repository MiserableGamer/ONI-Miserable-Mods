using System;
using ControlledFramerate.Core;
using ControlledFramerate.Options;
using ControlledFramerate.Strings;
using UnityEngine;
using UnityEngine.UI;

namespace ControlledFramerate.UI
{
    public static class TopBarButtons
    {
        private static MultiToggle menuButton;
        private static ToolTip menuTooltip;
        private static GameObject menuGO;

        public static void CreateButtons(TopLeftControlScreen instance)
        {
            try
            {
                if (menuGO != null && menuGO)
                    return;

                DestroyButtons();

                var templateGO = instance.sandboxToggle.gameObject;
                var parentGO = instance.sandboxToggle.transform.parent.gameObject;

                menuGO = Util.KInstantiateUI(templateGO, parentGO, true);
                var menuTransform = menuGO.transform;

                SetButtonIcon(menuTransform, SpriteHelper.Load("icon_benchmark"));
                menuGO.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 32f);
                menuGO.TryGetComponent(out menuButton);
                menuGO.TryGetComponent(out menuTooltip);
                if (menuTooltip != null)
                    menuTooltip.SetSimpleTooltip(ControlledFramerateStrings.MenuTooltip);

                menuTransform.SetAsLastSibling();

                menuButton.onClick = (System.Action)Delegate.Combine(menuButton.onClick, new System.Action(OnMenuClicked));

                RefreshButtonStates();
            }
            catch (Exception ex)
            {
                ControlledFramerateMod.Log($"Failed to create top bar buttons: {ex}");
            }
        }

        public static void DestroyButtons()
        {
            ToolbarPopupMenu.Hide();
            if (menuGO != null) { UnityEngine.Object.Destroy(menuGO); menuGO = null; }
            menuButton = null;
            menuTooltip = null;
        }

        private static void OnMenuClicked()
        {
            var rt = menuGO != null ? menuGO.GetComponent<RectTransform>() : null;
            if (rt != null)
                ToolbarPopupMenu.Toggle(rt);
            RefreshButtonStates();
        }

        public static void RefreshButtonStates()
        {
            if (menuButton != null)
                menuButton.ChangeState(ToolbarPopupMenu.IsOpen ? 2 : 1);

            UpdateSpeedTooltips();
        }

        public static void UpdateSpeedTooltips()
        {
            try
            {
                var scs = SpeedControlScreen.Instance;
                if (scs == null) return;

                var opts = ControlledFramerateOptions.Instance;
                bool adaptive = SpeedStateManager.CurrentMode == SpeedStateManager.SpeedMode.Adaptive;
                string suffix = adaptive ? " [Adaptive]" : "";

                SetSpeedTooltip(scs.speedButtonWidget_slow,
                    $"Speed 1: {opts.SlowSpeed:F1}x{suffix}",
                    scs.TooltipTextStyle, global::Action.CycleSpeed);

                SetSpeedTooltip(scs.speedButtonWidget_medium,
                    $"Speed 2: {opts.MediumSpeed:F1}x{suffix}",
                    scs.TooltipTextStyle, global::Action.CycleSpeed);

                SetSpeedTooltip(scs.speedButtonWidget_fast,
                    $"Speed 3: {opts.FastSpeed:F1}x{suffix}",
                    scs.TooltipTextStyle, global::Action.CycleSpeed);
            }
            catch (Exception ex)
            {
                ControlledFramerateMod.LogOnce("SpeedTooltips", $"Error updating speed tooltips: {ex}");
            }
        }

        private static void SetButtonIcon(Transform buttonTransform, Sprite icon)
        {
            var fg = buttonTransform.Find("FG");
            if (fg != null && fg.TryGetComponent<Image>(out var fgImage))
                fgImage.sprite = icon;

            var label = buttonTransform.Find("Label");
            if (label != null)
                label.gameObject.SetActive(false);
        }

        private static void SetSpeedTooltip(GameObject widget, string text, TextStyleSetting style, global::Action hotkey)
        {
            if (widget == null) return;
            var tt = widget.GetComponent<ToolTip>();
            if (tt == null) return;
            tt.ClearMultiStringTooltip();
            tt.AddMultiStringTooltip(
                GameUtil.ReplaceHotkeyString(text, hotkey), style);
        }
    }
}
