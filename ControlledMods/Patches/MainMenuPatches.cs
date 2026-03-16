using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace ControlledMods.Patches
{
    public static class MainMenuPatches
    {
        public static void ApplyPatch(Harmony harmony)
        {
            var onPrefabInit = AccessTools.Method(typeof(MainMenu), "OnPrefabInit");
            if (onPrefabInit != null)
                harmony.Patch(onPrefabInit, postfix: new HarmonyMethod(typeof(MainMenu_OnPrefabInit_Patch), nameof(MainMenu_OnPrefabInit_Patch.Postfix)));

            var configureButtons = AccessTools.Method(typeof(PauseScreen), "ConfigureButtonInfos");
            if (configureButtons != null)
                harmony.Patch(configureButtons, postfix: new HarmonyMethod(typeof(PauseScreen_ConfigureButtonInfos_Patch), nameof(PauseScreen_ConfigureButtonInfos_Patch.Postfix)));
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

        public static class PauseScreen_ConfigureButtonInfos_Patch
        {
            public static void Postfix(ref KButtonMenu.ButtonInfo[] ___buttons)
            {
                try
                {
                    if (___buttons == null) return;
                    var list = new List<KButtonMenu.ButtonInfo>(___buttons);
                    int insertIndex = list.Count >= 2 ? list.Count - 2 : list.Count;
                    list.Insert(insertIndex, new KButtonMenu.ButtonInfo(
                        "Restart", global::Action.NumActions,
                        new UnityAction(OnRestart)));
                    ___buttons = list.ToArray();
                }
                catch { }
            }
        }

        private static void OnRestart()
        {
            var ps = PauseScreen.Instance;
            if (ps == null) { App.instance.Restart(); return; }

            ps.gameObject.SetActive(false);
            var dialog = (ConfirmDialogScreen)GameScreenManager.Instance.StartScreen(
                ScreenPrefabs.Instance.ConfirmDialogScreen.gameObject,
                ps.transform.parent.gameObject,
                GameScreenManager.UIRenderTarget.ScreenSpaceOverlay);

            // Add "Restart into this save" checkbox below the message
            Toggle loadIntoSaveToggle = AddRestartIntoSaveCheckbox(dialog);

            dialog.PopupConfirmDialog(
                "Should I restart?\nAll unsaved progress will be lost.",
                () => OnRestartConfirm(true, loadIntoSaveToggle != null && loadIntoSaveToggle.isOn),
                () => OnCancelRestart(),
                "RESTART WITHOUT SAVING",
                () => OnRestartConfirm(false, loadIntoSaveToggle != null && loadIntoSaveToggle.isOn),
                null,
                "SAVE AND RESTART",
                "CANCEL");
        }

        private static Toggle AddRestartIntoSaveCheckbox(ConfirmDialogScreen dialog)
        {
            var popupMessage = Traverse.Create(dialog).Field("popupMessage").GetValue<LocText>();
            if (popupMessage == null) return null;

            var contentParent = popupMessage.transform.parent;
            if (contentParent == null) return null;

            var row = new GameObject("RestartIntoSaveRow");
            row.transform.SetParent(contentParent, false);
            row.transform.SetSiblingIndex(popupMessage.transform.GetSiblingIndex() + 1);

            var rowRect = row.AddComponent<RectTransform>();
            var rowLE = row.AddComponent<LayoutElement>();
            rowLE.minHeight = 24f;
            rowLE.preferredHeight = 24f;

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(0, 8, 0, 0);

            var toggleGO = new GameObject("Toggle");
            toggleGO.transform.SetParent(row.transform, false);
            var toggleRect = toggleGO.AddComponent<RectTransform>();
            var toggleLE = toggleGO.AddComponent<LayoutElement>();
            toggleLE.minWidth = 20f;
            toggleLE.minHeight = 20f;

            var bg = toggleGO.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            var checkmarkGO = new GameObject("Checkmark");
            checkmarkGO.transform.SetParent(toggleGO.transform, false);
            var checkmarkRT = checkmarkGO.AddComponent<RectTransform>();
            checkmarkRT.anchorMin = Vector2.zero;
            checkmarkRT.anchorMax = Vector2.one;
            checkmarkRT.offsetMin = new Vector2(4f, 4f);
            checkmarkRT.offsetMax = new Vector2(-4f, -4f);
            var checkmarkImg = checkmarkGO.AddComponent<Image>();
            checkmarkImg.color = Color.white;

            var toggle = toggleGO.AddComponent<Toggle>();
            toggle.targetGraphic = bg;
            toggle.graphic = checkmarkImg;
            toggle.isOn = false;

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(row.transform, false);
            labelGO.AddComponent<RectTransform>();
            var labelLE = labelGO.AddComponent<LayoutElement>();
            labelLE.flexibleWidth = 1f;
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = "Restart into this save";
            label.fontSize = 14f;
            label.raycastTarget = false;
            if (popupMessage != null && popupMessage.font != null)
                label.font = popupMessage.font;

            return toggle;
        }

        private static void OnRestartConfirm(bool saveFirst, bool loadIntoSave)
        {
            if (saveFirst)
            {
                string path = SaveLoader.GetActiveSaveFilePath();
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    try { SaveLoader.Instance.Save(path, false, true); }
                    catch { }
                }
            }

            if (loadIntoSave)
            {
                string path = SaveLoader.GetActiveSaveFilePath();
                if (!string.IsNullOrEmpty(path))
                    KPlayerPrefs.SetString("AutoResumeSaveFile", path);
            }

            App.instance.Restart();
        }

        private static void OnCancelRestart()
        {
            if (PauseScreen.Instance != null)
                PauseScreen.Instance.gameObject.SetActive(true);
        }
    }
}
