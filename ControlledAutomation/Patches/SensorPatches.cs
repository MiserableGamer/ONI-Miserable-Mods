using HarmonyLib;
using UnityEngine;
using ControlledAutomation.Components;
using ControlledAutomation.Options;

namespace ControlledAutomation.Patches
{
    /// <summary>
    /// Patches for sensor buildings that need inversion only.
    /// All sensors just need the SensorInverter component added.
    /// </summary>

    #region Liquid Sensors

    [HarmonyPatch(typeof(LiquidConduitElementSensorConfig))]
    public class LiquidConduitElementSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LiquidConduitElementSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(LiquidConduitDiseaseSensorConfig))]
    public class LiquidConduitDiseaseSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LiquidConduitDiseaseSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(LiquidConduitTemperatureSensorConfig))]
    public class LiquidConduitTemperatureSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LiquidConduitTemperatureSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(LogicElementSensorLiquidConfig))]
    public class LogicElementSensorLiquidConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LogicElementSensorLiquidConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(LiquidLimitValveConfig))]
    public class LiquidLimitValveConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LiquidLimitValveConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    #endregion

    #region Gas Sensors

    [HarmonyPatch(typeof(GasConduitElementSensorConfig))]
    public class GasConduitElementSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GasConduitElementSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(GasConduitDiseaseSensorConfig))]
    public class GasConduitDiseaseSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GasConduitDiseaseSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(GasConduitTemperatureSensorConfig))]
    public class GasConduitTemperatureSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GasConduitTemperatureSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(LogicElementSensorGasConfig))]
    public class LogicElementSensorGasConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LogicElementSensorGasConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(GasLimitValveConfig))]
    public class GasLimitValveConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GasLimitValveConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    #endregion

    #region Environmental Sensors

    [HarmonyPatch(typeof(LogicDuplicantSensorConfig))]
    public class LogicDuplicantSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LogicDuplicantSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(LogicPressureSensorGasConfig))]
    public class LogicPressureSensorGasConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LogicPressureSensorGasConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(LogicPressureSensorLiquidConfig))]
    public class LogicPressureSensorLiquidConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LogicPressureSensorLiquidConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(LogicTemperatureSensorConfig))]
    public class LogicTemperatureSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LogicTemperatureSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(LogicLightSensorConfig))]
    public class LogicLightSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LogicLightSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(LogicWattageSensorConfig))]
    public class LogicWattageSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LogicWattageSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(LogicDiseaseSensorConfig))]
    public class LogicDiseaseSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LogicDiseaseSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(LogicCritterCountSensorConfig))]
    public class LogicCritterCountSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LogicCritterCountSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(LogicRadiationSensorConfig))]
    public class LogicRadiationSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LogicRadiationSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(LogicHEPSensorConfig))]
    public class LogicHEPSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LogicHEPSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    #endregion

    #region Time Sensors

    [HarmonyPatch(typeof(LogicTimeOfDaySensorConfig))]
    public class LogicTimeOfDaySensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LogicTimeOfDaySensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(LogicTimerSensorConfig))]
    public class LogicTimerSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LogicTimerSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    #endregion

    #region Logic Components

    [HarmonyPatch(typeof(LogicCounterConfig))]
    public class LogicCounterConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LogicCounterConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    #endregion

    #region Conveyor Sensors

    [HarmonyPatch(typeof(SolidConduitElementSensorConfig))]
    public class SolidConduitElementSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SolidConduitElementSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(SolidConduitDiseaseSensorConfig))]
    public class SolidConduitDiseaseSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SolidConduitDiseaseSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(SolidConduitTemperatureSensorConfig))]
    public class SolidConduitTemperatureSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SolidConduitTemperatureSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    [HarmonyPatch(typeof(SolidLimitValveConfig))]
    public class SolidLimitValveConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SolidLimitValveConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    #endregion

    #region Space/DLC Sensors

    [HarmonyPatch(typeof(LogicClusterLocationSensorConfig))]
    public class LogicClusterLocationSensorConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LogicClusterLocationSensorConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<SensorInverter>();
        }
    }

    #endregion
}
