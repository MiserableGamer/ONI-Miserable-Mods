using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using PeterHan.PLib.Core;
using ControlledVisuals.Options;
using CVOptions = ControlledVisuals.Options.ControlledVisualsOptions;

namespace ControlledVisuals.Patches
{
    // Appends the two other temperature units in parentheses when option is on (e.g. 25 °C (77 °F, 298 K)).
    [HarmonyPatch(typeof(GameUtil), nameof(GameUtil.GetFormattedTemperature))]
    public static class GameUtil_GetFormattedTemperature_Patch
    {
        private const string FormatString = "##0.#";

        public static bool Prepare() => CVOptions.Instance.ShowAllTemperatureUnits;

        public static void Postfix(
            float temp,
            GameUtil.TimeSlice timeSlice,
            GameUtil.TemperatureInterpretation interpretation,
            bool displayUnits,
            bool roundInDestinationFormat,
            ref string __result)
        {
            try
            {
                if (interpretation != GameUtil.TemperatureInterpretation.Absolute ||
                    timeSlice != GameUtil.TimeSlice.None ||
                    !displayUnits)
                {
                    return;
                }

                // Already appended (e.g. GetFormattedTemperature called again in a chain); avoid duplicating.
                if (string.IsNullOrEmpty(__result) || __result.Contains(" ("))
                    return;

                // Game passes temp in Kelvin (internal unit); do not convert to Kelvin again.
                float kelvin = temp;
                var currentUnit = GameUtil.temperatureUnit;
                var secondary = new List<string>();

                // Append the two non-primary units in C, F, K order.
                if (currentUnit != GameUtil.TemperatureUnit.Celsius)
                {
                    float c = GameUtil.GetTemperatureConvertedFromKelvin(kelvin, GameUtil.TemperatureUnit.Celsius);
                    secondary.Add(c.ToString(FormatString) + STRINGS.UI.UNITSUFFIXES.TEMPERATURE.CELSIUS);
                }
                if (currentUnit != GameUtil.TemperatureUnit.Fahrenheit)
                {
                    float f = GameUtil.GetTemperatureConvertedFromKelvin(kelvin, GameUtil.TemperatureUnit.Fahrenheit);
                    secondary.Add(f.ToString(FormatString) + STRINGS.UI.UNITSUFFIXES.TEMPERATURE.FAHRENHEIT);
                }
                if (currentUnit != GameUtil.TemperatureUnit.Kelvin)
                {
                    secondary.Add(kelvin.ToString(FormatString) + STRINGS.UI.UNITSUFFIXES.TEMPERATURE.KELVIN);
                }

                if (secondary.Count < 2)
                    return;

                var builder = new StringBuilder(__result);
                builder.Append(" (");
                builder.Append(secondary[0]);
                builder.Append(", ");
                builder.Append(secondary[1]);
                builder.Append(")");
                __result = builder.ToString();
            }
            catch (System.Exception e)
            {
                PUtil.LogWarning("ControlledVisuals DisplayAllTemps failed: " + e.Message);
            }
        }
    }
}
