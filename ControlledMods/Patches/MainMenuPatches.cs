using HarmonyLib;
using UnityEngine;

namespace ControlledMods.Patches
{
    public static class MainMenuPatches
    {
        public static void ApplyPatch(Harmony harmony)
        {
            var onPrefabInit = AccessTools.Method(typeof(MainMenu), "OnPrefabInit");
            if (onPrefabInit != null)
                harmony.Patch(onPrefabInit, postfix: new HarmonyMethod(typeof(MainMenu_OnPrefabInit_Patch), nameof(MainMenu_OnPrefabInit_Patch.Postfix)));
        }

        public static class MainMenu_OnPrefabInit_Patch
        {
            public static void Postfix(MainMenu __instance)
            {
                try
                {
                    var buttonPrefab = __instance.buttonPrefab;
                    var buttonParent = __instance.buttonParent;
                    var normalButtonStyle = __instance.normalButtonStyle;
                    if (buttonPrefab == null || buttonParent == null) return;

                    // Find the quit button (last child of buttonParent)
                    int quitIndex = buttonParent.transform.childCount - 1;

                    // Create restart button using the same pattern as MakeButton
                    KButton restartButton = Util.KInstantiateUI<KButton>(buttonPrefab.gameObject, buttonParent, true);
                    restartButton.onClick += () => App.instance.Restart();

                    KImage image = restartButton.GetComponent<KImage>();
                    if (image != null && normalButtonStyle != null)
                    {
                        image.colorStyleSetting = normalButtonStyle;
                        image.ApplyColorStyleSetting();
                    }

                    LocText label = restartButton.GetComponentInChildren<LocText>();
                    if (label != null)
                    {
                        label.text = "RESTART";
                        label.fontSize = 14f;
                    }

                    // Place just above the quit button
                    restartButton.transform.SetSiblingIndex(quitIndex);
                }
                catch { }
            }
        }
    }
}
