using UnityEngine;
using PeterHan.PLib.UI;
using ControlledAutomation.Components;

namespace ControlledAutomation.UI
{
    public class RocketPlatformSideScreen : SideScreenContent
    {
        private GameObject checkbox1;
        private GameObject checkbox2;
        private RocketPlatformInverter target;

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

            PPanel mainPanel = new PPanel("MainPanel")
            {
                Direction = PanelDirection.Vertical,
                Margin = margin,
                Spacing = 4,
                FlexSize = Vector2.right
            };

            PCheckBox checkboxField1 = new PCheckBox("InvertOutput1Checkbox")
            {
                Text = STRINGS.CONTROLLEDAUTOMATION.ROCKET_INVERT_OUTPUT_1,
                ToolTip = STRINGS.CONTROLLEDAUTOMATION.ROCKET_INVERT_OUTPUT_1_TOOLTIP,
                OnChecked = OnCheck1,
                TextStyle = PUITuning.Fonts.TextDarkStyle
            };
            checkboxField1.AddOnRealize((obj) => checkbox1 = obj);
            mainPanel.AddChild(checkboxField1);

            PCheckBox checkboxField2 = new PCheckBox("InvertOutput2Checkbox")
            {
                Text = STRINGS.CONTROLLEDAUTOMATION.ROCKET_INVERT_OUTPUT_2,
                ToolTip = STRINGS.CONTROLLEDAUTOMATION.ROCKET_INVERT_OUTPUT_2_TOOLTIP,
                OnChecked = OnCheck2,
                TextStyle = PUITuning.Fonts.TextDarkStyle
            };
            checkboxField2.AddOnRealize((obj) => checkbox2 = obj);
            mainPanel.AddChild(checkboxField2);

            mainPanel.AddTo(gameObject);
            ContentContainer = gameObject;
            base.OnPrefabInit();
            UpdateState();
        }

        public override bool IsValidForTarget(GameObject target) =>
            target.GetComponent<RocketPlatformInverter>() != null;

        public override void SetTarget(GameObject new_target)
        {
            if (new_target == null) return;
            target = new_target.GetComponent<RocketPlatformInverter>();
            if (target != null) UpdateState();
        }

        public void UpdateState()
        {
            if (target == null) return;

            if (checkbox1 != null)
                PCheckBox.SetCheckState(checkbox1, target.InvertOutput1 ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);

            if (checkbox2 != null)
                PCheckBox.SetCheckState(checkbox2, target.InvertOutput2 ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);
        }

        public void OnCheck1(GameObject source, int state)
        {
            int newState = state == PCheckBox.STATE_CHECKED ? PCheckBox.STATE_UNCHECKED : PCheckBox.STATE_CHECKED;
            PCheckBox.SetCheckState(checkbox1, newState);
            PlaySound(GlobalAssets.GetSound("HUD_Click"));
            target.InvertOutput1 = (newState == PCheckBox.STATE_CHECKED);
        }

        public void OnCheck2(GameObject source, int state)
        {
            int newState = state == PCheckBox.STATE_CHECKED ? PCheckBox.STATE_UNCHECKED : PCheckBox.STATE_CHECKED;
            PCheckBox.SetCheckState(checkbox2, newState);
            PlaySound(GlobalAssets.GetSound("HUD_Click"));
            target.InvertOutput2 = (newState == PCheckBox.STATE_CHECKED);
        }

        public override string GetTitle() => STRINGS.CONTROLLEDAUTOMATION.SIDESCREEN_TITLE;
    }
}
