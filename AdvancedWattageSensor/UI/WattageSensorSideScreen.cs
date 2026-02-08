using AdvancedWattageSensor.Components;
using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;

namespace AdvancedWattageSensor.UI
{
    public class WattageSensorSideScreen : SideScreenContent
    {
        private AdvancedWattageSensorComponent target;
        private TMP_InputField labelInputField;
        private GameObject warningThresholdCheckbox;
        private GameObject sendGreenCheckbox;

        public override bool IsValidForTarget(GameObject target) =>
            target.GetComponent<AdvancedWattageSensorComponent>() != null;

        public override void SetTarget(GameObject target)
        {
            this.target = target?.GetComponent<AdvancedWattageSensorComponent>();
            RefreshUI();
        }

        public override void OnShow(bool show)
        {
            base.OnShow(show);
            if (show)
                RefreshUI();
        }

        public override void OnPrefabInit()
        {
            var mainPanel = new PPanel("WattageSensorLabelPanel")
            {
                Direction = PanelDirection.Vertical,
                Spacing = 4,
                FlexSize = Vector2.right
            };

            mainPanel.AddChild(new PLabel("LabelHeader")
            {
                Text = "Sensor Label",
                TextStyle = PUITuning.Fonts.TextDarkStyle
            });

            var textField = new PTextField("SensorLabelInput")
            {
                Text = "",
                MinWidth = 160,
                MaxLength = 16,
                Type = PTextField.FieldType.Text,
                OnTextChanged = OnLabelChanged,
                TextStyle = PUITuning.Fonts.TextDarkStyle,
                ToolTip = "Enter a name to show this sensor on the wattage monitor"
            };
            textField.AddOnRealize(obj =>
            {
                labelInputField = obj.GetComponentInChildren<TMP_InputField>();
            });
            mainPanel.AddChild(textField);

            mainPanel.AddChild(new PLabel("LabelHint")
            {
                Text = "Named sensors appear on the wattage monitor",
                TextStyle = PUITuning.Fonts.TextLightStyle
            });

            // Spacer
            mainPanel.AddChild(new PSpacer() { PreferredSize = new Vector2(0f, 8f) });

            // Warning threshold checkbox
            var warningCheck = new PCheckBox("UseWarningThreshold")
            {
                Text = "Use warning % as trigger",
                InitialState = PCheckBox.STATE_UNCHECKED,
                OnChecked = OnWarningThresholdChanged,
                TextStyle = PUITuning.Fonts.TextDarkStyle,
                ToolTip = "When enabled, the automation signal triggers at the warning percentage below the threshold (set in mod options) instead of the exact threshold value"
            };
            warningCheck.AddOnRealize(obj => { warningThresholdCheckbox = obj; });
            mainPanel.AddChild(warningCheck);

            // Signal polarity checkbox
            var greenCheck = new PCheckBox("SendGreenOnWarning")
            {
                Text = "Send Green when triggered",
                InitialState = PCheckBox.STATE_UNCHECKED,
                OnChecked = OnSendGreenChanged,
                TextStyle = PUITuning.Fonts.TextDarkStyle,
                ToolTip = "When checked, sends Green (active) when wattage crosses the warning threshold.\nWhen unchecked, sends Red (inactive) instead."
            };
            greenCheck.AddOnRealize(obj => { sendGreenCheckbox = obj; });
            mainPanel.AddChild(greenCheck);

            mainPanel.AddTo(gameObject);
            ContentContainer = gameObject;
            base.OnPrefabInit();
        }

        private void OnLabelChanged(GameObject source, string text)
        {
            if (target != null)
                target.SetLabel(text);
        }

        private void OnWarningThresholdChanged(GameObject source, int state)
        {
            if (target == null) return;
            // Toggle: unchecked -> checked, checked -> unchecked
            bool newValue = state == PCheckBox.STATE_UNCHECKED;
            target.useWarningThreshold = newValue;
            PCheckBox.SetCheckState(source, newValue ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);
        }

        private void OnSendGreenChanged(GameObject source, int state)
        {
            if (target == null) return;
            bool newValue = state == PCheckBox.STATE_UNCHECKED;
            target.sendGreenOnWarning = newValue;
            PCheckBox.SetCheckState(source, newValue ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);
        }

        private void RefreshUI()
        {
            if (target == null)
                return;

            if (labelInputField != null)
                labelInputField.text = target.sensorLabel ?? "";

            if (warningThresholdCheckbox != null)
                PCheckBox.SetCheckState(warningThresholdCheckbox,
                    target.useWarningThreshold ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);

            if (sendGreenCheckbox != null)
                PCheckBox.SetCheckState(sendGreenCheckbox,
                    target.sendGreenOnWarning ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);
        }
    }
}
