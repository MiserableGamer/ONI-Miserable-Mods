using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

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
                "action_copy",
                CopyMaterialsStrings.UI.COPY_MATERIALS.BUTTON_TEXT,
                () =>
                {
                    sourceField.SetValue(CopySettingsTool.Instance, go);
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