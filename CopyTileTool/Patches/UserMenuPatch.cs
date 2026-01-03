using CopyTileTool.Logic;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CopyTileTool.Patches
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

            // Only show button for tiles
            if (!CopyTileManager.IsTile(go)) return;

            var building = go.GetComponent<Building>();
            var pe = go.GetComponent<PrimaryElement>();
            if (building == null || pe == null) return;

            injectedThisCycle = true;

            var buttonInfo = new KIconButtonMenu.ButtonInfo(
                "action_mirror",
                CopyTileStrings.UI.COPY_TILE.BUTTON_TEXT,
                () =>
                {
                    OnCopyTileButtonClicked(go);
                },
                Action.NumActions,
                null, null, null,
                CopyTileStrings.UI.COPY_TILE.BUTTON_TOOLTIP,
                true
            );

            screen.AddButtons(new List<KIconButtonMenu.ButtonInfo> { buttonInfo });
        }

        private static void OnCopyTileButtonClicked(GameObject go)
        {
            var building = go.GetComponent<Building>();
            var pe = go.GetComponent<PrimaryElement>();

            if (building == null || pe == null) return;

            var state = CopyTileManager.CurrentState;

            if (state == ToolState.Idle || state == ToolState.SelectingDestination)
            {
                // First click - set destination
                CopyTileManager.SetDestination(building, pe.ElementID);

                // Set source for the CopySettingsTool
                sourceField.SetValue(CopySettingsTool.Instance, go);

                // Set our mode flag
                CopySettingsToolPatches.isTileCopyMode = true;

                // Activate the tool
                PlayerController.Instance.ActivateTool(CopySettingsTool.Instance);

                Vector3 pos = go.transform.position;
                CopyTileManager.ShowPopup(CopyTileStrings.UI.COPY_TILE.TILE_COPIED, pos);
            }
            else if (state == ToolState.SelectingSource)
            {
                // Second click - set source (what to replace)
                CopyTileManager.SetSource(building, pe.ElementID);

                Vector3 pos = go.transform.position;
                CopyTileManager.ShowPopup(CopyTileStrings.UI.COPY_TILE.SOURCE_SELECTED, pos);
            }
        }
    }
}

