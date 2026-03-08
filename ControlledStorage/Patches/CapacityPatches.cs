using HarmonyLib;
using TMPro;

namespace ControlledStorage.Patches
{
    // Vanilla 6-char limit blocks modded capacities; we use options to allow more characters.
    [HarmonyPatch(typeof(CapacityControlSideScreen), nameof(CapacityControlSideScreen.OnSpawn))]
    public static class CapacityControlSideScreen_OnSpawn_Patch
    {
        public static void Postfix(CapacityControlSideScreen __instance)
        {
            int characterLimit = ControlledStorageOptions.Instance.TotalCharacterLimit;

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
