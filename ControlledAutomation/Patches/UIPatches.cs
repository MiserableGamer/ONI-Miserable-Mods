using HarmonyLib;
using PeterHan.PLib.UI;
using ControlledAutomation.UI;
using ControlledAutomation.Options;

namespace ControlledAutomation.Patches
{
    /// <summary>
    /// Patches to register the custom sidescreens.
    /// </summary>

    [HarmonyPatch(typeof(DetailsScreen))]
    [HarmonyPatch("OnPrefabInit")]
    public static class DetailsScreen_OnPrefabInit_Patch
    {
        public static void Postfix()
        {
            // Register the thresholds sidescreen (for storage buildings with thresholds)
            // Place it directly after the ActiveRangeSideScreen
            if (ControlledAutomationOptions.Instance.EnableStorageThresholds)
            {
                PUIUtils.AddSideScreenContentWithOrdering<ThresholdsSideScreen>(
                    typeof(ActiveRangeSideScreen).FullName, false);
            }

            // Register the generic inversion sidescreen (for sensors and other buildings)
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
            {
                PUIUtils.AddSideScreenContent<InversionSideScreen>();
            }

            // Register the rocket platform sidescreen (for dual-output buildings)
            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
            {
                PUIUtils.AddSideScreenContent<RocketPlatformSideScreen>();
            }
        }
    }
}
