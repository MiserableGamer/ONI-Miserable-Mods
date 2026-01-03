using CopyMaterials.Logic;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CopyMaterials.Patches
{
    [HarmonyPatch(typeof(UserMenu), "AppendToScreen")]
    public static class UserMenu_AppendToScreen_Patch
    {
        private static bool injectedThisCycle = false;

        private static readonly FieldInfo sourceField = AccessTools.Field(typeof(CopySettingsTool), "sourceGameObject");

        public static void Prefix()
        {
            injectedThisCycle = false;
        }

        public static void Postfix(UserMenu __instance, GameObject go, UserMenuScreen screen)
        {
            if (go == null || injectedThisCycle) return;

            var building = go.GetComponent<Building>();
            var pe = go.GetComponent<PrimaryElement>();
            if (building == null || pe == null) return;

            injectedThisCycle = true;

            var buttonInfo = new KIconButtonMenu.ButtonInfo(
                "action_mirror",
                CopyMaterialsStrings.UI.COPY_MATERIALS.BUTTON_TEXT,
                () =>
                {
                    sourceField.SetValue(CopySettingsTool.Instance, go);
                    CopyMaterialsManager.SetSource(  // New: Capture source building/material/settings
                        go.GetComponent<Building>(),
                        go.GetComponent<PrimaryElement>()?.ElementID ?? SimHashes.Vacuum
                    );
                    CopySettingsToolPatches.isMaterialCopyMode = true;  // New: Set flag
                    PlayerController.Instance.ActivateTool(CopySettingsTool.Instance);

                    Vector3 pos = go.transform.position;
                    PopFXManager.Instance.SpawnFX(
                        PopFXManager.Instance.sprite_Plus,
                        "Materials copied",
                        null,
                        pos,
                        2f
                    );
                },
                Action.NumActions,
                null, null, null,
                CopyMaterialsStrings.UI.COPY_MATERIALS.BUTTON_TOOLTIP,
                true
            );

            screen.AddButtons(new List<KIconButtonMenu.ButtonInfo> { buttonInfo });
        }
    }
}