using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.UI;
using UnityEngine;
using static DetailsScreen;

namespace MiserableUtils.UI
{
    // Helper to register sidescreens - PLib's PUIUtils.AddSideScreenContent uses a renamed field.
    // Provides both overloads: parameterless (uses DetailsScreen.Instance) and parameterized (caller passes DetailsScreen).
    public static class SideScreenHelper
    {
        private static readonly FieldInfo SideScreensField = AccessTools.Field(typeof(DetailsScreen), "sideScreens");
        private static readonly FieldInfo ContentBodyField =
            AccessTools.Field(typeof(DetailsScreen), "sideScreen2ContentBody") ??
            AccessTools.Field(typeof(DetailsScreen), "sideScreenConfigContentBody");

        public static void AddSideScreen<T>(string name) where T : SideScreenContent
        {
            AddSideScreen<T>(name, DetailsScreen.Instance);
        }

        public static void AddSideScreen<T>(string name, DetailsScreen detailsScreen) where T : SideScreenContent
        {
            if (detailsScreen == null)
                return;

            RegisterScreen<T>(name, detailsScreen);
        }

        private static void RegisterScreen<T>(string name, DetailsScreen detailsScreen) where T : SideScreenContent
        {
            var screens = SideScreensField?.GetValue(detailsScreen) as List<SideScreenRef>;
            var contentBody = ContentBodyField?.GetValue(detailsScreen) as GameObject;
            if (screens == null || contentBody == null)
                return;

            var screenGo = new GameObject(name);
            screenGo.transform.SetParent(contentBody.transform, false);
            screenGo.SetActive(false);

            var rectTransform = screenGo.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;

            var layoutGroup = screenGo.AddComponent<BoxLayoutGroup>();
            layoutGroup.Params = new BoxLayoutParams
            {
                Alignment = TextAnchor.MiddleLeft,
                Margin = new RectOffset(0, 0, 0, 0)
            };

            var component = screenGo.AddComponent<T>();
            screens.Add(new SideScreenRef
            {
                name = name,
                offset = Vector2.zero,
                screenPrefab = component
            });
        }
    }
}
