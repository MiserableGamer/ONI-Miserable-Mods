using HarmonyLib;
using ControlledAutomation.UI;
using ControlledAutomation.Options;

namespace ControlledAutomation.Patches
{
    [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
    public static class DetailsScreen_OnPrefabInit_Patch
    {
        public static void Postfix()
        {
            if (ControlledAutomationOptions.Instance.EnableStorageThresholds)
                SideScreenHelper.AddSideScreen<ThresholdsSideScreen>("ControlledAutomationThresholdsSideScreen");

            if (ControlledAutomationOptions.Instance.EnableAutomationInversion)
            {
                SideScreenHelper.AddSideScreen<InversionSideScreen>("ControlledAutomationInversionSideScreen");
                SideScreenHelper.AddSideScreen<RocketPlatformSideScreen>("ControlledAutomationRocketPlatformSideScreen");
            }

            SideScreenHelper.AddSideScreen<TemperatureRangeSideScreen>("ControlledAutomationTemperatureRangeSideScreen");
        }
    }
}
