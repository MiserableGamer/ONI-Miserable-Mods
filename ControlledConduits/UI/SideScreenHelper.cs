using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using static DetailsScreen;

namespace ControlledConduits.UI
{
    internal static class SideScreenHelper
    {
        private static readonly FieldInfo SideScreensField = AccessTools.Field(typeof(DetailsScreen), "sideScreens");
        private static readonly FieldInfo ContentBodyField =
            AccessTools.Field(typeof(DetailsScreen), "sideScreen2ContentBody") ??
            AccessTools.Field(typeof(DetailsScreen), "sideScreenConfigContentBody");

        internal static void AddSideScreen<T>(string name, DetailsScreen detailsScreen) where T : SideScreenContent
        {
            if (detailsScreen == null)
                return;

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

            var layoutGroup = screenGo.AddComponent<PeterHan.PLib.UI.BoxLayoutGroup>();
            layoutGroup.Params = new PeterHan.PLib.UI.BoxLayoutParams
            {
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
