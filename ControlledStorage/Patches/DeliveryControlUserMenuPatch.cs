using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ControlledStorage.Patches
{
    [HarmonyPatch(typeof(UserMenu), "AppendToScreen")]
    public static class DeliveryControlUserMenuPatch
    {
        private static bool _injectedThisCycle;

        internal static bool Prepare() => ControlledStorageOptions.Instance.EnableDeliveryControl;

        [HarmonyPrefix]
        public static void Prefix() => _injectedThisCycle = false;

        [HarmonyPostfix]
        public static void Postfix(UserMenu __instance, GameObject go, UserMenuScreen screen)
        {
            if (go == null || _injectedThisCycle) return;
            if (go.GetComponent<StorageDeliveryControl>() == null) return;

            _injectedThisCycle = true;

            var buttonInfo = new KIconButtonMenu.ButtonInfo(
                "action_mirror",
                ControlledStorageStrings.UI.DELIVERY_CONTROL.COPY_DELIVERY_SETTINGS,
                () =>
                {
                    DeliveryControlCopyPatches.StartCopyDeliveryControl(go);
                    PlayerController.Instance.ActivateTool(CopySettingsTool.Instance);
                    var pos = go.transform.position;
                    PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, "Delivery settings - drag over targets", null, pos, 2f);
                },
                Action.NumActions,
                null, null, null,
                ControlledStorageStrings.UI.DELIVERY_CONTROL.COPY_DELIVERY_SETTINGS_TOOLTIP,
                true
            );

            screen.AddButtons(new List<KIconButtonMenu.ButtonInfo> { buttonInfo });
        }
    }
}
