using HarmonyLib;
using UnityEngine;
using ControlledAutomation.Components;
using ControlledAutomation.Options;

namespace ControlledAutomation.Patches
{
    // Rocket Platform has two automation outputs - each can be inverted independently

    [HarmonyPatch(typeof(LaunchPadConfig), nameof(LaunchPadConfig.DoPostConfigureComplete))]
    public static class LaunchPadConfig_Patch
    {
        public static void Postfix(GameObject go)
        {
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
                go.AddOrGet<RocketPlatformInverter>();
        }
    }

    [HarmonyPatch(typeof(LaunchPad), nameof(LaunchPad.Sim1000ms))]
    public static class LaunchPad_Patch
    {
        public static void Postfix(LaunchPad __instance, LogicPorts ___ports,
            HashedString ___statusPort, HashedString ___landedRocketPort)
        {
            RocketPlatformInverter inverter = RocketPlatformInverter.Get(__instance.gameObject);
            if (inverter == null || (!inverter.InvertOutput1 && !inverter.InvertOutput2))
                return;

            RocketModuleCluster landedRocket = __instance.LandedRocket;
            bool rocketPresent = landedRocket != null;
            bool rocketReady = landedRocket != null &&
                (landedRocket.CraftInterface.CheckReadyForAutomatedLaunch() ||
                 landedRocket.CraftInterface.HasTag(GameTags.RocketNotOnGround));

            if (inverter.InvertOutput1)
                ___ports.SendSignal(___statusPort, rocketReady ? 0 : 1);

            if (inverter.InvertOutput2)
                ___ports.SendSignal(___landedRocketPort, rocketPresent ? 0 : 1);
        }
    }
}
