using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PeterHan.PLib.UI;
using static DetailsScreen;

namespace ControlledAutomation.UI
{
    // Helper to register sidescreens - PLib's method uses a renamed field
    public static class SideScreenHelper
    {
        private static readonly FieldInfo sideScreensField = AccessTools.Field(typeof(DetailsScreen), "sideScreens");
        private static readonly FieldInfo contentBodyField =
            AccessTools.Field(typeof(DetailsScreen), "sideScreen2ContentBody") ??
            AccessTools.Field(typeof(DetailsScreen), "sideScreenConfigContentBody");

        public static void AddSideScreen<T>(string name) where T : SideScreenContent
        {
            if (DetailsScreen.Instance == null)
                return;

            var screens = sideScreensField?.GetValue(DetailsScreen.Instance) as List<SideScreenRef>;
            var contentBody = contentBodyField?.GetValue(DetailsScreen.Instance) as GameObject;

            if (screens == null || contentBody == null)
                return;

            var screenGO = new GameObject(name);
            screenGO.transform.SetParent(contentBody.transform, false);
            screenGO.SetActive(false);

            var rectTransform = screenGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;

            var layoutGroup = screenGO.AddComponent<BoxLayoutGroup>();
            layoutGroup.Params = new BoxLayoutParams
            {
                Alignment = TextAnchor.MiddleLeft,
                Margin = new RectOffset(0, 0, 0, 0)
            };

            var component = screenGO.AddComponent<T>();

            screens.Add(new SideScreenRef
            {
                name = name,
                offset = Vector2.zero,
                screenPrefab = component
            });
        }
    }
}
