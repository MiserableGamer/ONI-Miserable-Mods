using System;
using ControlledFramerate.UI;
using HarmonyLib;

namespace ControlledFramerate.Patches
{
    public static class UIPatches
    {
        [HarmonyPatch(typeof(TopLeftControlScreen), nameof(TopLeftControlScreen.OnActivate))]
        public static class TopLeftControlScreen_OnActivate_Postfix
        {
            public static void Postfix(TopLeftControlScreen __instance)
            {
                try
                {
                    TopBarButtons.CreateButtons(__instance);
                }
                catch (Exception ex)
                {
                    ControlledFramerateMod.Log($"Error creating UI buttons: {ex}");
                }
            }
        }

        [HarmonyPatch(typeof(PinnedResourcesPanel), "OnSpawn")]
        public static class PinnedResourcesPanel_OnSpawn_Postfix
        {
            public static void Postfix()
            {
                try
                {
                    AdaptiveStatusPanel.Create();
                }
                catch (Exception ex)
                {
                    ControlledFramerateMod.Log($"Error creating adaptive status panel: {ex}");
                }
            }
        }
    }
}
