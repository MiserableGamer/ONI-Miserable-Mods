using HarmonyLib;
using UnityEngine;
using PeterHan.PLib.UI;
using STRINGS;
using ControlledAutomation.Components;

namespace ControlledAutomation.UI
{
    /// <summary>
    /// Sidescreen for Rocket Platform with two independent inversion checkboxes.
    /// </summary>
    public class RocketPlatformSideScreen : SideScreenContent
    {
        private GameObject checkbox1;
        private GameObject checkbox2;
        private RocketPlatformInverter target;

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

            PPanel mainPanel = new PPanel("RocketInversionPanel")
            {
                Direction = PanelDirection.Vertical,
                Margin = margin,
                Spacing = 4,
                FlexSize = Vector2.right
            };

            // Checkbox for Output 1 (Rocket Present)
            PCheckBox checkbox1Field = new PCheckBox("InvertOutput1Checkbox")
            {
                Text = CONTROLLEDAUTOMATION.ROCKET_INVERT_OUTPUT_1,
                ToolTip = CONTROLLEDAUTOMATION.ROCKET_INVERT_OUTPUT_1_TOOLTIP,
                OnChecked = OnCheck1,
                TextStyle = PUITuning.Fonts.TextDarkStyle
            };
            checkbox1Field.AddOnRealize((obj) => checkbox1 = obj);
            mainPanel.AddChild(checkbox1Field);

            // Checkbox for Output 2 (Rocket Ready)
            PCheckBox checkbox2Field = new PCheckBox("InvertOutput2Checkbox")
            {
                Text = CONTROLLEDAUTOMATION.ROCKET_INVERT_OUTPUT_2,
                ToolTip = CONTROLLEDAUTOMATION.ROCKET_INVERT_OUTPUT_2_TOOLTIP,
                OnChecked = OnCheck2,
                TextStyle = PUITuning.Fonts.TextDarkStyle
            };
            checkbox2Field.AddOnRealize((obj) => checkbox2 = obj);
            mainPanel.AddChild(checkbox2Field);

            mainPanel.AddTo(gameObject);
            ContentContainer = gameObject;
            base.OnPrefabInit();
            UpdateState();
        }

        public override bool IsValidForTarget(GameObject target)
        {
            return target.GetComponent<RocketPlatformInverter>() != null;
        }

        public override void SetTarget(GameObject new_target)
        {
            if (new_target == null)
            {
                Debug.LogError("[ControlledAutomation] Invalid gameObject received");
                return;
            }
            target = new_target.GetComponent<RocketPlatformInverter>();
            if (target == null)
            {
                Debug.LogError("[ControlledAutomation] The gameObject does not contain a RocketPlatformInverter component");
                return;
            }
            UpdateState();
        }

        public void UpdateState()
        {
            if (target == null)
                return;

            if (checkbox1 != null)
            {
                PCheckBox.SetCheckState(checkbox1, target.InvertOutput1
                    ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);
            }

            if (checkbox2 != null)
            {
                PCheckBox.SetCheckState(checkbox2, target.InvertOutput2
                    ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);
            }
        }

        public void OnCheck1(GameObject source, int state)
        {
            int newState = state == PCheckBox.STATE_CHECKED ? PCheckBox.STATE_UNCHECKED : PCheckBox.STATE_CHECKED;
            PCheckBox.SetCheckState(checkbox1, newState);
            KFMOD.PlayUISound(WidgetSoundPlayer.getSoundPath(ToggleSoundPlayer.default_values[state]));
            target.InvertOutput1 = (newState == PCheckBox.STATE_CHECKED);
        }

        public void OnCheck2(GameObject source, int state)
        {
            int newState = state == PCheckBox.STATE_CHECKED ? PCheckBox.STATE_UNCHECKED : PCheckBox.STATE_CHECKED;
            PCheckBox.SetCheckState(checkbox2, newState);
            KFMOD.PlayUISound(WidgetSoundPlayer.getSoundPath(ToggleSoundPlayer.default_values[state]));
            target.InvertOutput2 = (newState == PCheckBox.STATE_CHECKED);
        }

        public override string GetTitle()
        {
            return CONTROLLEDAUTOMATION.SIDESCREEN_TITLE;
        }
    }
}
