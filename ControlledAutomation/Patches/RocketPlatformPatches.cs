using HarmonyLib;
using UnityEngine;
using ControlledAutomation.Components;
using ControlledAutomation.Options;

namespace ControlledAutomation.Patches
{
    /// <summary>
    /// Patches for Rocket Platform (LaunchPad) which has two automation outputs.
    /// </summary>

    [HarmonyPatch(typeof(LaunchPadConfig))]
    public class LaunchPadConfig_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LaunchPadConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<RocketPlatformInverter>();
        }
    }

    [HarmonyPatch(typeof(LaunchPad))]
    public class LaunchPad_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LaunchPad.OnCopySettings))]
        public static void OnCopySettings(LaunchPad __instance, object data)
        {
            GameObject otherGameObject = (GameObject)data;
            if (otherGameObject != null)
            {
                RocketPlatformInverter component = RocketPlatformInverter.Get(__instance.gameObject);
                RocketPlatformInverter otherComponent = RocketPlatformInverter.Get(otherGameObject);
                if (component != null && otherComponent != null)
                {
                    component.InvertOutput1 = otherComponent.InvertOutput1;
                    component.InvertOutput2 = otherComponent.InvertOutput2;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("UpdateLogicPorts")]
        public static void UpdateLogicPorts_Postfix(LaunchPad __instance)
        {
            RocketPlatformInverter inverter = RocketPlatformInverter.Get(__instance.gameObject);
            if (inverter == null)
                return;

            var ports = __instance.GetComponent<LogicPorts>();
            if (ports == null)
                return;

            // If either output is inverted, we need to re-send the inverted signals
            // The original method already sent signals, we just need to flip them if needed
            if (inverter.InvertOutput1 || inverter.InvertOutput2)
            {
                var building = __instance.GetComponent<Building>();
                if (building == null)
                    return;

                // Get current signal states from the logic network
                if (inverter.InvertOutput1)
                {
                    int cell1 = building.GetLogicOutputCellByIndex(0);
                    var network1 = Game.Instance.logicCircuitSystem.GetNetworkForCell(cell1);
                    if (network1 != null)
                    {
                        bool currentSignal1 = network1.OutputValue == 1;
                        ports.SendSignal(LaunchPad.LAUNCH_READY_PORT, currentSignal1 ? 0 : 1);
                    }
                }

                if (inverter.InvertOutput2 && building.Def.LogicOutputPorts.Count > 1)
                {
                    int cell2 = building.GetLogicOutputCellByIndex(1);
                    var network2 = Game.Instance.logicCircuitSystem.GetNetworkForCell(cell2);
                    if (network2 != null)
                    {
                        bool currentSignal2 = network2.OutputValue == 1;
                        ports.SendSignal(LaunchPad.ROCKET_PRESENCE_PORT, currentSignal2 ? 0 : 1);
                    }
                }
            }
        }
    }
}
