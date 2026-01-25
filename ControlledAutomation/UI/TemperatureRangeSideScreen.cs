using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PeterHan.PLib.UI;
using ControlledAutomation.Components;

namespace ControlledAutomation.UI
{
    public class TemperatureRangeSideScreen : SideScreenContent, IRender200ms
    {
        private TemperatureRangeSensor target;

        private GameObject currentTempLabelGO;
        private TMP_InputField centerInputField;
        private KSlider centerTempSlider;
        private TMP_InputField belowInputField;
        private KSlider rangeBelowSlider;
        private TMP_InputField aboveInputField;
        private KSlider rangeAboveSlider;
        private GameObject activeLabelGO;
        private GameObject invertCheckboxGO;

        private const float MinTempK = 1f;
        private const float MaxTempK = 1273.15f;
        private const float MaxRangeK = 500f;

        private bool needsFullRefresh = false;

        public override bool IsValidForTarget(GameObject target) =>
            target.GetComponent<TemperatureRangeSensor>() != null;

        public override void SetTarget(GameObject target)
        {
            this.target = target?.GetComponent<TemperatureRangeSensor>();
            needsFullRefresh = true;
            RefreshUI();
        }

        protected override void OnShow(bool show)
        {
            base.OnShow(show);
            if (show)
                needsFullRefresh = true;
        }

        public void Render200ms(float dt)
        {
            if (target == null) return;

            if (needsFullRefresh)
            {
                RefreshUI();
                needsFullRefresh = false;
            }
            else
            {
                RefreshCurrentTemperature();
            }
        }

        protected override void OnPrefabInit()
        {
            var margin = new RectOffset(8, 8, 8, 8);
            var baseLayout = gameObject.GetComponent<BoxLayoutGroup>();
            if (baseLayout != null)
            {
                baseLayout.Params = new BoxLayoutParams()
                {
                    Alignment = TextAnchor.MiddleCenter,
                    Margin = margin,
                };
            }

            PPanel mainPanel = new PPanel("TemperatureRangePanel")
            {
                Direction = PanelDirection.Vertical,
                Margin = margin,
                Spacing = 6,
                FlexSize = Vector2.right
            };

            // Current temp display
            var currentTempLabel = new PLabel("CurrentTempLabel")
            {
                Text = "Current Ambient Temperature:\n---",
                TextStyle = PUITuning.Fonts.TextDarkStyle
            };
            currentTempLabel.AddOnRealize((obj) => currentTempLabelGO = obj);
            mainPanel.AddChild(currentTempLabel);

            // Center temperature
            mainPanel.AddChild(new PLabel("CenterTitle") { Text = "Center Temperature", TextStyle = PUITuning.Fonts.TextDarkStyle });
            mainPanel.AddChild(CreateInputRowWithButtons("Center",
                () => target?.centerTemperature ?? 293.15f,
                (v) => { target?.SetCenterTemperature(v); RefreshUI(); },
                MinTempK, MaxTempK, 1f, 10f, false,
                out centerInputField));

            var centerSlider = new PSliderSingle("CenterSlider")
            {
                MinValue = MinTempK, MaxValue = MaxTempK, InitialValue = 293.15f,
                OnValueChanged = (src, val) => { target?.SetCenterTemperature(val); RefreshUI(); },
                FlexSize = Vector2.right
            };
            centerSlider.AddOnRealize((obj) => centerTempSlider = obj.GetComponentInChildren<KSlider>());
            mainPanel.AddChild(centerSlider);

            // Degrees below
            mainPanel.AddChild(new PLabel("BelowTitle") { Text = "Range Below (±)", TextStyle = PUITuning.Fonts.TextDarkStyle });
            mainPanel.AddChild(CreateInputRowWithButtons("Below",
                () => target?.degreesBelow ?? 10f,
                (v) => { target?.SetDegreesBelow(v); RefreshUI(); },
                0f, MaxRangeK, 1f, 10f, true,
                out belowInputField));

            var belowSlider = new PSliderSingle("BelowSlider")
            {
                MinValue = 0f, MaxValue = MaxRangeK, InitialValue = 10f,
                OnValueChanged = (src, val) => { target?.SetDegreesBelow(val); RefreshUI(); },
                FlexSize = Vector2.right
            };
            belowSlider.AddOnRealize((obj) => rangeBelowSlider = obj.GetComponentInChildren<KSlider>());
            mainPanel.AddChild(belowSlider);

            // Degrees above
            mainPanel.AddChild(new PLabel("AboveTitle") { Text = "Range Above (±)", TextStyle = PUITuning.Fonts.TextDarkStyle });
            mainPanel.AddChild(CreateInputRowWithButtons("Above",
                () => target?.degreesAbove ?? 10f,
                (v) => { target?.SetDegreesAbove(v); RefreshUI(); },
                0f, MaxRangeK, 1f, 10f, true,
                out aboveInputField));

            var aboveSlider = new PSliderSingle("AboveSlider")
            {
                MinValue = 0f, MaxValue = MaxRangeK, InitialValue = 10f,
                OnValueChanged = (src, val) => { target?.SetDegreesAbove(val); RefreshUI(); },
                FlexSize = Vector2.right
            };
            aboveSlider.AddOnRealize((obj) => rangeAboveSlider = obj.GetComponentInChildren<KSlider>());
            mainPanel.AddChild(aboveSlider);

            // Active range display
            var activeLabel = new PLabel("ActiveRange")
            {
                Text = "Active Range: --- to ---",
                TextStyle = PUITuning.Fonts.TextDarkStyle
            };
            activeLabel.AddOnRealize((obj) => activeLabelGO = obj);
            mainPanel.AddChild(activeLabel);

            // Invert checkbox
            var invertCheckbox = new PCheckBox("InvertCheckbox")
            {
                Text = "Send Green Signal Outside Range",
                ToolTip = "When checked, sends GREEN when temperature is OUTSIDE the range instead of inside",
                InitialState = PCheckBox.STATE_UNCHECKED,
                OnChecked = OnInvertChanged,
                TextStyle = PUITuning.Fonts.TextDarkStyle
            };
            invertCheckbox.AddOnRealize((obj) => invertCheckboxGO = obj);
            mainPanel.AddChild(invertCheckbox);

            mainPanel.AddTo(gameObject);
            ContentContainer = gameObject;
            base.OnPrefabInit();
        }

