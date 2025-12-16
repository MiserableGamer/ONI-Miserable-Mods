using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace CopyMaterials.Patches
{
    [HarmonyPatch(typeof(UserMenu), "AppendToScreen")]
    public static class UserMenu_AppendToScreen_Patch
    {
        private static bool injectedThisCycle = false;

        public static void Prefix()
        {
            // Reset before each AppendToScreen run
            injectedThisCycle = false;
        }

        public static void Postfix(UserMenu __instance, GameObject go, UserMenuScreen screen)
        {
            if (go == null || injectedThisCycle) return;

            var building = go.GetComponent<Building>();
            var pe = go.GetComponent<PrimaryElement>();
            if (building == null || pe == null) return;

            var buttonInfo = new KIconButtonMenu.ButtonInfo(
               "action_copy",
               CopyMaterialsStrings.UI.COPY_MATERIALS.BUTTON_TEXT,
               () => {
                   // Set the source building and element
                   CopyMaterials.Logic.CopyMaterialsManager.SetSource(building, pe.ElementID);

                   // Activate the copy tool
                   PlayerController.Instance.ActivateTool(CopySettingsTool.Instance);

                   // Instead of a popup on the source, show a global message
                   CopyMaterials.Logic.CopyMaterialsManager.ShowGlobalMessage(
                       CopyMaterialsStrings.UI.COPY_MATERIALS.MODE_ACTIVE
                   );
               },
               Action.NumActions,
               null, null, null,
               CopyMaterialsStrings.UI.COPY_MATERIALS.BUTTON_TOOLTIP,
               true
           );

            screen.AddButtons(new List<KIconButtonMenu.ButtonInfo> { buttonInfo });
            injectedThisCycle = true;
            Debug.Log("[CopyMaterials] Button injected once per refresh cycle");
        }
    }
}