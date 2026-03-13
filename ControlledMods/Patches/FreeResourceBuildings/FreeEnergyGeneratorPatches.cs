using System;
using HarmonyLib;
using ControlledMods.ModDetection;
using ControlledMods.Options;

namespace ControlledMods.Patches.FreeResourceBuildings
{
    public static class FreeEnergyGeneratorPatches
    {
        private static Type _freeEnergyGeneratorType;
        private static System.Reflection.FieldInfo _currentCapacityField;
        private static float _savedDefWattage;

        public static void ApplyPatches(Harmony harmony)
        {
            if (!FreeResourceBuildingsDetection.Loaded) return;
            var opts = ControlledModsOptions.Instance;
            if (!opts.FixFreeEnergyGeneratorSlider) return;

            _freeEnergyGeneratorType = AccessTools.TypeByName("FreeResourceBuildings.FreeEnergyGenerator");
            if (_freeEnergyGeneratorType == null)
            {
                ControlledModsMod.LogWarning("FreeEnergyGenerator type not found - skipping patches");
                return;
            }

            _currentCapacityField = AccessTools.Field(_freeEnergyGeneratorType, "currentCapacity");
            if (_currentCapacityField == null)
            {
                ControlledModsMod.LogWarning("FreeEnergyGenerator.currentCapacity field not found - skipping patches");
                return;
            }

            // Def-swap on EnergySim200ms: temporarily set the def wattage to the slider value
            // so the original method reads it via WattageRating, then restore it after
            var energySim = AccessTools.Method(_freeEnergyGeneratorType, "EnergySim200ms", new[] { typeof(float) });
            if (energySim != null)
            {
                harmony.Patch(energySim,
                    prefix: new HarmonyMethod(typeof(FreeEnergyGenerator_EnergySim200ms_Patch),
                        nameof(FreeEnergyGenerator_EnergySim200ms_Patch.Prefix)),
                    finalizer: new HarmonyMethod(typeof(FreeEnergyGenerator_EnergySim200ms_Patch),
                        nameof(FreeEnergyGenerator_EnergySim200ms_Patch.Finalizer)));
                ControlledModsMod.Log("Free Resource Buildings patches applied");
            }
            else
            {
                ControlledModsMod.LogWarning("FreeEnergyGenerator.EnergySim200ms not found");
            }

            // Display fix: patch WattageRating getter so UI shows the slider value outside of EnergySim
            var wattageRatingGetter = AccessTools.PropertyGetter(typeof(Generator), "WattageRating");
            if (wattageRatingGetter != null)
            {
                harmony.Patch(wattageRatingGetter,
                    postfix: new HarmonyMethod(typeof(Generator_WattageRating_Patch),
                        nameof(Generator_WattageRating_Patch.Postfix)));
                ControlledModsMod.Log("Free Resource Buildings patches applied");
            }

            ControlledModsMod.Log("Free Resource Buildings patches applied");
        }

        // Prefix: swap building def wattage to the slider value before the original method runs
        // Finalizer: restore the original def wattage (runs even if original throws)
        public static class FreeEnergyGenerator_EnergySim200ms_Patch
        {
            public static void Prefix(Generator __instance)
            {
                try
                {
                    if (_currentCapacityField == null) return;
                    float sliderValue = (float)_currentCapacityField.GetValue(__instance);
                    _savedDefWattage = __instance.building.Def.GeneratorWattageRating;
                    __instance.building.Def.GeneratorWattageRating = sliderValue;
                }
                catch (Exception ex)
                {
                    ControlledModsMod.LogWarning($"FreeEnergyGenerator prefix error: {ex.Message}");
                }
            }

            public static void Finalizer(Generator __instance)
            {
                try
                {
                    __instance.building.Def.GeneratorWattageRating = _savedDefWattage;
                }
                catch { }
            }
        }

        // Postfix on Generator.get_WattageRating: for FreeEnergyGenerator instances,
        // return currentCapacity (slider value) * Efficiency so the UI displays correctly
        public static class Generator_WattageRating_Patch
        {
            public static void Postfix(Generator __instance, ref float __result)
            {
                if (_freeEnergyGeneratorType == null || _currentCapacityField == null) return;
                if (!_freeEnergyGeneratorType.IsInstanceOfType(__instance)) return;

                float sliderValue = (float)_currentCapacityField.GetValue(__instance);
                float defWattage = __instance.building.Def.GeneratorWattageRating;

                if (defWattage > 0f)
                    __result = sliderValue * (__result / defWattage);
                else
                    __result = sliderValue;
            }
        }
    }
}
