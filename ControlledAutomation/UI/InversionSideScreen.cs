using UnityEngine;
using PeterHan.PLib.UI;
using ControlledAutomation.Components;

namespace ControlledAutomation.UI
{
    public class InversionSideScreen : SideScreenContent
    {
        private GameObject checkbox;
        private SensorInverter target;

        public override void OnPrefabInit()
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

            PCheckBox checkboxField = new PCheckBox("InvertCheckbox")
            {
                Text = STRINGS.CONTROLLEDAUTOMATION.INVERT_CHECKBOX,
                ToolTip = STRINGS.CONTROLLEDAUTOMATION.INVERT_CHECKBOX_TOOLTIP,
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
            return target.GetComponent<SensorInverter>() != null;
        }

        public override void SetTarget(GameObject new_target)
        {
            if (new_target == null)
                return;

            target = new_target.GetComponent<SensorInverter>();
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
        }

        public override string GetTitle() => STRINGS.CONTROLLEDAUTOMATION.SIDESCREEN_TITLE;
    }
}
