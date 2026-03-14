using System;
using System.Collections.Generic;
using HarmonyLib;

namespace ControlledMods.Patches
{
    public static class DebugPaintElementScreenPatches
    {
        private static readonly Dictionary<int, Action<object>> TemperatureUnitHandlers = new Dictionary<int, Action<object>>();
        private static int sampleDepth;

        public static void ApplyPatches(Harmony harmony)
        {
            var type = typeof(DebugPaintElementScreen);

            var onSpawn = AccessTools.Method(type, "OnSpawn");
            if (onSpawn != null)
                harmony.Patch(onSpawn, postfix: new HarmonyMethod(typeof(DebugPaintElementScreenPatches), nameof(OnSpawnPostfix)));

            var destroyInstance = AccessTools.Method(type, "DestroyInstance");
            if (destroyInstance != null)
                harmony.Patch(destroyInstance, prefix: new HarmonyMethod(typeof(DebugPaintElementScreenPatches), nameof(DestroyInstancePrefix)));

            var setupLocText = AccessTools.Method(type, "SetupLocText");
            if (setupLocText != null)
                harmony.Patch(setupLocText, postfix: new HarmonyMethod(typeof(DebugPaintElementScreenPatches), nameof(SetupLocTextPostfix)));

            var sampleCell = AccessTools.Method(type, "SampleCell");
            if (sampleCell != null)
            {
                harmony.Patch(sampleCell,
                    prefix: new HarmonyMethod(typeof(DebugPaintElementScreenPatches), nameof(SampleCellPrefix)),
                    postfix: new HarmonyMethod(typeof(DebugPaintElementScreenPatches), nameof(SampleCellPostfix)));
            }

            var onChangeTemp = AccessTools.Method(type, "OnChangeTemperature");
            if (onChangeTemp != null)
                harmony.Patch(onChangeTemp, prefix: new HarmonyMethod(typeof(DebugPaintElementScreenPatches), nameof(OnChangeTemperaturePrefix)));
        }

        private static void OnSpawnPostfix(DebugPaintElementScreen __instance)
        {
            RefreshTemperatureLabel(__instance);
            RefreshTemperatureText(__instance);
            SubscribeTemperatureUnitChanged(__instance);
        }

        private static void DestroyInstancePrefix()
        {
            if (DebugPaintElementScreen.Instance != null)
                UnsubscribeTemperatureUnitChanged(DebugPaintElementScreen.Instance);
        }

        private static void SetupLocTextPostfix(DebugPaintElementScreen __instance)
        {
            RefreshTemperatureLabel(__instance);
        }

        private static void SampleCellPrefix()
        {
            sampleDepth++;
        }

        private static void SampleCellPostfix(DebugPaintElementScreen __instance)
        {
            if (sampleDepth > 0)
                sampleDepth--;
            RefreshTemperatureText(__instance);
        }

        // Replaces the original: converts input from the player's display unit to Kelvin
        private static bool OnChangeTemperaturePrefix(DebugPaintElementScreen __instance)
        {
            float inputValue;
            try
            {
                inputValue = Convert.ToSingle(__instance.temperatureInput.text);
            }
            catch
            {
                inputValue = -1f;
            }

            // Sampled values arrive as Kelvin; typed values are in the player's display unit
            float kelvin = sampleDepth > 0 ? inputValue : GameUtil.GetTemperatureConvertedToKelvin(inputValue);
            if (kelvin <= 0f)
                kelvin = 1f;

            __instance.temperature = kelvin;
            __instance.temperatureInput.text = FormatTemperatureForDisplay(kelvin);
            return false;
        }

        private static void SubscribeTemperatureUnitChanged(DebugPaintElementScreen instance)
        {
            if (Game.Instance == null || instance == null) return;

            int instanceId = instance.GetInstanceID();
            if (TemperatureUnitHandlers.ContainsKey(instanceId)) return;

            Action<object> handler = _ =>
            {
                if (instance == null) return;
                RefreshTemperatureLabel(instance);
                RefreshTemperatureText(instance);
            };

            TemperatureUnitHandlers[instanceId] = handler;
            Game.Instance.Subscribe((int)GameHashes.TemperatureUnitChanged, handler);
        }

        private static void UnsubscribeTemperatureUnitChanged(DebugPaintElementScreen instance)
        {
            if (Game.Instance == null || instance == null) return;

            int instanceId = instance.GetInstanceID();
            if (TemperatureUnitHandlers.TryGetValue(instanceId, out Action<object> handler))
            {
                Game.Instance.Unsubscribe((int)GameHashes.TemperatureUnitChanged, handler);
                TemperatureUnitHandlers.Remove(instanceId);
            }
        }

        private static void RefreshTemperatureLabel(DebugPaintElementScreen instance)
        {
            if (instance == null) return;
            HierarchyReferences refs = instance.GetComponent<HierarchyReferences>();
            LocText temperatureLabel = refs?.GetReference<LocText>("TemperatureLabel");
            if (temperatureLabel != null)
                temperatureLabel.text = "Temperature " + GameUtil.GetTemperatureUnitSuffix();
        }

        private static void RefreshTemperatureText(DebugPaintElementScreen instance)
        {
            if (instance == null || instance.temperatureInput == null) return;
            if (instance.temperature > 0f)
                instance.temperatureInput.text = FormatTemperatureForDisplay(instance.temperature);
        }

        private static string FormatTemperatureForDisplay(float kelvin)
        {
            return GameUtil.GetConvertedTemperature(kelvin, false).ToString("0.##");
        }
    }
}
