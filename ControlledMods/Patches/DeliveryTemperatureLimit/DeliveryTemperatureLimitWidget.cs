using System;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;

namespace ControlledMods.Patches.DeliveryTemperatureLimit
{
    /// <summary>
    /// Replacement widget for the Delivery Temperature Limit mod's broken TemperatureLimitWidget.
    /// Creates the same two-input temperature UI using our current PLib (which handles the
    /// TMPro.FaceInfo → UnityEngine.TextCore.FaceInfo change correctly).
    /// Talks to DTL's TemperatureLimit component via reflection.
    /// </summary>
    public class DeliveryTemperatureLimitWidget : KMonoBehaviour
    {
        private static readonly Type TemperatureLimitType = AccessTools.TypeByName("DeliveryTemperatureLimit.TemperatureLimit");
        private static readonly MethodInfo GetMethod = AccessTools.Method(TemperatureLimitType, "Get", new[] { typeof(GameObject) });
        private static readonly PropertyInfo LowLimitProp = AccessTools.Property(TemperatureLimitType, "LowLimit");
        private static readonly PropertyInfo HighLimitProp = AccessTools.Property(TemperatureLimitType, "HighLimit");
        private static readonly MethodInfo SetLowLimitMethod = AccessTools.Method(TemperatureLimitType, "SetLowLimit", new[] { typeof(int) });
        private static readonly MethodInfo SetHighLimitMethod = AccessTools.Method(TemperatureLimitType, "SetHighLimit", new[] { typeof(int) });
        private static readonly MethodInfo IsDisabledMethod = AccessTools.Method(TemperatureLimitType, "IsDisabled");
        private static readonly MethodInfo DisableMethod = AccessTools.Method(TemperatureLimitType, "Disable");

        private GameObject lowInput;
        private GameObject highInput;
        private object target; // DeliveryTemperatureLimit.TemperatureLimit (via reflection)

        public override void OnPrefabInit()
        {
            var margin = new RectOffset(4, 4, 4, 4);
            var baseLayout = gameObject.GetComponent<BoxLayoutGroup>();
            if (baseLayout != null)
            {
                baseLayout.Params = new BoxLayoutParams
                {
                    Alignment = TextAnchor.MiddleLeft,
                    Margin = margin
                };
            }

            var panel = new PPanel("MainPanel")
            {
                Direction = PanelDirection.Horizontal,
                Margin = margin,
                Spacing = 4,
                FlexSize = Vector2.right
            };

            var lowField = new PTextField("lowLimit")
            {
                Type = PTextField.FieldType.Integer,
                OnTextChanged = OnTextChangedLow,
                MinWidth = 72
            };
            lowField.AddOnRealize(obj => lowInput = obj);

            var highField = new PTextField("highLimit")
            {
                Type = PTextField.FieldType.Integer,
                OnTextChanged = OnTextChangedHigh,
                MinWidth = 72
            };
            highField.AddOnRealize(obj => highInput = obj);

            var label = new PLabel("label")
            {
                TextStyle = PUITuning.Fonts.TextDarkStyle,
                Text = "Temperatures:"
            };
            var separator = new PLabel("separator")
            {
                TextStyle = PUITuning.Fonts.TextDarkStyle,
                Text = "-"
            };

            panel.AddChild(label);
            panel.AddChild(lowField);
            panel.AddChild(separator);
            panel.AddChild(highField);
            panel.AddTo(gameObject, -2);

            base.OnPrefabInit();
            UpdateInputs();
        }

        public void SetTarget(object newTarget)
        {
            target = newTarget;
            UpdateInputs();
        }

        public static object GetTemperatureLimit(GameObject go)
        {
            if (GetMethod == null || go == null) return null;
            try { return GetMethod.Invoke(null, new object[] { go }); }
            catch { return null; }
        }

        private int GetLowLimit()
        {
            try { return target != null && LowLimitProp != null ? (int)LowLimitProp.GetValue(target) : 0; }
            catch { return 0; }
        }

