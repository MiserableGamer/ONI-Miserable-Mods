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
        private TMP_InputField bufferInputField;
        private KSlider bufferSlider;

        private const float MinTempK = 1f;
        private const float MaxTempK = 1273.15f;
        private const float MaxRangeK = 500f;

        private bool needsFullRefresh = false;

        private const float ArrowButtonSize = 21f;
        private const float ArrowIconSize = 13f;
        private static readonly RectOffset ArrowButtonMargin = new RectOffset(4, 4, 4, 4);

        private static ColorStyleSetting _lightButtonStyle;
        private static ColorStyleSetting LightButtonStyle
        {
            get
            {
                if (_lightButtonStyle == null)
                {
                    _lightButtonStyle = ScriptableObject.CreateInstance<ColorStyleSetting>();
                    _lightButtonStyle.activeColor = new Color(0.75f, 0.75f, 0.75f);
                    _lightButtonStyle.inactiveColor = new Color(1f, 1f, 1f);
                    _lightButtonStyle.hoverColor = new Color(0.9f, 0.9f, 0.9f);
                    _lightButtonStyle.disabledColor = new Color(0.5f, 0.5f, 0.5f);
                    _lightButtonStyle.disabledActiveColor = new Color(0.6f, 0.6f, 0.6f);
                    _lightButtonStyle.disabledhoverColor = new Color(0.55f, 0.55f, 0.55f);
                }
                return _lightButtonStyle;
            }
        }

        private static readonly Color ArrowTintPurple = new Color(0.53f, 0.27f, 0.40f);

        public override bool IsValidForTarget(GameObject target) =>
            target.GetComponent<TemperatureRangeSensor>() != null;

        public override void SetTarget(GameObject target)
        {
            this.target = target?.GetComponent<TemperatureRangeSensor>();
            needsFullRefresh = true;
            RefreshUI();
        }

        public override void OnShow(bool show)
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

        public override void OnPrefabInit()
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

            // Signal buffer
            mainPanel.AddChild(new PLabel("BufferTitle") { Text = "Signal Buffer", TextStyle = PUITuning.Fonts.TextDarkStyle });

            var bufferRow = new PPanel("BufferRow")
            {
                Direction = PanelDirection.Horizontal,
                Spacing = 4,
                FlexSize = Vector2.right,
                Alignment = TextAnchor.MiddleCenter
            };

            var bufferTextField = new PTextField("BufferInput")
            {
                Text = "0",
                MinWidth = 60,
                MaxLength = 6,
                Type = PTextField.FieldType.Float,
                OnTextChanged = (source, text) =>
                {
                    if (float.TryParse(text, out float parsed))
                    {
                        target?.SetBufferDuration(parsed);
                        RefreshUI();
                    }
                },
                TextStyle = PUITuning.Fonts.TextDarkStyle
            };
            bufferTextField.AddOnRealize((obj) => bufferInputField = obj.GetComponentInChildren<TMP_InputField>());
            bufferRow.AddChild(bufferTextField);

            bufferRow.AddChild(new PLabel("BufferUnits") { Text = "s", TextStyle = PUITuning.Fonts.TextDarkStyle });
            mainPanel.AddChild(bufferRow);

            var bufferSliderCtrl = new PSliderSingle("BufferSlider")
            {
                MinValue = 0f, MaxValue = 200f, InitialValue = 0f,
                OnValueChanged = (src, val) => { target?.SetBufferDuration(val); RefreshUI(); },
                FlexSize = Vector2.right
            };
            bufferSliderCtrl.AddOnRealize((obj) => bufferSlider = obj.GetComponentInChildren<KSlider>());
            mainPanel.AddChild(bufferSliderCtrl);

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

            var doubleArrow = PUITuning.Images.GetSpriteByName("game_speed_play2");
            var singleArrow = PUITuning.Images.Arrow;

            row.AddChild(new PButton(name + "DecLarge")
            {
                Sprite = doubleArrow,
                SpriteTransform = ImageTransform.FlipHorizontal,
                SpriteSize = new Vector2(ArrowIconSize, ArrowIconSize),
                SpriteTint = ArrowTintPurple,
                Margin = ArrowButtonMargin,
                FlexSize = Vector2.zero,
                Color = LightButtonStyle,
                OnClick = (src) => setValue(Mathf.Clamp(getValue() - largeStep, min, max)),
                ToolTip = $"-{largeStep}°"
            });

            row.AddChild(new PButton(name + "DecSmall")
            {
                Sprite = singleArrow,
                SpriteTransform = ImageTransform.FlipHorizontal,
                SpriteSize = new Vector2(ArrowIconSize, ArrowIconSize),
                SpriteTint = ArrowTintPurple,
                Margin = ArrowButtonMargin,
                FlexSize = Vector2.zero,
                Color = LightButtonStyle,
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
                Sprite = singleArrow,
                SpriteTransform = ImageTransform.None,
                SpriteSize = new Vector2(ArrowIconSize, ArrowIconSize),
                SpriteTint = ArrowTintPurple,
                Margin = ArrowButtonMargin,
                FlexSize = Vector2.zero,
                Color = LightButtonStyle,
                OnClick = (src) => setValue(Mathf.Clamp(getValue() + smallStep, min, max)),
                ToolTip = $"+{smallStep}°"
            });

            row.AddChild(new PButton(name + "IncLarge")
            {
                Sprite = doubleArrow,
                SpriteTransform = ImageTransform.None,
                SpriteSize = new Vector2(ArrowIconSize, ArrowIconSize),
                SpriteTint = ArrowTintPurple,
                Margin = ArrowButtonMargin,
                FlexSize = Vector2.zero,
                Color = LightButtonStyle,
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

            if (bufferInputField != null && !bufferInputField.isFocused)
                bufferInputField.text = target.bufferDuration.ToString("F1");

            if (bufferSlider != null && Mathf.Abs(bufferSlider.value - target.bufferDuration) > 0.1f)
                bufferSlider.value = target.bufferDuration;
        }

        public override string GetTitle() => "Adv. Thermo Sensor";
        public override int GetSideScreenSortOrder() => 20;
    }
}