        private PPanel CreateInputRowWithButtons(string name,
            System.Func<float> getValue, System.Action<float> setValue,
            float min, float max, float smallStep, float largeStep, bool isRelative,
            out TMP_InputField inputFieldRef)
        {
            TMP_InputField localInputField = null;

            var row = new PPanel(name + "Row")
            {
                Direction = PanelDirection.Horizontal,
                Spacing = 2,
                FlexSize = Vector2.right,
                Alignment = TextAnchor.MiddleCenter
            };

            row.AddChild(new PButton(name + "DecLarge")
            {
                Text = "<<",
                OnClick = (src) => setValue(Mathf.Clamp(getValue() - largeStep, min, max)),
                ToolTip = $"-{largeStep}°"
            });

            row.AddChild(new PButton(name + "DecSmall")
            {
                Text = "<",
                OnClick = (src) => setValue(Mathf.Clamp(getValue() - smallStep, min, max)),
                ToolTip = $"-{smallStep}°"
            });

            var textField = new PTextField(name + "Input")
            {
                Text = "0",
                MinWidth = 80,
                MaxLength = 10,
                Type = PTextField.FieldType.Float,
                OnTextChanged = (source, text) =>
                {
                    if (float.TryParse(text, out float parsed))
                    {
                        float kelvin = isRelative ? parsed : ConvertDisplayToKelvin(parsed);
                        setValue(Mathf.Clamp(kelvin, min, max));
                    }
                },
                TextStyle = PUITuning.Fonts.TextDarkStyle
            };
            textField.AddOnRealize((obj) =>
            {
                localInputField = obj.GetComponentInChildren<TMP_InputField>();
                if (name == "Center") centerInputField = localInputField;
                else if (name == "Below") belowInputField = localInputField;
                else if (name == "Above") aboveInputField = localInputField;
            });
            row.AddChild(textField);

            row.AddChild(new PButton(name + "IncSmall")
            {
                Text = ">",
                OnClick = (src) => setValue(Mathf.Clamp(getValue() + smallStep, min, max)),
                ToolTip = $"+{smallStep}°"
            });

            row.AddChild(new PButton(name + "IncLarge")
            {
                Text = ">>",
                OnClick = (src) => setValue(Mathf.Clamp(getValue() + largeStep, min, max)),
                ToolTip = $"+{largeStep}°"
            });

            inputFieldRef = localInputField;
            return row;
        }