        private int GetHighLimit()
        {
            try { return target != null && HighLimitProp != null ? (int)HighLimitProp.GetValue(target) : 0; }
            catch { return 0; }
        }

        private void SetLowLimit(int value)
        {
            try { SetLowLimitMethod?.Invoke(target, new object[] { value }); }
            catch { }
        }

        private void SetHighLimit(int value)
        {
            try { SetHighLimitMethod?.Invoke(target, new object[] { value }); }
            catch { }
        }

        private bool IsDisabled()
        {
            try { return target != null && IsDisabledMethod != null && (bool)IsDisabledMethod.Invoke(target, null); }
            catch { return true; }
        }

        private void Disable()
        {
            try { DisableMethod?.Invoke(target, null); }
            catch { }
        }

        private void UpdateInputs()
        {
            if (target == null || lowInput == null) return;

            if (IsDisabled())
            {
                EmptyInputs();
            }
            else
            {
                SetInputValue(lowInput, GetLowLimit());
                SetInputValue(highInput, GetHighLimit());
            }
            UpdateToolTip();
        }

        private void OnTextChangedLow(GameObject source, string text)
        {
            if (IsDisabled())
                SetHighLimit(5000);

            int num = ParseAndApply(text, SetLowLimit, 0);
            if (num != -1 && num > GetHighLimit())
                SetInputValue(highInput, num);

            UpdateToolTip();
        }

        private void OnTextChangedHigh(GameObject source, string text)
        {
            if (IsDisabled())
                SetLowLimit(0);

            int num = ParseAndApply(text, SetHighLimit, 5000);
            if (num != -1 && num < GetLowLimit())
                SetInputValue(lowInput, num);

            UpdateToolTip();
        }

        private int ParseAndApply(string text, Action<int> setValueFunc, int fallback)
        {
            text = text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                Disable();
                EmptyInputs();
                return -1;
            }

            string suffix = GameUtil.GetTemperatureUnitSuffix();
            if (text.EndsWith(suffix))
                text = text.Remove(text.Length - suffix.Length);

            int num;
            if (int.TryParse(text, out num))
                num = (int)Math.Round((double)GameUtil.GetTemperatureConvertedToKelvin((float)num));
            else
                num = fallback;

            setValueFunc(num);
            return num;
        }

        private void SetInputValue(GameObject input, int kelvinValue)
        {
            if (input == null) return;
            var field = input.GetComponent<TMP_InputField>();
            if (field == null) return;
            string formatted = GameUtil.GetFormattedTemperature((float)kelvinValue, GameUtil.TimeSlice.None,
                GameUtil.TemperatureInterpretation.Absolute, true, true);
            if (field.text != formatted)
                field.text = formatted;
        }

        private void EmptyInputs()
        {
            ClearInput(lowInput);
            ClearInput(highInput);
        }

        private static void ClearInput(GameObject input)
        {
            if (input == null) return;
            var field = input.GetComponent<TMP_InputField>();
            if (field != null && !string.IsNullOrEmpty(field.text))
                field.text = "";
        }

        private void UpdateToolTip()
        {
            string tooltip;
            if (!IsDisabled())
            {
                string low = GameUtil.GetFormattedTemperature((float)GetLowLimit(), GameUtil.TimeSlice.None,
                    GameUtil.TemperatureInterpretation.Absolute, true, true);
                string high = GameUtil.GetFormattedTemperature((float)GetHighLimit(), GameUtil.TimeSlice.None,
                    GameUtil.TemperatureInterpretation.Absolute, true, true);
                tooltip = string.Format("Only objects in the temperature range from <b>{0}</b> to <b>{1}</b> may be delivered", low, high);
            }
            else
            {
                tooltip = "No limit on the temperature range of objects that may be delivered";
            }

            PUIElements.SetToolTip(lowInput, tooltip);
            PUIElements.SetToolTip(highInput, tooltip);
        }
    }
}
