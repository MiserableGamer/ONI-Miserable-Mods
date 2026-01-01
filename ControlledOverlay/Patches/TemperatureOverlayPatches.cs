using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ControlledOverlay.Patches
{
	// Adds a text input field to the Temperature Overlay's relative temperature slider
	// Allows users to type exact temperature values instead of only using the slider
	[HarmonyPatch(typeof(TemperatureOverlayThresholdAdjustmentWidget), "OnSpawn")]
	public static class TemperatureOverlayThresholdAdjustmentWidget_OnSpawn_Patch
	{
		// Temperature range constants (from the original widget)
		private const float MaxTemperatureRange = 700f;
		private const float TemperatureWindowSize = 200f;
		private static readonly float MinimumSelectionTemperature = TemperatureWindowSize / 2f; // 100K

		public static void Postfix(TemperatureOverlayThresholdAdjustmentWidget __instance)
		{
			// Get the scrollbar and center text via reflection
			var scrollbarField = AccessTools.Field(typeof(TemperatureOverlayThresholdAdjustmentWidget), "scrollbar");
			var centerTextField = AccessTools.Field(typeof(TemperatureOverlayThresholdAdjustmentWidget), "scrollBarRangeCenterText");

			if (scrollbarField == null || centerTextField == null) return;

			var scrollbar = scrollbarField.GetValue(__instance) as Scrollbar;
			var centerText = centerTextField.GetValue(__instance) as LocText;

			if (scrollbar == null || centerText == null) return;

			// Create input field as a sibling of the center text
			CreateTemperatureInputField(__instance, scrollbar, centerText);
		}

		private static void CreateTemperatureInputField(
			TemperatureOverlayThresholdAdjustmentWidget widget,
			Scrollbar scrollbar,
			LocText centerText)
		{
			// Create a new GameObject for our input field
			var inputGO = new GameObject("TemperatureInputField");
			inputGO.transform.SetParent(centerText.transform.parent, false);

			// Add RectTransform and position it near the center text
			var rectTransform = inputGO.AddComponent<RectTransform>();
			
			// Copy position from center text
			var centerRect = centerText.GetComponent<RectTransform>();
			rectTransform.anchorMin = centerRect.anchorMin;
			rectTransform.anchorMax = centerRect.anchorMax;
			rectTransform.pivot = centerRect.pivot;
			rectTransform.anchoredPosition = centerRect.anchoredPosition;
			rectTransform.sizeDelta = new Vector2(80f, 24f);

			// Add Image component for background
			var image = inputGO.AddComponent<Image>();
			image.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

			// Create text area child
			var textAreaGO = new GameObject("Text Area");
			textAreaGO.transform.SetParent(inputGO.transform, false);
			var textAreaRect = textAreaGO.AddComponent<RectTransform>();
			textAreaRect.anchorMin = Vector2.zero;
			textAreaRect.anchorMax = Vector2.one;
			textAreaRect.offsetMin = new Vector2(5f, 2f);
			textAreaRect.offsetMax = new Vector2(-5f, -2f);

			// Create text child
			var textGO = new GameObject("Text");
			textGO.transform.SetParent(textAreaGO.transform, false);
			var textRect = textGO.AddComponent<RectTransform>();
			textRect.anchorMin = Vector2.zero;
			textRect.anchorMax = Vector2.one;
			textRect.offsetMin = Vector2.zero;
			textRect.offsetMax = Vector2.zero;

			// Add TextMeshPro text component
			var textComponent = textGO.AddComponent<TextMeshProUGUI>();
			textComponent.fontSize = 12f;
			textComponent.color = Color.white;
			textComponent.alignment = TextAlignmentOptions.Center;
			textComponent.enableWordWrapping = false;

			// Add TMP_InputField
			var inputField = inputGO.AddComponent<TMP_InputField>();
			inputField.textViewport = textAreaRect;
			inputField.textComponent = textComponent;
			inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
			inputField.characterLimit = 8;
			inputField.caretColor = Color.white;
			inputField.selectionColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);

			// Set initial value from current scrollbar position
			float currentKelvin = GetKelvinFromScrollPercentage(scrollbar.value);
			float displayTemp = GameUtil.GetConvertedTemperature(currentKelvin, false);
			inputField.text = displayTemp.ToString("F0");

			// Subscribe to input field changes
			inputField.onEndEdit.AddListener((string value) =>
			{
				OnTemperatureInputChanged(value, scrollbar);
			});

			// Also update input field when scrollbar changes
			scrollbar.onValueChanged.AddListener((float value) =>
			{
				if (!inputField.isFocused)
				{
					float kelvin = GetKelvinFromScrollPercentage(value);
					float converted = GameUtil.GetConvertedTemperature(kelvin, false);
					inputField.text = converted.ToString("F0");
				}
			});

			// Hide the original center text since we're replacing it with an input
			centerText.gameObject.SetActive(false);
		}

		private static void OnTemperatureInputChanged(string value, Scrollbar scrollbar)
		{
			if (string.IsNullOrEmpty(value)) return;

			if (float.TryParse(value, out float temperature))
			{
				// Convert from user's unit preference to Kelvin
				float kelvin = GameUtil.GetTemperatureConvertedToKelvin(temperature);

				// Clamp to valid range
				float minKelvin = MinimumSelectionTemperature;
				float maxKelvin = MinimumSelectionTemperature + MaxTemperatureRange;
				kelvin = Mathf.Clamp(kelvin, minKelvin, maxKelvin);

				// Convert to scroll percentage
				float scrollPercentage = KelvinToScrollPercentage(kelvin);

				// Set the scrollbar value (this will trigger SetUserConfig via OnValueChanged)
				scrollbar.value = scrollPercentage;
			}
		}

		private static float KelvinToScrollPercentage(float kelvin)
		{
			kelvin -= MinimumSelectionTemperature;
			if (kelvin < 1f)
			{
				kelvin = 1f;
			}
			return Mathf.Clamp01(kelvin / MaxTemperatureRange);
		}

		private static float GetKelvinFromScrollPercentage(float scrollPercentage)
		{
			return MinimumSelectionTemperature + MaxTemperatureRange * scrollPercentage;
		}
	}
}