        private float ConvertDisplayToKelvin(float displayTemp)
        {
            if (GameUtil.temperatureUnit == GameUtil.TemperatureUnit.Fahrenheit)
                return (displayTemp - 32f) * 5f / 9f + 273.15f;
            else if (GameUtil.temperatureUnit == GameUtil.TemperatureUnit.Kelvin)
                return displayTemp;
            else
                return displayTemp + 273.15f;
        }

        private string GetNumericDisplay(float kelvin, bool isRelative)
        {
            float displayValue;
            if (isRelative)
            {
                displayValue = (GameUtil.temperatureUnit == GameUtil.TemperatureUnit.Fahrenheit)
                    ? kelvin * 9f / 5f : kelvin;
            }
            else
            {
                if (GameUtil.temperatureUnit == GameUtil.TemperatureUnit.Fahrenheit)
                    displayValue = (kelvin - 273.15f) * 9f / 5f + 32f;
                else if (GameUtil.temperatureUnit == GameUtil.TemperatureUnit.Kelvin)
                    displayValue = kelvin;
                else
                    displayValue = kelvin - 273.15f;
            }
            return displayValue.ToString("F1");
        }

        private void OnInvertChanged(GameObject source, int state)
        {
            if (target != null)
            {
                int newState = state == PCheckBox.STATE_CHECKED ? PCheckBox.STATE_UNCHECKED : PCheckBox.STATE_CHECKED;
                PCheckBox.SetCheckState(invertCheckboxGO, newState);
                target.SetActivateInsideRange(newState == PCheckBox.STATE_UNCHECKED);
            }
        }

        private void SetLabelText(GameObject labelGO, string text)
        {
            if (labelGO == null) return;

            var locText = labelGO.GetComponentInChildren<LocText>();
            if (locText != null) { locText.text = text; return; }

            var tmpText = labelGO.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null) { tmpText.text = text; return; }

            var textComp = labelGO.GetComponentInChildren<Text>();
            if (textComp != null) textComp.text = text;
        }

        private void RefreshCurrentTemperature()
        {
            if (target == null || currentTempLabelGO == null) return;

            int cell = Grid.PosToCell(target);
            float currentTemp = Grid.Temperature[cell];
            string currentTempStr = currentTemp > 0 ? GameUtil.GetFormattedTemperature(currentTemp) : "---";
            SetLabelText(currentTempLabelGO, $"Current Ambient Temperature:\n{currentTempStr}");
        }

        private void RefreshUI()
        {
            if (target == null) return;

            RefreshCurrentTemperature();

            if (centerInputField != null && !centerInputField.isFocused)
                centerInputField.text = GetNumericDisplay(target.centerTemperature, false);

            if (belowInputField != null && !belowInputField.isFocused)
                belowInputField.text = GetNumericDisplay(target.degreesBelow, true);

            if (aboveInputField != null && !aboveInputField.isFocused)
                aboveInputField.text = GetNumericDisplay(target.degreesAbove, true);

            string lower = GameUtil.GetFormattedTemperature(target.LowerBound);
            string upper = GameUtil.GetFormattedTemperature(target.UpperBound);
            SetLabelText(activeLabelGO, $"Active Range: {lower} to {upper}");

            if (invertCheckboxGO != null)
                PCheckBox.SetCheckState(invertCheckboxGO, target.activateInsideRange ? PCheckBox.STATE_UNCHECKED : PCheckBox.STATE_CHECKED);

            if (centerTempSlider != null && Mathf.Abs(centerTempSlider.value - target.centerTemperature) > 0.1f)
                centerTempSlider.value = target.centerTemperature;

            if (rangeBelowSlider != null && Mathf.Abs(rangeBelowSlider.value - target.degreesBelow) > 0.1f)
                rangeBelowSlider.value = target.degreesBelow;

            if (rangeAboveSlider != null && Mathf.Abs(rangeAboveSlider.value - target.degreesAbove) > 0.1f)
                rangeAboveSlider.value = target.degreesAbove;
        }

        public override string GetTitle() => "Adv. Thermo Sensor";
        public override int GetSideScreenSortOrder() => 20;
    }
}
