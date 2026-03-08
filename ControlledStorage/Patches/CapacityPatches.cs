using HarmonyLib;
using TMPro;

namespace ControlledStorage.Patches
{
    /// <summary>
    /// Increases the character limit on the capacity control text input field.
    /// Vanilla limits to 6 characters, preventing larger capacity values with mods.
    /// </summary>
    [HarmonyPatch(typeof(CapacityControlSideScreen), nameof(CapacityControlSideScreen.OnSpawn))]
    public static class CapacityControlSideScreen_OnSpawn_Patch
    {
        public static void Postfix(CapacityControlSideScreen __instance)
        {
            int characterLimit = ControlledStorageOptions.Instance.TotalCharacterLimit;

            // Use Harmony Traverse for cleaner field access
            var numberInput = Traverse.Create(__instance)
                .Field("numberInput")
                .GetValue();

            if (numberInput is UnityEngine.Component component)
            {
                var tmpInputField = component.GetComponentInChildren<TMP_InputField>();
                if (tmpInputField != null)
                {
                    tmpInputField.characterLimit = characterLimit;
                }
            }
        }
    }
}
