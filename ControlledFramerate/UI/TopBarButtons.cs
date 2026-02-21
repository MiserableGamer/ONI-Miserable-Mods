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
        private static MultiToggle benchmarkButton;
        private static MultiToggle adaptiveButton;
        private static ToolTip benchmarkTooltip;
        private static ToolTip adaptiveTooltip;

        private static GameObject benchmarkGO;
        private static GameObject adaptiveGO;

        public static void CreateButtons(TopLeftControlScreen instance)
        {
            try
            {
                if (benchmarkGO != null && benchmarkGO)
                    return;

                DestroyButtons();

                var templateGO = instance.sandboxToggle.gameObject;
                var parentGO = instance.sandboxToggle.transform.parent.gameObject;

                benchmarkGO = Util.KInstantiateUI(templateGO, parentGO, true);
                var benchTransform = benchmarkGO.transform;

                SetButtonIcon(benchTransform, SpriteHelper.Load("icon_benchmark"));
                benchmarkGO.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 40f);
                benchmarkGO.TryGetComponent(out benchmarkButton);
                benchmarkGO.TryGetComponent(out benchmarkTooltip);
                if (benchmarkTooltip != null)
                    benchmarkTooltip.SetSimpleTooltip(ControlledFramerateStrings.BenchmarkTooltip);

                benchTransform.SetSiblingIndex(0);

                benchmarkButton.onClick = (System.Action)Delegate.Combine(benchmarkButton.onClick, new System.Action(OnBenchmarkClicked));

                adaptiveGO = Util.KInstantiateUI(templateGO, parentGO, true);
                var adaptTransform = adaptiveGO.transform;

                SetButtonIcon(adaptTransform, SpriteHelper.Load("icon_adaptive"));
                adaptiveGO.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 40f);
                adaptiveGO.TryGetComponent(out adaptiveButton);
                adaptiveGO.TryGetComponent(out adaptiveTooltip);

                adaptTransform.SetSiblingIndex(1);

                adaptiveButton.onClick = (System.Action)Delegate.Combine(adaptiveButton.onClick, new System.Action(OnAdaptiveClicked));

                RefreshButtonStates();
            }
            catch (Exception ex)
            {
                ControlledFramerateMod.Log($"Failed to create top bar buttons: {ex}");
            }
        }

        public static void DestroyButtons()
        {
            if (benchmarkGO != null) { UnityEngine.Object.Destroy(benchmarkGO); benchmarkGO = null; }
            if (adaptiveGO != null) { UnityEngine.Object.Destroy(adaptiveGO); adaptiveGO = null; }
            benchmarkButton = null;
            adaptiveButton = null;
            benchmarkTooltip = null;
            adaptiveTooltip = null;
        }

        private static void OnBenchmarkClicked()
        {
            KMonoBehaviour.PlaySound(GlobalAssets.GetSound("HUD_Click"));

            if (BenchmarkEngine.IsRunning)
            {
                BenchmarkEngine.Cancel();
            }
            else
            {
                BenchmarkOverlay.ShowConfig();
            }

            RefreshButtonStates();
        }

        private static void OnAdaptiveClicked()
        {
            if (!SpeedStateManager.HasBenchmarkData) return;

            KMonoBehaviour.PlaySound(GlobalAssets.GetSound("HUD_Click"));
            SpeedStateManager.ToggleAdaptive();
            RefreshButtonStates();
            UpdateSpeedTooltips();
        }

        public static void RefreshButtonStates()
        {
            if (benchmarkButton != null)
            {
                benchmarkButton.ChangeState(BenchmarkEngine.IsRunning ? 2 : 1);
            }

            if (adaptiveButton != null)
            {
                if (!SpeedStateManager.HasBenchmarkData)
                {
                    adaptiveButton.ChangeState(0);
                    if (adaptiveTooltip != null)
                        adaptiveTooltip.SetSimpleTooltip(ControlledFramerateStrings.AdaptiveTooltipDisabled);
                }
                else if (SpeedStateManager.CurrentMode == SpeedStateManager.SpeedMode.Adaptive)
                {
                    adaptiveButton.ChangeState(2);
                    if (adaptiveTooltip != null)
                        adaptiveTooltip.SetSimpleTooltip(ControlledFramerateStrings.AdaptiveTooltipOn);
                }
                else
                {
                    adaptiveButton.ChangeState(1);
                    if (adaptiveTooltip != null)
                        adaptiveTooltip.SetSimpleTooltip(ControlledFramerateStrings.AdaptiveTooltipOff);
                }
            }

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
