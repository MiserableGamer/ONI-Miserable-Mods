using HarmonyLib;
using PeterHan.PLib.Core;
using System.Text;
using UnityEngine;

namespace HighPrecisionTemperatureFIXED
{
    public class HighPrecisionTemperatureFIXEDMod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            // Initialize PLib
            PUtil.InitLibrary();

            // Apply Harmony patches
            harmony.PatchAll();
        }
    }

    // Patch AppendFormattedTemperature to always use high precision (4 decimal places)
    [HarmonyPatch(typeof(GameUtil), nameof(GameUtil.AppendFormattedTemperature))]
    public static class AppendFormattedTemperature_Patch
    {
        // Prefix returns false to skip the original method entirely
        public static bool Prefix(
            StringBuilder builder,
            float temp,
            GameUtil.TimeSlice timeSlice,
            GameUtil.TemperatureInterpretation interpretation,
            bool displayUnits,
            bool roundInDestinationFormat)
        {
            // Handle temperature interpretation (same as original)
            if (interpretation == GameUtil.TemperatureInterpretation.Absolute)
            {
                temp = GameUtil.GetConvertedTemperature(temp, roundInDestinationFormat);
            }
            else
            {
                // Relative temperature (delta)
                temp = GetConvertedTemperatureDelta(temp);
            }

            // Apply time slice (same as original)
            temp = GameUtil.ApplyTimeSlice(temp, timeSlice);

            // HIGH PRECISION: Always use 4 decimal places instead of conditional formatting
            builder.AppendFormat("{0:##0.####}", temp);

            // Append unit suffix if requested (same as original)
            if (displayUnits)
            {
                builder.Append(GameUtil.GetTemperatureUnitSuffix());
            }

            // Add time slice text (same as original)
            AddTimeSliceText(builder, timeSlice);

            // Return false to skip the original method
            return false;
        }

        // Helper: Convert temperature delta based on unit setting
        // This mirrors GameUtil.GetConvertedTemperatureDelta which is private
        private static float GetConvertedTemperatureDelta(float kelvin_delta)
        {
            switch (GameUtil.temperatureUnit)
            {
                case GameUtil.TemperatureUnit.Fahrenheit:
                    return kelvin_delta * 1.8f;
                case GameUtil.TemperatureUnit.Celsius:
                case GameUtil.TemperatureUnit.Kelvin:
                default:
                    return kelvin_delta;
            }
        }

        // Helper: Add time slice text to the builder
        // This mirrors GameUtil.AddTimeSliceText(StringBuilder, TimeSlice) which exists but we replicate for clarity
        private static void AddTimeSliceText(StringBuilder builder, GameUtil.TimeSlice timeSlice)
        {
            switch (timeSlice)
            {
                case GameUtil.TimeSlice.PerSecond:
                    builder.Append(STRINGS.UI.UNITSUFFIXES.PERSECOND);
                    break;
                case GameUtil.TimeSlice.PerCycle:
                    builder.Append(STRINGS.UI.UNITSUFFIXES.PERCYCLE);
                    break;
            }
        }
    }
}
