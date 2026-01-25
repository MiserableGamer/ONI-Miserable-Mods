using HarmonyLib;
using UnityEngine;
using System.Reflection;
using PeterHan.PLib.UI;
using ControlledAutomation.Components;

namespace ControlledAutomation.UI
{
    // Shows "Send Green Signal When Low" checkbox below the threshold sliders
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
                Text = STRINGS.CONTROLLEDAUTOMATION.INVERT_CHECKBOX_STORAGE,
                ToolTip = STRINGS.CONTROLLEDAUTOMATION.INVERT_CHECKBOX_STORAGE_TOOLTIP,
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
                return;

            target = new_target.GetComponent<StorageThresholds>();
            if (target == null)
                target = new_target.GetComponent<RefrigeratorThresholds>();

            if (target != null)
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
            PlaySound(GlobalAssets.GetSound("HUD_Click"));
            target.InvertSignal = (newState == PCheckBox.STATE_CHECKED);
            UpdateActiveRangeSideScreenTooltips();
        }

        private static readonly MethodInfo refreshTooltipsMethod =
            AccessTools.Method(typeof(ActiveRangeSideScreen), "RefreshTooltips");

        private void UpdateActiveRangeSideScreenTooltips()
        {
            GameObject parent = PUIUtils.GetParent(gameObject);
            if (parent == null)
                return;

            Transform transform = parent.transform.Find("ActivationRangeSideScreen");
            if (transform == null)
                return;

            ActiveRangeSideScreen screen = transform.gameObject.GetComponent<ActiveRangeSideScreen>();
            if (screen != null)
                refreshTooltipsMethod?.Invoke(screen, null);
        }

        public override string GetTitle() => "";
    }
}
