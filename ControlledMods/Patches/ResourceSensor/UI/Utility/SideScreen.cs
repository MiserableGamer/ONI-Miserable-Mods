// Clone-from-vanilla sidescreen helper (same approach as ResourceSensorFIXED)
using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static DetailsScreen;

namespace ControlledMods.ResourceSensor.UI
{
    public static class SideScreen
    {
        public static T AddClonedSideScreen<T>(string name, string originalName, Type originalType, bool addToScreens = true) where T : class
        {
            if (!GetElements(out List<SideScreenRef> screens, out GameObject contentBody))
                return default;

            var oldPrefab = FindOriginal(originalName, screens);
            if (oldPrefab == null) return default;

            var newPrefab = Copy<T>(oldPrefab, contentBody, name, originalType);
            if (addToScreens && newPrefab != null)
                screens.Add(NewSideScreen(name, newPrefab));

            return newPrefab as T;
        }

        private static bool GetElements(out List<SideScreenRef> screens, out GameObject contentBody)
        {
            screens = null;
            contentBody = null;
            var instance = DetailsScreen.Instance;
            if (instance == null) return false;
            screens = AccessTools.Field(instance.GetType(), "sideScreens")?.GetValue(instance) as List<SideScreenRef>;
            contentBody = AccessTools.Field(instance.GetType(), "sideScreen2ContentBody")?.GetValue(instance) as GameObject;
            return screens != null && contentBody != null;
        }

        private static SideScreenContent FindOriginal(string name, List<SideScreenRef> screens)
        {
            var entry = screens.Find(s => s.name == name);
            return entry.screenPrefab;
        }

        private static SideScreenContent Copy<T>(SideScreenContent original, GameObject contentBody, string name, Type originalType)
        {
            var screen = Util.KInstantiateUI<SideScreenContent>(original.gameObject, contentBody).gameObject;
            UnityEngine.Object.Destroy(screen.GetComponent(originalType));

            var prefab = screen.AddComponent(typeof(T)) as SideScreenContent;
            if (prefab != null)
                prefab.name = name.Trim();

            screen.SetActive(false);
            return prefab;
        }

        private static SideScreenRef NewSideScreen(string name, SideScreenContent prefab)
        {
            return new SideScreenRef
            {
                name = name,
                offset = Vector2.zero,
                screenPrefab = prefab
            };
        }
    }
}
