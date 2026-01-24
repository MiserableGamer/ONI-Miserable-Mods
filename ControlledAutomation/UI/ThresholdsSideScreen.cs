using HarmonyLib;
using UnityEngine;
using System.Reflection;
using PeterHan.PLib.UI;
using STRINGS;
using ControlledAutomation.Components;

namespace ControlledAutomation.UI
{
    /// <summary>
    /// Sidescreen that shows the "Send Green Signal When Low" checkbox for storage buildings
    /// with thresholds. The actual threshold sliders are handled by the game's ActiveRangeSideScreen.
    /// This sidescreen should appear directly below the ActiveRangeSideScreen.
    /// </summary>
    public class ThresholdsSideScreen : SideScreenContent
    {
        private GameObject checkbox;
        private ThresholdsBase target;

        protected override void OnPrefabInit()
        {
            var margin = new RectOffset(4, 4, 4, 4);
            var baseLayout = gameObject.GetComponent<BoxLayoutGroup>();
            if (baseLayout != null)
            {
                baseLayout.Params = new BoxLayoutParams()
                {
                    Alignment = TextAnchor.MiddleLeft,
                    Margin = margin,
                };
            }

            PPanel panel = new PPanel("MainPanel")
            {
                Direction = PanelDirection.Horizontal,
                Margin = margin,
                Spacing = 4,
                FlexSize = Vector2.right
            };

            PCheckBox checkboxField = new PCheckBox("InvertStorageCheckbox")
            {
                Text = CONTROLLEDAUTOMATION.INVERT_CHECKBOX_STORAGE,
                ToolTip = CONTROLLEDAUTOMATION.INVERT_CHECKBOX_STORAGE_TOOLTIP,
                OnChecked = OnCheck,
                TextStyle = PUITuning.Fonts.TextDarkStyle
            };
            checkboxField.AddOnRealize((obj) => checkbox = obj);
            panel.AddChild(checkboxField);
            panel.AddTo(gameObject);
            ContentContainer = gameObject;
            base.OnPrefabInit();
            UpdateState();
        }

        public override bool IsValidForTarget(GameObject target)
        {
            return target.GetComponent<StorageThresholds>() != null
                || target.GetComponent<RefrigeratorThresholds>() != null;
        }

        public override void SetTarget(GameObject new_target)
        {
            if (new_target == null)
            {
                Debug.LogError("[ControlledAutomation] Invalid gameObject received");
                return;
            }

            target = new_target.GetComponent<StorageThresholds>();
            if (target == null)
                target = new_target.GetComponent<RefrigeratorThresholds>();

            if (target == null)
            {
                Debug.LogError("[ControlledAutomation] The gameObject received does not contain a ThresholdsBase component");
                return;
            }
            UpdateState();
        }

        public void UpdateState()
        {
            if (target == null || checkbox == null)
                return;
            PCheckBox.SetCheckState(checkbox, target.InvertSignal
                ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);
        }

        public void OnCheck(GameObject source, int state)
        {
            int newState = state == PCheckBox.STATE_CHECKED ? PCheckBox.STATE_UNCHECKED : PCheckBox.STATE_CHECKED;
            PCheckBox.SetCheckState(checkbox, newState);
            KFMOD.PlayUISound(WidgetSoundPlayer.getSoundPath(ToggleSoundPlayer.default_values[state]));
            target.InvertSignal = (newState == PCheckBox.STATE_CHECKED);
            UpdateActiveRangeSideScreenTooltips();
        }

        private static readonly MethodInfo refreshTooltipsMethod
            = AccessTools.Method(typeof(ActiveRangeSideScreen), "RefreshTooltips");

        private void UpdateActiveRangeSideScreenTooltips()
        {
            GameObject parent = PUIUtils.GetParent(gameObject);
            if (parent == null)
                return;

            // The game object is called 'Activation...' and not 'Active...'.
            Transform transform = parent.transform.Find("ActivationRangeSideScreen");
            if (transform == null)
                return;

            ActiveRangeSideScreen screen = transform.gameObject.GetComponent<ActiveRangeSideScreen>();
            if (screen == null)
                return;

            refreshTooltipsMethod?.Invoke(screen, null);
        }

        public override string GetTitle()
        {
            return ""; // No title needed since this goes below the threshold sliders
        }
    }
}
