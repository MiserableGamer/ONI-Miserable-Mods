using HarmonyLib;
using TMPro;
using UnityEngine;

namespace CapacityControl.Patches
{
	// Increases the character limit on the capacity control text input field
	// The vanilla game limits this to 6 characters, which prevents manually entering
	// larger capacity values when using mods that increase storage capacity
	[HarmonyPatch(typeof(CapacityControlSideScreen), "OnSpawn")]
	public static class CapacityControlSideScreen_OnSpawn_Patch
	{
		public static void Postfix(CapacityControlSideScreen __instance)
		{
			// Get the configured character limit (defaults to 8 if config missing)
			int characterLimit = GetCharacterLimit();

			// Find the KNumberInputField component which contains the TMP_InputField
			var numberInputField = AccessTools.Field(typeof(CapacityControlSideScreen), "numberInput");
			if (numberInputField == null) return;

			var numberInput = numberInputField.GetValue(__instance);
			if (numberInput == null) return;

			// KNumberInputField wraps a TMP_InputField - find it
			var inputFieldComponent = numberInput as Component;
			if (inputFieldComponent == null) return;

			// Get the TMP_InputField from the component or its children
			var tmpInputField = inputFieldComponent.GetComponentInChildren<TMP_InputField>();
			if (tmpInputField != null)
			{
				tmpInputField.characterLimit = characterLimit;
			}
		}

		private static int GetCharacterLimit()
		{
			try
			{
				return CapacityControlOptions.Instance.TotalCharacterLimit;
			}
			catch
			{
				// If options can't be read for any reason, use a safe default
				return 8;
			}
		}
	}
}
