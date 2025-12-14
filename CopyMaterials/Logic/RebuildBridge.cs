using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace CopyMaterialsTool
{
    internal static class RebuildBridge
    {
        private static MethodInfo _requestRebuildMI;
        private static Type _screenType;

        internal static bool TryInit()
        {
            if (_requestRebuildMI != null) return true;

            // In dnSpy, search for the class that implements the "Change Material" side screen.
            // Common name patterns include "ChangeBuildingMaterialSideScreen".
            _screenType = AccessTools.TypeByName("ChangeBuildingMaterialSideScreen");
            if (_screenType == null) return false;

            // In dnSpy, find the method called when pressing the rebuild button.
            // Replace "OnRebuildClicked" with the real name you find.
            _requestRebuildMI = AccessTools.Method(_screenType, "OnRebuildClicked");
            return _requestRebuildMI != null;
        }

        internal static bool RequestRebuild(GameObject building, int[] elementIds)
        {
            if (!TryInit()) return false;

            try
            {
                // The actual handler likely lives on the screen instance, not the Type,
                // so you'll usually need to fetch the active side screen instance.
                // Spaced Out uses DetailsScreen + SideScreenContent. You can locate the instance via:
                // DetailsScreen.Instance.sideScreenRefs, or the screen's singleton, depending on class design.

                var instance = FindActiveChangeMaterialScreenInstance();
                if (instance == null) return false;

                // Many implementations store pending selection on the screen, then click rebuild.
                // So:
                // 1) Set the screen’s selected elements for the current target
                // 2) Call the rebuild click handler
                SetSelectedElementsOnScreen(instance, elementIds);
                _requestRebuildMI.Invoke(instance, null);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[CopyMaterialsTool] RebuildBridge failed: " + e);
                return false;
            }
        }

        private static object FindActiveChangeMaterialScreenInstance()
        {
            // This is intentionally left “pluggable” because the instance access differs by build.
            // In dnSpy: find how the game creates/displays the change-material side screen and copy that access path.
            return null;
        }

        private static void SetSelectedElementsOnScreen(object screenInstance, int[] elementIds)
        {
            // In dnSpy: identify the field the screen uses to store the current element choices.
            // e.g., Traverse.Create(screenInstance).Field("selectedElements").SetValue(<ElementID[]>)
        }
    }
}
